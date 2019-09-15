using System.Windows;

namespace RememberTheMeetup
{
    /// <summary>
    /// Interaction logic for Dialog.xaml
    /// </summary>
    public partial class Dialog : Window
    {
        public string returnMeetupCode { get; set; }
        AuthKeys authKeys;

        public Dialog(AuthKeys ak)
        {
            this.InitializeComponent();
            this.authKeys = ak;
        }

        public void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(MeetupCode.Text))
            {
                authKeys.MyMeetupCode = MeetupCode.Text;
                Close();
            }
            else
            {
                MessageBox.Show("You must copy the code from the website into the textfield.");
            }
        }

        /// <summary>
        /// Exit dialog
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
