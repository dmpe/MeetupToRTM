using System;
using System.Collections.Generic;
using RestSharp;
using Flurl;
using Newtonsoft.Json;
using System.Windows;
using System.ComponentModel;
using System.Text.RegularExpressions;
using NodaTime.Text;
using System.Globalization;

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
        public readonly AuthKeys ak = null;

        List<MeetupJSONEventResults> list_meetup_event_res = null;
        List<string> list_meetup_vanue_res = null;
        List<string> rtm_string_add_tasks = null;
        List<string> rtm_string_get_our_tasks = null;

        public string event_meetup = string.Empty;
        public string event_meetup_venue = string.Empty;


        public MeetUp()
        {

        }

        public MeetUp(AuthKeys ak)
        {
            this.ak = ak;
            rtm_string_add_tasks = new List<string>();
            rtm_string_get_our_tasks = new List<string>();
            list_meetup_vanue_res = new List<string>();

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
        public List<MeetupJSONEventResults> GetMeetupData(string URL)
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
                list_meetup_event_res = JsonConvert.DeserializeObject<List<MeetupJSONEventResults>>(content);
            }
            catch (JsonException)
            {
                //_textboxLog += list_meetup_event_res.ToString() + Environment.NewLine;
                //OnPropertyChanged("TextBoxValue");
            }

            return list_meetup_event_res;
        }

        public void ValidateMeetupData(List<MeetupJSONEventResults> list_meetup_event_res)
        {
            if (list_meetup_event_res.Count == 0)
            {
                Console.WriteLine("Does not contain any upcomming meetups");
                MainWindow.Handle_Other("Does not contain any upcomming meetups");
                MessageBox.Show("No upcomming events have been found in Meetup. Thus the application cannot transfer them to RTM.", "Information Message", MessageBoxButton.OK);                
            }
        }

        public void returnSampleData(List<MeetupJSONEventResults> list_meetup_event_res)
        {
            foreach (var item in list_meetup_event_res)
            {
                Console.WriteLine(item.Link);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="list_meetup_res"></param>
        /// <returns></returns>
        public List<string> PrepareMeetupTaskList_ToString(List<MeetupJSONEventResults> list_meetup_res)
        {
            foreach (var item in list_meetup_res)
            {
                event_meetup = string.Concat("ID-MeetupRTM: ", item.Id, " ", 
                    DeleteChars(item.Name), " ",
                    ConvertToEUDate(item.Local_date), " ",
                    item.Local_time, " ",
                    item.Link, " ");

                //Console.WriteLine(event_meetup);
                MainWindow.Handle_Other(event_meetup);

                rtm_string_add_tasks.Add(event_meetup);
            }

            return rtm_string_add_tasks;
        }

        /// <summary>
        /// If RTM was able to parse strings and create tasks from them, this method recreates final names from RTM itself
        /// </summary>
        /// <param name="list_meetup_res"></param>
        /// <returns></returns>
        public List<string> PrepareMeetupTaskList_RTM_ToString(List<MeetupJSONEventResults> list_meetup_res)
        {
            foreach (var item in list_meetup_res)
            {
                event_meetup = string.Concat("ID-MeetupRTM: ", item.Id, " ",
                    DeleteChars(item.Name));

                //Console.WriteLine(event_meetup);
                MainWindow.Handle_Other(event_meetup);

                rtm_string_get_our_tasks.Add(event_meetup);
            }

            return rtm_string_get_our_tasks;
        }

        /// <summary>
        ///
        /// TODO: work on exceptions
        /// </summary>
        /// <param name="list_meetup_res"></param>
        /// <returns></returns>
        public List<string> PrepareMeetupTaskList_Venue_ToString(List<MeetupJSONEventResults> list_meetup_res)
        {

            foreach (var item in list_meetup_res)
            {
                try
                {
                    event_meetup_venue = string.Concat("\n Name: ", item.Venue.Name, "\n",
                        "Address: ", item.Venue.Address_1, "\n",
                        "City: ", item.Venue.City, "\n",
                        "Lat + Lon: ", item.Venue.Lat, " ", item.Venue.Lon);
                }
                catch (NullReferenceException e)
                {
                    // TODO: better exception handling
                }

                bool inoe = String.IsNullOrEmpty(event_meetup_venue);

                if (inoe == false)
                {
                    //Console.WriteLine(event_meetup_venue);
                }
                else
                {
                    string ev_loc = " Nah,......Event location is empty at present";
                    //Console.WriteLine(event_meetup_venue + ev_loc);
                    MainWindow.Handle_Other(ev_loc);
                }
                list_meetup_vanue_res.Add(event_meetup_venue);
            }
            return list_meetup_vanue_res;
        }


        /// <summary>
        /// Delete # and @ from event name, plus numbers from name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string DeleteChars(string name)
        {
            string new_name = Regex.Replace(name, "[—?@–#&!$%-=]", "", RegexOptions.Compiled);
            string new_name2 = Regex.Replace(new_name, @"[\d-]", "", RegexOptions.Compiled);
            return new_name2;
        }

        /// <summary>
        /// Convert from  yyyy-mm-dd to dd-mm-yyyy
        /// </summary>
        /// <param name="local_date"></param>
        /// <returns></returns>
        public static string ConvertToEUDate(string local_date)
        {

            var pattern = LocalDateTimePattern.CreateWithInvariantCulture("yyyy-MM-dd");
            var date = pattern.Parse(local_date).Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            //Console.WriteLine("converted datetime" + dateTime);
            //Console.WriteLine("converted original" + pattern.Format(pattern.Parse(text).Value));

            return date;
        }
    }

    /// <summary>
    /// Create class that we can map our JSON to.
    /// </summary>
    public class MeetupJSONEventResults : MeetUp
    {
        public string How_to_find_us { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public int Rsvp_limit { get; set; }
        public string Status { get; set; }
        // The local date of the Meetup in ISO 8601 format
        public string Local_date { get; set; }
        // The local time of the Meetup in ISO 8601 format
        public string Local_time { get; set; }
        public long Updated { get; set; }
        public int UTC_offset { get; set; }
        public int Waitlist_count { get; set; }
        public int Yes_rsvp_count { get; set; }
        public MeetupJSONVenueResults Venue { get; set; }
        public string Link { get; set; }
        public string Description { get; set; }
        public string Visibility { get; set; }
        public bool saved { get; set; }
        public bool Pro_is_email_shared { get; set; }
    }

    public class MeetupJSONVenueResults : MeetUp
    {
        public string Name { get; set; }
        public float Lat { get; set; }
        public float Lon { get; set; }
        public string Address_1 { get; set; }
        public string City { get; set; }
    }

}
