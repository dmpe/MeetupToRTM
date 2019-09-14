using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using MeetupToRTM;
using NLog;
using RememberTheMeetup.MeetUp;
using RememberTheMilkApi.Helpers;
using RememberTheMilkApi.Objects;

namespace RememberTheMeetup.RTM
{
    class RTM
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        public AuthKeys authKeys = null;

        private List<MeetupJSONEvents> meetupEventData = null;
        private List<string> meetupVenueData = null;
        private List<RtmMeetupTasks> rtmMeetupTasksData = null;

        private string timeline = string.Empty;
        private string listId = string.Empty;
        private string target_rtm_meetup_list = string.Empty;


        /// <summary>
        /// Empty constructor generating a random number (string)
        /// </summary>
        public RTM()
        {
        }

        /// <summary>
        /// RTM constructor which takes <c>AuthKeys</c> keys
        /// </summary>
        /// <see href="https://www.rememberthemilk.com/services/api/methods.rtm">RTM Methods API</see>
        /// <param name="authKeys">Passing <c>AuthKeys</c> keys</param>
        public RTM(AuthKeys authKs)
        {
            this.authKeys = authKs;
        }

        /// <summary>
        /// Take Auth keys and initiate basic RTM connection by creating or reading authtoken file
        /// </summary>
        /// <param name="authKeys">Passing <c>AuthKeys</c> keys</param>
        public void InitiateConnection(AuthKeys authKeys)
        {
            RtmConnectionHelper.InitializeRtmConnection(authKeys.MyRTMkey, authKeys.MyRTMsecret);
            MainWindow.SetLoggingMessage_Other("RTM: Initiate Connection using: " + authKeys.MyRTMkey + " and " + authKeys.MyRTMsecret);

            string rtmUriWithWritePerms = RtmConnectionHelper.GetAuthenticationUrl(RtmConnectionHelper.Permissions.Write);
            MainWindow.SetLoggingMessage_Other("RTM: we are inside of InitiateConnection() method: " + rtmUriWithWritePerms);

            Process.Start(rtmUriWithWritePerms);
            System.Threading.Thread.Sleep(2000);

            RtmApiResponse authResponseToken = RtmConnectionHelper.GetApiAuthToken();

            logger.Info("RTM: authResponseToken: -> " + authResponseToken.Auth.Token);
            MainWindow.SetLoggingMessage_Other("RTM: " + authResponseToken.Auth.User.FullName + 
                " token: -> " + authResponseToken.Auth.Token);

            if (!File.Exists("authtoken.authtoken"))
            {
                // if file does not exist, then create it by writing into it
                CreateAuthFile("authtoken.authtoken", authResponseToken.Auth.Token);
            }
            else
            {
                // if file does exist, then check for its creation time
                DateTime file_created = File.GetLastWriteTime("authtoken.authtoken");
                long file_len = new FileInfo("authtoken.authtoken").Length;

                // if that time is older than 1 day, check for token's validity
                if ((DateTime.Now - file_created).TotalHours >= 25 || file_len < 10)
                {
                    try
                    {
                        RtmConnectionHelper.CheckApiAuthToken();
                    }
                    catch (Exception e)
                    {
                        string mes = "RTM: checking for the RTM token has failed. Creating new one";
                        MainWindow.SetLoggingMessage_Other(mes);
                        logger.Error(mes);
                        MainWindow.SetLoggingMessage_Other(e.Message);
                    }

                    CreateAuthFile("authtoken.authtoken", authResponseToken.Auth.Token);
                }
                else
                {
                    string file_text = File.ReadAllText(@"authtoken.authtoken");
                    string msg = "RTM: Our file_text to read from: ";
                    logger.Info(msg + file_text);
                    MainWindow.SetLoggingMessage_Other(msg + file_text);
                    RtmConnectionHelper.SetApiAuthToken(file_text);
                }
            }
        }

        /// <summary>
        /// Create Auth File and set RTM token 
        /// </summary>
        /// <param name="name">File name</param>
        /// <param name="token">Token from RTM</param>
        public void CreateAuthFile(string name, string token)
        {
            using (var fs = new FileStream(name, FileMode.Create, FileAccess.ReadWrite))
            {
                using (var sw = new StreamWriter(fs, Encoding.UTF8))
                {
                    sw.Write(token);
                    sw.Close();
                    fs.Close();
                }
            }
            RtmConnectionHelper.SetApiAuthToken(token);
        }

