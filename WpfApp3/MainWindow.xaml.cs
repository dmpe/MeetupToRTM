using MeetupToRTM.MeetupHelpers;
using MeetupToRTM.RememberTM_Helpers;
using System.Collections.ObjectModel;
using IniParser;
using IniParser.Model;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

using System.Diagnostics;
using NLog;
using System.IO;
using System.Reflection;
using RememberTheMeetup;
using System;
using MeetupToRTM.MeetupJSONHelpers;
using System.Collections.Generic;

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
        MeetUp meetup_inst = null;
        string[] key_ar;

        public static ObservableCollection<string> ListBoxData { get; set; }
        bool checkbox_value = false;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;

            ListBoxData = new ObservableCollection<string>() { "Used for logging..." };

            LoggingListBox.ItemsSource = ListBoxData;
            key_ar = ReadConfig();
        }

        /// <summary>
        /// If file present, then read from it, otherwise leave empty -> error
        /// </summary>
        /// <returns>array of RTM/Meetup keys</returns>
        public string[] ReadConfig()
        {
            string[] key_array = null;
            string MeetupKey;
            string MeetupSecretKey;
            string RTMkey;
            string RTMsecret;
            try
            {
                // This will get the current PROJECT directory
                string currentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                string projectDirectory = Path.Combine(currentDirectory + "\\RTM_Meetup_secrets.ini");
                var parser = new FileIniDataParser();
                if (File.Exists(projectDirectory))
                {
                    IniData data = parser.ReadFile(projectDirectory);
                    KeyDataCollection keyCol = data["private_information"];
                    MeetupKey = keyCol["MeetupKey_file"];
                    MeetupSecretKey = keyCol["MeetupSecretKey_file"];
                    RTMkey = keyCol["RTMkey_file"];
                    RTMsecret = keyCol["RTMsecret_file"];
                }
                else
                {
                    MeetupSecretKey = string.Empty;
                    MeetupKey = string.Empty;
                    RTMkey = string.Empty;
                    RTMsecret = string.Empty;
                }
                key_array = new string[] { MeetupKey, MeetupSecretKey, RTMkey, RTMsecret };
                logger.Info("our keys: " + key_array[0] + " ..." + key_array[1] + " ..." + key_array[2] + " ..." + key_array[3]);

            } catch(FileNotFoundException ex) 
            {
                logger.Error(ex);
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
                MyRTMkey = key_ar[2] ?? RTMkey.Text,
                MyRTMsecret = key_ar[3] ?? RTMsecret.Text,
                MyMeetupKey = key_ar[0] ?? MeetupKey.Text,
                MyMeetupKeySecret = key_ar[1] ?? MeetupSecretKey.Text
            };

            meetup_inst = new MeetUp(ak, RTM_Web_UI_Format.Text);
            rtm = new RTM();

            // initiate RTM connection
            SetLoggingMessage_Other(rtmCon);
            rtm.InitiateConnection(ak);
            logger.Info("Done with RTM authKeys");

            // initiate Meetup connection
            meetup_inst.InitiateConnection();
            logger.Info(meetupCon);
            SetLoggingMessage_Other(meetupCon);

            // open meetup dialog where you can insert code, code stored
            // https://stackoverflow.com/questions/2796470/wpf-create-a-dialog-prompt
            var myDia = new Dialog();
            myDia.ShowDialog();
            string meetupCode = myDia.return_MeetupKey;
            Console.WriteLine("Meetup: so far correct! code: " + meetupCode);
            var JsOb = meetup_inst.AuthorizeTokenAsync(meetupCode);
            logger.Info("Done with Meetup authKeys: " + JsOb.access_token);
            SetLoggingMessage_Other("Done with Meetup authKeys" + JsOb.access_token);

            string meetupDataURL = meetup_inst.CreateDataURL();
            var meetupEventData = meetup_inst.GetMeetupData(meetupDataURL);

            meetup_inst.GetSampleData(meetupEventData); // for testing
            var rtmMeetupTasksData = meetup_inst.Create_RTM_Tasks_From_Events(meetupEventData);
            var mu_event_venue = meetup_inst.PrepareMeetupTaskList_Venue_ToString(meetupEventData);
            rtm.SetRTMTasks(meetupEventData, rtmMeetupTasksData, mu_event_venue, checkbox_value);
        }

        /// <summary>
        /// Exit the application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void exit(object sender, RoutedEventArgs e)
        {
            Close();
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
        /// 
        /// </summary>
        /// <param name="checkBox">CheckBox value</param>
        private void Handle_Checkbox(CheckBox checkBox)
        {
            checkbox_value = CheckForExisitingRTMTasksFromThisApplication.IsChecked.Value;
            ListBoxData.Add("Change in the CheckBox value:   " + checkbox_value);
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
