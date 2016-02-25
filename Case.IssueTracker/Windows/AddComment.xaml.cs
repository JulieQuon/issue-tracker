using System.Windows;

namespace Case.IssueTracker.Windows
{
    /// <summary>
    /// Interaction logic for AddComment.xaml
    /// </summary>
    public partial class AddComment : Window
    {
        //private string[] statuses = new string[] { "Error", "Warning", "Info", "Unknown" };

        public AddComment()
        {
            InitializeComponent();

            //comboStatuses.ItemsSource = statuses;
            //comboStatuses.SelectedIndex = 3;
        }
        private void OKBtnClick(object sender, RoutedEventArgs e)
        {
            if (comment.Text == "")
            {
                MessageBox.Show("Please write a comment or cancel.", "No comment", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            DialogResult = true;
        }
        private void CancelBtnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
