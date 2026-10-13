using InflexCore.DataImplementation.Reflection;

namespace InflexCore.DataImplementation.Accessors;

public abstract class InflexLogMessagesAccessor : AppManager.InflexAccessorBase<InflexLogMessages, InflexLogMessagesAccessor>
{
	[InflexSqlQuery("insert into <T> (text_message, proc_name, msg_kind) values (:messageText, :processName, :messageKind)")]
	public abstract void Log(string messageText, string processName, decimal messageKind);
}
