using System;
using System.Windows;
using System.Windows.Controls;
using Case.IssueTracker.Data;

namespace Case.IssueTracker.UserControls
{
    /// <summary>
    /// Interaction logic for BCFPanel.xaml
    /// </summary>
    public partial class BCFPanel : UserControl
    {
        public event EventHandler<IntArg> ComponentsShowBCFEH;
        public event EventHandler<StringArg> OpenImageEH;

        public BCFPanel()
        {
            InitializeComponent();
        }

        private void ComponentsShow(object sender, RoutedEventArgs e)
        {

            try
            {
                if (issueList.SelectedIndex != -1)
                {
                    if (ComponentsShowBCFEH != null)
                    {
                        ComponentsShowBCFEH(this, new IntArg(issueList.SelectedIndex));
                    }
                }
            }
            catch (System.Exception ex1)
            {
                MessageBox.Show("exception: " + ex1);
            }
        }

        private void OpenImage(object sender, RoutedEventArgs e)
        {
            if (OpenImageEH != null)
            {
                OpenImageEH(this, new StringArg((string)((Button)sender).Tag));
            }
        }
        public int listIndex
        {
            get { return issueList.SelectedIndex; }
            set { issueList.SelectedIndex = value; }
        }


    }
}
