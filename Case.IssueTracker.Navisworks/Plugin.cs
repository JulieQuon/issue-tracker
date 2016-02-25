using System;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Forms.VisualStyles;
using Autodesk.Navisworks.Api.Plugins;

namespace Case.IssueTracker.Navisworks
{
  [Plugin("Case.IssueTracker.Navisworks.Plugin", "CASE", DisplayName = "CASE Issue Tracker", ToolTip = "CASE Issue Tracker")]
  [DockPanePlugin(800, 500, FixedSize = false)]
  class Plugin : DockPanePlugin
  {
    NavisWindow ns;

    public override Control CreateControlPane()
    {
      //create an ElementHost
      ElementHost eh = new ElementHost();

      //assign the control
      //eh.Anchor = AnchorStyles.Top;
      eh.Dock = DockStyle.Top;
      eh.Anchor = AnchorStyles.Top;
      ns = new NavisWindow();
      eh.Child = ns;
      eh.HandleCreated += eh_HandleCreated;
      eh.CreateControl();
        
      //return the ElementHost
      return eh;
    }

    void eh_HandleCreated(object sender, EventArgs e)
    {
      ElementHost eh = sender as ElementHost;
      eh.Dock = DockStyle.Top;
      eh.Anchor = AnchorStyles.Top;

    }

    public override void DestroyControlPane(Control pane)
    {
      ns.mainPan.onClosing(null);
      pane.Dispose();
       
    }
  }
}