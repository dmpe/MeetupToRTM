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

namespace MeetupToRTM.RememberTM_Helpers
{
    class RTM
    {
        public AuthKeys authentication = null;
        public MeetUp meetup_inst = null;
        public List<MeetupJSONResults> mu_data = null;

        /// <summary>
        /// Empty constructor
        /// </summary>
        public RTM()
        {

        }
        /// <summary>
        /// https://www.rememberthemilk.com/services/api/methods.rtm
        /// </summary>
        /// <param name="aka"></param>
        public RTM(AuthKeys aka)
        {
            authentication = aka;

        }


        /// <summary>
        /// Take Auth keys and innitiate basic RTM connection
        /// </summary>
        /// <param name="aka"></param>
        public void InnitiateConnection(AuthKeys aka)
        {
            Console.WriteLine("Initiate Connection using:" + aka.MyRTMkey + " and " + aka.MyRTMsecret);
            RtmConnectionHelper.InitializeRtmConnection(aka.MyRTMkey, aka.MyRTMsecret);

            string urlRTM = RtmConnectionHelper.GetAuthenticationUrl(RtmConnectionHelper.Permissions.Write);
            Console.WriteLine("we are inside of OpenAuthentication() method: " + urlRTM);
            Process.Start(urlRTM);
            System.Threading.Thread.Sleep(2000);

            RtmApiResponse authResponse = RtmConnectionHelper.GetApiAuthToken();

            Console.WriteLine(authResponse.Auth.User.FullName);
            Console.WriteLine("authResponse.Auth.Token: -> " + authResponse.Auth.Token);

            using (FileStream fs = new FileStream("authtoken.authtoken", FileMode.Create, FileAccess.ReadWrite))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
                {
                    sw.Write(authResponse.Auth.Token);
                    sw.Close();
                    fs.Close();
                }
            }
            RtmConnectionHelper.SetApiAuthToken(authResponse.Auth.Token);
            RtmConnectionHelper.CheckApiAuthToken();
        }

        /// <summary>
        /// https://www.rememberthemilk.com/services/api/methods/rtm.tasks.add.rtm
        /// </summary>
        public void CreateRTMTask(MeetUp meetup_inst, List<MeetupJSONResults> mu_data)
        {
            Console.WriteLine("we are creating a new task");
            this.meetup_inst = meetup_inst;
            this.mu_data = mu_data;

            //RtmApiResponse taskResponse = RtmMethodHelper.GetTasksList();
            //RtmApiResponse list_response = RtmMethodHelper.GetListsList();

            //Console.WriteLine(taskResponse.ToString(), list_response.ToString());
            try
            {
                // here error
                string timeline = RtmConnectionHelper.CreateTimeline().TimeLine;
                Console.Write(timeline);
            } catch (Exception e)
            {
                Console.Write(e.Message);
            }



            //RtmApiResponse created_task = RtmMethodHelper.AddTask(timeline);
        }
    }
}
