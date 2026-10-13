using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;

namespace InflexCLR.Environment;

public static class Saumi4
{
	private const string INFLEXUSER_ASSEMBLY_NAME = "InflexUser";

	private const string INFLEXUSER_TYPE_FULLNAME = "InflexCLR.User.Saumi4User";

	private const string INFLEXUSER_TYPE_ONSTART_METHOD_NAME = "OnStart";

	private const string INFLEXUSER_TYPE_ONCLOSE_METHOD_NAME = "OnClose";

	private const string UPDATESERVICE_DIRECTORY_NAME = "UpdateService";

	private const string UPDATESERVICE_SAUMISIDE_ASSEMBLY_NAME = "UpdateServiceSaumiSideSupport";

	private const string UPDATESAUMIMANAGER_TYPE_FULLNAME = "UpdateService.UpdateServiceSaumiSideSupport.UpdateSaumiManager";

	private const string SAS_INTEGRATION_DLL_NAME = "SasIntegration.dll";

	private const string SMEV_DIRECTORY_NAME = "Smev";

	private static string _UserName;

	private static string _Database;

	private static bool _IsInflexUserModuleLoaded;

	private static string _ErrorCompileString;

	private static int[] _Items;

	private static Type _InflexUserType;

	private static Assembly _InflexUserAssembly;

	private static Assembly _SasIntegrationAssembly;

	public static string UserName
	{
		get
		{
			return _UserName;
		}
		set
		{
			_UserName = value;
		}
	}

	public static string Database
	{
		get
		{
			return _Database;
		}
		set
		{
			_Database = value;
		}
	}

	public static bool UserInitModulLoad => _IsInflexUserModuleLoaded;

	public static string ErrorCompileString
	{
		get
		{
			return _ErrorCompileString;
		}
		set
		{
			_ErrorCompileString = value;
		}
	}

	public static int[] Items
	{
		get
		{
			return _Items;
		}
		set
		{
			_Items = value;
		}
	}

	private static Assembly SasIntegrationAssembly
	{
		get
		{
			if ((object)_SasIntegrationAssembly == null)
			{
				string text = Path.Combine(SaumiHelper.SaumiDirectory, Path.Combine("Smev", "SasIntegration.dll"));
				if (File.Exists(text))
				{
					try
					{
						_SasIntegrationAssembly = Assembly.LoadFrom(text);
					}
					catch (Exception)
					{
						return null;
					}
				}
			}
			return _SasIntegrationAssembly;
		}
	}

	static Saumi4()
	{
		_UserName = string.Empty;
		_Database = string.Empty;
		_IsInflexUserModuleLoaded = false;
		_ErrorCompileString = string.Empty;
		_InflexUserType = null;
		_InflexUserAssembly = null;
		try
		{
			AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainAssemblyResolve;
			AppDomain.CurrentDomain.TypeResolve += CurrentDomainTypeResolve;
			_InflexUserAssembly = AppDomain.CurrentDomain.Load("InflexUser");
			Type[] exportedTypes = _InflexUserAssembly.GetExportedTypes();
			foreach (Type type in exportedTypes)
			{
				if (type.FullName == "InflexCLR.User.Saumi4User")
				{
					_IsInflexUserModuleLoaded = true;
					_InflexUserType = type;
					break;
				}
			}
		}
		catch
		{
			_IsInflexUserModuleLoaded = false;
			_InflexUserType = null;
		}
	}

	private static Assembly CurrentDomainTypeResolve(object sender, ResolveEventArgs args)
	{
		if (args.Name == "UpdateService.UpdateServiceSaumiSideSupport.UpdateSaumiManager")
		{
			return Assembly.Load("UpdateServiceSaumiSideSupport");
		}
		return null;
	}

	private static Assembly CurrentDomainAssemblyResolve(object sender, ResolveEventArgs args)
	{
		AssemblyName assemblyName = new AssemblyName(args.Name);
		string text = Path.Combine(SaumiHelper.SaumiDirectory, "UpdateService\\" + assemblyName.Name + ".dll");
		if (File.Exists(text))
		{
			return Assembly.LoadFrom(text);
		}
		return null;
	}

	public static object GetPlugIn(string pluginName)
	{
		try
		{
			Assembly assembly = null;
			string text = Path.GetExtension(pluginName).ToLower();
			assembly = ((!(text == ".dll") && !(text == ".exe")) ? AppDomain.CurrentDomain.Load(pluginName) : Assembly.LoadFrom(pluginName));
			Type[] exportedTypes = assembly.GetExportedTypes();
			foreach (Type type in exportedTypes)
			{
				object[] customAttributes = type.GetCustomAttributes(typeof(PlugInObjectAttribute), inherit: true);
				if (customAttributes.Length > 0)
				{
					Type type2 = type.GetInterface(typeof(ISaumiPlugIn).Name);
					if ((object)type2 != null)
					{
						string fullName = type.FullName;
						return assembly.CreateInstance(fullName);
					}
				}
			}
			throw new Exception("PlugIn объект не найден");
		}
		catch (Exception ex)
		{
			throw new Exception("PlugIn не установлен. " + ex.Message, ex);
		}
	}

