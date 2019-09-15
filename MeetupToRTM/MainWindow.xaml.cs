using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using IniParser;
using NLog;
using RememberTheMeetup;
using RememberTheMeetup.MeetUp;
using RememberTheMeetup.RTM;

namespace MeetupToRTM
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// <see href="https://www.wpf-tutorial.com/data-binding/responding-to-changes/"></see>
    /// <see href="https://docs.microsoft.com/en-us/dotnet/framework/wpf/data/data-binding-overview"></see>
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        AuthKeys ak = null;
        RTM rtm = null;
        MeetUp meetup = null;
        string[] iniConfigOfSecrets;

        private static ObservableCollection<string> ListBoxData;
        bool checkbox_value = false;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            ListBoxData = new ObservableCollection<string>() { "Used for logging..." };

            LoggingListBox.ItemsSource = ListBoxData;
            iniConfigOfSecrets = ReadConfig();
        }

        /// <summary>
        /// If file present, then read from it, otherwise leave empty -> error
        /// </summary>
        /// <returns>array of RTM/Meetup keys</returns>
        public string[] ReadConfig()
        {
            string[] key_array = null;
            try
            {
                // This will get the current PROJECT directory
                string currentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                string projectDirectory = Path.Combine(currentDirectory + "\\RTM_Meetup_secrets.ini");
                logger.Info("RTM_Meetup_secrets.ini is looked in: " + projectDirectory);
                var parser = new FileIniDataParser();
                string MeetupKey = null;
                string MeetupSecretKey = null;
                string RTMkey = null;
                string RTMsecret = null;
                if (File.Exists(projectDirectory))
                {
                    var keyCol = parser.ReadFile(projectDirectory)["private_information"];
                    MeetupKey = keyCol["MeetupKey_file"];
                    MeetupSecretKey = keyCol["MeetupSecretKey_file"];
                    RTMkey = keyCol["RTMkey_file"];
                    RTMsecret = keyCol["RTMsecret_file"];
                }
                key_array = new string[] { MeetupKey, MeetupSecretKey, RTMkey, RTMsecret };
                logger.Info("our secret keys: " + key_array[0] + " ..." + key_array[1] + " ..." + key_array[2] + " ..." + key_array[3]);

            }
            catch (FileNotFoundException ex)
            {
                logger.Error(ex.Message);
            }
            return key_array;
        }


        /// <summary 
        /// This is main method for converting meetup data into RTM tasks
        /// </summary>
        /// <see href="https://www.newtonsoft.com/json">newtonsoft.com/json</see>
        /// <see href="https://www.meetup.com/meetup_api/docs/self/events/">Meetup Events API Documentation</see>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Click_Button(object sender, RoutedEventArgs e)
        {
            string rtmCon = "RTM: Innitiate Connection...now...";
            string meetupCon = "MeetUp: Innitiate Connection...now...";
            ak = new AuthKeys
            {
                MyRTMkey = iniConfigOfSecrets[2] ?? RTMkey.Text,
                MyRTMsecret = iniConfigOfSecrets[3] ?? RTMsecret.Text,
                MyMeetupKey = iniConfigOfSecrets[0] ?? MeetupKey.Text,
                MyMeetupKeySecret = iniConfigOfSecrets[1] ?? MeetupSecretKey.Text
            };

            meetup = new MeetUp(ak, RTM_Web_UI_Format.Text);
            rtm = new RTM();

            // initiate RTM connection
            SetLoggingMessage_Other(rtmCon);
            rtm.InitiateConnection(ak);
            logger.Info("Done with RTM authKeys");

            // initiate Meetup connection
            SetLoggingMessage_Other(meetupCon);
            meetup.InitiateConnection();
            logger.Info("Done with MeetUp 'code' key");

            // open meetup dialog where you can insert code
            // https://stackoverflow.com/questions/2796470/wpf-create-a-dialog-prompt
            var myDia = new Dialog(ak);
            myDia.ShowDialog();
            Console.WriteLine("Meetup: so far correct! token: " + ak.MyMeetupCode);
            var JsOb = meetup.AuthorizeTokenAsync(ak.MyMeetupCode);
            logger.Info("Done with Meetup authKeys: " + JsOb.access_token);
            SetLoggingMessage_Other("Done with Meetup authKeys" + JsOb.access_token);

            string meetupDataURL = meetup.CreateDataURL();
            var meetupEventData = meetup.GetMeetupData(meetupDataURL);

            meetup.GetSampleData(meetupEventData);
            var rtmMeetupTasksData = meetup.Create_RTM_Tasks_From_Events(meetupEventData);
            var mu_event_venue = meetup.PrepareMeetupTaskList_Venue_ToString(meetupEventData);
            rtm.SetRTMTasks(meetupEventData, rtmMeetupTasksData, mu_event_venue, checkbox_value);
        }

        /// <summary>
        /// Exit the application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void exit(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Used for CheckBox, i.e. if true, then skip adding duplicates
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckForExisitingRTMTasks_Checked(object sender, RoutedEventArgs e)
        {
            Handle_Checkbox(sender as CheckBox);
        }

        /// <summary>
        /// Otherwise add them, which is also by default
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckForExisitingRTMTasks_UnChecked(object sender, RoutedEventArgs e)
        {
            Handle_Checkbox(sender as CheckBox);
        }

        /// <summary>
        /// Add to the listbox
        /// </summary>
        /// <param name="checkBox">CheckBox value</param>
        private void Handle_Checkbox(CheckBox checkBox)
        {
            checkbox_value = CheckForExisitingRTMTasksFromThisApplication.IsChecked.Value;
            if (checkbox_value != true)
            {
                ListBoxData.Add("Your change in the CheckBox value has no impact, yet: " + checkbox_value);
            }
        }

        /// <summary>
        /// Log messages to ListBox on the right
        /// </summary>
        /// <param name="str"></param>
        public static void SetLoggingMessage_Other(string str)
        {
            ListBoxData.Add(str);
        }

        /// <summary>
        /// By default, listbox items cannot be copied to clipboard. 
        /// This method adds such functionality.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopyToClipBoard(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.C)
            {
                string selectedString = LoggingListBox.SelectedItem.ToString();
                Clipboard.SetText(selectedString);

                logger.Info(selectedString);
                string info = "Copied to the clipboard";
                SetLoggingMessage_Other(info);
                logger.Info(info);
            }
        }
        /// <summary>
        /// Hyperlinks need to be opened using this function -- the user starts a browser
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenInBrowser(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }

}
