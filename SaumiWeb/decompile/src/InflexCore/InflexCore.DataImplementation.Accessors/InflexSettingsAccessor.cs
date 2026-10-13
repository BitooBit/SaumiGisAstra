using System.Collections.Generic;
using System.Linq;

namespace InflexCore.DataImplementation.Accessors;

public abstract class InflexSettingsAccessor : AppManager.InflexAccessorBase<InflexSettings, InflexSettingsAccessor>
{
	public IList<InflexSettings> SelectAllAsList()
	{
		return SelectAll().ToList();
	}
}