	public static void PlugInObjectExecute(ISaumiPlugIn plugin, int threadApartment)
	{
		if (plugin != null)
		{
			Thread thread = new Thread(plugin.Execute);
			if (threadApartment == 1)
			{
				thread.SetApartmentState(ApartmentState.STA);
			}
			if (threadApartment == 2)
			{
				thread.SetApartmentState(ApartmentState.MTA);
			}
			thread.Start();
		}
	}

	private static string[] GetExportClasses(Assembly assembly)
	{
		if ((object)assembly == null)
		{
			return new string[0];
		}
		Dictionary<Type, SaumiExportAttribute> dictionary = new Dictionary<Type, SaumiExportAttribute>();
		Dictionary<Type, SaumiExportAttribute> dictionary2 = new Dictionary<Type, SaumiExportAttribute>();
		List<string> list = new List<string>();
		Type[] exportedTypes = assembly.GetExportedTypes();
		foreach (Type type in exportedTypes)
		{
			object[] customAttributes = type.GetCustomAttributes(typeof(SaumiExportAttribute), inherit: false);
			if (customAttributes.Length > 0 && !type.IsAbstract)
			{
				if ((customAttributes[0] as SaumiExportAttribute).Position >= 0)
				{
					dictionary2.Add(type, customAttributes[0] as SaumiExportAttribute);
				}
				else
				{
					dictionary.Add(type, customAttributes[0] as SaumiExportAttribute);
				}
			}
		}
		if (dictionary2.Count > 0)
		{
			Type[] array = new Type[dictionary2.Count];
			dictionary2.Keys.CopyTo(array, 0);
			SaumiExportAttribute[] array2 = new SaumiExportAttribute[dictionary2.Count];
			dictionary2.Values.CopyTo(array2, 0);
			for (int j = 0; j < dictionary2.Count; j++)
			{
				for (int num = dictionary2.Count - 1; num > j; num--)
				{
					if (IsChangePlaceWithPosition(array[num - 1], array2[num - 1], array[num], array2[num]))
					{
						Type type2 = array[num - 1];
						SaumiExportAttribute saumiExportAttribute = array2[num - 1];
						array[num - 1] = array[num];
						array2[num - 1] = array2[num];
						array[num] = type2;
						array2[num] = saumiExportAttribute;
					}
				}
			}
			Type[] array3 = array;
			foreach (Type type3 in array3)
			{
				list.Add(assembly.Location + "," + type3.FullName);
			}
		}
		if (dictionary.Count > 0)
		{
			Type[] array4 = new Type[dictionary.Count];
			dictionary.Keys.CopyTo(array4, 0);
			SaumiExportAttribute[] array5 = new SaumiExportAttribute[dictionary.Count];
			dictionary.Values.CopyTo(array5, 0);
			for (int l = 0; l < dictionary.Count; l++)
			{
				for (int num2 = dictionary.Count - 1; num2 > l; num2--)
				{
					if (IsChangePlace(array4[num2 - 1], array5[num2 - 1], array4[num2], array5[num2]))
					{
						Type type4 = array4[num2 - 1];
						SaumiExportAttribute saumiExportAttribute2 = array5[num2 - 1];
						array4[num2 - 1] = array4[num2];
						array5[num2 - 1] = array5[num2];
						array4[num2] = type4;
						array5[num2] = saumiExportAttribute2;
					}
				}
			}
			Type[] array6 = array4;
			foreach (Type type5 in array6)
			{
				list.Add(assembly.Location + "," + type5.FullName);
			}
		}
		return list.ToArray();
	}

	public static string[] GetExportClasses()
	{
		string[] exportClasses = GetExportClasses(Assembly.GetExecutingAssembly());
		string[] exportClasses2 = GetExportClasses(_InflexUserAssembly);
		string[] exportClasses3 = GetExportClasses(SasIntegrationAssembly);
		string[] array = new string[exportClasses.Length + exportClasses2.Length + exportClasses3.Length];
		exportClasses.CopyTo(array, 0);
		exportClasses2.CopyTo(array, exportClasses.Length);
		exportClasses3.CopyTo(array, exportClasses.Length + exportClasses2.Length);
		return array;
	}

