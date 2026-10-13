using System.Collections.Generic;
using System.Linq;

namespace InflexCore.DataImplementation.Accessors;

public abstract class InflexUserSettingsAccessor : AppManager.InflexAccessorBase<InflexUserSettings, InflexUserSettingsAccessor>
{
	public IList<InflexUserSettings> SelectAllAsList()
	{
		return SelectAll().ToList();
	}
}
