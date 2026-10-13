using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using BLToolkit.Data;
using BLToolkit.Data.DataProvider;
using BLToolkit.DataAccess;
using BLToolkit.Mapping;
using BLToolkit.Reflection;
using BLToolkit.Reflection.Extension;
using BLToolkit.Reflection.MetadataProvider;
using InflexCore.Action;
using InflexCore.Components;
using InflexCore.DataImplementation;
using InflexCore.DataImplementation.Accessors;
using InflexCore.DataImplementation.DataProvider;
using InflexCore.DataImplementation.Reflection;
using InflexCore.Native.GW;
using InflexCore.Role;
using InflexCore.log;
using OracleUtils.Connection;

namespace InflexCore;

public class AppManager : IDisposable, ISessionCache
{
	public abstract class AppManagerComponent
	{
		private ICollection<AppManagerComponent> _EmptyComponents;

		protected AppManager AppManager { get; private set; }

		protected ICollection<AppManagerComponent> AppManagerComponents
		{
			get
			{
				if (AppManager != null)
				{
					return AppManager._Components;
				}
				return _EmptyComponents;
			}
		}

		protected AppManagerComponent()
		{
			_EmptyComponents = new List<AppManagerComponent>();
		}

		public void SetAppManager(AppManager appManager)
		{
			AppManager = appManager;
		}

		public virtual void Initialize()
		{
		}

		public virtual void AfterInitialize()
		{
		}
	}

	private interface IInflexAccessorBase
	{
		AppManager AppManager { get; set; }
	}

	public abstract class InflexAccessorBase<T, TA> : DataAccessor<T, TA>, IInflexAccessor, IInflexAccessorBase where TA : DataAccessor<T>
	{
		private static MemberMapper[] _mms;

		private static object[] _defaultValues;

		protected AppManager AppManager { get; set; }

		AppManager IInflexAccessorBase.AppManager
		{
			get
			{
				return AppManager;
			}
			set
			{
				AppManager = value;
			}
		}

		protected override DbManager CreateDbManager()
		{
			CheckInstance();
			return _Instance.CreateDbManager();
		}

		public override DbManager GetDbManager()
		{
			return CreateDbManager();
		}

		public void UseTransaction()
		{
			if (AppManager._Transaction == null)
			{
				base.BeginTransaction();
				AppManager._Transaction = base.DbManager.Transaction;
				AppManager._TransactionUseCount = 0;
			}
			AppManager._TransactionUseCount++;
		}

		public override void BeginTransaction()
		{
			if (AppManager._Transaction != null)
			{
				throw new InvalidOperationException("Transaction != null");
			}
			UseTransaction();
		}

		public override void RollbackTransaction()
		{
			AppManager._TransactionUseCount--;
			if (AppManager._Transaction != null && AppManager._TransactionUseCount <= 0)
			{
				AppManager._TransactionUseCount = 0;
				AppManager._Transaction = null;
				base.RollbackTransaction();
			}
		}

		public override void CommitTransaction()
		{
			AppManager._TransactionUseCount--;
			if (AppManager._TransactionUseCount == 0)
			{
				AppManager._Transaction = null;
				base.CommitTransaction();
			}
		}

		public IEnumerable<T> SelectAll()
		{
			return SelectAll<T>();
		}

		public IEnumerable<TN> SelectAll<TN>()
		{
			if ((object)typeof(T) == typeof(TN))
			{
				return new SqlQuery<TN>(GetDbManager(), dispose: true).SelectAll().AsEnumerable();
			}
			return new SqlQuery<T>(GetDbManager(), dispose: true).SelectAll().Cast<TN>();
		}

		public virtual T SelectByKey(params object[] keys)
		{
			return new SqlQuery<T>(GetDbManager(), dispose: true).SelectByKey(keys);
		}

		public void SaveAndFlush(T obj)
		{
			UseTransaction();
			try
			{
				Save(obj, flush: true);
				CommitTransaction();
			}
			catch (Exception)
			{
				RollbackTransaction();
				throw;
			}
		}

		public void Save(T obj)
		{
			Save(obj, flush: false);
		}

		private void Save(T obj, bool flush)
		{
			bool breakSave = false;
			BeforeSave(obj, flush, ref breakSave);
			if (!breakSave)
			{
				if (!IsPersist(obj))
				{
					Insert(obj, flush);
				}
				else
				{
					Update(obj, flush);
				}
				AfterSave(obj, flush);
			}
		}

