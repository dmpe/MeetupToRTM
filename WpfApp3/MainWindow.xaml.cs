using System;
using System.Collections.Generic;
using System.Windows;
using RestSharp;
using Newtonsoft.Json;
using MeetupToRTM.MeetupHelpers;
using MeetupToRTM.RememberTM_Helpers;
using System.ComponentModel;

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

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new RTM();
            //LoggginTextBlock.DataContext = this;

            ak = new AuthKeys
            {
                MyRTMkey = RTMkey.Text,
                MyRTMsecret = RTMsecret.Text,
                MyMeetupKey = MeetupKey.Text
            };

            meetup_inst = new MeetUp(ak);
            rtm = new RTM(ak);
        }

        /// <summary>
        /// https://www.newtonsoft.com/json
        /// https://www.meetup.com/meetup_api/docs/self/events/
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Click_Button(object sender, RoutedEventArgs e)
        {
            // initiate connection
            rtm.InnitiateConnection(ak);

            var meetup_url = meetup_inst.CreateURL(ak.MyMeetupKey);
            Console.WriteLine("we are at meetup link: " + meetup_url);

            List<MeetupJSONResults> mu_data = meetup_inst.getMeetupData(meetup_url);
            meetup_inst.returnSampleData(mu_data);

            rtm.CreateRTMTask(meetup_inst, mu_data);

        }

        public void exit(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }

}
