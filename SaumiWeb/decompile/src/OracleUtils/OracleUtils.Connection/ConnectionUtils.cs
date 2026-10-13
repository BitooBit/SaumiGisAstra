using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using System.Threading;
using CommonUtils;
using CommonUtils.AppDomainUtils;
using CommonUtils.EnvironmentUtils;
using CommonUtils.ReportUtils;
using OracleUtils.Scanner;

namespace OracleUtils.Connection;

public static class ConnectionUtils
{
	public const string PASSWORD_MASKED_VALUE = "*****";

	public const ConnectionPrivileges DEFAULT_CONNECTION_PRIVILEGE = ConnectionPrivileges.NORMAL;

	public const bool DEFAULT_POOLING_VALUE = false;

	public const string DEFAULT_CONNECTION_TIMEOUT = "15";

	public static string GetShortConnectionString(string datasource, string user, string password, ConnectionPrivileges privilege = ConnectionPrivileges.NORMAL, bool pooling = false, string timeout = "15")
	{
		return string.Format("DATA SOURCE={0};USER ID=\"{1}\";PASSWORD=\"{2}\";{3}POOLING={4};{5}", new object[6]
		{
			datasource,
			user,
			password,
			(privilege == ConnectionPrivileges.NORMAL) ? string.Empty : string.Concat("DBA Privilege=", privilege, ";"),
			pooling ? "TRUE" : "FALSE",
			(timeout == "15") ? string.Empty : ("CONNECTION TIMEOUT=" + timeout + ";")
		});
	}

	public static string GetNologShortConnectionString(string datasource, ConnectionPrivileges privilege = ConnectionPrivileges.NORMAL, bool pooling = false, string timeout = "15")
	{
		return string.Format("DATA SOURCE={0};{1}POOLING={2};{3}", new object[4]
		{
			datasource,
			(privilege == ConnectionPrivileges.NORMAL) ? string.Empty : string.Concat("DBA Privilege=", privilege, ";"),
			pooling ? "TRUE" : "FALSE",
			(timeout == "15") ? string.Empty : ("CONNECTION TIMEOUT=" + timeout + ";")
		});
	}

	public static string GetFullConnectionString(string host, string port, string service, string user, string password, ConnectionPrivileges privilege = ConnectionPrivileges.NORMAL, bool pooling = false, string timeout = "15")
	{
		return string.Format("DATA SOURCE=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST={0})(PORT={1})))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME={2})));USER ID=\"{3}\";PASSWORD=\"{4}\";{5}POOLING={6};{7}", new object[8]
		{
			host,
			port,
			service,
			user,
			password,
			(privilege == ConnectionPrivileges.NORMAL) ? string.Empty : string.Concat("DBA Privilege=", privilege, ";"),
			pooling ? "TRUE" : "FALSE",
			(timeout == "15") ? string.Empty : ("CONNECTION TIMEOUT=" + timeout + ";")
		});
	}

	public static string GetNologFullConnectionString(string host, string port, string service, ConnectionPrivileges privilege = ConnectionPrivileges.NORMAL, bool pooling = false, string timeout = "15")
	{
		return string.Format("DATA SOURCE=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST={0})(PORT={1})))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME={2})));{3}POOLING={4};{5}", new object[6]
		{
			host,
			port,
			service,
			(privilege == ConnectionPrivileges.NORMAL) ? string.Empty : string.Concat("DBA Privilege=", privilege, ";"),
			pooling ? "TRUE" : "FALSE",
			(timeout == "15") ? string.Empty : ("CONNECTION TIMEOUT=" + timeout + ";")
		});
	}

	public static string SecureConnectionString(string connectionString)
	{
		string pattern = "PASSWORD\\s*=\\s*\"(.*?)\";";
		string pattern2 = "PASSWORD\\s*=\\s*(.*?);";
		bool flag = false;
		Regex regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
		Match match = regex.Match(connectionString);
		if (!match.Success)
		{
			flag = true;
			regex = new Regex(pattern2, RegexOptions.IgnoreCase | RegexOptions.Singleline);
			match = regex.Match(connectionString);
			if (!match.Success)
			{
				UtilsLoggerWrapper.Warning("Can't secure connection string, password value wasn't found at connection string, cause return empty string.");
				return string.Empty;
			}
		}
		string value = match.Groups[0].Value;
		string newValue = string.Format("PASSWORD={0}{1}{0};", flag ? string.Empty : "\"", "*****");
		return connectionString.Replace(value, newValue);
	}

	public static bool TryOpenConnection(string connectionString, out IDbConnection connection, out AssemblyNameDescription odpNetAssemblyDesciption, out Exception catchedException, bool shouldSearchReferencesRecursively = true)
	{
		if (!TryGetOdpNetAssembly(shouldSearchReferencesRecursively, out var odpNetAssembly) || (object)odpNetAssembly == null)
		{
			UtilsLoggerWrapper.Error("Can't connect to Oracle database! ODP.NET assembly was not found!");
			TypeLoadException ex = new TypeLoadException("Can't load OracleConnection type! ODP.NET assembly was not found!");
			connection = null;
			odpNetAssemblyDesciption = null;
			catchedException = ex;
			return false;
		}
		odpNetAssemblyDesciption = new AssemblyNameDescription(odpNetAssembly);
		return TryOpenConnection(connectionString, odpNetAssembly, out connection, out catchedException);
	}

