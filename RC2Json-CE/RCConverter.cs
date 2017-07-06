/*
RC2Json CE - Tool di trasformazione .RC in TB Json 
Copyright (C) 2017 Microarea s.p.a.

This program is free software: you can redistribute it and/or modify it under the 
terms of the GNU General Public License as published by the Free Software Foundation, 
either version 3 of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful, 
but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
or FITNESS FOR A PARTICULAR PURPOSE. 

See the GNU General Public License for more details.
*/

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RC2Json
{
	public class RCConverter
	{
		Dictionary<string, string> projectMacros = new Dictionary<string, string>();
		/// <summary>
		/// Analizza il file *.rc o tutti quelli di una cartella
		/// e produce un file *.hjson e tanti file *.tbjson
		/// Il file *.hjson serve per far conoscere le risorse ai file c++
		/// mentre quelli *.tbjson contengono le definizioni delle form in formato json
		/// </summary>
		/// <param name="rcFileOrFolder"></param>
		public int ProcessRC(string rcFileOrFolder)
		{

			if (Path.GetExtension(rcFileOrFolder).IsEmpty())
			{
				int ret = 0;

				/*string[] files = Directory.GetFiles(rcFileOrFolder, "*.tbjson", SearchOption.AllDirectories);
				foreach (string f in files)
				{
					try
					{
						//se sono Normal (non ho l'attributo Archive) allora l'ho generato automaticamente, lo posso cancellare
						if ((File.GetAttributes(f) | FileAttributes.Normal) == FileAttributes.Normal)
						{
							File.Delete(f);
							//se esiste, cancello anche l'header c++
							string hJson = Path.ChangeExtension(f, ".hjson");
							if (File.Exists(hJson))
								File.Delete(hJson);

							string folder = Path.GetDirectoryName(f);
							if (Directory.GetFileSystemEntries(folder).Length == 0)
								Directory.Delete(folder);
						}
					}
					catch (Exception ex)
					{
						Diagnostic.WriteLine(string.Format("Error deleting file '{0}: {1}", f, ex.Message));
						ret = -1;
					}
				}

				//cancello le sottodirectory vuote delle jsonforms
				foreach (string jsonFolder in Directory.GetDirectories(rcFileOrFolder, JsonConstants.JSON_FOLDER_NAME, SearchOption.AllDirectories))
				{
					foreach (string sub in Directory.GetDirectories(jsonFolder, "*", SearchOption.AllDirectories))
					{
						try
						{
							if (Directory.GetFileSystemEntries(sub).Length == 0)
								Directory.Delete(sub);
						}
						catch (Exception ex)
						{
							Diagnostic.WriteLine(string.Format("Error deleting folder '{0}: {1}", sub, ex.Message));
							ret = -1;
						}
					}
				}*/

				//non ho estensione: è una cartella, quindi applico ricorsivamente a tutti i file rc trovati nella cartella e nelle sottocartelle
				string[] files = Directory.GetFiles(rcFileOrFolder, "*.rc", SearchOption.AllDirectories);
				foreach (string f in files)
				{
					try
					{
						ret = Math.Min(ret, ProcessRC(f));
					}
					catch (Exception ex)
					{
						Diagnostic.WriteLine(string.Format("Error processing file '{0}: {1}", f, ex.Message));
						ret = -1;
					}
				}
				return 0;
			}
			FileInfo fi = new FileInfo(rcFileOrFolder);

			if (!fi.Exists)
			{
				throw new ApplicationException(string.Format("File '{0}' does not exist.", rcFileOrFolder));
			}
			//se path relativo lo trasformo in assoluto
			rcFileOrFolder = fi.FullName;
			//parto dal presupposto che debba esistere la coppia *.rc/*.hrc
			string hrcFile = Path.ChangeExtension(rcFileOrFolder, ".hrc");
			if (!File.Exists(hrcFile))
			{
				Diagnostic.WriteLine(string.Format("File '{0}' does not exist.", hrcFile));
				return -1;
			}

			HRCParser hrcParser = new HRCParser();
			RCParser rcParser = new RCParser(hrcParser);
			try
			{
				using (StreamReader reader = new StreamReader(hrcFile))
					hrcParser.Parse(reader.ReadToEnd());
				if (!rcParser.Parse(rcFileOrFolder))
					return -1;


				hrcParser.AssignTypes(rcParser);
			}
			catch (Exception ex)
			{
				throw new ApplicationException(string.Format("Error parsing file '{0}': {1}", hrcFile, ex.Message), ex);
			}
			//cerco la cartella che contiene il module.config
			string moduleFolder = Helper.FindModuleFolder(rcFileOrFolder);

			if (string.IsNullOrEmpty(moduleFolder))
			{
				throw new ApplicationException(string.Format("Cannot find module folder for file '{0}', module.config not found.", rcFileOrFolder));
			}
			string appFolder = Path.GetDirectoryName(moduleFolder);
			string containerFolder = Path.GetDirectoryName(appFolder);
			string jsonCategoryFolder;
			string categoryName = null;
			string typeToken = null;//modulo
			if (JsonUserDocuments.GetDocumentFolder(rcFileOrFolder, out categoryName))
			{
				jsonCategoryFolder = string.Concat(moduleFolder, "\\ModuleObjects\\", categoryName, "\\", JsonConstants.JSON_FOLDER_NAME);
				typeToken = "D.";//documento
			}
			else
			{
				categoryName = Path.GetFileNameWithoutExtension(rcFileOrFolder);
				string jsonFolder = Path.Combine(moduleFolder, JsonConstants.JSON_FOLDER_NAME);
				jsonCategoryFolder = Path.Combine(jsonFolder, categoryName);
				typeToken = "M.";//modulo
			}
			
			string jsonContext = string.Concat(
				typeToken,											//tipologia
				Path.GetFileNameWithoutExtension(appFolder),		//applicazione
				'.',
				Path.GetFileNameWithoutExtension(moduleFolder),		//modulo
				'.',
				categoryName										//categoria
				);
			//genero il file con le define (*.hjson) da includere nei cpp
			string hJsonFile = Path.ChangeExtension(hrcFile, JsonConstants.JSON_HEADER_EXT);
			
			Diagnostic.WriteLine(string.Format("Generating include file '{0}'", hJsonFile));
			//stream dell'.hjson corrispondente al'.hrc
			//questo file contiene le include degli hjson relativi ad ogni singola dialog, più le define orfane, non relative ad alcuna dialog
			using (StreamWriter hsw = new StreamWriter(hJsonFile, false, Encoding.UTF8))
			{
				hsw.Write("#pragma once\r\n");
				hsw.Write("#include\t<TbNameSolver\\TBResourcesMap.h>\r\n");

				foreach (var dlg in rcParser.Dialogs)
				{
					//creo la sottocartella corrispondente al nome del file rc (raggruppamento logico)
					if (!Directory.Exists(jsonCategoryFolder))
						Directory.CreateDirectory(jsonCategoryFolder);
					string jsonFile = Path.Combine(jsonCategoryFolder, string.Concat(dlg.Id, JsonConstants.JSON_FORM_EXT));
					Diagnostic.WriteLine(string.Format("Generating json file '{0}'", jsonFile));
                 
					//creo il file *.tbjson, uno per ogni risorsa di dialogo
					using (StreamWriter sw = new StreamWriter(jsonFile, false, Encoding.UTF8))
					{
						using (RCJsonWriter writer = new RCJsonWriter(sw))
						{
							writer.RCFile = rcFileOrFolder;
							writer.Formatting = Newtonsoft.Json.Formatting.Indented;
							dlg.WriteTo(writer);

						}
					}
					//imposto questo attributo (rimuovendo Archive) per riconoscere che il file è generato automaticamente
					//File.SetAttributes(jsonFile, FileAttributes.Normal);
				}

				foreach (var block in rcParser.Accelerators)
				{
					//creo la sottocartella corrispondente al nome del file rc (raggruppamento logico)
					if (!Directory.Exists(jsonCategoryFolder))
						Directory.CreateDirectory(jsonCategoryFolder);
					string jsonFile = Path.Combine(jsonCategoryFolder, string.Concat(block.Id, JsonConstants.JSON_FORM_EXT));
					Diagnostic.WriteLine(string.Format("Generating json file '{0}'", jsonFile));
					//creo il file *.tbjson, uno per ogni risorsa di dialogo
					using (StreamWriter sw = new StreamWriter(jsonFile, false, Encoding.UTF8))
					{
						using (JsonWriter writer = new JsonTextWriter(sw))
						{
							writer.Formatting = Newtonsoft.Json.Formatting.Indented;
							block.WriteTo(writer);

						}
					}

					//imposto questo attributo (rimuovendo Archive) per riconoscere che il file è generato automaticamente
					//File.SetAttributes(jsonFile, FileAttributes.Normal);
				}


				string owner = "";
				HrcType type = HrcType.OTHER;
                StreamWriter currentWriter = hsw;
                StreamWriter singleStreamWriter = null;
				foreach (var include in hrcParser.HrcStructure)
				{
					if (IsStaticControl(include))
						continue;

					//quando cambia l'owner, cambia lo stream su cui scrivo
					if (owner != include.Owner)
					{
						owner = include.Owner;
						if (singleStreamWriter != null)
						{
							singleStreamWriter.Close();
							singleStreamWriter = null;
							currentWriter = hsw;
						}
						if (include.InSpecificFile)
						{
							string hSingleJsonFile = Path.Combine(jsonCategoryFolder, string.Concat(owner, JsonConstants.JSON_HEADER_EXT));
							//scrivo l'include del singolo filettino json relativo alla singola dialog
							hsw.Write("#include\t<");
							hsw.Write(Helper.GetRelativePath(hSingleJsonFile, Helper.FindApplicationFolder(hSingleJsonFile)));
							hsw.Write(">\r\n");
							singleStreamWriter = new StreamWriter(hSingleJsonFile, false, Encoding.UTF8);
							singleStreamWriter.Write("#pragma once\r\n");
							singleStreamWriter.Write("#include\t<TbNameSolver\\TBResourcesMap.h>\r\n");
							currentWriter = singleStreamWriter;
						}
						else
						{
							//se non ho una risorsa specifica, scrivo nell'hjson globale
							currentWriter = hsw;
						}
					}
					if (!include.InSpecificFile)
					{
						if (type != include.Type)
						{
							type = include.Type;
							currentWriter.Write("\r\n//-----------------------------------------------------------------------------------------------------");
							currentWriter.Write("\r\n//Type:\t");
                            currentWriter.Write(type.ToString());
							currentWriter.Write("\r\n//-----------------------------------------------------------------------------------------------------\r\n");
						}
					}
					//if (include.Type != HrcType.OTHER)
					//	continue; //inclusi dal singolo hjson di dialogo
					if (include.Type == HrcType.IDC_CURSOR)
					{
                        currentWriter.Write("#define\t");
                        currentWriter.Write(include.Name);
                        currentWriter.Write("\t\t");
                        currentWriter.Write(include.Value);
                        currentWriter.Write("\r\n");
					}
					else if (include.Type == HrcType.IDC)
					{
                        currentWriter.Write("#define\t");
                        currentWriter.Write(include.Name);
                        currentWriter.Write("\t\t\tGET_IDC(");
                        currentWriter.Write(include.Name);
                        currentWriter.Write(")\r\n");
					}
					else if (include.Type == HrcType.IDD)
					{
                        currentWriter.Write("#define\t");
                        currentWriter.Write(include.Name);
                        currentWriter.Write("\t\t\tGET_IDD(");
						currentWriter.Write(include.Name);
						currentWriter.Write(", ");
						currentWriter.Write(jsonContext);
                        currentWriter.Write(")\r\n");
					}
					else if (include.Type == HrcType.IDR_ACCELERATOR)
					{
                        currentWriter.Write("#define\t");
                        currentWriter.Write(include.Name);
                        currentWriter.Write("\t\t\tGET_IDR(");
						currentWriter.Write(include.Name);
						currentWriter.Write(", ");
						currentWriter.Write(jsonContext);
                        currentWriter.Write(")\r\n");
					}
					else if (include.Type == HrcType.ID)
					{
                        currentWriter.Write("#define\t");
                        currentWriter.Write(include.Name);
                        currentWriter.Write("\t\t\tGET_ID(");
                        currentWriter.Write(include.Name);
                        currentWriter.Write(")\r\n");
					}
					else
					{
						if (include.Type == HrcType.OTHER)
							Diagnostic.WriteLine(string.Concat("WARNING: unused macro ", include.Name, " in file ", hrcFile));
                        currentWriter.Write("#define\t");
                        currentWriter.Write(include.Name);
                        currentWriter.Write("\t\t");
                        currentWriter.Write(include.Value);
                        currentWriter.Write("\r\n");
					}
				}
				if (singleStreamWriter != null)
				{
					singleStreamWriter.Close();
					singleStreamWriter = null;
					currentWriter = hsw;
				}

				if (rcParser.StringTable.Count > 0)
				{
					currentWriter.Write("\r\n//-----------------------------------------------------------------------------------------------------");
					currentWriter.Write("\r\n//Strings:");
					currentWriter.Write("\r\n//-----------------------------------------------------------------------------------------------------");
					currentWriter.Write("\r\n");

					//recupero la macro che idendifica il progetto di appartenenza leggendo il beginh.dex
					string macro = GetProjectMacroId(Path.GetDirectoryName(rcFileOrFolder));
					//condiziono la compilazione di questo codice all'appartenenza al progetto
					if (!string.IsNullOrEmpty(macro))
					{
						currentWriter.Write("#ifdef ");
						currentWriter.Write(macro);
						currentWriter.Write("\r\n");
					}
					foreach (var s in rcParser.StringTable)
					{
						//static UINT IDS_A = GET_IDS(ID_A, _TB("AA"), _T(__FILE__));

						currentWriter.Write("static UINT IDS_");
						currentWriter.Write(s.Key);
						currentWriter.Write("\t\t=GET_IDS(");
						currentWriter.Write(s.Key);
						currentWriter.Write(", _TB_STRING(\"");
						currentWriter.Write(s.Value);
						//hsw.Write("\"), _T(__FILE__));\r\n");
						currentWriter.Write("\"), L\"");
						currentWriter.Write(hJsonFile.Substring(containerFolder.Length).Replace("\\", "\\\\"));//metto il path relativo a partire dalla cartella di applicazione
						currentWriter.Write("\");\r\n");
					}
					if (!string.IsNullOrEmpty(macro))
					{
						currentWriter.Write("#endif\r\n");
					}
					currentWriter.Write("\r\n");
				}
			}
#if DEBUG
			SaveStylesToFile(rcParser);
			SaveStringTableToFile(rcParser);
#endif
			return 0;
		}

		private static bool IsStaticControl(HRCStructure include)
		{
			return Regex.IsMatch(include.Name, "(IDC_STATIC(_([12]?\\d))?$)|(IDC_STATIC_AREA(_[23])?$)");
		}

		private string GetProjectMacroId(string folder)
		{
			string macro;
			if (projectMacros.TryGetValue(folder, out macro))
				return macro;
			string file = Path.Combine(folder, "beginh.dex");
			if (!File.Exists(file))
			{
				macro = "";
			}
			else
			{
				string line;
				using (StreamReader sr = new StreamReader(file))
				{
					while (null != (line = sr.ReadLine()))
					{
						Match m = Regex.Match(line, @"#\s*ifdef\s+(?<macro>\w+)");
						if (m.Success)
						{
							string v = m.Groups["macro"].Value;

							if (v.Equals("_TbMultiDll"))
							{
								while (null != (line = sr.ReadLine()))
								{
									m = Regex.Match(line, @"#\s*include\s+[""<](?<path>[\w\\\.]+)["">]");
									if (m.Success)
									{
										string otherFile = m.Groups["path"].Value;
										string tmpFolder = folder;
										do
										{
											string otherFilePath = Path.Combine(tmpFolder, otherFile);
											if (File.Exists(otherFilePath))
												return GetProjectMacroId(Path.GetDirectoryName(otherFilePath));
											tmpFolder = Path.GetDirectoryName(tmpFolder);
										}
										while (!string.IsNullOrEmpty(tmpFolder));
									}
								}
							}

							if (!v.Equals("_AFXDLL"))
							{
								macro = v;
								break;
							}
						}
					}
				}
			}
			projectMacros[folder] = macro;
			return macro;
		}

		/// <summary>
		/// Controlla il file *.rc o tutti quelli di una cartella
		/// per verificare la corrispondenza di nomi fra rc e hrc
		/// </summary>
		/// <param name="rcFileOrFolder"></param>
		public int CheckRC(string rcFileOrFolder)
		{

			if (Path.GetExtension(rcFileOrFolder).IsEmpty())
			{
				//non ho estensione: è una cartella, quindi applico ricorsivamente a tutti i file rc trovati nella cartella e nelle sottocartelle
				string[] files = Directory.GetFiles(rcFileOrFolder, "*.rc", SearchOption.AllDirectories);
				int ret = 0;
				foreach (string f in files)
				{
					try
					{
						ret = Math.Min(ret, CheckRC(f));
					}
					catch (Exception ex)
					{
						Diagnostic.WriteLine(string.Format("Error processing file '{0}: {1}", f, ex.Message));
						ret = -1;
					}
				}
				return ret;
			}
			FileInfo fi = new FileInfo(rcFileOrFolder);

			if (!fi.Exists)
			{
				throw new ApplicationException(string.Format("File '{0}' does not exist.", rcFileOrFolder));
			}
			//se path relativo lo trasformo in assoluto
			rcFileOrFolder = fi.FullName;

			HRCParser hrcParser = new HRCParser();
			RCParser rcParser = new RCParser(hrcParser);
			try
			{
				if (!rcParser.Check(rcFileOrFolder))
					return -1;
			}
			catch (Exception ex)
			{
				throw new ApplicationException(string.Format("Error parsing file '{0}': {1}", rcFileOrFolder, ex.Message), ex);
			}



			return 0;
		}
		/// <summary>
		/// salva tutti gli stili trovati su file (va in append) per scopi statistici
		/// </summary>
		/// <param name="rcParser"></param>
		private void SaveStylesToFile(RCParser rcParser)
		{
			List<string> styles = new List<string>();
			string fi = AppDomain.CurrentDomain.BaseDirectory + "\\styles.txt";
			if (File.Exists(fi))
			{
				string line;
				using (StreamReader sr = new StreamReader(fi))
				{
					while (null != (line = sr.ReadLine()))
						styles.Add(line);

				}
			}
			foreach (string s in rcParser.Styles)
				if (!styles.Contains(s))
					styles.Add(s);
			styles.Sort();
			using (StreamWriter sw = new StreamWriter(fi))
			{
				foreach (string s in styles)
					sw.WriteLine(s);
			}
		}
		private void SaveStringTableToFile(RCParser rcParser)
		{
			string fi = AppDomain.CurrentDomain.BaseDirectory + "\\stringtable.txt";

			using (StreamWriter sw = new StreamWriter(fi, true, Encoding.UTF8))
			{
				foreach (var el in rcParser.StringTable)
					sw.WriteLine(el.Key + "\t\t\t" + el.Value);
			}
		}
	}
}
