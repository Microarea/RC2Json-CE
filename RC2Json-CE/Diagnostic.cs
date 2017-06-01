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