	public static bool TryOpenConnection(string connectionString, string odpNetAssemblyPath, out IDbConnection connection, out Exception catchedException)
	{
		Assembly odpNetAssembly = GetOdpNetAssembly(odpNetAssemblyPath);
		if ((object)odpNetAssembly == null)
		{
			UtilsLoggerWrapper.Error("Can't connect to Oracle database! ODP.NET assembly was not found!");
			TypeLoadException ex = new TypeLoadException("Can't load OracleConnection type! ODP.NET assembly was not found!");
			connection = null;
			catchedException = ex;
			return false;
		}
		return TryOpenConnection(connectionString, odpNetAssembly, out connection, out catchedException);
	}

	private static bool TryOpenConnection(string connectionString, Assembly odpNetAssembly, out IDbConnection connection, out Exception catchedException)
	{
		try
		{
			Type type = odpNetAssembly.GetType("Oracle.DataAccess.Client.OracleConnection");
			UtilsLoggerWrapper.Info("Before OracleConnection.IsAvailable");
			PropertyInfo property = type.GetProperty("IsAvailable", BindingFlags.Static | BindingFlags.Public);
			bool flag = (bool)property.GetValue(null, null);
			UtilsLoggerWrapper.Info("After OracleConnection.IsAvailable, result = " + flag);
			UtilsLoggerWrapper.Info("Before OracleConnection.ctor() using connection string = " + SecureConnectionString(connectionString));
			connection = (IDbConnection)Activator.CreateInstance(type, BindingFlags.Instance | BindingFlags.Public, null, new object[1] { connectionString }, null);
			UtilsLoggerWrapper.Info("After OracleConnection.ctor()");
			UtilsLoggerWrapper.Info("Before OracleConnection.Open()");
			connection.Open();
			UtilsLoggerWrapper.Info("After OracleConnection.Open()");
			catchedException = null;
			return true;
		}
		catch (Exception ex)
		{
			UtilsLoggerWrapper.Error("Can't connect to Oracle database! Exception occurred: " + ex);
			connection = null;
			catchedException = ex;
			return false;
		}
	}

	public static void CloseConnection(IDbConnection connection)
	{
		if (connection != null)
		{
			UtilsLoggerWrapper.Info("Before OracleConnection.Close()");
			connection.Close();
			UtilsLoggerWrapper.Info("After OracleConnection.Close()");
			UtilsLoggerWrapper.Info("Before OracleConnection.Dispose()");
			connection.Dispose();
			UtilsLoggerWrapper.Info("After OracleConnection.Dispose()");
		}
	}

	public static bool TryGetOracleExceptionNumber(Exception exception, out int? oracleExceptionNumber)
	{
		if (!AssemblyLoadUtils.TryFindAppDomainAssemblyByShortName("Oracle.DataAccess", out var foundAssembly) || (object)foundAssembly == null)
		{
			UtilsLoggerWrapper.Error("Can't get OracleException number! ODP.NET assembly was not found!");
			oracleExceptionNumber = null;
			return false;
		}
		oracleExceptionNumber = GetOracleExceptionNumber(foundAssembly, exception);
		return oracleExceptionNumber.HasValue;
	}

	private static int? GetOracleExceptionNumber(string odpNetAssemblyPath, Exception exception)
	{
		Assembly odpNetAssembly = GetOdpNetAssembly(odpNetAssemblyPath);
		if ((object)odpNetAssembly == null)
		{
			UtilsLoggerWrapper.Error("Can't get OracleException number! ODP.NET assembly was not found!");
			return null;
		}
		return GetOracleExceptionNumber(odpNetAssembly, exception);
	}

	private static int? GetOracleExceptionNumber(Assembly odpNetAssembly, Exception exception)
	{
		if (exception == null)
		{
			UtilsLoggerWrapper.Error("Can't get OracleException number! Specified exception can't be null!");
			return null;
		}
		Type type = exception.GetType();
		Type type2 = odpNetAssembly.GetType("Oracle.DataAccess.Client.OracleException");
		if ((object)type == type2)
		{
			PropertyInfo property = type2.GetProperty("Number", BindingFlags.Instance | BindingFlags.Public);
			if ((object)property != null)
			{
				return (int)property.GetValue(exception, null);
			}
		}
		return null;
	}

