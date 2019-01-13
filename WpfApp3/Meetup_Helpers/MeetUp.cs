using System;
using System.Collections.Generic;
using RestSharp;
using Flurl;
using Newtonsoft.Json;
using System.Windows;
using System.ComponentModel;

namespace MeetupToRTM.MeetupHelpers
{
    public class MeetUp // : INotifyPropertyChanged
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
        AuthKeys ak = null;
        List<MeetupJSONResults> list_meetup_res = null;

        public MeetUp()
        {

        }

        public MeetUp(AuthKeys ak)
        {
            this.ak = ak;
        }

        /// <summary>
        /// Creates Flurl-based URL which can access Meetups data
        /// </summary>
        /// <returns></returns>
        public string CreateURL(string meetup_api_key)
        {
            var my_url = host
                .AppendPathSegment(self)
                .AppendPathSegment(events)
                .SetQueryParams(new
                {
                    sign,
                    status,
                    scroll,
                    key = meetup_api_key
                });

            return my_url;
        }

        /// <summary>
        /// Extract meetup JSON data and store them in the list of events
        /// </summary>
        /// <returns></returns>
        public List<MeetupJSONResults> getMeetupData(string URL)
        {           

            var client = new RestClient
            {
                BaseUrl = new Uri(URL)
            };
            var request = new RestRequest();
            IRestResponse response = client.Execute(request);
            var content = response.Content;

            try
            {
                list_meetup_res = JsonConvert.DeserializeObject<List<MeetupJSONResults>>(content);
            }
            catch (JsonException)
            {
                //_textboxLog += list_meetup_res.ToString() + Environment.NewLine;
                //OnPropertyChanged("TextBoxValue");
            }

            return list_meetup_res;
        }

        public void validateMeetupData(List<MeetupJSONResults> list_meetup_res)
        {
            if (list_meetup_res.Count == 0)
            {
                Console.WriteLine("Does not contain any upcomming meetups");
                MessageBox.Show("No upcomming events have been found in Meetup. Thus the application cannot transfer them to RTM.", "Information Message", MessageBoxButton.OK);                
            }
        }

        public void returnSampleData(List<MeetupJSONResults> list_meetup_res)
        {
            foreach (var item in list_meetup_res)
            {
                Console.WriteLine(item.Link);
            }
        }
    }

    /// <summary>
    /// Create class that we can map our JSON to.
    /// </summary>
    public class MeetupJSONResults : MeetUp
    {
        public string Name { get; set; }
        public int Rsvp_limit { get; set; }
        public string Status { get; set; }
        public string Local_date { get; set; }
        public string Local_time { get; set; }
        public long Updated { get; set; }
        public int UTC_offset { get; set; }
        public int Waitlist_count { get; set; }
        public int Yes_rsvp_count { get; set; }
        public Venue Venue { get; set; }
        public string Link { get; set; }
        public string Description { get; set; }
        public string Visibility { get; set; }
        public bool Pro_is_email_shared { get; set; }
        public string How_to_find_us { get; set; }
    }

    public class Venue : MeetUp
    {
        public string Name { get; set; }
        public float Lat { get; set; }
        public float Lon { get; set; }
        public string Address_1 { get; set; }
        public string City { get; set; }
    }



}
