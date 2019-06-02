﻿using MeetupToRTM.MeetupHelpers;
using MeetupToRTM.MeetupJSONHelpers;
using MeetupToRTM.RememberTM_Helpers;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using IniParser;
using IniParser.Model;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

using System.Diagnostics;
using NLog;
using System;
using System.IO;
using System.Reflection;

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
            KeyDataCollection keyCol = null;
            string[] key_array = null;
            string MeetupKey;
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
                    keyCol = data["private_information"];
                    MeetupKey = keyCol["MeetupKey_file"];
                    RTMkey = keyCol["RTMkey_file"];
                    RTMsecret = keyCol["RTMsecret_file"];
                }
                else
                {
                    MeetupKey = string.Empty;
                    RTMkey = string.Empty;
                    RTMsecret = string.Empty;
                }
                key_array = new string[] { MeetupKey, RTMkey, RTMsecret };
                logger.Info("our keys: " + key_array[0] + " ..." + key_array[1] + " ..." + key_array[2]);
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
            ak = new AuthKeys
            {
                MyRTMkey = key_ar[1] ?? RTMkey.Text,
                MyRTMsecret = key_ar[2] ?? RTMsecret.Text,
                MyMeetupKey = key_ar[0] ?? MeetupKey.Text
            };

            meetup_inst = new MeetUp(ak, RTM_Web_UI_Format.Text);
            rtm = new RTM(ak);

            // initiate connection
            SetLoggingMessage_Other("RTM: Innitiate Connection...now...");
            rtm.InitiateConnection(ak);

            var meetup_url = meetup_inst.SetMeetupURL(ak.MyMeetupKey);
            logger.Info("we are at meetup link: " + meetup_url);
            SetLoggingMessage_Other("RTM: we are at meetup link: " + meetup_url);

            List<MeetupJSONEventResults> mu_data = meetup_inst.GetMeetupData(meetup_url);

            meetup_inst.GetSampleData(mu_data); // for testing

            var mu_event = meetup_inst.Create_RTM_Tasks_From_Events(mu_data);
            var mu_event_vanue = meetup_inst.PrepareMeetupTaskList_Venue_ToString(mu_data);

            rtm.SetRTMTasks(mu_data, mu_event, mu_event_vanue, checkbox_value);

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