	private static void UnloadClientUnmanagedLibraries(string odpNetUnmanagedLibrariesPath)
	{
		try
		{
			UtilsLoggerWrapper.Info("Get all modules loaded from ODP.NET unmanaged libraries directory path " + odpNetUnmanagedLibrariesPath);
			IEnumerable<string> allLoadedModuleNamesToCurrentProcessFrom = ProcessUtils.GetAllLoadedModuleNamesToCurrentProcessFrom(odpNetUnmanagedLibrariesPath);
			if (allLoadedModuleNamesToCurrentProcessFrom != null && allLoadedModuleNamesToCurrentProcessFrom.Any())
			{
				UtilsLoggerWrapper.Info("Unmanaged ODP.NET libraries were loaded to current process, try to unload their...");
				IList<string> list = new List<string>();
				foreach (string item in allLoadedModuleNamesToCurrentProcessFrom)
				{
					if (!ProcessUtils.IsModuleLoadedToCurrentProcess(item))
					{
						UtilsLoggerWrapper.Warning("Module " + item + " was already unloaded! Should not to unload it!");
					}
					else if (!ProcessUtils.UnloadModuleFromCurrentProcess(item))
					{
						list.Add(item);
						UtilsLoggerWrapper.Warning("Can't unload module " + item + " now!");
					}
					else
					{
						UtilsLoggerWrapper.Info("Module " + item + " was successfully unloaded");
					}
				}
				if (list.Count > 0)
				{
					bool flag = true;
					foreach (string item2 in list)
					{
						if (ProcessUtils.IsModuleLoadedToCurrentProcess(item2))
						{
							flag = false;
						}
					}
					if (flag)
					{
						UtilsLoggerWrapper.Info("All unmanaged ODP.NET libraries were unloaded from current process");
					}
					else
					{
						UtilsLoggerWrapper.Error("Can't unload all unmanaged ODP.NET libraries from current process!");
					}
				}
				else
				{
					UtilsLoggerWrapper.Info("All unmanaged ODP.NET libraries were successfully unloaded from current process");
				}
			}
			else
			{
				UtilsLoggerWrapper.Info("Unmanaged ODP.NET libraries were not loaded to current process");
			}
		}
		catch (Exception ex)
		{
			UtilsLoggerWrapper.Error("Can't unload client unmanaged libraries. Exception occurred: " + ex);
		}
	}

	public static ConnectionResult CheckConnectionUsingAppDomain(string connectionString, string odpNetAssemblyPath, string odpNetUnmanagedLibrariesPath, bool traceProcessModules = false)
	{
		UtilsLoggerWrapper.Info("Checking connection using temporary application domain...");
		AppDomainConnectionChecker appDomainConnectionChecker = new AppDomainConnectionChecker();
		if (traceProcessModules)
		{
			UtilsLoggerWrapper.Info("Process modules before:");
			UtilsLoggerWrapper.Info(EnvironmentScanner.CallingProcessDescription);
		}
		string text = (FileLogger.IsInitialized ? FileLogger.LogFilePath : null);
		ConnectionResult result = appDomainConnectionChecker.Execute(new string[3] { connectionString, odpNetAssemblyPath, text }, "CheckConnectionDomain");
		if (traceProcessModules)
		{
			UtilsLoggerWrapper.Info("Process modules after connection checking:");
			UtilsLoggerWrapper.Info(EnvironmentScanner.CallingProcessDescription);
		}
		UtilsLoggerWrapper.Info("Unloading client unmanaged libraries...");
		UnloadClientUnmanagedLibraries(odpNetUnmanagedLibrariesPath);
		if (traceProcessModules)
		{
			UtilsLoggerWrapper.Info("Process modules after unloading client unmanaged libraries:");
			UtilsLoggerWrapper.Info(EnvironmentScanner.CallingProcessDescription);
		}
		return result;
	}

