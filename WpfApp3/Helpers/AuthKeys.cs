namespace MeetupToRTM
{
    public class AuthKeys
    {

        /// <summary>
        /// Empty constructor
        /// </summary>
        public AuthKeys()
        {

        }

        /// <summary>
        /// Set user provided API Keys
        /// </summary>
        /// <param name="myRTMkey"></param>
        /// <param name="myRTMsecret"></param>
        /// <param name="myMeetupKey"></param>
        public AuthKeys(string myRTMkey, string myRTMsecret, string myMeetupKey)
        {
            MyRTMkey = myRTMkey;
            MyRTMsecret = myRTMsecret;
            MyMeetupKey = myMeetupKey;
        }

        /// <summary>
        /// Get and Set Method
        /// </summary>
        public string MyRTMkey { get; set; }

        /// <summary>
        /// Get and Set Method
        /// </summary>
        public string MyRTMsecret { get; set; }

        /// <summary>
        /// Get and Set Method
        /// </summary>
        public string MyMeetupKey { get; set; }
    }
}
