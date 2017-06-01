using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RC2Json
{
	class JsonUserDocuments
	{
		bool hasFile = false;
		List<string> documents = new List<string>();
		List<string> hjsons = new List<string>();
		static JsonUserDocuments jsonUsers;

		private JsonUserDocuments(string rcFile)
		{
            // cerca un file nel folder locale
            string currDir = Path.GetDirectoryName(rcFile);
            string xmlFile = Path.Combine(currDir, JsonConstants.JSON_USERS_FILE);
            if (!File.Exists(xmlFile))
            {
                // cerca un file generico per la intera app
			    string subPath = "\\standard\\applications\\";
			    int idx = rcFile.IndexOf(subPath, StringComparison.InvariantCultureIgnoreCase);
			    if (idx == -1)
			    {
				    hasFile = false;
				    return;
			    }
			    string stdAppPath = rcFile.Substring(0, idx + subPath.Length);
                int slash = rcFile.IndexOf("\\", idx + subPath.Length, StringComparison.InvariantCultureIgnoreCase);
                if (slash == -1)
                {
                    hasFile = false;
                    return;
                }
                string appPath = rcFile.Substring(0, slash);
                 xmlFile = Path.Combine(appPath, JsonConstants.JSON_USERS_FILE);
			    if (!File.Exists(xmlFile))
			    { 
				    hasFile = false;
				    return;
			    }
            }
            XmlDocument doc = new XmlDocument();
			try
			{
				doc.Load(xmlFile);
				foreach (XmlElement el in doc.DocumentElement.ChildNodes)
				{
					string hjson = el.GetAttribute("hjson");
					string docName = el.GetAttribute("name");
					if (!string.IsNullOrEmpty(hjson) && !string.IsNullOrEmpty(docName))
					{
						documents.Add(docName);
						hjsons.Add(hjson);
					}
				}
			}
			catch (Exception)
			{
				hasFile = false;
				return;
			}
			hasFile = true;
		}

		internal static bool GetDocumentFolder(string rcFile, out string categoryName)
		{
			categoryName = "";
			JsonUserDocuments docs = GetJsonUsers(rcFile);
			if (docs.hasFile)
            {
                string hjson = Path.ChangeExtension(rcFile, ".hjson");
                if (docs.FindDocumentFolder(hjson, out categoryName))
                    return true;
            }

            // tenta con euristica basata sul fatto che il nome del file inizia con "UI" 
            // ed esiste una descirizone documento con lo stesso nome
            string rcName = Path.GetFileNameWithoutExtension(rcFile);
            if (!rcName.StartsWith("UI"))
                return false;

            string moduleObjFolder = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(rcFile)), "ModuleObjects");
            string docNSpace = rcName.Substring(2);

            if (Directory.Exists(Path.Combine(moduleObjFolder, docNSpace)))
            {
                categoryName = docNSpace;
                return true;
            }

            return false;
			
		}

		private bool FindDocumentFolder(string hjson, out string categoryName)
		{
			categoryName = "";
			for (int i = 0; i < hjsons.Count; i++)
			{
				if (hjson.EndsWith(hjsons[i], StringComparison.InvariantCultureIgnoreCase))
				{
					categoryName = documents[i];
					return true;
				}
			}
			return false;
		}

		private static JsonUserDocuments GetJsonUsers(string rcFile)
		{
			if (jsonUsers == null)
			{
				jsonUsers = new JsonUserDocuments(rcFile);
			}
			return jsonUsers;

		}
	}
}