        /// <summary>
        /// <see href="https://www.rememberthemilk.com/services/api/methods/rtm.tasks.add.rtm"></see>
        /// Main method where tasks are created (i.e. the process is started), and then called further down the chain.
        /// <param name="mu_data"></param>
        /// <param name="mu_data_strings"></param>
        /// <param name="mu_venue_strings"></param>
        /// <param name="checkbox"></param>
        /// </summary>
        public void SetRTMTasks(List<MeetupJSONEvents> mu_data, List<RtmMeetupTasks> mu_data_strings,
                                List<string> mu_venue_strings, bool checkbox)
        {
            string msg = "RTM: we are creating a new set of tasks";
            MainWindow.SetLoggingMessage_Other(msg);
            logger.Info(msg);

            rtmMeetupTasksData = mu_data_strings;
            this.meetupEventData = mu_data;
            meetupVenueData = mu_venue_strings;

            int upcomming_events = mu_data.Count;
            string msgUpcommingEvents = "RTM: Number of upcomming entries: " + upcomming_events;
            logger.Info(msgUpcommingEvents);
            MainWindow.SetLoggingMessage_Other("\n" + msgUpcommingEvents);

            if (checkbox)
            {
                // add all even if they already exist
                AddTasks(true);
            }
            else
            {
                // only add those which do not exist
                AddTasks(false);
            }
        }

        /// <summary>
        /// Here we need to incorporate logic for the checkbox and
        /// checking if task already exist, then skip
        /// </summary>
        /// <see href="https://stackoverflow.com/questions/9524681/find-if-lista-contains-any-elements-not-in-listb"/>
        /// <param name="addAllTasks">by default it should be true</param>
        private void AddTasks(bool addAllTasks)
        {
            timeline = GetRTMTimeline();

            if (addAllTasks == true)
            {
                try
                {
                    foreach (var task_str in rtmMeetupTasksData)
                    {
                        LogTasks(task_str.Long_Task_Description);
                        var created_task = RtmMethodHelper.AddTask(timeline, task_str.Long_Task_Description, parse: "1");

                        listId = created_task.List.Id;

                        string strr = "timeline_id: " + timeline + "\n" + "listId: " + listId + "\n";
                        logger.Info(strr);
                        MainWindow.SetLoggingMessage_Other(strr);
                    }
                    GetStoredRTMTasks(listId, timeline);

                }
                catch (Exception e)
                {
                    Console.Write("RTM: some error in AddTasks " + e.Message);
                    logger.Error(e.Message);
                }

            } else
            {
                // see https://github.com/dmpe/MeetupToRTM/issues/2
                // get all tasks where name begins with ID-MeetupRTM: 257831299 
                // using GetMeetupTasks_FinalRTMStringList method .... and 

                // compare two lists
                // if event id exists in the list, then do not add
                // if does not then 
                // created_task = RtmMethodHelper.AddTask(timeline, task_str, parse: "1");
            }
        }

        /// <summary>
        /// TaskSeriesID and ListID are the same !
        /// </summary>
        /// <param name="list_id"></param>
        /// <param name="timeline"></param>
        public void GetStoredRTMTasks(string list_id, string timeline)
        {
            RtmApiResponse listResponse = RtmMethodHelper.GetListsList();
            RtmApiResponse taskResponse = RtmMethodHelper.GetTasksList();

            //return all lists - 7 currently
            int tssListIds2 = listResponse.ListCollection.Lists.Count();
            logger.Info("number of RTM lists: " + tssListIds2);

            RtmApiListObject[] lst_array = listResponse.ListCollection.Lists.ToArray();

            foreach (var list_name in lst_array)
            {
                logger.Info(list_name.Name);
                if (list_name.Name == "ID-MeetupRTM")
                {
                    target_rtm_meetup_list = list_name.Id;
                }
            }

            logger.Info("target_rtm_meetup_list: -->" + target_rtm_meetup_list);

            var list_res = RtmMethodHelper.GetTasksList(target_rtm_meetup_list);
            var coun = list_res.TaskSeriesCollection.TaskSeriesList.Select(x => x.TaskSeries.Count()).Sum();
            //var count = list_res.TaskSeriesCollection.TaskSeriesList.SelectMany(a => a.TaskSeries);
            logger.Info("count of tasks in the list: -->" + coun + ".......... " + "--------------");

            var ssslist_id = taskResponse.TaskSeriesCollection.TaskSeriesList.FirstOrDefault();

            logger.Info("list id and task/taskseries id: " + ssslist_id.Id);
            // TODO: List all tasks id, then check for their names and if we really need them again

        }

        /// <summary>
        /// Returns a newly created RTM timeline
        /// </summary>
        /// <returns>a new timeline</returns>
        private string GetRTMTimeline()
        {
            timeline = RtmConnectionHelper.CreateTimeline().TimeLine;
            return timeline;
        }

        /// <summary>
        /// Log information about tasks
        /// </summary>
        /// <param name="task_str"></param>
        private void LogTasks(string taskStatus)
        {
            logger.Info("...............");
            logger.Info(taskStatus);
            logger.Info("...............");
            MainWindow.SetLoggingMessage_Other(taskStatus);
        }
    }
}
