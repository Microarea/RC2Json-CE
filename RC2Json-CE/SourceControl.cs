using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RC2Json
{
	class SourceControl 
	{
		public SourceControl()
		{
		}
		
		public bool CheckOutIfNeeded(String file)
		{
			try
			{
				if ((File.GetAttributes(file) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
					return CheckOut(file);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}
		public bool CheckOut(String file)
		{
			WorkspaceInfo workspaceInfo = Workstation.Current.GetLocalWorkspaceInfo(file);
			if (workspaceInfo == null)
				return false;

			using (TfsTeamProjectCollection server = new TfsTeamProjectCollection(workspaceInfo.ServerUri))
			{
				Workspace workspace = workspaceInfo.GetWorkspace(server);
				return workspace.PendEdit(file) == 1;
			}
		}

	}
}
