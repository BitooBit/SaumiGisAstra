using System.ComponentModel;

namespace WinFormsExplorerWindow.Actions;

public abstract class ExplorerWindowAsyncToolAction : ExplorerWindowToolAction
{
	private BackgroundWorker _Worker = new BackgroundWorker();

	public new bool InExecute
	{
		get
		{
			BackgroundWorker worker = _Worker;
			if (base.InExecute)
			{
				return worker?.IsBusy ?? false;
			}
			return false;
		}
	}

	protected ExplorerWindow ExplorerWindow => base.DirectContext.ExplorerWindow;

	protected ExplorerWindowAsyncToolAction()
	{
		_Worker.DoWork += WorkerDoWork;
		_Worker.RunWorkerCompleted += WorkerRunCompleted;
	}

	private void WorkerRunCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
		ExplorerWindow.ShowSplashPanel(splashPanelVisibility: false);
	}

	private void WorkerDoWork(object sender, DoWorkEventArgs e)
	{
		ProtectedDirectExecuteAsync();
	}

	protected sealed override void ProtectedDirectExecute()
	{
		ExplorerWindow.ShowSplashPanel(splashPanelVisibility: true, "Пожалуйста, подождите...", canStop: false);
		_Worker.RunWorkerAsync();
	}

	protected abstract void ProtectedDirectExecuteAsync();

	protected override void OnBeforeContextDispose()
	{
		BackgroundWorker worker = _Worker;
		if (worker != null)
		{
			worker.DoWork -= WorkerDoWork;
			worker.RunWorkerCompleted -= WorkerRunCompleted;
			worker.Dispose();
		}
		_Worker = null;
		base.OnBeforeContextDispose();
	}
}