	public static ConnectionResult CheckConnectionUsingProcess(string connectionString, string odpNetAssemblyPath)
	{
		string logFilePath = FileLogger.LogFilePath;
		bool flag = FileLogger.IsInitialized && !string.IsNullOrEmpty(logFilePath);
		string tempFileName = Path.GetTempFileName();
		string directoryName = Path.GetDirectoryName(tempFileName);
		string fileName = Path.GetFileName(tempFileName);
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
		AppDomain currentDomain = AppDomain.CurrentDomain;
		AssemblyBuilder assemblyBuilder = currentDomain.DefineDynamicAssembly(new AssemblyName
		{
			Name = fileNameWithoutExtension
		}, (AssemblyBuilderAccess)2, directoryName);
		ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(fileNameWithoutExtension, fileName);
		TypeBuilder typeBuilder = moduleBuilder.DefineType("Program", TypeAttributes.Public);
		MethodBuilder methodBuilder = typeBuilder.DefineMethod("Main", MethodAttributes.Public | MethodAttributes.Static, null, null);
		ILGenerator iLGenerator = methodBuilder.GetILGenerator();
		if (flag)
		{
			string text = typeof(FileLogger).Namespace;
			string text2 = typeof(UtilsLoggerWrapper).Namespace;
			if (string.CompareOrdinal(text, text2) == 0)
			{
				iLGenerator.UsingNamespace(text);
			}
			else
			{
				iLGenerator.UsingNamespace(text);
				iLGenerator.UsingNamespace(text2);
			}
		}
		string usingNamespace = typeof(ConnectionUtils).Namespace;
		iLGenerator.UsingNamespace(usingNamespace);
		if (flag)
		{
			iLGenerator.Emit(OpCodes.Ldstr, logFilePath);
			iLGenerator.Emit(OpCodes.Ldc_I4, 10485760L);
			iLGenerator.Emit(OpCodes.Conv_I8);
			iLGenerator.Emit(OpCodes.Ldc_I4, Convert.ToInt32(value: true));
			iLGenerator.Emit(OpCodes.Call, typeof(FileLogger).GetMethod("Initialize", BindingFlags.Static | BindingFlags.Public, null, new Type[3]
			{
				typeof(string),
				typeof(long),
				typeof(bool)
			}, null));
			ConstructorInfo constructor = typeof(Action<string>).GetConstructor(new Type[2]
			{
				typeof(object),
				typeof(IntPtr)
			});
			iLGenerator.Emit(OpCodes.Nop);
			iLGenerator.Emit(OpCodes.Ldnull);
			MethodInfo method = typeof(FileLogger).GetMethod("Info", BindingFlags.Static | BindingFlags.Public, null, new Type[1] { typeof(string) }, null);
			iLGenerator.Emit(OpCodes.Ldftn, method);
			iLGenerator.Emit(OpCodes.Newobj, constructor);
			iLGenerator.Emit(OpCodes.Ldnull);
			MethodInfo method2 = typeof(FileLogger).GetMethod("Warning", BindingFlags.Static | BindingFlags.Public, null, new Type[1] { typeof(string) }, null);
			iLGenerator.Emit(OpCodes.Ldftn, method2);
			iLGenerator.Emit(OpCodes.Newobj, constructor);
			iLGenerator.Emit(OpCodes.Ldnull);
			MethodInfo method3 = typeof(FileLogger).GetMethod("Error", BindingFlags.Static | BindingFlags.Public, null, new Type[1] { typeof(string) }, null);
			iLGenerator.Emit(OpCodes.Ldftn, method3);
			iLGenerator.Emit(OpCodes.Newobj, constructor);
			iLGenerator.Emit(OpCodes.Call, typeof(UtilsLoggerWrapper).GetMethod("SetLoggerProvider", BindingFlags.Static | BindingFlags.Public, null, new Type[3]
			{
				typeof(Action<string>),
				typeof(Action<string>),
				typeof(Action<string>)
			}, null));
		}
		iLGenerator.BeginExceptionBlock();
		iLGenerator.Emit(OpCodes.Ldstr, connectionString);
		iLGenerator.Emit(OpCodes.Ldstr, odpNetAssemblyPath);
		MethodInfo method4 = typeof(ConnectionUtils).GetMethod("CheckConnection", BindingFlags.Static | BindingFlags.Public, null, new Type[2]
		{
			typeof(string),
			typeof(string)
		}, null);
		iLGenerator.Emit(OpCodes.Call, method4);
		iLGenerator.Emit(OpCodes.Call, typeof(Environment).GetMethod("Exit", BindingFlags.Static | BindingFlags.Public, null, new Type[1] { typeof(int) }, null));
		iLGenerator.BeginCatchBlock(typeof(Exception));
		iLGenerator.Emit(OpCodes.Pop);
		iLGenerator.Emit(OpCodes.Ldc_I4, 0);
		iLGenerator.Emit(OpCodes.Call, typeof(Environment).GetMethod("Exit", BindingFlags.Static | BindingFlags.Public, null, new Type[1] { typeof(int) }, null));
		iLGenerator.EndExceptionBlock();
		iLGenerator.Emit(OpCodes.Ret);
		typeBuilder.CreateType();
		assemblyBuilder.SetEntryPoint(methodBuilder.GetBaseDefinition(), PEFileKinds.ConsoleApplication);
		AppPlatformBits systemEnvironmentBits = EnvironmentScanner.SystemEnvironmentBits;
		PePlatformType pePlatformType = PeHeaderScanner.GetPePlatformType(odpNetAssemblyPath);
		if (systemEnvironmentBits == AppPlatformBits.X64 && pePlatformType == PePlatformType.x86)
		{
			assemblyBuilder.Save(fileName, PortableExecutableKinds.Required32Bit, ImageFileMachine.I386);
		}
		else
		{
			assemblyBuilder.Save(fileName);
		}
		string baseDirectory = currentDomain.BaseDirectory;
		string fileName2 = Path.GetFileName(typeof(CommonConstants).Assembly.Location);
		string fileName3 = Path.GetFileName(typeof(OracleConstants).Assembly.Location);
		string text3 = Path.Combine(directoryName, fileName2);
		string text4 = Path.Combine(directoryName, fileName3);
		File.Copy(Path.Combine(baseDirectory, fileName2), text3, overwrite: true);
		File.Copy(Path.Combine(baseDirectory, fileName3), text4, overwrite: true);
		File.Move(tempFileName, tempFileName = Path.ChangeExtension(tempFileName, "exe"));
		ProcessStartInfo processStartInfo = new ProcessStartInfo(tempFileName);
		processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
		ProcessStartInfo startInfo = processStartInfo;
		ConnectionResult exitCode;
		using (Process process = Process.Start(startInfo))
		{
			process.WaitForExit();
			exitCode = (ConnectionResult)process.ExitCode;
		}
		File.Delete(text4);
		File.Delete(text3);
		int num = 0;
		bool flag2 = false;
		Exception ex = null;
		while (!flag2 && num < 5)
		{
			try
			{
				Thread.Sleep(200);
				num++;
				File.Delete(tempFileName);
				flag2 = true;
			}
			catch (Exception ex2)
			{
				ex = ex2;
			}
		}
		if (!flag2)
		{
			throw ex ?? new IOException("Can't remove file " + tempFileName);
		}
		return exitCode;
	}

