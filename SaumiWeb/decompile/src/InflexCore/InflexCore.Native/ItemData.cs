using System;

namespace InflexCore.Native;

[Serializable]
public class ItemData
{
	public int ObjID { get; set; }

	public int ObjType { get; set; }

	public int ObjSubType { get; set; }

	public string ObjTypeTxt { get; set; }

	public int Indicator { get; set; }

	public string Description { get; set; }

	public int Tag { get; set; }

	public int Tag1 { get; set; }

	public IInflexID ToInflexID()
	{
		return ToInflexID(AppManager.Instance);
	}

	public IInflexID ToInflexID(AppManager appManager)
	{
		IInflexType inflexType = appManager.GetType(ObjType, ObjSubType);
		if (inflexType == null)
		{
			inflexType = new ManualItemDataInflexType(this);
		}
		return new MockInflexID(inflexType, ObjID);
	}

	public static ItemData Parse(IInflexID inflexID)
	{
		IInflexType inflexType = inflexID.NativeType ?? inflexID.Type;
		ItemData itemData = new ItemData();
		itemData.ObjID = decimal.ToInt32(inflexID.ID);
		itemData.ObjType = (inflexType.TypeID.HasValue ? decimal.ToInt32(inflexType.TypeID.Value) : 0);
		itemData.ObjSubType = (inflexType.SubtypeID.HasValue ? decimal.ToInt32(inflexType.SubtypeID.Value) : 0);
		itemData.ObjTypeTxt = inflexType.Name;
		return itemData;
	}
}
