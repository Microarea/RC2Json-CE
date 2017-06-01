using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RC2Json;
using UnitTestProject.Properties;
using System.IO;
using System.Text.RegularExpressions;

namespace UnitTestProject
{
	[TestClass]
	public class UnitTest1
	{
		[TestMethod]
		public void TestRC()
		{
			new RCConverter().ProcessRC("test.rc");
			using (StreamReader sr = new StreamReader(JsonConstants.JSON_FOLDER_NAME + "\\IDD_FORMVIEW.tbjson"))
			{
				string s1 = sr.ReadToEnd();
				string s2 = Resources.IDD_FORMVIEW;
				
				//pulisco l'id perché dinamico e cambia ogni volta
				// "id": "id_4ffdb00a-6167-4509-bc29-88b44095757b",
				s1 = Regex.Replace(s1, "\"id\": \"[^\"]+\",", "\"id\": \"\",");
				s2 = Regex.Replace(s2, "\"id\": \"[^\"]+\",", "\"id\": \"\",");

				//pulisco l'id perché potrebbe cambiare se rinumerano le risorse
				s1 = Regex.Replace(s1, "\"rcId\": [\\d]+,", "\"rcId\": 0");
				s2 = Regex.Replace(s2, "\"rcId\": [\\d]+,", "\"rcId\": 0");

				Assert.AreEqual(s1, s2);
			}
			
		}

		
		[TestMethod]
		public void TestCppUpdater()
		{
			new SourceFilesUpdater().ProcessSourcesFolder(AppDomain.CurrentDomain.BaseDirectory);
		}
	}
}
