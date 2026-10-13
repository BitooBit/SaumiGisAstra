using System;
using InflexCore;
using InflexCore.Native;
using InflexGisComponent;

namespace WinFormsExplorerWindow.GisComponent;

public class GisContainer : IGisComponentContainer, IDisposable
{
	private IGisComponentWindow _Window;

	private AppManager _AppManager;

	private RestrictByQuery _Restriction;

	private IActionListAccessor _ActionsAccessor;

	private readonly object _DisposeSync = new object();

	public IGisComponentWindow Window => _Window;

	public AppManager AppManager => _AppManager;

	public RestrictByQuery TempLayerRestriction => _Restriction;

	public IActionListAccessor ActionsAccessor => _ActionsAccessor;

	public GisContainer(IGisComponentWindow gisComponentHostwindow, IActionListAccessor actionsAccessor, AppManager appManager, RestrictByQuery restriction)
	{
		_ActionsAccessor = actionsAccessor;
		_Window = gisComponentHostwindow;
		_AppManager = appManager;
		_Restriction = restriction;
	}

	public void Dispose()
	{
		lock (_DisposeSync)
		{
			_ActionsAccessor = null;
			_AppManager = null;
			_Restriction = null;
			_Window = null;
		}
	}
}