		private void Insert(T obj, bool flush)
		{
			bool breakInsert = false;
			BeforeInsert(obj, flush, ref breakInsert);
			if (!breakInsert)
			{
				new InflexSqlQuery<T>(GetDbManager(), dispose: true).Insert(obj);
				AfterInsert(obj, flush);
			}
		}

		private void Update(T obj, bool flush)
		{
			bool breakUpdate = false;
			BeforeUpdate(obj, flush, ref breakUpdate);
			if (!breakUpdate)
			{
				new InflexSqlQuery<T>(GetDbManager(), dispose: true).Update(obj);
				AfterUpdate(obj, flush);
			}
		}

		protected virtual void BeforeSave(T obj, bool flush, ref bool breakSave)
		{
			breakSave = false;
		}

		protected virtual void AfterSave(T obj, bool flush)
		{
		}

		private void Update(T obj)
		{
			Update(obj, flush: false);
		}

		protected virtual void BeforeUpdate(T obj, bool flush, ref bool breakUpdate)
		{
			breakUpdate = false;
		}

		protected virtual void AfterUpdate(T obj, bool flush)
		{
		}

		private void Insert(T obj)
		{
			Insert(obj, flush: false);
		}

		protected virtual void BeforeInsert(T obj, bool flush, ref bool breakInsert)
		{
			breakInsert = false;
		}

		protected virtual void AfterInsert(T obj, bool flush)
		{
		}

		public virtual bool IsPersist(T obj)
		{
			if (_mms == null || _defaultValues == null)
			{
				Type typeFromHandle = typeof(T);
				TypeExtension typeExtension = TypeExtension.GetTypeExtension(typeFromHandle, base.Extensions);
				List<MemberMapper> list = new List<MemberMapper>();
				List<object> list2 = new List<object>();
				foreach (MemberMapper item in base.MappingSchema.GetObjectMapper(typeFromHandle))
				{
					if (item.MapMemberInfo.SqlIgnore)
					{
						continue;
					}
					MemberAccessor memberAccessor = item.MemberAccessor;
					if (TypeHelper.IsScalar(memberAccessor.Type))
					{
						base.MappingSchema.MetadataProvider.GetPrimaryKeyOrder(typeFromHandle, typeExtension, memberAccessor, out var isSet);
						if (isSet)
						{
							Type type = item.Type;
							list2.Add(type.Default());
							list.Add(item);
						}
					}
				}
				_mms = list.ToArray();
				_defaultValues = list2.ToArray();
			}
			if (_mms.Length == 0 || _defaultValues.Length == 0 || _mms.Length != _defaultValues.Length)
			{
				throw new InvalidOperationException();
			}
			for (int i = 0; i < _mms.Length; i++)
			{
				object value = _mms[i].GetValue(obj);
				object obj2 = _defaultValues[i];
				if ((value == null && obj2 == null) || (value != null && value.Equals(obj2)) || (obj2 != null && obj2.Equals(value)))
				{
					return false;
				}
			}
			return true;
		}

		Type IInflexAccessor.GetAccessorType()
		{
			return typeof(T);
		}
	}

	public abstract class NativeInflexAccessor
	{
		protected AppManager AppManager { get; private set; }

		protected InflexGwManager Inflex32Manager => AppManager.Inflex32Manager;

		protected virtual IInflexGw Inflex32 => AppManager.Inflex32Manager.Inflex32;

		protected NativeInflexAccessor(AppManager appManager)
		{
			AppManager = appManager;
		}
	}

	[Obsolete]
	public abstract class InflexDbAccessor
	{
		protected string Password
		{
			get
			{
				CheckInstance();
				return _Instance._Password;
			}
		}

		protected IDbConnection DbConnection
		{
			get
			{
				CheckInstance();
				return _Instance._Connection;
			}
		}

		public DbManager CreateDbManager()
		{
			CheckInstance();
			return _Instance.CreateDbManager();
		}
	}

	private readonly string _Password;

	private InflexGwManager _Inflex32Manager;

	private readonly Dictionary<decimal, Dictionary<decimal, IInflexType>> _InflexTypesCache = new Dictionary<decimal, Dictionary<decimal, IInflexType>>();

