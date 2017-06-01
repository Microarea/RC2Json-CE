using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO; 
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RC2Json
{
	class IntellisenseGenerator
	{
		static DateTime AsmDate = File.GetLastWriteTime(Assembly.GetExecutingAssembly().Location);
		public static int GenerateIntellisense(string path)
		{
			//salverò i file nel path di installazione
			string installationPath = Helper.GetInstallationPath(path);
			if (string.IsNullOrEmpty(installationPath))
			{
				Diagnostic.WriteLine("Cannot find installation path");
				return -1;
			}
			//GenerateIntelliDB(path, installationPath);
			//cerco tutti i file con estensione *.cpp
			List<string> files = new List<string>();
			string appsFolder = Path.Combine(installationPath, "standard\\applications");
			string tbFolder = Path.Combine(installationPath, "standard\\taskbuilder");

			//metto i sorgenti delle applicazioni
			files.AddRange(Directory.GetFiles(appsFolder, "*.h", SearchOption.AllDirectories));
			files.AddRange(Directory.GetFiles(appsFolder, "*.cpp", SearchOption.AllDirectories));
			
			//e gli interface di tb
			files.AddRange(Directory.GetFiles(tbFolder, "*interface.cpp", SearchOption.AllDirectories));
			
			string intelliFileControls = Path.Combine(installationPath, JsonConstants.INTELLI_FILE_CONTROLS);
			string intelliFileDbts = Path.Combine(installationPath, JsonConstants.INTELLI_FILE_DBTS);
			string hjsonOwnerFile = Path.Combine(installationPath, JsonConstants.HJSON_DOC_OWNER);
			if (UpdateNeeded(files.ToArray(), new string[] { intelliFileControls, intelliFileDbts, hjsonOwnerFile }))
			{
				ParsedControlList controls = new ParsedControlList();
				CppInfo info = new CppInfo();
				foreach (string f in files)
				{
					CPPParser parser = new CPPParser(controls, info);
					Diagnostic.WriteLine("Parsing file " + f);
					parser.Parse(f);
				}

				GenerateIntelliControls(controls, intelliFileControls);

				GenerateIntelliDbts(info, intelliFileDbts);

				GenerateHJsonOwnerMap(info, hjsonOwnerFile, appsFolder);
			}
			return 0;
		}

		private static void GenerateHJsonOwnerMap(CppInfo info, string hjsonOwnerFile, string appsFolder)
		{
			XmlDocument doc = new XmlDocument();
			XmlElement root = doc.CreateElement("Documents");
			doc.AppendChild(root);
			foreach (var entry in info.Includes)
			{
				List<string> hjsons = entry.Value;
				List<CppClass> classes = info.GetViewClassesInFile(entry.Key);
				foreach (var hjson in hjsons)
					foreach (var cl in classes)
					{
						if (string.IsNullOrEmpty(cl.DocName))
							continue;
						XmlElement el = doc.CreateElement("Document");
						root.AppendChild(el);
						el.SetAttribute("hjson", hjson);
						el.SetAttribute("name", cl.DocName);
						el.SetAttribute("viewClass", cl.ToString());
						el.SetAttribute("declarationFile", cl.DeclarationFile.Substring(appsFolder.Length));
						el.SetAttribute("implementationFile", cl.ImplementationFile.Substring(appsFolder.Length));

					}
			}

			doc.Save(hjsonOwnerFile);
		}

		private static void GenerateIntelliDbts(CppInfo info, string intelliFileDbts)
		{

			string tmp = Path.GetTempFileName();
			using (StreamWriter sw = new StreamWriter(tmp, false, new UTF8Encoding(false)))
			{
				using (JsonTextWriter writer = new JsonTextWriter(sw))
				{
					writer.Formatting = Newtonsoft.Json.Formatting.Indented;
					writer.WriteStartArray();
					foreach (Document doc in info.GetDocuments())
					{
						if (doc.Dbts.Count == 0)
							continue;
						writer.WriteStartObject();
						writer.WritePropertyName(JsonConstants.NAME);
						writer.WriteValue(doc.Name);
						writer.WritePropertyName(JsonConstants.DBTS);
						writer.WriteStartArray();
						foreach (var dbt in doc.Dbts)
						{
							writer.WriteStartObject();
							writer.WritePropertyName(JsonConstants.NAME);
							writer.WriteValue(dbt.Name);
							writer.WritePropertyName(JsonConstants.FIELDS);
							writer.WriteStartArray();
							foreach (var field in dbt.Fields)
							{
								if (field.IsIncomplete)
								{
									string msg = string.Concat("WARNING! Incomplete field ", field);
									Diagnostic.WriteLine(msg);
								}
								else
								{
									writer.WriteStartObject();
									writer.WritePropertyName(JsonConstants.DATA_TYPE);
									writer.WriteValue(field.DataType);
									writer.WritePropertyName(JsonConstants.NAME);
									writer.WriteValue(field.Name);
									writer.WritePropertyName(JsonConstants.VARIABLE);
									writer.WriteValue(field.Variable);
									writer.WriteEndObject();
								}
							}
							writer.WriteEndArray();
							writer.WriteEndObject();
						}
						writer.WriteEndArray();
						writer.WriteEndObject();
					}
					writer.WriteEndArray();
				}
			}
			File.Copy(tmp, intelliFileDbts, true);
			File.Delete(tmp);
		}

		private static void GenerateIntelliControls(ParsedControlList controls, string intelliFileControls)
		{
			//nome file per i controlli registrati
			string tmp = Path.GetTempFileName();
			using (StreamWriter sw = new StreamWriter(tmp, false, new UTF8Encoding(false)))
			{
				using (JsonTextWriter writer = new JsonTextWriter(sw))
				{
					writer.Formatting = Newtonsoft.Json.Formatting.Indented;
					writer.WriteStartArray();

					foreach (var c in controls)
					{
						writer.WriteStartObject();
						writer.WritePropertyName(JsonConstants.DATA_TYPE);
						writer.WriteValue(c.Key);
						writer.WritePropertyName(JsonConstants.CONTROLS);
						writer.WriteStartArray();
						foreach (var c1 in c.Value)
							writer.WriteValue(c1.Name);
						writer.WriteEndArray();
						writer.WriteEndObject();
					}
					writer.WriteEndArray();
				}
			}
			File.Copy(tmp, intelliFileControls, true);
			File.Delete(tmp);
		}

		private static void GenerateIntelliDB(string path, string installationPath)
		{
			//nome file per gli oggetti di database
			string intelliFileDB = Path.Combine(installationPath, JsonConstants.INTELLI_FILE_DB);

			//cero le sottocartelle databasescript
			string[] folders = Directory.GetDirectories(path, "databasescript", SearchOption.AllDirectories);
			//per ognuna, se contiene la sub create\all, cerco tutti i file .sql
			List<String> files = new List<string>();
			foreach (var folder in folders)
			{
				string sub = Path.Combine(folder, "create\\all");
				if (Directory.Exists(sub))
					files.AddRange(Directory.GetFiles(sub, "*.sql"));
			}
			if (UpdateNeeded(files.ToArray(), new string[] { intelliFileDB }))
			{
				string tmp = Path.GetTempFileName();
				using (StreamWriter sw = new StreamWriter(tmp, false, new UTF8Encoding(false)))
				{
					using (RCJsonWriter writer = new RCJsonWriter(sw))
					{
						writer.Formatting = Newtonsoft.Json.Formatting.Indented;
						writer.WriteStartArray();
						foreach (string f in files)
						{
							SQLParser parser = new SQLParser();
							parser.Parse(f);
							foreach (TableStructure t in parser.Tables)
							{
								t.WriteTo(writer);
							}
						}

						writer.WriteEndArray();
					}
				}

				File.Copy(tmp, intelliFileDB, true);
				File.Delete(tmp);
			}
		}
		//confronta le date dei file per vedere se c'è bisogno di aggiornamento
		private static bool UpdateNeeded(string[] files, string[] targetFiles)
		{
			FileInfo[] fis = new FileInfo[targetFiles.Length];
			for (int i = 0; i < targetFiles.Length; i++)
			{
				FileInfo fi = new FileInfo(targetFiles[i]);
				if (!fi.Exists || fi.LastWriteTime < AsmDate)
					return true;
				fis[i] = fi;
			}
			foreach (var file in files)
			{
				FileInfo fi1 = new FileInfo(file);
				foreach (FileInfo fi in fis)
					if (fi1.LastWriteTime > fi.LastWriteTime)
						return true;
			}
			return false;

		}

		
	}
}
