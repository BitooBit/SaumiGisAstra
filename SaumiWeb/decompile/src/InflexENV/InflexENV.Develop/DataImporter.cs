using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using InflexCLR.Environment;
using InflexCLR.SPTypes;

namespace InflexENV.Develop;

[SaumiExport]
public class DataImporter
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

	private T Import<T>(int Index)
	{
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Invalid comparison between Unknown and I4
		try
		{
			string text = _FileName;
			if (string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(_Directory) && Index >= 0)
			{
				string[] files = Directory.GetFiles(_Directory, "*.bin", SearchOption.TopDirectoryOnly);
				if (Index < files.Length)
				{
					text = files[Index];
				}
			}
			if (string.IsNullOrEmpty(text))
			{
				OpenFileDialog val = new OpenFileDialog();
				((FileDialog)val).Filter = "Файлы экспорта (*.bin)|*.bin";
				((FileDialog)val).RestoreDirectory = true;
				if ((int)((CommonDialog)val).ShowDialog() == 1)
				{
					text = ((FileDialog)val).FileName;
				}
			}
			if (!string.IsNullOrEmpty(text))
			{
				FileStream fileStream = File.Open(text, FileMode.Open);
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				T result = (T)binaryFormatter.Deserialize(fileStream);
				fileStream.Close();
				return result;
			}
		}
		catch (Exception ex)
		{
			if (string.IsNullOrEmpty(_Directory))
			{
				MessageBox.Show(ex.Message);
			}
		}
		return default(T);
	}

	public TADDRESS ImportADDRESS()
	{
		return Import<TADDRESS>(-1);
	}

	public TCLIENT ImportCLIENT()
	{
		return Import<TCLIENT>(-1);
	}

	public TLAYERPROPS ImportLAYERPROPS()
	{
		return Import<TLAYERPROPS>(-1);
	}

	public TMOVEITEM ImportMOVEITEM()
	{
		return Import<TMOVEITEM>(-1);
	}

	public TMOVEMENT ImportMOVEMENT()
	{
		return Import<TMOVEMENT>(-1);
	}

	public TMOVEPERIOD ImportMOVEPERIOD()
	{
		return Import<TMOVEPERIOD>(-1);
	}

	public TMOVESET ImportMOVESET()
	{
		return Import<TMOVESET>(-1);
	}

	public TOBLIGATION ImportOBLIGATION()
	{
		return Import<TOBLIGATION>(-1);
	}

	public TOBLIGFACTOR ImportOBLIGFACTOR()
	{
		return Import<TOBLIGFACTOR>(-1);
	}

	public TPROPERTY ImportPROPERTY()
	{
		return Import<TPROPERTY>(-1);
	}

	public TUSINGSQUARE ImportUSINGSQUARE()
	{
		return Import<TUSINGSQUARE>(-1);
	}
}