	private readonly DataProviderBase _DataProvider;

	private readonly IDbConnection _Connection;

	private IDbTransaction _Transaction;

	private int _TransactionUseCount;

	private bool _Disposed;

	private ISessionCache _Cache;

	private ActionManager _ActionList;

	private HashSet<string> _EnabledRoles = new HashSet<string>();

	private InflexLicenseManager _LicenseManager;

	private ICollection<AppManagerComponent> _Components;

	[Obsolete]
	private static AppManager _Instance;

	private InflexGwManager Inflex32Manager
	{
		get
		{
			if (_Inflex32Manager == null)
			{
				_Inflex32Manager = new InflexGwManager(UserName, _Password, DataSource, MainSchema);
			}
			return _Inflex32Manager;
		}
	}

	public INativeSQLAccessor NativeSQLAccessor => Accessor<BlToolkitSQLAccessor>();

	public string InflexPath { get; private set; }

	public ISimpleSettingsAccessor UserSettings { get; private set; }

	public ISimpleSettingsAccessor AppSettings { get; private set; }

	public string UserName { get; set; }

	public string DataSource { get; set; }

	public string MainSchema { get; set; }

	public string AppName { get; set; }

	public InflexLicenseManager LicenseManager
	{
		get
		{
			if (_LicenseManager == null)
			{
				_LicenseManager = new InflexLicenseManager(Inflex32Manager.Inflex32);
			}
			return _LicenseManager;
		}
	}

	public ActionManager ActionList
	{
		get
		{
			if (_ActionList == null)
			{
				_ActionList = new ActionManager();
				InitActions();
			}
			return _ActionList;
		}
	}

	[Obsolete]
	internal static AppManager Instance => _Instance;

	[Obsolete]
	public static bool Initialized => _Instance != null;

	[Obsolete]
	public static string InflexPathStatic
	{
		get
		{
			CheckInstance();
			return _Instance.InflexPath;
		}
	}

	[Obsolete]
	public static string CurrentUser
	{
		get
		{
			CheckInstance();
			return _Instance.UserName;
		}
	}

	[Obsolete]
	public static string DataBase
	{
		get
		{
			CheckInstance();
			return _Instance.DataSource;
		}
	}

	[Obsolete]
	public static string InflexSchema
	{
		get
		{
			CheckInstance();
			return _Instance.MainSchema;
		}
	}

	[Obsolete]
	public static ActionManager ActionManager
	{
		get
		{
			CheckInstance();
			return _Instance.ActionList;
		}
	}

	static AppManager()
	{
		DbManager.AddDataProvider(new InflexOdpDataProvider(showSql: true));
		MetadataProviderBase.OnCreateProvider += MetadataProviderBase_OnCreateProvider;
		FastLog.Configure(new FileInfo("log\\log4net.config.xml"));
	}

	private static void MetadataProviderBase_OnCreateProvider(MetadataProviderBase parentProvider)
	{
		parentProvider.AddProvider(new InflexMetadataProvider());
	}

	public static AppManager Init(string userName, string password, string dataBase, string inflexSchema)
	{
		InflexConnection inflexConnection = new InflexConnection();
		inflexConnection.UserName = userName;
		inflexConnection.Password = password;
		inflexConnection.DataBase = dataBase;
		inflexConnection.MainSchema = inflexSchema;
		return Init(inflexConnection);
	}

	public static AppManager Init(InflexConnection connection)
	{
		if (_Instance != null)
		{
			throw new NotSupportedException();
		}
		_Instance = new AppManager(connection.UserName, connection.Password, connection.DataBase, connection.DbaPrivilege, connection.MainSchema);
		_Instance.Initialize();
		return _Instance;
	}

	public bool RegisterComponents(params AppManagerComponent[] components)
	{
		if (_Components.Count > 0)
		{
			throw new NotSupportedException("Only one time register components supported.");
		}
		bool flag = false;
		foreach (AppManagerComponent appManagerComponent in components)
		{
			_Components.Add(appManagerComponent);
			appManagerComponent.SetAppManager(this);
			if (appManagerComponent is NsActionManager)
			{
				flag = true;
			}
		}
		if (!flag)
		{
			NsActionManager nsActionManager = new NsActionManager();
			_Components.Add(nsActionManager);
			nsActionManager.SetAppManager(this);
		}
		if (ValidateComponentsCollection())
		{
			foreach (AppManagerComponent component in _Components)
			{
				component.Initialize();
			}
			foreach (AppManagerComponent component2 in _Components)
			{
				component2.AfterInitialize();
			}
			return true;
		}
		return false;
	}

