using System;
using System.IO;

namespace RC2Json
{
	public class Program
	{

		static int Main(string[] args)
		{
			StreamWriter log = null;
			try
			{
				if (args.Length < 2)
				{
					throw new ApplicationException("Usage: RC2Json /rc <filename>");
				}
				string cmd = args[0];
				switch (cmd)
				{
					case "/rc":
						return new RCConverter().ProcessRC(args[1]);
					case "/checkrc":
						return new RCConverter().CheckRC(args[1]);
					case "/intellisense":
						return IntellisenseGenerator.GenerateIntellisense(args[1]);
					case "/updateprojects":
						new SourceFilesUpdater().ProcessProjectsFolder(args[1]);
						break;
					case "/updatesources":
						new SourceFilesUpdater().ProcessSourcesFolder(args[1]);
						break;
					case "/compact":
						new JsonShrinker().Compact(args[1], args.Length >= 3 && args[2] == "all");
						break;
					case "/cmp":
						if (args.Length < 3)
						{
							Diagnostic.WriteLine("Usage: RC2Json /cmp file1 file2 [original_tool_path]");
							return -1;
						}

						new JsonComparer().Compare(args[1], args[2], args.Length > 3 ? args[3] : "");
						return 0;
				}
				return 0;
			}
			catch (Exception ex)
			{
				Diagnostic.WriteLine(ex.ToString());
				return -1;
			}
			finally
			{
				if (log != null)
					log.Dispose();
			}

		}



		
	

	}
}
