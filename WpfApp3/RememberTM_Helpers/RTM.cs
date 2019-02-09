using MeetupToRTM.MeetupHelpers;
using MeetupToRTM.MeetupJSONHelpers;
using NLog;
using RememberTheMilkApi.Helpers;
using RememberTheMilkApi.Objects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace MeetupToRTM.RememberTM_Helpers
{
    class RTM
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        public AuthKeys authentication = null;
        public MeetUp meetup_inst = null;

        private List<MeetupJSONEventResults> mu_data = null;
        private List<string> mu_venue = null;
        private List<string> mu_event = null;

        private readonly string random_n;
        private string timeline = string.Empty;
        private string list_id = string.Empty;

        RtmApiResponse created_task = null;
        ObservableCollection<string> ListBoxData { get; set; }
        Random r = new Random();

        /// <summary>
        /// Empty constructor
        /// </summary>
        public RTM()
        {
            random_n = r.Next().ToString();
        }

        /// <summary>
        /// <see href="https://www.rememberthemilk.com/services/api/methods.rtm">RTM Methods API</see>
        /// </summary>
        /// <param name="aka">Passing <c>AuthKeys</c> keys</param>
        public RTM(AuthKeys aka)
        {
            authentication = aka;
            meetup_inst = new MeetUp(aka);
            random_n = r.Next().ToString();
        }

        /// <summary>
        /// Take Auth keys and initiate basic RTM connection by creating or reading authtoken file
        /// </summary>
        /// <param name="aka">Passing <c>AuthKeys</c> keys</param>
        public void InitiateConnection(AuthKeys aka)
        {
            RtmConnectionHelper.InitializeRtmConnection(aka.MyRTMkey, aka.MyRTMsecret);
            MainWindow.SetLoggingMessage_Other("RTM: Initiate Connection using: " + aka.MyRTMkey + " and " + aka.MyRTMsecret);

            string urlRTM = RtmConnectionHelper.GetAuthenticationUrl(RtmConnectionHelper.Permissions.Write);
            MainWindow.SetLoggingMessage_Other("we are inside of InitiateConnection() method: " + urlRTM);

            Process.Start(urlRTM);
            System.Threading.Thread.Sleep(2000);

            RtmApiResponse authResponse = RtmConnectionHelper.GetApiAuthToken();

            logger.Info("authResponse.Auth.Token: -> " + authResponse.Auth.Token);
            MainWindow.SetLoggingMessage_Other(authResponse.Auth.User.FullName + " authResponse.Auth.Token: -> " + authResponse.Auth.Token);

            if (!File.Exists("authtoken.authtoken"))
            {
                // if file does not exist, then create it by writing into it
                CreateAuthFile("authtoken.authtoken", authResponse.Auth.Token);
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
                    } catch(Exception e)
                    {
                        string mes = "checking for the RTM token has failed. Creating new one";
                        MainWindow.SetLoggingMessage_Other(mes);
                        logger.Error(mes);
                        MainWindow.SetLoggingMessage_Other(e.Message);
                    }
                    
                    CreateAuthFile("authtoken.authtoken", authResponse.Auth.Token);

                }
                else
                {
                    string file_text = File.ReadAllText(@"authtoken.authtoken");
                    logger.Info("Our file_text: " + file_text);
                    MainWindow.SetLoggingMessage_Other("Read from existing authtoken file: " + file_text);
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
            using (FileStream fs = new FileStream(name, FileMode.Create, FileAccess.ReadWrite))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
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
        /// Main method where creating of tasks is started, and then called further down the chain.
        /// <param name="mu_data"></param>
        /// <param name="mu_data_strings"></param>
        /// <param name="mu_venue_strings"></param>
        /// <param name="checkbox"></param>
        /// </summary>
        public void SetRTMTasks(List<MeetupJSONEventResults> mu_data, List<string> mu_data_strings, List<string> mu_venue_strings, bool checkbox)
        {
            GetRTMTimeline();

            string msg = "we are creating a new set of task";
            MainWindow.SetLoggingMessage_Other(msg);
            logger.Info(msg);

            mu_event = mu_data_strings;
            this.mu_data = mu_data;
            mu_venue = mu_venue_strings;

            int upcomming_events = mu_data.Count;

            logger.Info("Number of upcomming entries: " + upcomming_events);
            MainWindow.SetLoggingMessage_Other("\n Number of upcomming entries: " + upcomming_events);
            
            if(checkbox == true) {
                // add all even if they already exist
                AddTasks(true);
            } else {
                // only add those which do not exist
                AddTasks(false);
            }
        }

        private string GetRTMTimeline()
        {
            timeline = RtmConnectionHelper.CreateTimeline().TimeLine;
            return timeline;
        }

        /// <summary>
        /// Here we need to incorporate logic for the checkbox and
        /// checking if task already exist, then skip
        /// </summary>
        /// <param name="addAllTasks"></param>
        private void AddTasks(bool addAllTasks)
        {
            try
            {
                foreach (var task_str in mu_event)
                {
                    logger.Info("...............");
                    logger.Info(task_str);
                    logger.Info("...............");

                    MainWindow.SetLoggingMessage_Other(task_str);
                    // get all tasks where name begins with ID-MeetupRTM: 257831299 using GetMeetupTasks_FinalRTMStringList method .... and 

                    created_task = RtmMethodHelper.AddTask(timeline, task_str, parse: "1");
                    int ctionId = created_task.ListCollection.Lists.Count;

                    list_id = created_task.List.Id;

                    string strr = "timeline_id: " + timeline + "\n" + "list_id: " + list_id + "\n" + ctionId + "\n";
                    logger.Info(strr);
                    MainWindow.SetLoggingMessage_Other(strr);
                    GetStoredRTMTasks(list_id, timeline);
                }
            }
            catch (Exception e)
            {
                Console.Write("some error in AddTasks " + e.Message);
                logger.Error(e.Message);
            }
        }

        /// <summary>
        /// TaskSeriesID and ListID are the same !
        /// </summary>
        /// <param name="list_id"></param>
        /// <param name="timeline"></param>
        public void GetStoredRTMTasks(string list_id, string timeline) 
        {
            //result.list.taskseries.task.id
            //string taskseries_id = lj.
            //string taskseries_dk = created_task.TaskSeriesCollection.TaskSeriesList.ToString();
            //System.Threading.Thread.Sleep(5000);
            // + "taskseries_id" + taskseries_id + "\n" + taskseries_dk
            //RtmApiResponse undoResponse = RtmMethodHelper.UndoTransaction(timeline, transactionId);


            RtmApiResponse listResponse = RtmMethodHelper.GetListsList();
            RtmApiResponse taskResponse = RtmMethodHelper.GetTasksList();

            logger.Info("return task id");

            RtmApiTaskSeriesList tsaks = taskResponse.TaskSeriesCollection.TaskSeriesList
                .Where(task => Equals(task.Id, "648052754")).FirstOrDefault();

            //int terte = taskResponse.TaskSeriesCollection.TaskSeriesList.Count();
            RtmApiListObject tssListIds = listResponse.ListCollection.Lists
                .Where(list => Equals(list.Id, list_id))
                .FirstOrDefault();

            RtmApiTaskSeriesList ssslist_id = taskResponse.TaskSeriesCollection.TaskSeriesList.FirstOrDefault();

            IList<RtmApiTaskSeries> tssTasks = taskResponse.TaskSeriesCollection.TaskSeriesList
                //.Where(taskSeriesList => taskSeriesList.TaskSeries.Any() && tssListIds2.Contains(taskSeriesList.Id))
                .SelectMany(taskSeriesList => taskSeriesList.TaskSeries)
                .ToList();

            // are same
            logger.Info(tssListIds.Id);
            logger.Info(ssslist_id.Id);
            logger.Info("list id and task/taskseries id: ");
            // TODO: List all tasks id, then check for their names and if we really need them again

        }
        # region in_development
        //public bool IfTaskAlreadyExists(List<MeetupJSONEventResults> mu_data)
        //{
        //    var mu_our_names = meetup_inst.GetMeetupTasks_FinalRTMStringList(mu_data);

        //    RtmApiResponse listResponse = RtmMethodHelper.GetListsList();
        //    IList<string> tssListIds = listResponse.ListCollection.Lists.Where(list => 
        //    string.Equals(list.Name, task, StringComparison.OrdinalIgnoreCase)).Select(list => 
        //    list.Id).ToList();

        //    foreach (var task in mu_our_names) { 
        //        if (TaskSeries contains task.name)
        //        {
        //            return false;
        //        }
        //        else
        //        // this is new task
        //        {
        //            return true;
        //        }
        //    }
        //}
        #endregion
    }
}
