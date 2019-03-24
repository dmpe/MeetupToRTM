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
    public class RTM_Meetup_Tasks
    {
        private string meetup_id;
        private string long_description_input;
        private string short_description_title_field;

        public string MeetupID { get => meetup_id; set => meetup_id = value; }
        public string Long_Task_Description { get => long_description_input; set => long_description_input = value; }
        public string Short_Task_Description { get => short_description_title_field; set => short_description_title_field = value; }

        public RTM_Meetup_Tasks(string id, string input_long, string task_field_short)
        {
            this.meetup_id = id;
            this.long_description_input = input_long;
            this.short_description_title_field = task_field_short;
        }

        public RTM_Meetup_Tasks()
        {
        }
    }


    public class MeetUp
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        // Meetup final variables
        // source object
        public static readonly string status = "upcoming";
        public static readonly string scroll = "next_upcoming";
        public static readonly bool sign = true;
        public readonly AuthKeys keys = null;

        List<MeetupJSONEventResults> list_of_meetup_events = null;
        List<RTM_Meetup_Tasks> rtm_string_tasks = new List<RTM_Meetup_Tasks>();

        List<string> list_meetup_venue_res = new List<string>();
        
        public MeetUp()
        {
        }

        public MeetUp(AuthKeys aka)
        {
            keys = aka;
        }

        /// <summary>
        /// Creates Flurl-based URL which can access MeetUp data
        /// </summary>
        /// <param name="meetup_api_key">MeetUp Key provided by the user in the GUI</param>
        /// <returns>URL which is called for the JSON output</returns>
        public string SetMeetupURL(string meetup_api_key)
        {
            var my_url = "https://api.meetup.com"
                .AppendPathSegment("self")
                .AppendPathSegment("events")
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
                list_of_meetup_events = JsonConvert.DeserializeObject<List<MeetupJSONEventResults>>(content);
            }
            catch (JsonException ex)
            {
                logger.Error(ex, "Could not deserialize JSON Object");
            }

            return list_of_meetup_events;
        }

        /// <summary>
        /// If there are zero events, log this information to the user as well
        /// </summary>
        /// <param name="list_meetup_event_res">List of <c>MeetupJSONEventResults</c> events</param>
        public void ValidateMeetupData(List<MeetupJSONEventResults> list_meetup_event_res)
        {
            if (list_meetup_event_res.Count == 0)
            {
                string log = "Meetup: Does not contain any upcomming meetups";
                Console.WriteLine(log);
                logger.Error(log);
                MainWindow.SetLoggingMessage_Other(log);

                MessageBox.Show("No upcomming events have been found in Meetup. Thus the application cannot transfer them to RTM.", "Information Message", MessageBoxButton.OK);                
            }
        }

        /// <summary>
        /// Returns a sample of data, i.e. URL links
        /// </summary>
        /// <param name="list_of_meetup_events_url_links">URL event links</param>
        public void GetSampleData(List<MeetupJSONEventResults> list_of_meetup_events_url_links)
        {
            foreach (var item in list_of_meetup_events_url_links)
            {
                Console.WriteLine(item.Link);
            }
        }

        /// <summary>
        /// Create a list of "strings" (tasks) which are later passed to RTM for parsing and establishing RTM tasks
        /// </summary>
        /// <param name="list_meetup_results">List of events, from <c>GetMeetupData</c> method</param>
        /// <seealso cref="GetMeetupData(string URL)"/>
        /// <returns>A list of strings/events which are pushed to become RTM tasks</returns>
        public List<RTM_Meetup_Tasks> Create_RTM_Tasks_From_Events(List<MeetupJSONEventResults> list_meetup_results)
        {
            foreach (var item in list_meetup_results)
            {
                string event_meetup_long = string.Concat("ID-MeetupRTM: ", item.Id, " ", 
                    DeleteChars(item.Name), " ",
                    ConvertToEUDate(item.Local_date), " ",
                    item.Local_time, " ",
                    item.Link, " ");
                string event_meetup_short = string.Concat("ID-MeetupRTM: ", item.Id, " ", DeleteChars(item.Name));

                logger.Info(event_meetup_long);
                MainWindow.SetLoggingMessage_Other(event_meetup_long);

                RTM_Meetup_Tasks rtm_task = new RTM_Meetup_Tasks(item.Id, event_meetup_long, event_meetup_short);
                rtm_string_tasks.Add(rtm_task);
            }

            return rtm_string_tasks;
        }

        /// <summary>
        /// Prepares a list of venues for events       
        /// </summary>
        /// <param name="list_meetup_res"></param>
        /// <returns>Return a list of venues for events</returns>
        public List<string> PrepareMeetupTaskList_Venue_ToString(List<MeetupJSONEventResults> list_meetup_res)
        {
            foreach (var item in list_meetup_res)
            {
                string event_meetup_venue = string.Empty;
                try
                {
                    event_meetup_venue = string.Concat("\n Name: ", item.Venue.Name, "\n",
                        "Address: ", item.Venue.Address_1, "\n",
                        "City: ", item.Venue.City, "\n",
                        "Lat + Lon: ", item.Venue.Lat, " ", item.Venue.Lon);
                }
                catch (NullReferenceException e)
                {
                    string ev_loc = "Meetup: Nah,......Event/Venue location is empty at present";
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
            new_name = Regex.Replace(new_name, @"[\d-]", "", RegexOptions.Compiled);
            return new_name;
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
            logger.Info("converted datetime: " + local_date);

            return date;
        }
    }



}
