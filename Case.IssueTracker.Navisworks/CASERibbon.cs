using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Autodesk.Navisworks.Api.Plugins;

namespace Case.IssueTracker.Navisworks
{

	/// <summary>
	/// Obfuscation Ignore for External Interface
	/// </summary>
	[Obfuscation(Exclude = true, ApplyToMembers = false)]
	[Plugin("CASEIssueTrackerRibbon", "CASE", DisplayName = "CASE Design, Inc.")]
	[RibbonLayout("RibbonDefinition.xaml")]
	[RibbonTab("ID_case")]
	[Command("ID_caseissuetracker", DisplayName = "Case Issue Tracker", Icon = "CASEIssueTrackerIcon16x16.png", LargeIcon = "CASEIssueTrackerIcon32x32.png", ToolTip = "CASE Issue Tracker", ExtendedToolTip = "CASE Issue Tracker")]
	public class CASERibbon : CommandHandlerPlugin
	{
#if Version2014

    public const string NavisVersion = "2014";

#elif Version2015

    public const string NavisVersion = "2015";

#elif Version2016
    
        public const string NavisVersion = "2016";
    
#endif
    /// <summary>
    /// Constructor, just initialises variables.
    /// </summary>
    public CASERibbon()
		{

		}

		public override int ExecuteCommand(string commandId, params string[] parameters)
		{
			switch (commandId)
			{
				case "ID_caseissuetracker":
					{

						LaunchPlugin();
						break;
					}

				default:
					{
						MessageBox.Show("You have clicked on the command with ID = '" + commandId + "'");
						break;
					}
			}

			return 0;
		}

		public override bool TryShowCommandHelp(String commandId)
		{
			MessageBox.Show("Showing Help for command with the Id " + commandId);
			return true;
		}

		/// <summary>
		/// Launch
		/// </summary>
		public void LaunchPlugin()
		{

			// Running Navis
			if (Autodesk.Navisworks.Api.Application.IsAutomated)
			{
				throw new InvalidOperationException("Invalid when running using Automation");
			}

			// Version
			if (!Autodesk.Navisworks.Api.Application.Version.RuntimeProductName.Contains(NavisVersion))
			{
				MessageBox.Show("Incompatible Navisworks Version" +
									 "\nThis Add-In was built for Navisworks "+ NavisVersion + ", please contact info@case-inc for assistance...",
									 "Cannot Continue!",
									 MessageBoxButtons.OK,
									 MessageBoxIcon.Error);
				return;
			}

			//Find the plugin
			PluginRecord pr = Autodesk.Navisworks.Api.Application.Plugins.FindPlugin("Case.IssueTracker.Navisworks.Plugin.CASE");

			if (pr != null && pr is DockPanePluginRecord && pr.IsEnabled)
			{
				string m_issuetracker = "Case.IssueTracker.dll";

				//check if it needs loading
				if (pr.LoadedPlugin == null)
				{
					pr.LoadPlugin();
				}

				DockPanePlugin dpp = pr.LoadedPlugin as DockPanePlugin;
				if (dpp != null)
				{
					//switch the Visible flag
					dpp.Visible = !dpp.Visible;
				}
			}

		}
	

	}
}