	public static ConnectionResult CheckConnection(string connectionString, string odpNetAssemblyPath)
	{
		if (string.IsNullOrEmpty(connectionString))
		{
			UtilsLoggerWrapper.Error("Connection string is not specified, creating OracleConnection was aborted!");
			return ConnectionResult.NotExistsOrUnavailable;
		}
		Exception catchedException = null;
		UtilsLoggerWrapper.Info("Trying open connection...");
		TryOpenConnection(connectionString, odpNetAssemblyPath, out var connection, out catchedException);
		UtilsLoggerWrapper.Info("Closing connection...");
		CloseConnection(connection);
		if (catchedException != null)
		{
			UtilsLoggerWrapper.Error("Exception " + catchedException.Message + " was handled during the opening connection");
			int? oracleExceptionNumber = GetOracleExceptionNumber(odpNetAssemblyPath, catchedException);
			if (oracleExceptionNumber.HasValue)
			{
				int valueOrDefault = oracleExceptionNumber.GetValueOrDefault();
				if (oracleExceptionNumber.HasValue && valueOrDefault == 1017)
				{
					UtilsLoggerWrapper.Error("Specified USERNAME or PASSWORD is WRONG, specified database is available, but connection wasn't opened!");
					return ConnectionResult.ExistsAndAvailable;
				}
				UtilsLoggerWrapper.Error(catchedException.ToString());
				return ConnectionResult.NotExistsOrUnavailable;
			}
			UtilsLoggerWrapper.Error(catchedException.ToString());
			return ConnectionResult.Failed;
		}
		UtilsLoggerWrapper.Info("Connection was successfully opened");
		return ConnectionResult.Connected;
	}

	public static bool IsConnectionPossible(string connectionString, string odpNetAssemblyPath)
	{
		ConnectionResult connectionResult = CheckConnection(connectionString, odpNetAssemblyPath);
		if (connectionResult != ConnectionResult.Connected)
		{
			return connectionResult == ConnectionResult.ExistsAndAvailable;
		}
		return true;
	}

	public static bool ExecuteCommand(string sql, IDbConnection connection, string odpNetAssemblyPath)
	{
		try
		{
			Assembly odpNetAssembly = GetOdpNetAssembly(odpNetAssemblyPath);
			if ((object)odpNetAssembly == null)
			{
				UtilsLoggerWrapper.Error("Can't execute any SQL-command! ODP.NET assembly was not found!");
				return false;
			}
			Type type = odpNetAssembly.GetType("Oracle.DataAccess.Client.OracleCommand");
			UtilsLoggerWrapper.Info("Creating command for SQL \"" + sql + "\" using specified connection");
			UtilsLoggerWrapper.Info("Before OracleCommand.ctor() using sql = " + sql);
			IDbCommand dbCommand = (IDbCommand)Activator.CreateInstance(type, BindingFlags.Instance | BindingFlags.Public, null, new object[2] { sql, connection }, null);
			UtilsLoggerWrapper.Info("After OracleCommand.ctor()");
			UtilsLoggerWrapper.Info("Above command was created");
			UtilsLoggerWrapper.Info("Executing above command");
			UtilsLoggerWrapper.Info("Before OracleCommand.ExecuteNonQuery()");
			int num = dbCommand.ExecuteNonQuery();
			UtilsLoggerWrapper.Info("After OracleCommand.ExecuteNonQuery()");
			UtilsLoggerWrapper.Info("Above command was successfully executed. Executing result: \"" + num + "\".");
			return true;
		}
		catch (Exception arg)
		{
			UtilsLoggerWrapper.Error($"Can't execute specified SQL-command \"{sql}\"! Exception occurred: \"{arg}\".");
			return false;
		}
	}

	private static Assembly GetOdpNetAssembly(string odpNetAssemblyFilePath)
	{
		AssemblyName assemblyName = AssemblyName.GetAssemblyName(odpNetAssemblyFilePath);
		if (!AssemblyLoadUtils.TryFindAppDomainAssemblyByName(assemblyName, out var foundAssembly))
		{
			return Assembly.Load(assemblyName);
		}
		return foundAssembly;
	}

