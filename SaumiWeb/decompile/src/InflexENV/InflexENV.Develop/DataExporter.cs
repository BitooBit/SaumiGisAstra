using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using InflexCLR.Environment;
using InflexCLR.SPTypes;

namespace InflexENV.Develop;

[SaumiExport]
public class DataExporter
{
	private string _FileName;

	private string _Directory;

	public void SetFileName(string FileName)
	{
		if (!string.IsNullOrEmpty(FileName))
		{
			_FileName = FileName;
		}
		else
		{
			_FileName = string.Empty;
		}
	}

	public void SetDirectory(string Dir)
	{
		if (!string.IsNullOrEmpty(Dir))
		{
			_Directory = Dir;
		}
		else
		{
			_Directory = string.Empty;
		}
	}

	private void Export(object data)
	{
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Expected O, but got Unknown
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Invalid comparison between Unknown and I4
		if (data == null)
		{
			return;
		}
		try
		{
			string text = _FileName;
			if (string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(_Directory))
			{
				text = DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day + "-" + DateTime.Now.Hour + "-" + DateTime.Now.Minute + "-" + DateTime.Now.Second + "-" + data.GetType().Name + ".bin";
				text = Path.Combine(_Directory, text);
			}
			if (string.IsNullOrEmpty(text))
			{
				SaveFileDialog val = new SaveFileDialog();
				((FileDialog)val).RestoreDirectory = true;
				((FileDialog)val).FileName = DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day + "-" + DateTime.Now.Hour + "-" + DateTime.Now.Minute + "-" + DateTime.Now.Second + "-" + data.GetType().Name + ".bin";
				((FileDialog)val).Filter = "Файл экспорта (*.bin)|*.bin";
				if ((int)((CommonDialog)val).ShowDialog() == 1)
				{
					text = ((FileDialog)val).FileName;
				}
			}
			if (!string.IsNullOrEmpty(text))
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				FileStream fileStream = File.Open(text, FileMode.Create);
				binaryFormatter.Serialize(fileStream, data);
				fileStream.Close();
			}
		}
		catch (Exception ex)
		{
			if (string.IsNullOrEmpty(_Directory))
			{
				MessageBox.Show(ex.Message);
			}
		}
		_FileName = string.Empty;
	}

	public void ExportADDRESS(TADDRESS data)
	{
		Export(data);
	}

	public void ExportCLIENT(TCLIENT data)
	{
		Export(data);
	}

	public void ExportLAYERPROPS(TLAYERPROPS data)
	{
		Export(data);
	}

	public void ExportMOVEITEM(TMOVEITEM data)
	{
		Export(data);
	}

	public void ExportMOVEMENT(TMOVEMENT data)
	{
		Export(data);
	}

	public void ExportMOVEPERIOD(TMOVEPERIOD data)
	{
		Export(data);
	}

	public void ExportMOVESET(TMOVESET data)
	{
		Export(data);
	}

	public void ExportOBLIGATION(TOBLIGATION data)
	{
		Export(data);
	}

	public void ExportOBLIGFACTOR(TOBLIGFACTOR data)
	{
		Export(data);
	}

	public void ExportPROPERTY(TPROPERTY data)
	{
		Export(data);
	}

	public void ExportUSINGSQUARE(TUSINGSQUARE data)
	{
		Export(data);
	}
}
