using System;
using MeetupToRTM.MeetupHelpers;
using System.Diagnostics;
using RememberTheMilkApi.Helpers;
using RememberTheMilkApi.Objects;
using System.ComponentModel;
using System.Windows;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.ObjectModel;
using MeetupToRTM.Meetup_Helpers;

namespace MeetupToRTM.RememberTM_Helpers
{
    class RTM
    {
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
        /// Take Auth keys and initiate basic RTM connection
        /// </summary>
        /// <param name="aka">Passing <c>AuthKeys</c> keys</param>
        public void InitiateConnection(AuthKeys aka)
        {
            RtmConnectionHelper.InitializeRtmConnection(aka.MyRTMkey, aka.MyRTMsecret);
            MainWindow.Handle_Other("RTM: Initiate Connection using: " + aka.MyRTMkey + " and " + aka.MyRTMsecret);

            string urlRTM = RtmConnectionHelper.GetAuthenticationUrl(RtmConnectionHelper.Permissions.Write);
            MainWindow.Handle_Other("we are inside of InitiateConnection() method: " + urlRTM);

            Process.Start(urlRTM);
            System.Threading.Thread.Sleep(2000);

            RtmApiResponse authResponse = RtmConnectionHelper.GetApiAuthToken();

            //Console.WriteLine(authResponse.Auth.User.FullName);
            //Console.WriteLine("authResponse.Auth.Token: -> " + authResponse.Auth.Token);
            MainWindow.Handle_Other(authResponse.Auth.User.FullName + " authResponse.Auth.Token: -> " + authResponse.Auth.Token);

            string auth_token = string.Empty;

            if (!File.Exists("authtoken.authtoken"))
            {
                // if file does not exist, then create it by writing into it
                CreateAuthFile("authtoken.authtoken", authResponse.Auth.Token);
            }
            else
            {
                // if file does exist, then check for its creation time
                DateTime file_created = File.GetLastWriteTime("authtoken.authtoken");
                long file_len = new System.IO.FileInfo(@"c:\Users\jm\Documents\Visual Studio 2017\Projects\WpfApp3\WpfApp3\bin\x86\Release\authtoken.authtoken").Length;

                // if that time is older than 1 day, check for its validity
                if ((DateTime.Now - file_created).TotalHours >= 25 || file_len < 10)
                {
                    try
                    {
                        RtmConnectionHelper.CheckApiAuthToken();
                    } catch(Exception e)
                    {
                        MainWindow.Handle_Other("checking for the RTM token has failed. Creating new one");
                        MainWindow.Handle_Other(e.Message);
                    }
                    
                    CreateAuthFile("authtoken.authtoken", authResponse.Auth.Token);

                }
                else
                {
                    string file_text = File.ReadAllText(@"authtoken.authtoken");
                    Console.WriteLine("Our file_text: " + file_text);
                    MainWindow.Handle_Other("Read from existing authtoken file: " + file_text);
                    RtmConnectionHelper.SetApiAuthToken(file_text);
                }

            }
        }

        /// <summary>
        /// Create Auth File and set RTM token 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="token"></param>
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
        /// <param name="mu_data"></param>
        /// <param name="mu_data_strings"></param>
        /// <param name="mu_venue_strings"></param>
        /// <param name="checkbox"></param>
        /// </summary>
        public void CreateRTMTasks(List<MeetupJSONEventResults> mu_data, List<string> mu_data_strings, List<string> mu_venue_strings, bool checkbox)
        {
            MainWindow.Handle_Other("we are creating a new set of task");
            //Console.WriteLine("we are creating a new set of task");
            mu_event = mu_data_strings;
            this.mu_data = mu_data;
            mu_venue = mu_venue_strings;

            RtmApiResponse listResponse = RtmMethodHelper.GetListsList();

            int upcomming_events = mu_data.Count;

            //Console.WriteLine("Number of upcomming entries: " + upcomming_events);
            MainWindow.Handle_Other("Number of upcomming entries: " + upcomming_events);
            
            if(checkbox == true) {
                // only add those which do not exist
                AddTasks(false);
            } else {
                // add all even if they exist
                AddTasks(true);
            }
        }

        /// <summary>
        /// Here we need to incorporate logic for the checkbox and
        /// checking if task already exist, then skip
        /// </summary>
        /// <param name="addAllTasks"></param>
        public void AddTasks(bool addAllTasks)
        {
            try
            {
                timeline = RtmConnectionHelper.CreateTimeline().TimeLine;
                foreach (var task_string in mu_event)
                {

                    Console.WriteLine("...............");
                    Console.WriteLine(task_string);
                    Console.WriteLine("...............");

                    MainWindow.Handle_Other(task_string);
                    // get all tasks where name begins .... and 

                    created_task = RtmMethodHelper.AddTask(timeline, task_string, parse: "1");
                    
                    list_id = created_task.List.Id;
                    //string taskseries_id = lj.
                    //string taskseries_dk = created_task.TaskSeriesCollection.TaskSeriesList.ToString();
                    //System.Threading.Thread.Sleep(5000);

                    //int ctionId = created_task.ListCollection.Lists.Count;
                    Console.WriteLine("timeline_id: " + timeline + "\n" + "list_id: " + list_id + "\n");
                    MainWindow.Handle_Other("timeline_id: " + timeline + "\n" + "list_id: " + list_id + "\n");
                    // + "taskseries_id" + taskseries_id + "\n" + taskseries_dk
                    //RtmApiResponse undoResponse = RtmMethodHelper.UndoTransaction(timeline, transactionId);


                }

                ReturnTaskId(list_id, timeline);
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
        }

        /// <summary>
        /// TaskSeriesID and ListID are the same !
        /// </summary>
        /// <param name="list_id"></param>
        /// <param name="timeline"></param>
        public void ReturnTaskId(string list_id, string timeline) 
        {
            //result.list.taskseries.task.id
            RtmApiResponse listResponse = RtmMethodHelper.GetListsList();
            RtmApiResponse taskResponse = RtmMethodHelper.GetTasksList();

            MainWindow.Handle_Other("return task id");

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
            Console.WriteLine(tssListIds.Id);
            Console.WriteLine(ssslist_id.Id);
            MainWindow.Handle_Other("list id and task/taskseries id: " + tsaks.Id + " " + tssListIds.Id + " " + ssslist_id.Id + " " + tssTasks.ToString());
            // TODO: List all tasks id, then check for their names and if we really need them again

        }
        # region in_development
        //public bool IfTaskAlreadyExists(List<MeetupJSONEventResults> mu_data)
        //{
        //    var mu_our_names = meetup_inst.PrepareMeetupTaskList_RTM_ToString(mu_data);

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
