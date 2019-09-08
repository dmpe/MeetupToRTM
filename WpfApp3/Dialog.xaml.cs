using MeetupToRTM;
using System;
using System.Windows;

namespace RememberTheMeetup
{
    /// <summary>
    /// Interaction logic for Dialog.xaml
    /// </summary>
    public partial class Dialog : Window
    {
        public AuthKeys keys = null;
        public Dialog()
        {
            InitializeComponent();
        }

        public void btnSubmit_Click(object sender, RoutedEventArgs e) 
        {
            if (!string.IsNullOrEmpty(MeetupCode.Text))
            {
                string strUserName = MeetupCode.Text;
                
                this.Close();
            }
            else
            {
                MessageBox.Show("Must provide a user name in the textbox.");
            }
        }

        /// <summary>
        /// Exit the application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
