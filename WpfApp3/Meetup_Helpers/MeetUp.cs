using System;
using System.Collections.Generic;
using System.Windows;
using System.Text.RegularExpressions;
using System.Globalization;

using MeetupToRTM.MeetupJSONHelpers;

using NodaTime.Text;
using NLog;
using RestSharp;
using Flurl;
using Newtonsoft.Json;

namespace MeetupToRTM.MeetupHelpers
{
    public class MeetUp
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

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
        List<string> list_meetup_venue_res = null;
        List<string> rtm_string_tasks_long = null;
        List<string> rtm_string_tasks_short = null;

        private string event_meetup = string.Empty;
        private string event_meetup_venue = string.Empty;

        public MeetUp()
        {

        }

        public MeetUp(AuthKeys aka)
        {
            ak = aka;
            rtm_string_tasks_long = new List<string>();
            rtm_string_tasks_short = new List<string>();
            list_meetup_venue_res = new List<string>();
        }

        /// <summary>
        /// Creates Flurl-based URL which can access MeetUp data
        /// </summary>
        /// <param name="meetup_api_key">MeetUp Key provided by the user in GUI</param>
        /// <returns>URL which returns JSON output</returns>
        public string SetMeetupURL(string meetup_api_key)
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
        /// <param name="URL">URL which is returned </param>
        /// <returns>Data (list of events) being associated with <c>MeetupJSONEventResults</c> class</returns>
        public List<MeetupJSONEventResults> GetMeetupData(string URL)
        {           
            var client = new RestClient
            {
                BaseUrl = new Uri(URL)
            };
            var request = new RestRequest();

            IRestResponse response = client.Execute(request);
            if (response.ErrorException != null)
            {
                const string message = "Error retrieving response. Check inner details for more info.";
                logger.Error(message);
                var RTMException = new ApplicationException(message, response.ErrorException);
                throw RTMException;
            }
            var content = response.Content;

            try
            {
                list_meetup_event_res = JsonConvert.DeserializeObject<List<MeetupJSONEventResults>>(content);
            }
            catch (JsonException ex)
            {
                logger.Error(ex, "Could not deserialize JSON Object");
            }

            return list_meetup_event_res;
        }

        /// <summary>
        /// If there are zero events, log it to the user as well
        /// </summary>
        /// <param name="list_meetup_event_res">List of <c>MeetupJSONEventResults</c> events</param>
        public void ValidateMeetupData(List<MeetupJSONEventResults> list_meetup_event_res)
        {
            if (list_meetup_event_res.Count == 0)
            {
                string log = "Does not contain any upcomming meetups";
                Console.WriteLine(log);
                logger.Error(log);
                MainWindow.SetLoggingMessage_Other(log);

                MessageBox.Show("No upcomming events have been found in Meetup. Thus the application cannot transfer them to RTM.", "Information Message", MessageBoxButton.OK);                
            }
        }

        /// <summary>
        /// Returns a sample of data, i.e. URL links
        /// </summary>
        /// <param name="list_meetup_event_res">URL event links</param>
        public void GetSampleData(List<MeetupJSONEventResults> list_meetup_event_res)
        {
            foreach (var item in list_meetup_event_res)
            {
                Console.WriteLine(item.Link);
            }
        }

        /// <summary>
        /// Create a list of strings which are later passed to RTM for parsing and establishing RTM tasks
        /// </summary>
        /// <param name="list_meetup_res">List of events, from <c>GetMeetupData</c> method</param>
        /// <seealso cref="GetMeetupData(string URL)"/>
        /// <returns>A list of strings which are pushed to become RTM tasks</returns>
        public List<string> SetMeetupTasks_FinalRTMStringList(List<MeetupJSONEventResults> list_meetup_res)
        {
            foreach (var item in list_meetup_res)
            {
                event_meetup = string.Concat("ID-MeetupRTM: ", item.Id, " ", 
                    DeleteChars(item.Name), " ",
                    ConvertToEUDate(item.Local_date), " ",
                    item.Local_time, " ",
                    item.Link, " ");

                logger.Info(event_meetup);
                MainWindow.SetLoggingMessage_Other(event_meetup);

                rtm_string_tasks_long.Add(event_meetup);
            }

            return rtm_string_tasks_long;
        }

        /// <summary>
        /// If RTM was able to parse strings and create tasks from them, this method recreates final tasks names that are now in RTM itself.
        /// This method does not call any RTM API.
        /// </summary>
        /// <param name="list_meetup_res"></param>
        /// <returns>a list of tasks as they are now stored in RTM</returns>
        public List<string> GetMeetupTasks_FinalRTMStringList(List<MeetupJSONEventResults> list_meetup_res)
        {
            foreach (var item in list_meetup_res)
            {
                event_meetup = string.Concat("ID-MeetupRTM: ", item.Id, " ", DeleteChars(item.Name));
                rtm_string_tasks_short.Add(event_meetup);
            }

            return rtm_string_tasks_short;
        }

        /// <summary>
        /// Prepares a list of events       
        /// </summary>
        /// <param name="list_meetup_res"></param>
        /// <returns>Return a list of venues for events</returns>
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
                    string ev_loc = " Nah,......Event/Venue location is empty at present";
                    logger.Error(e, ev_loc, event_meetup_venue);
                    MainWindow.SetLoggingMessage_Other(ev_loc);
                    event_meetup_venue = "Empty venue";
                }

                list_meetup_venue_res.Add(event_meetup_venue);
            }
            return list_meetup_venue_res;
        }


        /// <summary>
        /// Delete # and @ from event name, plus numbers from name
        /// </summary>
        /// <param name="name">string which contains name of the event</param>
        /// <returns>a cleaned event name</returns>
        public string DeleteChars(string name)
        {
            string new_name = Regex.Replace(name, "[—?@–#&!$%-=]", "", RegexOptions.Compiled);
            string new_name2 = Regex.Replace(new_name, @"[\d-]", "", RegexOptions.Compiled);
            return new_name2;
        }

        /// <summary>
        /// Convert from yyyy-mm-dd to dd-mm-yyyy
        /// </summary>
        /// <param name="local_date">ISO date</param>
        /// <returns>EU date format</returns>
        public string ConvertToEUDate(string local_date)
        {
            var pattern = LocalDateTimePattern.CreateWithInvariantCulture("yyyy-MM-dd");
            var date = pattern.Parse(local_date).Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            logger.Info("converted datetime" + local_date);

            return date;
        }
    }



}
