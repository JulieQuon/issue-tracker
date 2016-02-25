using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

namespace Case.IssueTracker.Revit.Entry
{

	/// <summary>
	/// Obfuscation Ignore for External Interface
	/// </summary>
	[Obfuscation(Exclude = true, ApplyToMembers = false)]
	[Transaction(TransactionMode.Manual)]
	public class AppMain : IExternalApplication
	{
		private string _path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
#if Version2014

    public const string RevitVersion = "2014";

#elif Version2015
    
        public const string RevitVersion = "2015";
    
#elif Version2016
    
        public const string RevitVersion = "2016";
    
#endif
    #region Revit IExternalApplciation Implementation

    /// <summary>
    /// Startup
    /// </summary>
    /// <param name="application"></param>
    /// <returns></returns>
    public Result OnStartup(UIControlledApplication application)
		{

			try
			{
        // Version
        if (!application.ControlledApplication.VersionName.Contains(RevitVersion))
				{
					using (TaskDialog td = new TaskDialog("Cannot Continue"))
					{
						td.TitleAutoPrefix = false;
						td.MainInstruction = "Incompatible Revit Version";
						td.MainContent = "This Add-In was built for Revit "+ RevitVersion + ", please contact CASE for assistance.";
						td.Show();
					}
					return Result.Cancelled;
				}

				// Master Tab 
				const string c_tabName = "CASE";

				try
				{
					// Create the Tab
					application.CreateRibbonTab(c_tabName);
				}
				catch { }


        string m_issuetracker = "Case.IssueTracker.dll";
        // Tab
        RibbonPanel m_panel = application.CreateRibbonPanel(c_tabName, "Case Issue Tracker");

				// Button Data
				PushButton m_pushButton = m_panel.AddItem(new PushButtonData("Issue Tracker",
																								 "Issue Tracker",
																								 Path.Combine(_path, "Case.IssueTracker.Revit.dll"),
																								 "Case.IssueTracker.Revit.Entry.CmdMain")) as PushButton;

				// Images and Tooltip
				if (m_pushButton != null)
				{
					m_pushButton.Image = LoadPngImgSource("Case.IssueTracker.Assets.CASEIssueTrackerIcon16x16.png", m_issuetracker);
					m_pushButton.LargeImage = LoadPngImgSource("Case.IssueTracker.Assets.CASEIssueTrackerIcon32x32.png", m_issuetracker);
					m_pushButton.ToolTip = "Case Issue Manager";
				}
            
            }
			catch (Exception ex1)
			{
				MessageBox.Show("exception: " + ex1);
				return Result.Failed;
			}

			return Result.Succeeded;
		}

		/// <summary>
		/// Shut Down
		/// </summary>
		/// <param name="application"></param>
		/// <returns></returns>
		public Result OnShutdown(UIControlledApplication application)
		{
			return Result.Succeeded;
		}

#endregion

#region Private Members

		/// <summary>
		/// Get System Architecture
		/// </summary>
		/// <returns></returns>
		static string ProgramFilesx86()
		{
			if (8 == IntPtr.Size || (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432"))))
				return Environment.GetEnvironmentVariable("ProgramFiles(x86)");

			return Environment.GetEnvironmentVariable("ProgramFiles");
		}


		/// <summary>
		/// Load an Image Source from File
		/// </summary>
		/// <param name="sourceName"></param>
		/// <param name="path"></param>
		/// <returns></returns>
		private ImageSource LoadPngImgSource(string sourceName, string path)
		{

			try
			{
				// Assembly & Stream
				Assembly m_assembly = Assembly.LoadFrom(Path.Combine(path));
				Stream m_icon = m_assembly.GetManifestResourceStream(sourceName);

				// Decoder
				PngBitmapDecoder m_decoder = new PngBitmapDecoder(m_icon, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);

				// Source
				ImageSource m_source = m_decoder.Frames[0];
				return (m_source);

			}
			catch { }

			// Fail
			return null;

		}

#endregion

	}
}