using System;
using System.IO;
using System.Windows.Forms;
using Gis.GuiWpfShell;
using InflexCore.Action.Attributes;
using InflexCore.Native;
using InflexData;
using InflexData.RestrictionQuery;
using InflexGisComponent;
using WinFormsExplorerWindow.Config;
using WinFormsExplorerWindow.GisComponent;

namespace WinFormsExplorerWindow.Actions;

[AutoInstanceAction]
[ToolActionType(MultiActionType = typeof(ViewAction), GroupIndex = 400, Index = 200)]
public class ShowGisAction : ExplorerWindowToolAction
{
	private GisWrapper _GisWrapper;

	private bool _SaveWorkspaceID;

	private GisContainer _GisContainer;

	public override bool IsHiddenWhenDisabled => true;

	public override string Caption => "Карта";

	private string GisConfigFile => Path.Combine(Path.Combine(ConfigLoader.GetGisBinPath(), "settings"), "GIS_CONFIG.xml");

	public int WorkspaceID { get; set; }

	protected override bool? DirectIsAllowed()
	{
		return GisLoader.IsGisAllowed(base.Context.Type) && base.DirectContext.ExplorerWindow != null && base.DirectContext.ExplorerWindow.GisWindow != null && !base.DirectContext.DataView.InExecute();
	}

	protected override void ProtectedDirectExecute()
	{
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Expected O, but got Unknown
		IGisComponentWindow gisWindow = base.DirectContext.ExplorerWindow.GisWindow;
		if (gisWindow.IsVisible)
		{
			if (gisWindow.IsMinimized)
			{
				gisWindow.Normalize();
			}
			gisWindow.Activate();
			return;
		}
		RestrictByQuery restriction = ((!(base.DirectContext.DataView.DataQuery is RestrictionDataQuery restrictionDataQuery)) ? null : (restrictionDataQuery.Restriction as RestrictByQuery));
		_GisContainer = new GisContainer(gisWindow, base.DirectContext.ExplorerWindow.GridActionsAccessor, base.DirectContext.AppManager, restriction);
		_GisWrapper = new GisWrapper((IGisComponentContainer)(object)_GisContainer);
		if (WorkspaceID == -1)
		{
			WorkspaceID = new GisIntoExplorerWindowConfig(base.DirectContext.AppManager).WorkspaceID;
			_SaveWorkspaceID = true;
		}
		if (WorkspaceID != -1)
		{
			_GisWrapper.WorkspaceID = WorkspaceID;
			_GisWrapper.UseWorkspaceID = true;
		}
		GisLoader.LoadMap(GisConfigFile, (IGisApplicationContainer)(object)_GisWrapper);
		ShowGisFullScreen();
	}

	private void DisposeGisWrapper()
	{
		GisWrapper gisWrapper = _GisWrapper;
		if (gisWrapper != null)
		{
			if (_SaveWorkspaceID)
			{
				new GisIntoExplorerWindowConfig(base.DirectContext.AppManager).WorkspaceID = gisWrapper.GetCurrentWorkspaceID();
			}
			gisWrapper.Dispose();
			_GisWrapper = null;
		}
	}

	protected override void OnBeforeContextDispose()
	{
		if (_GisWrapper != null)
		{
			DisposeGisWrapper();
		}
		base.OnBeforeContextDispose();
	}

	private void ShowGisFullScreen()
	{
		ExplorerWindow explorerWindow = base.DirectContext.ExplorerWindow;
		InflexDataView dataView = base.DirectContext.DataView;
		explorerWindow.GisWindow.Title = "Карта - " + ((Control)explorerWindow).Text + " - " + ((dataView != null && dataView.DataQuery != null) ? dataView.DataQuery.HumanyAdaptedRestrictionTextShort : string.Empty);
		explorerWindow.GisWindow.Closed += GisWindowClosed;
		explorerWindow.GisWindow.Show();
		explorerWindow.GisWindow.Activate();
		explorerWindow.GisWindow.EnableModelessKeyboardInterop();
	}

	private void GisWindowClosed(object sender, EventArgs e)
	{
		IGisComponentWindow val = (IGisComponentWindow)((sender is IGisComponentWindow) ? sender : null);
		if (val != null)
		{
			val.Closed -= GisWindowClosed;
		}
		if (_GisWrapper != null)
		{
			DisposeGisWrapper();
		}
		if (_GisContainer != null)
		{
			_GisContainer.Dispose();
			_GisContainer = null;
		}
	}
}
