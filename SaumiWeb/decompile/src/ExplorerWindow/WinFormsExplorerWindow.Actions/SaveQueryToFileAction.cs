using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CommonUtils.IOUtils;
using ExplorerWindowBase.Action;
using InflexCore.Action.Attributes;
using InflexCore.Native;
using InflexData.RestrictionQuery;

namespace WinFormsExplorerWindow.Actions;

[ToolActionType(MultiActionType = typeof(FileAction), GroupIndex = 100, Index = 300, GlyphUri = "SaveToFile")]
[AutoInstanceAction]
public class SaveQueryToFileAction : ExplorerWindowToolAction, IAbstractSaveAction
{
	public override string Caption => "Сохранить запрос в файл...";

	protected override bool? DirectIsAllowed()
	{
		return base.DirectContext.DataView.DataQuery is RestrictionDataQuery restrictionDataQuery && restrictionDataQuery.Restriction is RestrictByQuery && !(restrictionDataQuery.Restriction as RestrictByQuery).IsFullQuery;
	}

	protected override void ProtectedDirectExecute()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected O, but got Unknown
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Invalid comparison between Unknown and I4
		RestrictionDataQuery restrictionDataQuery = (RestrictionDataQuery)base.DirectContext.DataView.DataQuery;
		if (restrictionDataQuery.Restriction is RestrictByQuery { IsFullQuery: false, Report: var report } restrictByQuery)
		{
			SaveFileDialog val = new SaveFileDialog();
			((FileDialog)val).AddExtension = true;
			val.OverwritePrompt = true;
			((FileDialog)val).RestoreDirectory = true;
			((FileDialog)val).FileName = report.Name;
			((FileDialog)val).DefaultExt = "Запросы SAUMI (*.mmp)|*.mmp";
			((FileDialog)val).Filter = "Запросы SAUMI (*.mmp)|*.mmp|Все файлы (*.*)|(*.*)";
			DialogResult val2 = ((CommonDialog)val).ShowDialog();
			if ((int)val2 == 1)
			{
				decimal iD = report.ID;
				string text = restrictByQuery.RestrictionQueryText.Trim().Replace(Environment.NewLine, "\\n");
				text = text.Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ");
				text = Regex.Replace(text, "\\s+", " ", RegexOptions.Multiline);
				string fileName = ((FileDialog)val).FileName;
				IniUtils.IniWriteValue(fileName, "MMP", "QueryID", iD.ToString());
				IniUtils.IniWriteValue(fileName, "MMP", "Restriction", text);
			}
		}
	}

	private string AddFileExtention(string fileName, string fullPath)
	{
		if (!fileName.ToUpper().Contains(".MMP"))
		{
			return fullPath += ".MMP";
		}
		return fullPath;
	}
}
