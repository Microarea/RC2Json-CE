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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace RC2Json
{
	public enum DiagnosticType { Error, Warning }
	class ExtendedInfo
	{
		Dictionary<string, string> infos = new Dictionary<string, string>();
		internal void Add(string info, long detail)
		{
			Add(info, detail.ToString());
		}

		internal void Add(string info, string detail)
		{
			infos[info] = detail;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			foreach (var entry in infos)
			{
				sb.Append(entry.Key);
				sb.Append(": ");
				sb.AppendLine(entry.Value);
			}
			return sb.ToString();
		}
	}
	public class Diagnostic
	{
		bool error = false;
		internal void Set(DiagnosticType diagnosticType, string explain, ExtendedInfo info = null)
		{
			if (diagnosticType == DiagnosticType.Error)
				error = true;
			
			Write(DateTime.Now.ToString());
			Write(" - ");
			Write(diagnosticType == DiagnosticType.Error ? "ERROR - " : "WARNING - ");
			WriteLine(explain);
			if (info != null)
			{
				WriteLine(info.ToString());
			}
		}

		internal static void Write(string s)
		{
			Console.Write(s);
			Trace.Write(s);
		}

		internal static void WriteLine(string s)
		{
			Console.WriteLine(s);
			Trace.WriteLine(s);
		}

		public bool Error { get {return error; }}

		internal void Clear(DiagnosticType diagnosticType)
		{
			error = false;
		}
	}
}