	private bool ValidateComponentsCollection()
	{
		foreach (AppManagerComponent component in _Components)
		{
			IEnumerable<ReferenceComponentAttribute> enumerable = Attribute.GetCustomAttributes(component.GetType(), typeof(ReferenceComponentAttribute)).Cast<ReferenceComponentAttribute>();
			foreach (ReferenceComponentAttribute item in enumerable)
			{
				ReferenceComponentAttribute attributeLocal = item;
				if (_Components.Where((AppManagerComponent c) => attributeLocal.ReferenceComponent.IsInstanceOfType(c)).FirstOrDefault() == null)
				{
					return false;
				}
			}
			IEnumerable<SingleComponentAttribute> enumerable2 = Attribute.GetCustomAttributes(component.GetType(), typeof(SingleComponentAttribute)).Cast<SingleComponentAttribute>();
			foreach (SingleComponentAttribute item2 in enumerable2)
			{
				SingleComponentAttribute attributeLocal2 = item2;
				if (_Components.Where((AppManagerComponent c) => attributeLocal2.SingleComponent.IsInstanceOfType(c)).Count() != 1)
				{
					return false;
				}
			}
		}
		return true;
	}

	public TA Accessor<TA>() where TA : DataAccessor
	{
		TA val = DataAccessor.CreateInstance<TA>(CreateDbManager(), dispose: true);
		if (val is IInflexAccessorBase)
		{
			(val as IInflexAccessorBase).AppManager = this;
		}
		return val;
	}

	public bool RoleEnabled<T>() where T : NameRole
	{
		return RoleEnabled(typeof(T));
	}

	public bool RoleEnabled(Type roleType)
	{
		if ((object)roleType == null)
		{
			throw new ArgumentNullException("roleType");
		}
		if (!typeof(NameRole).IsAssignableFrom(roleType))
		{
			throw new ArgumentException("NameRole is not assignable from " + roleType);
		}
		NameRole role = (NameRole)Activator.CreateInstance(roleType, this);
		return RoleEnabled(role);
	}

	public bool RoleEnabled(NameRole role)
	{
		if (role == null)
		{
			throw new ArgumentNullException("role");
		}
		return _EnabledRoles.Contains(role.RoleName);
	}

	public IEnumerable<IInflexType> GetTypes()
	{
		return Accessor<InflexTypeAccessor>().SelectAll<IInflexType>();
	}

	public IEnumerable<IInflexType> GetDictTypes()
	{
		return Accessor<InflexDictionaryTypeAccessor>().SelectAll<IInflexType>();
	}

	public IEnumerable<IInflexReport> GetReports()
	{
		return Accessor<InflexReportAccessor>().SelectAll<IInflexReport>();
	}

	public IInflexReport GetReport(decimal reportID)
	{
		return Accessor<InflexReportAccessor>().SelectByKey(reportID);
	}

	public IInflexType GetType(decimal typeID)
	{
		return GetType(typeID, 0m);
	}

	public IInflexType GetType(decimal typeID, decimal subTypeID)
	{
		if (!_InflexTypesCache.TryGetValue(typeID, out var value))
		{
			value = new Dictionary<decimal, IInflexType>();
			_InflexTypesCache[typeID] = value;
		}
		if (!value.TryGetValue(subTypeID, out var value2))
		{
			value2 = (value[subTypeID] = Accessor<InflexTypeAccessor>().SelectByType(typeID, subTypeID));
		}
		return value2;
	}

	public IInflexType GetDictType(decimal dictTypeID)
	{
		return Accessor<InflexDictionaryTypeAccessor>().SelectByKey(dictTypeID);
	}

	private AppManager()
	{
		AppName = "SAUMI";
		InflexPath = AppDomain.CurrentDomain.BaseDirectory;
		_Components = new List<AppManagerComponent>();
	}