	private static bool TryGetOdpNetAssembly(bool shouldSearchReferencesRecursively, out Assembly odpNetAssembly)
	{
		if (AssemblyLoadUtils.TryFindAppDomainAssemblyByShortName("Oracle.DataAccess", out odpNetAssembly) && (object)odpNetAssembly != null)
		{
			return true;
		}
		if (AssemblyLoadUtils.TryLoadAssemblyFromReferences("Oracle.DataAccess", out odpNetAssembly, shouldSearchReferencesRecursively) && (object)odpNetAssembly != null)
		{
			return true;
		}
		if (AssemblyLoadUtils.TryLoadAssemblyFromProcessModules("Oracle.DataAccess", out odpNetAssembly) && (object)odpNetAssembly != null)
		{
			return true;
		}
		IEnumerable<OracleClientInstallation> allOracleClientInstallations = OracleScanner.GetAllOracleClientInstallations();
		string shortConnectionString = GetShortConnectionString(string.Empty, string.Empty, string.Empty);
		foreach (OracleClientInstallation item in allOracleClientInstallations)
		{
			if (!item.HasRelatedOdpNetInstallation)
			{
				continue;
			}
			IList<OdpNetInstallation> relatedOdpNetInstallations = item.RelatedOdpNetInstallations;
			foreach (OdpNetInstallation item2 in relatedOdpNetInstallations)
			{
				string odpNetLibraryPath = item2.OdpNetLibraryPath;
				if (CheckConnectionUsingProcess(shortConnectionString, odpNetLibraryPath) != ConnectionResult.Failed)
				{
					AssemblyName assemblyName = AssemblyName.GetAssemblyName(odpNetLibraryPath);
					odpNetAssembly = Assembly.Load(assemblyName);
					return true;
				}
			}
		}
		return (object)odpNetAssembly != null;
	}

	public static ConnectionResult CheckOciConnectionUsingAppDomain(string connectionString, string ociLibraryPath, bool traceProcessModules = false)
	{
		UtilsLoggerWrapper.Info("Checking connection using temporary application domain...");
		AppDomainOciConnectionChecker appDomainOciConnectionChecker = new AppDomainOciConnectionChecker();
		if (traceProcessModules)
		{
			UtilsLoggerWrapper.Info("Process modules before:");
			UtilsLoggerWrapper.Info(EnvironmentScanner.CallingProcessDescription);
		}
		ProcessUtils.LoadModuleToCurrentProcess(ociLibraryPath);
		if (traceProcessModules)
		{
			UtilsLoggerWrapper.Info("Process modules after loading oci.dll:");
			UtilsLoggerWrapper.Info(EnvironmentScanner.CallingProcessDescription);
		}
		string text = (FileLogger.IsInitialized ? FileLogger.LogFilePath : null);
		ConnectionResult result = appDomainOciConnectionChecker.Execute(new string[3] { connectionString, ociLibraryPath, text }, "CheckOciConnectionDomain");
		if (traceProcessModules)
		{
			UtilsLoggerWrapper.Info("Process modules after connection checking:");
			UtilsLoggerWrapper.Info(EnvironmentScanner.CallingProcessDescription);
		}
		UtilsLoggerWrapper.Info("Unloading client unmanaged libraries...");
		string directoryName = Path.GetDirectoryName(ociLibraryPath);
		UnloadClientUnmanagedLibraries(directoryName);
		if (traceProcessModules)
		{
			UtilsLoggerWrapper.Info("Process modules after unloading client unmanaged libraries:");
			UtilsLoggerWrapper.Info(EnvironmentScanner.CallingProcessDescription);
		}
		return result;
	}

