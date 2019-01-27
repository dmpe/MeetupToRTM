using System;
using System.Collections.Generic;
using System.Windows;
using MeetupToRTM.MeetupHelpers;
using MeetupToRTM.RememberTM_Helpers;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text;

namespace MeetupToRTM
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// https://www.wpf-tutorial.com/data-binding/responding-to-changes/
    /// https://docs.microsoft.com/en-us/dotnet/framework/wpf/data/data-binding-overview
    /// </summary>
    public partial class MainWindow : Window
    {
        RTM rtm = null;
        AuthKeys ak = null;
        MeetUp meetup_inst = null;
        public static ObservableCollection<string> ListBoxData { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;

            ListBoxData = new ObservableCollection<string>() { "Used for logging..." };
            ak = new AuthKeys
            {
                MyRTMkey = RTMkey.Text,
                MyRTMsecret = RTMsecret.Text,
                MyMeetupKey = MeetupKey.Text
            };

            meetup_inst = new MeetUp(ak);
            rtm = new RTM(ak);
            LoggingListBox.ItemsSource = ListBoxData;
        }

        /// <summary>
        /// <see href="https://www.newtonsoft.com/json">newtonsoft.com/json</see>
        /// <see href="https://www.meetup.com/meetup_api/docs/self/events/">Meetup Events API Documentation</see>
        /// 
        /// This is main method for converting meetup data into RTM tasks
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Click_Button(object sender, RoutedEventArgs e)
        {
            // initiate connection
            Handle_Other("RTM: Innitiate Connection...now...");
            rtm.InnitiateConnection(ak);

            var meetup_url = meetup_inst.CreateURL(ak.MyMeetupKey);
            //Console.WriteLine("we are at meetup link: " + meetup_url);
            Handle_Other("RTM: we are at meetup link: " + meetup_url);

            List<MeetupJSONEventResults> mu_data = meetup_inst.GetMeetupData(meetup_url);

            meetup_inst.returnSampleData(mu_data); // for testing

            var mu_event = meetup_inst.PrepareMeetupTaskList_ToString(mu_data);
            var mu_event_vanue = meetup_inst.PrepareMeetupTaskList_Venue_ToString(mu_data);

            //rtm.CreateRTMTasks(mu_data, mu_event, mu_event_vanue, checkbox);

            
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
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckForExisitingRTMTasksFromThisApplication_Checked(object sender, RoutedEventArgs e)
        {
            Handle_Checkbox(sender as CheckBox);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckForExisitingRTMTasksFromThisApplication_UnChecked(object sender, RoutedEventArgs e)
        {
            Handle_Checkbox(sender as CheckBox);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="checkBox"></param>
        private void Handle_Checkbox(CheckBox checkBox)
        {
            bool checkbox_value = CheckForExisitingRTMTasksFromThisApplication.IsChecked.Value;
            ListBoxData.Add("Change in the CheckBox value:   " + checkbox_value);
        }

        public static void Handle_Other(string str)
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
                string s = LoggingListBox.SelectedItem.ToString();
                Clipboard.SetText(s);

                //Console.WriteLine(s);
                Handle_Other("Copied to the clipboard");
            }
        }

    }

}