	private AppManager(string userName, string password, string dataBase, ConnectionPrivileges dbaPrivilege, string mainSchema)
		: this()
	{
		_Password = password ?? "";
		UserName = userName ?? "";
		DataSource = dataBase ?? "";
		MainSchema = mainSchema ?? "";
		string shortConnectionString = ConnectionUtils.GetShortConnectionString(DataSource, UserName, _Password, dbaPrivilege);
		DbManager.AddConnectionString("Odp", "AppManager_Default", shortConnectionString);
		DbManager.DefaultConfiguration = "AppManager_Default";
		_DataProvider = DbManager.GetDataProvider("Odp.AppManager_Default");
		_Connection = _DataProvider.CreateConnectionObject();
		_Connection.ConnectionString = DbManager.GetConnectionString("Odp.AppManager_Default");
		_Connection.Open();
	}

	private void Initialize()
	{
		LoadEnabledRoles();
		UserSettings = new UserSettings(Accessor<InflexUserSettingsAccessor>());
		AppSettings = new AppSettings(this);
	}

	private DbManager CreateDbManager()
	{
		if (_Transaction != null)
		{
			return new DbManager(_DataProvider, _Transaction);
		}
		return new DbManager(_DataProvider, _Connection);
	}

	private void LoadEnabledRoles()
	{
		foreach (SessionRoles item in Accessor<SessionRolesAccessor>().SelectAll())
		{
			_EnabledRoles.Add(item.RoleName);
		}
	}

	private void InitActions()
	{
		if (_Components.Count == 0)
		{
			RegisterComponents();
		}
		foreach (AppManagerComponent component in _Components)
		{
			if (component is IActionFactory actionFactory)
			{
				_ActionList.AddActionFactory(actionFactory);
			}
		}
		_ActionList.AddActionFactory(new ShowReferenceActionFactory(Inflex32Manager));
		_ActionList.AddActionFactory(new FastReportFormActionFactory(this, Inflex32Manager));
	}

	public void Dispose()
	{
		if (!_Disposed)
		{
			try
			{
				_Disposed = true;
				DirectDispose();
			}
			finally
			{
				Done(this);
			}
		}
	}

	private void DirectDispose()
	{
		if (_Connection != null)
		{
			try
			{
				_Connection.Dispose();
			}
			catch (Exception ex)
			{
				FastLog.Loger<AppManager>().Error((object)"Connection dispose", ex);
			}
		}
		if (_Inflex32Manager != null)
		{
			try
			{
				_Inflex32Manager.Dispose();
			}
			catch (Exception ex2)
			{
				FastLog.Loger<AppManager>().Error((object)"Inflex32Manager dispose", ex2);
			}
			_Inflex32Manager = null;
		}
	}

	object ISessionCache.Get(ISessionCacheObject cacheObject)
	{
		InitCache();
		return _Cache.Get(cacheObject);
	}

	void ISessionCache.Set(ISessionCacheObject cacheObject)
	{
		InitCache();
		_Cache.Set(cacheObject);
	}

	private void InitCache()
	{
		if (_Cache == null)
		{
			_Cache = new SimpleSessionCache();
		}
	}

	[Obsolete]
	private static void Done(AppManager manager)
	{
		if (_Instance == manager)
		{
			_Instance = null;
			return;
		}
		throw new NotSupportedException();
	}

	[Obsolete]
	public static TA AccessorStatic<TA>() where TA : DataAccessor
	{
		CheckInstance();
		return _Instance.Accessor<TA>();
	}

	[Obsolete]
	private static void CheckInstance()
	{
		if (_Instance == null)
		{
			throw new InvalidOperationException();
		}
	}

	[Obsolete]
	public static IEnumerable<IInflexType> GetTypesStatic()
	{
		CheckInstance();
		return _Instance.GetTypes();
	}

	[Obsolete]
	public static IEnumerable<IInflexReport> GetReportsStatic()
	{
		CheckInstance();
		return _Instance.GetReports();
	}

	[Obsolete]
	public static IInflexReport GetReportStatic(decimal reportID)
	{
		CheckInstance();
		return _Instance.GetReport(reportID);
	}

	[Obsolete]
	public static IInflexType GetTypeStatic(decimal typeID, decimal subTypeID)
	{
		CheckInstance();
		return _Instance.GetType(typeID, subTypeID);
	}

	[Obsolete]
	public static IInflexType GetTypeStatic(decimal typeID)
	{
		return GetTypeStatic(typeID, 0m);
	}
}
