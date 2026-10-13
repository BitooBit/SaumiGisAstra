using System.Collections;
using System.Collections.Generic;

namespace InflexCore.MetaData;

public abstract class InflexPropertyCollection : IEnumerable<IInflexProperty>, IEnumerable
{
	public virtual IInflexProperty this[string name]
	{
		get
		{
			name = name.ToUpper();
			using (IEnumerator<IInflexProperty> enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					IInflexProperty current = enumerator.Current;
					if (current.Name.ToUpper() == name)
					{
						return current;
					}
				}
			}
			return null;
		}
	}

	protected IInflexType OwnerType { get; set; }

	public InflexPropertyCollection(IInflexType ownerType)
	{
		OwnerType = ownerType;
	}

	public abstract IEnumerator<IInflexProperty> GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
