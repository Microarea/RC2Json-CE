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
using System.Threading.Tasks;

namespace RC2Json
{
	class Helper
	{
		internal static string GetContextName(string filename)
		{
			return Path.GetFileNameWithoutExtension(filename).ToLower();
		}
		internal static string FindModuleFolder(string file)
		{
			string folder = Path.GetDirectoryName(file);
			while (!string.IsNullOrEmpty(folder) && !File.Exists(Path.Combine(folder, "module.config")))
				folder = Path.GetDirectoryName(folder);
			return folder;
		}
		internal static string FindApplicationFolder(string file)
		{
			return Path.GetDirectoryName(FindModuleFolder(file));
		}

		internal static string FindBitmapFolder(string file)
		{
			string moduleFolder = FindModuleFolder(file);
			if (string.IsNullOrEmpty(moduleFolder))
				return null;

			return Path.Combine(moduleFolder, "Files\\Images");
		}

		public static string GetInstallationPath(string path)
		{
			string folderName = "";
			while (!string.IsNullOrEmpty(folderName = Path.GetFileName(path)))
			{
				if ("Standard".Equals(folderName, StringComparison.InvariantCultureIgnoreCase) || "Custom".Equals(folderName, StringComparison.InvariantCultureIgnoreCase))
					return Path.GetDirectoryName(path);
				path = Path.GetDirectoryName(path);
			}
			return null;
		}

		internal static string GetRelativePath(string file, string referencePath)
		{
			int idx = file.IndexOf(referencePath, StringComparison.InvariantCultureIgnoreCase);
			if (idx == 0)
				return file.Substring(referencePath.Length+1);
			return file;
		}

		
	}
}
