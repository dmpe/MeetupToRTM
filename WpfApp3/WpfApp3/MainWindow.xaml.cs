using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Flurl;
using RestSharp;
using Newtonsoft.Json;
using RestSharp.Authenticators;
using System.ComponentModel;
using RememberTheMilkApi.Helpers;
using RememberTheMilkApi.Objects;
using System.Diagnostics;

namespace WpfApp3
{
    /// <summary>
    /// Create class that we can map our JSON to.
    /// </summary>
    public class MeetupJSONResults
    {
        public string name { get; set; }
        public int rsvp_limit { get; set; }
        public string status { get; set; }
        public string local_date { get; set; }
        public string local_time { get; set; }
        public long updated { get; set; }
        public int utc_offset { get; set; }
        public int waitlist_count { get; set; }
        public int yes_rsvp_count { get; set; }
        public Venue venue { get; set; }
        public string link { get; set; }
        public string description { get; set; }
        public string visibility { get; set; }
        public bool pro_is_email_shared { get; set; }
        public string how_to_find_us { get; set; }
    }

    public class Venue
    {
        public string name { get; set; }
        public float lat { get; set; }
        public float lon { get; set; }
        public string address_1 { get; set; }
        public string city { get; set; }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// https://www.wpf-tutorial.com/data-binding/responding-to-changes/
    /// https://docs.microsoft.com/en-us/dotnet/framework/wpf/data/data-binding-overview
    /// </summary>
    public partial class MainWindow : Window
    {
        // Meetup final variables
        // source object
        public static readonly string format = "json";
        public static readonly string host = "https://api.meetup.com";
        public static readonly byte version = 2;
        public static readonly string self = "self";
        public static readonly string events = "events";
        public static readonly string status = "upcoming";
        public static readonly string scroll = "next_upcoming";
        public static readonly bool sign = true;

        public static readonly string website = "www.b40re.tk";

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// https://flurl.io/docs/fluent-http/
        /// https://www.newtonsoft.com/json
        /// https://www.meetup.com/meetup_api/docs/self/events/
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Click_Button(object sender, RoutedEventArgs e)
        {

        AutKeys ak = new AutKeys
        {
            MyRTMkey = RTMkey.Text,
            MyRTMsecret = RTMsecret.Text,
            MyMeetupKey = MeetupKey.Text
        };

        var url = createURL(ak.MyMeetupKey);
        var client = new RestClient();
        client.BaseUrl = new System.Uri(url);
        var request = new RestRequest();
        IRestResponse response = client.Execute(request);
        var content = response.Content;

        //Console.WriteLine(ak.MyMeetupKey);
        //Console.WriteLine(response.Content);

        List<MeetupJSONResults> list_meetup_res = JsonConvert.DeserializeObject<List<MeetupJSONResults>>(content);
        Console.WriteLine(list_meetup_res[0].link);

        RtmConnectionHelper.InitializeRtmConnection(ak.MyRTMkey, ak.MyRTMsecret);

        string urlRTM = RtmConnectionHelper.GetAuthenticationUrl(RtmConnectionHelper.Permissions.Write);
        Process.Start(urlRTM);
        Console.WriteLine(urlRTM);

        RtmApiResponse authResponse = RtmConnectionHelper.GetApiAuthToken();

        RtmConnectionHelper.SetApiAuthToken(authResponse.Auth.Token);
        RtmApiResponse tokenResponse = RtmConnectionHelper.CheckApiAuthToken();
        RtmApiResponse taskResponse = RtmMethodHelper.GetTasksList();


        }

        public void exit(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Creates URL which can access my Meetups 
        /// </summary>
        /// <returns></returns>
        public string createURL(string meetup)
        {
            var my_url = host
                .AppendPathSegment(self)
                .AppendPathSegment(events)
                .SetQueryParams(new
                {
                    sign = sign,
                    status = status,
                    scroll = scroll,
                    key = meetup
                });

            Console.WriteLine(my_url);
            return my_url;
        }

    }

    public class AutKeys
    {
        // RTM final variables
        // source objects with "one way to source"
        private string myRTMkey;
        private string myRTMsecret;
        private string myMeetupKey;

        public AutKeys()
        {
        }

        public AutKeys(string myRTMkey, string myRTMsecret, string myMeetupKey)
        {
            this.myRTMkey = myRTMkey;
            this.myRTMsecret = myRTMsecret;
            this.myMeetupKey = myMeetupKey;
        }

        public string MyRTMkey { get => myRTMkey; set => myRTMkey = value; }
        public string MyRTMsecret { get => myRTMsecret; set => myRTMsecret = value; }
        public string MyMeetupKey { get => myMeetupKey; set => myMeetupKey = value; }

    }

}
