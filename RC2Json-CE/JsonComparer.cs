using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RC2Json
{
	internal class JsonComparer
	{
		public JsonComparer()
		{
		}

		internal void Compare(string file1, string file2, string originalToolPath)
		{
			if (string.IsNullOrEmpty(originalToolPath) || !File.Exists(originalToolPath))
			{
				string path = Environment.GetEnvironmentVariable("VS140COMNTOOLS");
				originalToolPath = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(path)), "ide\\vsDiffMerge.exe");
				if (!File.Exists(originalToolPath))
					return;
			}
            string tmpFile1 = Path.Combine(Path.GetTempPath(), "first_" + Path.GetFileName(file1));
			string tmpFile2 = Path.Combine(Path.GetTempPath(), "second_" + Path.GetFileName(file2));
			using (StreamReader reader = new StreamReader(file1))
			{
				MyJObject root = MyJObject.Parse(new JsonTextReader(reader));
				using (StreamWriter sw = new StreamWriter(tmpFile1, false, Encoding.UTF8))
				{
					root = SortProperties(root);
					JsonTextWriter jtw = new JsonTextWriter(sw);
					jtw.Formatting = Formatting.Indented;
					root.ToString(jtw);
				}
			}
			using (StreamReader reader = new StreamReader(file2))
			{
				MyJObject root = MyJObject.Parse(new JsonTextReader(reader));
				using (StreamWriter sw = new StreamWriter(tmpFile2, false, Encoding.UTF8))
				{
					root = SortProperties(root);
					JsonTextWriter jtw = new JsonTextWriter(sw);
					jtw.Formatting = Formatting.Indented;
					root.ToString(jtw);
				}
			}

			DateTime dt1Start = new FileInfo(tmpFile1).LastWriteTime;
			DateTime dt2Start = new FileInfo(tmpFile2).LastWriteTime;

			Process p = Process.Start(originalToolPath, string.Concat(tmpFile1, " ", tmpFile2));
			p.WaitForExit();

			DateTime dt1End = new FileInfo(tmpFile1).LastWriteTime;
			DateTime dt2End = new FileInfo(tmpFile2).LastWriteTime;
			if (dt1End > dt1Start)//il file è stato modificato: riporto le modifiche su quello originario
				File.Copy(tmpFile1, file1, true);
			if (dt2End > dt2Start)//il file è stato modificato: riporto le modifiche su quello originario
				File.Copy(tmpFile2, file2, true);

			File.Delete(tmpFile1);
			File.Delete(tmpFile2);
		}

		private static string AdjustComments(StreamReader reader)
		{
			StringBuilder sb = new StringBuilder();
			string s;
			while ((s = reader.ReadLine()) != null)
			{
				int idx = s.IndexOf("//");
				if (idx != -1)
				{
					s = s.Substring(0, idx) + "/*" + s.Substring(idx + 2) + "*/";
				}
				sb.Append(s);
			}
			String adjusted = sb.ToString();
			return adjusted;
		}

		private static MyJObject SortProperties(MyJObject original)
		{
			var newObject = new MyJObject();

			foreach (var prop in original.OrderBy(p => p.Name))
			{
				if (prop.Value is MyJObject)
					prop.Value = SortProperties((MyJObject)prop.Value);
				else if (prop.Value is MyJArray)
					prop.Value = SortArray((MyJArray)prop.Value);
				newObject.Add(prop);
				
			}
			return newObject;
		}

		private static MyJArray SortArray(MyJArray ar)
		{
			for (int i = 0; i < ar.Count; i++)
			{
				if (ar[i] is JObject)
					ar[i] = SortProperties((MyJObject)ar[i]);
				else if (ar[i] is MyJArray)
					ar[i] = SortArray((MyJArray)ar[i]);
			}
			return ar;
		}
	}
}