	private static bool IsChangePlaceWithPosition(Type typeLeft, SaumiExportAttribute attrLeft, Type typeRight, SaumiExportAttribute attrRight)
	{
		if (attrRight.Position != -1)
		{
			if (attrLeft.Position < 0)
			{
				return true;
			}
			if (attrRight.Position < attrLeft.Position)
			{
				return true;
			}
		}
		if (typeLeft.IsSubclassOf(typeRight))
		{
			return true;
		}
		return false;
	}

	private static bool IsChangePlace(Type typeLeft, SaumiExportAttribute attrLeft, Type typeRight, SaumiExportAttribute attrRight)
	{
		if (typeLeft.IsSubclassOf(typeRight))
		{
			return true;
		}
		return false;
	}

	public static void StartUser()
	{
		if ((object)_InflexUserType != null)
		{
			_InflexUserType.GetMethod("OnStart")?.Invoke(null, null);
		}
	}

	public static void CloseUser()
	{
		if ((object)_InflexUserType != null)
		{
			_InflexUserType.GetMethod("OnClose")?.Invoke(null, null);
		}
	}

	public static void TestConnection()
	{
		OracleProvider.GetConnection();
	}

	public static bool CompileAssembly(string assemblyFile, string assemblyBody)
	{
		string[] array = CompileAssemblyErr(assemblyFile, assemblyBody);
		if (array == null || array.Length == 0)
		{
			return false;
		}
		if (bool.TryParse(array[0], out var result))
		{
			return result;
		}
		return false;
	}

	public static string[] CompileAssemblyErr(string assemblyFile, string assemblyBody)
	{
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Expected O, but got Unknown
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Expected O, but got Unknown
		Type type = null;
		object obj = null;
		if (Path.GetFileName(assemblyFile).ToLower() == "sptypes.dll")
		{
			try
			{
				type = Type.GetType("UpdateService.UpdateServiceSaumiSideSupport.UpdateSaumiManager");
				obj = Activator.CreateInstance(type, SaumiHelper.SaumiDirectory, "USER", string.Empty, string.Empty, string.Empty);
				if (!(bool)type.GetMethod("IsUpdateServiceAvailable").Invoke(obj, new object[0]))
				{
					obj = null;
				}
			}
			catch
			{
				obj = null;
			}
		}
		List<string> list = new List<string>();
		ErrorCompileString = string.Empty;
		try
		{
			CodeDomProvider val = CodeDomProvider.CreateProvider("C#");
			string text = assemblyFile;
			if (obj != null)
			{
				string tempFileName = Path.GetTempFileName();
				if (File.Exists(tempFileName))
				{
					File.Delete(tempFileName);
				}
				Directory.CreateDirectory(tempFileName);
				text = Path.Combine(tempFileName, Path.GetFileName(assemblyFile));
			}
			CompilerParameters val2 = new CompilerParameters(new string[3] { "System.dll", "System.Data.dll", "System.Xml.dll" });
			val2.OutputAssembly = text;
			val2.IncludeDebugInformation = false;
			val2.GenerateInMemory = true;
			val2.GenerateExecutable = false;
			val2.CompilerOptions = "/target:library /platform:x86";
			CompilerResults val3 = val.CompileAssemblyFromSource(val2, new string[1] { assemblyBody });
			if (val3.Errors.HasErrors)
			{
				list.Clear();
				list.Add(bool.FalseString);
				foreach (CompilerError item in (CollectionBase)(object)val3.Errors)
				{
					CompilerError val4 = item;
					list.Add(val4.Line + ":" + val4.Column + ":" + (val4.IsWarning ? "warning" : "error") + ": " + val4.ErrorText);
					object errorCompileString = ErrorCompileString;
					ErrorCompileString = string.Concat(new object[9]
					{
						errorCompileString,
						"Строка ",
						val4.Line,
						" >> ",
						val4.IsWarning ? "warning:" : "Error:",
						val4.ErrorText,
						"\nфайл:",
						val4.FileName,
						"\n"
					});
				}
				return list.ToArray();
			}
			if ((object)type != null && obj != null && File.Exists(text))
			{
				try
				{
					byte[] array = File.ReadAllBytes(text);
					Directory.Delete(Path.GetDirectoryName(text), recursive: true);
					type.GetMethod("ProvideSpTypesLib").Invoke(obj, new object[1] { array });
				}
				catch (Exception ex)
				{
					list.Clear();
					list.Add(ex.Message);
					list.Add(ex.StackTrace);
					return list.ToArray();
				}
			}
			list.Clear();
			list.Add(bool.TrueString);
			return list.ToArray();
		}
		catch (Exception ex2)
		{
			list.Clear();
			list.Add(bool.FalseString);
			for (Exception ex3 = ex2; ex3 != null; ex3 = ex3.InnerException)
			{
				list.Add(ex3.Message);
			}
			return list.ToArray();
		}
	}
}
