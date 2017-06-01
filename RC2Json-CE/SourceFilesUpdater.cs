using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace RC2Json
{
	public class SourceFilesUpdater
	{
		private string appsFolder;
		private string tbFolder;
		private string frameworkFolder;
		private string extensionFolder;
		private string currentFolder;
		/// <summary>
		/// Analizza i file di progetto nella cartella e relative sottocartelle, 
		/// quindi cambia le impostazioni dei file *.rc che non sono soggetti a build
		/// associando loro questo custom build tool
		/// </summary>
		/// <param name="path"></param>
		internal void ProcessProjectsFolder(string path)
		{
			string[] files = Directory.GetFiles(path, "*.vcxproj", SearchOption.AllDirectories);
			foreach (string f in files)
			{
				try
				{
					ProcessVCFile(f);
					AdjustFilterFile(f);
				}
				catch (Exception ex)
				{
					Diagnostic.WriteLine(string.Format("Error processing file '{0}: {1}", f, ex.Message));
				}
			}
		}
		/// <summary>
		/// Modifica il file xml di progetto cambiando le impostazioni dei file *.rc che non sono soggetti a build
		/// ed associando loro questo custom build tool
		/// </summary>
		/// <param name="f"></param>
		private void ProcessVCFile(string f)
		{
			string ns = "http://schemas.microsoft.com/developer/msbuild/2003";
			XmlDocument doc = new XmlDocument();
			doc.Load(f);
			XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
			nsmgr.AddNamespace("ns", ns);
			bool modified = false;
			string folder = Path.GetDirectoryName(f);
			foreach (XmlElement cmdEl in doc.SelectNodes("//ns:CustomBuild/ns:Command", nsmgr))
			{
				if (cmdEl.InnerText.Equals("$(MicroareaUtility)\\RCTOJSON\\RCTOJSON.exe /rc %(FullPath)"))
				{
					cmdEl.InnerText = @"$(MicroareaUtility)\RCTOJSON\RCTOJSON.exe /rc ""%(FullPath)""";
					modified = true;
				}
			}
			//per ogni rc escluso dalla build
			foreach (XmlElement el in doc.SelectNodes("//ns:ResourceCompile[ns:ExcludedFromBuild]", nsmgr))
			{
				string includeFile = Path.Combine(folder, Path.ChangeExtension(el.GetAttribute("Include"), ".hrc"));
				//per ogni rc escluso dalla build, deve esistere anche il corrispondente hrc
				//altrimenti sono nel caso di rc incluso in altro rc (accorpamento dll)
				if (!File.Exists(includeFile))
					continue;
				//creo la sezione di custom build per questo rc
				XmlElement newEl = doc.CreateElement("CustomBuild", ns);
				newEl.SetAttribute("Include", el.GetAttribute("Include"));
				foreach (XmlElement excludeEl in el.SelectNodes("ns:ExcludedFromBuild", nsmgr))
				{
					string condition = excludeEl.GetAttribute("Condition");
					XmlElement cmdEl = doc.CreateElement("Command", ns);
					cmdEl.SetAttribute("Condition", condition);
					cmdEl.InnerText = @"$(MicroareaUtility)\RCTOJSON\RCTOJSON.exe /rc ""%(FullPath)""";
					newEl.AppendChild(cmdEl);

					XmlElement outputsEl = doc.CreateElement("Outputs", ns);
					outputsEl.SetAttribute("Condition", condition);
					//il file hjson è quello che contiene le define per i file cpp
					//la data di questo file viene confrontata con quella dell'rc per saperre se la build va fatta o meno
					outputsEl.InnerText = @"%(RelativeDir)%(Filename).hjson";
					newEl.AppendChild(outputsEl);

					XmlElement elAdditional = doc.CreateElement("AdditionalInputs", ns);
					elAdditional.SetAttribute("Condition", condition);
					elAdditional.InnerText = @"%(RelativeDir)%(Filename).hrc";
					el.ParentNode.AppendChild(elAdditional);
				}
				//e la sostituisco al tool corrente
				el.ParentNode.ReplaceChild(newEl, el);
				modified = true;
			}

			//Cerco eventuali nodi senza AdditionalInputs ed integro (fatta in un secondo tempo)
			foreach (XmlElement el in doc.SelectNodes("//ns:CustomBuild/ns:Outputs[text()='%(RelativeDir)%(Filename).hjson']", nsmgr))
			{
				string condition = el.GetAttribute("Condition");
				bool found = false;
				foreach (XmlElement add in el.ParentNode.SelectNodes("ns:AdditionalInputs", nsmgr))
				{
					if (add.GetAttribute("Condition") == condition)
					{
						found = true;
						break;
					}
				}
				if (!found)
				{
					XmlElement elAdditional = doc.CreateElement("AdditionalInputs", ns);
					elAdditional.SetAttribute("Condition", condition);
					elAdditional.InnerText = @"%(RelativeDir)%(Filename).hrc";
					el.ParentNode.AppendChild(elAdditional);
					modified = true;
				}

			}
			if (modified)
			{
				doc.Save(f);
				Diagnostic.WriteLine("Modified project file: " + f);
			}

		}

		/// <summary>
		/// Modifica il file xml di filtro di progetto cambiando le impostazioni dei file *.rc che non sono soggetti a build
		/// </summary>
		/// <param name="f"></param>
		private void AdjustFilterFile(string f)
		{
			string ns = "http://schemas.microsoft.com/developer/msbuild/2003";
			string filterFile = f + ".filters";
			if (!File.Exists(filterFile))
				return;
			XmlDocument projDoc = new XmlDocument();
			projDoc.Load(f);

			XmlDocument filterDoc = new XmlDocument();
			filterDoc.Load(filterFile);
			XmlNamespaceManager nsmgr = new XmlNamespaceManager(filterDoc.NameTable);
			nsmgr.AddNamespace("ns", ns);

			bool modified = false;
			string folder = Path.GetDirectoryName(f);
			//per ogni rc escluso dalla build
			foreach (XmlElement el in projDoc.SelectNodes("//ns:CustomBuild", nsmgr))
			{
				string include = el.GetAttribute("Include");

				foreach (XmlElement targetEl in filterDoc.SelectNodes("//ns:ResourceCompile[@Include=\"" + include + "\"]", nsmgr))
				{
					//creo la sezione di custom build per questo rc
					XmlElement newEl = filterDoc.CreateElement("CustomBuild", ns);
					newEl.SetAttribute("Include", targetEl.GetAttribute("Include"));
					foreach (XmlNode n in targetEl.ChildNodes)
						newEl.AppendChild(n);
					//e la sostituisco al tool corrente
					targetEl.ParentNode.ReplaceChild(newEl, targetEl);
					modified = true;
				}
			}
			if (modified)
			{
				filterDoc.Save(filterFile);
				Diagnostic.WriteLine("Modified project file: " + filterFile);
			}

		}

		public void ProcessSourcesFolder(string folder)
		{
			List<string> files = new List<string>();
			//metto i sorgenti delle applicazioni
			files.AddRange(Directory.GetFiles(folder, "*.h", SearchOption.AllDirectories));
			files.AddRange(Directory.GetFiles(folder, "*.cpp", SearchOption.AllDirectories));
			
			foreach (var file in files)
			{
				ProcessSourceFile(file);
			}
			
		}
		private string Replace(Match m)
		{
			string name = m.Groups["name"].ToString();
			string jsonName = name + ".hjson";
			//se non esiste il file, significa che non c'era rc contenente risorse che ci interessano
			if (!File.Exists(Path.Combine(tbFolder, jsonName)) &&
				!File.Exists(Path.Combine(frameworkFolder, jsonName)) &&
				!File.Exists(Path.Combine(extensionFolder, jsonName)) && 
				!File.Exists(Path.Combine(appsFolder, jsonName)) && 
				!File.Exists(Path.Combine(currentFolder, jsonName)))
				return m.ToString();

			return string.Format("#include {0}{1}.hjson{2} //JSON AUTOMATIC UPDATE",
											m.Groups["open"],
											name,
											m.Groups["close"]
							 				);
		}
		//per correggere un casino che ho fatto in precedenza
		public void ProcessSourceFileToTapull(string file)
		{
			string installationPath = Helper.GetInstallationPath(file);
			appsFolder = Path.Combine(installationPath, "standard\\applications\\erp");
			tbFolder = Path.Combine(installationPath, "standard\\taskbuilder");
			frameworkFolder = Path.Combine(tbFolder, "framework");
			extensionFolder = Path.Combine(tbFolder, "extensons");
			currentFolder = Path.GetDirectoryName(file);

			StringBuilder sb = new StringBuilder();
			string pattern1 = "//JSON AUTOMATIC UPDATE", pattern2 = "\\.hjsona[\">]";
			List<string> included = new List<string>();
			bool modified = false;
			Encoding e;
			using (StreamReader sr = new StreamReader(file, Encoding.Default, true))
			{
				string text;
				while (null != (text = sr.ReadLine()))
				{
					if (Regex.IsMatch(text, pattern1))
					{
						modified = true;
						if (Regex.IsMatch(text, pattern2))
						{
							if (!included.Contains(text.Trim()))
							{
								sb.AppendLine(text.Replace(".hjsona", ".hjson"));
								included.Add(text.Trim());
							}
						}
						continue;
					}
					sb.AppendLine(text);
				}
				e = sr.CurrentEncoding;
			}
			if (modified)
			{
				using (StreamWriter sw = new StreamWriter(file, false, e))
				{
					sw.Write(sb.ToString());
				}
			}
		}

		public void ProcessSourceFile(string file)
		{
			string installationPath = Helper.GetInstallationPath(file);
			appsFolder = Path.Combine(installationPath, "standard\\applications\\erp");
			tbFolder = Path.Combine(installationPath, "standard\\taskbuilder");
			frameworkFolder = Path.Combine(tbFolder, "framework");
			extensionFolder = Path.Combine(tbFolder, "extensions");
			currentFolder = Path.GetDirectoryName(file);
				
			StringBuilder sb = new StringBuilder();
			string pattern = @"#include\s+(?<open>[""|<])(?<name>[\w\\]+)\.hrc(?<close>[>""])";
			
			bool modified = false;
			Encoding e;
			using (StreamReader sr = new StreamReader(file, Encoding.Default, true))
			{
				string text, newText;
				while (null != (text = sr.ReadLine()))
				{
					newText = Regex.Replace(text, pattern, Replace);
					if (newText != text)
					{
						sb.AppendLine(newText);
						modified = true;
						continue;
					}
					sb.AppendLine(text);
				}
				e = sr.CurrentEncoding;
			}
			if (modified)
			{
				if (new FileInfo(file).IsReadOnly)
				{ 
				}
				using (StreamWriter sw = new StreamWriter(file, false, e))
				{
					sw.Write(sb.ToString());
				}
			}
		}
	}
}
