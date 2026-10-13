using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace CommonUtils.ReportUtils;

public static class TraceUtils
{
	public const string CALLER_DESCRIPTION_FORMAT_TEMPLATE = "{0}.{1}";

	public static string GetCallerDescription(Type calledType)
	{
		StackTrace stackTrace = new StackTrace();
		StackFrame[] frames = stackTrace.GetFrames();
		if (frames == null)
		{
			return string.Empty;
		}
		bool flag = false;
		for (int i = 0; i < frames.Length; i++)
		{
			MethodBase method = frames[i].GetMethod();
			Type reflectedType = method.ReflectedType;
			bool flag2 = (object)reflectedType == calledType || (reflectedType.IsNested && (object)reflectedType.DeclaringType == calledType);
			if (flag && !flag2)
			{
				return $"{reflectedType.Name}.{method.Name}";
			}
			flag = flag2;
		}
		return string.Empty;
	}

	public static string GetCallerDescription(int skipCallersCount)
	{
		StackTrace stackTrace = new StackTrace(skipCallersCount);
		StackFrame[] frames = stackTrace.GetFrames();
		if (frames == null)
		{
			return string.Empty;
		}
		StackFrame stackFrame = frames.FirstOrDefault();
		if (stackFrame == null)
		{
			return string.Empty;
		}
		MethodBase method = stackFrame.GetMethod();
		Type reflectedType = method.ReflectedType;
		return $"{reflectedType.Name}.{method.Name}";
	}
}
