using InflexCLR.SPTypes;

namespace InflexCLR.Environment;

[SaumiExport]
public class SaumiContext
{
	private static TADDRESS _Address;

	private static TCLIENT _Client;

	private static TOBLIGATION _Obligation;

	private static TMOVEMENT _Movement;

	private static TMOVESET _MoveSet;

	private static TPROPERTY _Property;

	public static TADDRESS Address
	{
		get
		{
			return _Address;
		}
		set
		{
			_Address = value;
		}
	}

	public static TCLIENT Client
	{
		get
		{
			return _Client;
		}
		set
		{
			_Client = value;
		}
	}

	public static TOBLIGATION Obligation
	{
		get
		{
			return _Obligation;
		}
		set
		{
			_Obligation = value;
		}
	}

	public static TMOVEMENT Movement
	{
		get
		{
			return _Movement;
		}
		set
		{
			_Movement = value;
		}
	}

	public static TMOVESET MoveSet
	{
		get
		{
			return _MoveSet;
		}
		set
		{
			_MoveSet = value;
		}
	}

	public static TPROPERTY Property
	{
		get
		{
			return _Property;
		}
		set
		{
			_Property = value;
		}
	}

	public void SetAddress(TADDRESS Address)
	{
		SaumiContext.Address = Address;
	}

	public void SetClient(TCLIENT Client)
	{
		SaumiContext.Client = Client;
	}

	public void SetObligation(TOBLIGATION Obligation)
	{
		SaumiContext.Obligation = Obligation;
	}

	public void SetMovement(TMOVEMENT Movement)
	{
		SaumiContext.Movement = Movement;
	}

	public void SetMoveSet(TMOVESET MoveSet)
	{
		SaumiContext.MoveSet = MoveSet;
	}

	public void SetProperty(TPROPERTY Property)
	{
		SaumiContext.Property = Property;
	}
}
