using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetupToRTM
{
    public class AuthKeys
    {
        // RTM final variables
        // source objects with "one way to source"
        private string myRTMkey;
        private string myRTMsecret;
        private string myMeetupKey;

        public AuthKeys()
        {
        }

        public AuthKeys(string myRTMkey, string myRTMsecret, string myMeetupKey)
        {
            this.myRTMkey = myRTMkey;
            this.myRTMsecret = myRTMsecret;
            this.myMeetupKey = myMeetupKey;
        }

        public string MyRTMkey
        {
            get => myRTMkey;
            set => myRTMkey = value;
        }
        public string MyRTMsecret
        {
            get => myRTMsecret;
            set => myRTMsecret = value;
        }
        public string MyMeetupKey
        {
            get => myMeetupKey;
            set => myMeetupKey = value;
        }
    }
}