	public static ConnectionResult CheckOciConnectionUsingProcess(string connectionString, string ociLibraryPath)
	{
		string logFilePath = FileLogger.LogFilePath;
		bool flag = FileLogger.IsInitialized && !string.IsNullOrEmpty(logFilePath);
		string tempFileName = Path.GetTempFileName();
		string directoryName = Path.GetDirectoryName(tempFileName);
		string fileName = Path.GetFileName(tempFileName);
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
		AppDomain currentDomain = AppDomain.CurrentDomain;
		AssemblyBuilder assemblyBuilder = currentDomain.DefineDynamicAssembly(new AssemblyName
		{
			Name = fileNameWithoutExtension
		}, (AssemblyBuilderAccess)2, directoryName);
		ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(fileNameWithoutExtension, fileName);
		TypeBuilder typeBuilder = moduleBuilder.DefineType("Program", TypeAttributes.Public);
		MethodBuilder methodBuilder = typeBuilder.DefineMethod("Main", MethodAttributes.Public | MethodAttributes.Static, null, null);
		ILGenerator iLGenerator = methodBuilder.GetILGenerator();
		if (flag)
		{
			string text = typeof(FileLogger).Namespace;
			string text2 = typeof(UtilsLoggerWrapper).Namespace;
			if (string.CompareOrdinal(text, text2) == 0)
			{
				iLGenerator.UsingNamespace(text);
			}
			else
			{
				iLGenerator.UsingNamespace(text);
				iLGenerator.UsingNamespace(text2);
			}
		}
		string usingNamespace = typeof(ConnectionUtils).Namespace;
		iLGenerator.UsingNamespace(usingNamespace);
		if (flag)
		{
			iLGenerator.Emit(OpCodes.Ldstr, logFilePath);
			iLGenerator.Emit(OpCodes.Ldc_I4, 10485760L);
			iLGenerator.Emit(OpCodes.Conv_I8);
			iLGenerator.Emit(OpCodes.Ldc_I4, Convert.ToInt32(value: true));
			iLGenerator.Emit(OpCodes.Call, typeof(FileLogger).GetMethod("Initialize", BindingFlags.Static | BindingFlags.Public, null, new Type[3]
			{
				typeof(string),
				typeof(long),
				typeof(bool)
			}, null));
			ConstructorInfo constructor = typeof(Action<string>).GetConstructor(new Type[2]
			{
				typeof(object),
				typeof(IntPtr)
			});
			iLGenerator.Emit(OpCodes.Nop);
			iLGenerator.Emit(OpCodes.Ldnull);
			MethodInfo method = typeof(FileLogger).GetMethod("Info", BindingFlags.Static | BindingFlags.Public, null, new Type[1] { typeof(string) }, null);
			iLGenerator.Emit(OpCodes.Ldftn, method);
			iLGenerator.Emit(OpCodes.Newobj, constructor);
			iLGenerator.Emit(OpCodes.Ldnull);
			MethodInfo method2 = typeof(FileLogger).GetMethod("Warning", BindingFlags.Static | BindingFlags.Public, null, new Type[1] { typeof(string) }, null);
			iLGenerator.Emit(OpCodes.Ldftn, method2);
			iLGenerator.Emit(OpCodes.Newobj, constructor);
			iLGenerator.Emit(OpCodes.Ldnull);
			MethodInfo method3 = typeof(FileLogger).GetMethod("Error", BindingFlags.Static | BindingFlags.Public, null, new Type[1] { typeof(string) }, null);
			iLGenerator.Emit(OpCodes.Ldftn, method3);
			iLGenerator.Emit(OpCodes.Newobj, constructor);
			iLGenerator.Emit(OpCodes.Call, typeof(UtilsLoggerWrapper).GetMethod("SetLoggerProvider", BindingFlags.Static | BindingFlags.Public, null, new Type[3]
			{
				typeof(Action<string>),
				typeof(Action<string>),
				typeof(Action<string>)
			}, null));
		}
		iLGenerator.BeginExceptionBlock();
		iLGenerator.Emit(OpCodes.Ldstr, connectionString);
		iLGenerator.Emit(OpCodes.Ldstr, ociLibraryPath);
		MethodInfo method4 = typeof(ConnectionUtils).GetMethod("CheckOciConnection", BindingFlags.Static | BindingFlags.Public, null, new Type[2]
		{
			typeof(string),
			typeof(string)
		}, null);
		iLGenerator.Emit(OpCodes.Call, method4);
		iLGenerator.Emit(OpCodes.Call, typeof(Environment).GetMethod("Exit", BindingFlags.Static | BindingFlags.Public, null, new Type[1] { typeof(int) }, null));
		iLGenerator.BeginCatchBlock(typeof(Exception));
		iLGenerator.Emit(OpCodes.Pop);
		iLGenerator.Emit(OpCodes.Ldc_I4, 0);
		iLGenerator.Emit(OpCodes.Call, typeof(Environment).GetMethod("Exit", BindingFlags.Static | BindingFlags.Public, null, new Type[1] { typeof(int) }, null));
		iLGenerator.EndExceptionBlock();
		iLGenerator.Emit(OpCodes.Ret);
		typeBuilder.CreateType();
		assemblyBuilder.SetEntryPoint(methodBuilder.GetBaseDefinition(), PEFileKinds.ConsoleApplication);
		AppPlatformBits systemEnvironmentBits = EnvironmentScanner.SystemEnvironmentBits;
		PePlatformType pePlatformType = PeHeaderScanner.GetPePlatformType(ociLibraryPath);
		if (systemEnvironmentBits == AppPlatformBits.X64 && pePlatformType == PePlatformType.x86)
		{
			assemblyBuilder.Save(fileName, PortableExecutableKinds.Required32Bit, ImageFileMachine.I386);
		}
		else
		{
			assemblyBuilder.Save(fileName);
		}
		string baseDirectory = currentDomain.BaseDirectory;
		string fileName2 = Path.GetFileName(typeof(CommonConstants).Assembly.Location);
		string fileName3 = Path.GetFileName(typeof(OracleConstants).Assembly.Location);
		string text3 = Path.Combine(directoryName, fileName2);
		string text4 = Path.Combine(directoryName, fileName3);
		File.Copy(Path.Combine(baseDirectory, fileName2), text3, overwrite: true);
		File.Copy(Path.Combine(baseDirectory, fileName3), text4, overwrite: true);
		File.Move(tempFileName, tempFileName = Path.ChangeExtension(tempFileName, "exe"));
		ProcessStartInfo processStartInfo = new ProcessStartInfo(tempFileName);
		processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
		ProcessStartInfo startInfo = processStartInfo;
		ConnectionResult exitCode;
		using (Process process = Process.Start(startInfo))
		{
			process.WaitForExit();
			exitCode = (ConnectionResult)process.ExitCode;
		}
		File.Delete(text4);
		File.Delete(text3);
		int num = 0;
		bool flag2 = false;
		Exception ex = null;
		while (!flag2 && num < 5)
		{
			try
			{
				Thread.Sleep(200);
				num++;
				File.Delete(tempFileName);
				flag2 = true;
			}
			catch (Exception ex2)
			{
				ex = ex2;
			}
		}
		if (!flag2)
		{
			throw ex ?? new IOException("Can't remove file " + tempFileName);
		}
		return exitCode;
	}

	public static ConnectionResult CheckOciConnection(string connectionString, string ociLibraryPath)
	{
		if (string.IsNullOrEmpty(connectionString))
		{
			UtilsLoggerWrapper.Error("Connection string is not specified, creating OracleConnection was aborted!");
			return ConnectionResult.NotExistsOrUnavailable;
		}
		string environmentVariable = Environment.GetEnvironmentVariable("PATH");
		UtilsLoggerWrapper.Info("Current PATH environment variable value: " + environmentVariable);
		string directoryName = Path.GetDirectoryName(ociLibraryPath);
		UtilsLoggerWrapper.Info("OCI library directory: " + directoryName);
		string text = directoryName + ';' + environmentVariable;
		UtilsLoggerWrapper.Info("Updating PATH environment variable to value: " + text);
		Environment.SetEnvironmentVariable("PATH", text);
		UtilsLoggerWrapper.Info("PATH environment variable value was successfully updated");
		try
		{
			if (TryGetOciProviderAssembly(shouldSearchReferencesRecursively: false, out var ociProviderAssembly) && (object)ociProviderAssembly != null)
			{
				try
				{
					Type type = ociProviderAssembly.GetType("System.Data.OracleClient.OracleConnection");
					UtilsLoggerWrapper.Info("Before OracleConnection.ctor() using connection string = " + SecureConnectionString(connectionString));
					IDbConnection dbConnection = (IDbConnection)Activator.CreateInstance(type, BindingFlags.Instance | BindingFlags.Public, null, new object[1] { connectionString }, null);
					UtilsLoggerWrapper.Info("After OracleConnection.ctor()");
					UtilsLoggerWrapper.Info("Before OracleConnection.Open()");
					dbConnection.Open();
					UtilsLoggerWrapper.Info("After OracleConnection.Open()");
					UtilsLoggerWrapper.Info("Connection was successfully opened");
					UtilsLoggerWrapper.Info("Closing connection...");
					UtilsLoggerWrapper.Info("Before OracleConnection.Close()");
					dbConnection.Close();
					UtilsLoggerWrapper.Info("After OracleConnection.Close()");
					UtilsLoggerWrapper.Info("Before OracleConnection.Dispose()");
					dbConnection.Dispose();
					UtilsLoggerWrapper.Info("After OracleConnection.Dispose()");
					return ConnectionResult.Connected;
				}
				catch (Exception ex)
				{
					UtilsLoggerWrapper.Error("Can't connect to Oracle database! Exception occurred: " + ex);
					UtilsLoggerWrapper.Error("Exception " + ex.Message + " was handled during the opening connection");
					Type type2 = ex.GetType();
					Type type3 = ociProviderAssembly.GetType("System.Data.OracleClient.OracleException");
					if ((object)type2 == type3)
					{
						PropertyInfo property = type3.GetProperty("Number", BindingFlags.Instance | BindingFlags.Public);
						if ((object)property != null)
						{
							int num = (int)property.GetValue(ex, null);
							int num2 = num;
							if (num2 == 1017)
							{
								UtilsLoggerWrapper.Error("Specified USERNAME or PASSWORD is WRONG, specified database is available, but connection wasn't opened!");
								return ConnectionResult.ExistsAndAvailable;
							}
							UtilsLoggerWrapper.Error(ex.ToString());
							return ConnectionResult.NotExistsOrUnavailable;
						}
					}
					UtilsLoggerWrapper.Error(ex.ToString());
					return ConnectionResult.Failed;
				}
			}
			UtilsLoggerWrapper.Error("Can't find assembly System.Data.OracleClient");
			return ConnectionResult.Failed;
		}
		finally
		{
			UtilsLoggerWrapper.Info("Resetting PATH environment variable to its real value: " + environmentVariable);
			Environment.SetEnvironmentVariable("PATH", environmentVariable);
			UtilsLoggerWrapper.Info("PATH environment variable value was reset");
		}
	}

	public static bool IsOciConnectionPossible(string connectionString, string ociLibraryPath)
	{
		ConnectionResult connectionResult = CheckOciConnection(connectionString, ociLibraryPath);
		if (connectionResult != ConnectionResult.Connected)
		{
			return connectionResult == ConnectionResult.ExistsAndAvailable;
		}
		return true;
	}

	private static bool TryGetOciProviderAssembly(bool shouldSearchReferencesRecursively, out Assembly ociProviderAssembly)
	{
		if (AssemblyLoadUtils.TryFindAppDomainAssemblyByShortName("System.Data.OracleClient", out ociProviderAssembly) && (object)ociProviderAssembly != null)
		{
			return true;
		}
		if (AssemblyLoadUtils.TryLoadAssemblyFromReferences("System.Data.OracleClient", out ociProviderAssembly, shouldSearchReferencesRecursively) && (object)ociProviderAssembly != null)
		{
			return true;
		}
		if (AssemblyLoadUtils.TryLoadAssemblyFromProcessModules("System.Data.OracleClient", out ociProviderAssembly) && (object)ociProviderAssembly != null)
		{
			return true;
		}
		if (AssemblyLoadUtils.TryLoadAssemblyFromGac("System.Data.OracleClient", out ociProviderAssembly) && (object)ociProviderAssembly != null)
		{
			return true;
		}
		return (object)ociProviderAssembly != null;
	}
}
