namespace RememberTheMeetup
{
    /// <summary>
    /// Class used for storing authentication keys.
    /// </summary>
    public class AuthKeys
    {
        /// <summary>
        /// Gets or Sets RTM key
        /// </summary>
        public string MyRTMkey { get; set; }

        /// <summary>
        /// Gets or Sets RTM secret
        /// </summary>
        public string MyRTMsecret { get; set; }

        /// <summary>
        /// Gets or Sets Meetup key
        /// </summary>
        public string MyMeetupKey { get; set; }

        /// <summary>
        /// Gets or Sets Meetup Secret
        /// </summary>
        public string MyMeetupKeySecret { get; set; }

        /// <summary>
        /// Gets or Sets Meetup Auth Token
        /// </summary>
        public string MyMeetupToken { get; set; }

        /// <summary>
        /// Gets or Sets Meetup "code" (OAuth2)
        /// </summary>
        public string MyMeetupCode { get; set; }

        /// <summary>
        /// Empty constructor
        /// </summary>
        public AuthKeys()
        {
        }

        /// <summary>
        /// Constructor initiating major RTM and Meetup keys
        /// </summary>
        /// <param name="myRTMkey"></param>
        /// <param name="myRTMsecret"></param>
        /// <param name="myMeetupKey"></param>
        /// <param name="myMeetupSecretKey"></param>
        public AuthKeys(string myRTMkey, string myRTMsecret, string myMeetupKey, string myMeetupSecretKey)
        {
            MyRTMkey = myRTMkey;
            MyRTMsecret = myRTMsecret;
            MyMeetupKey = myMeetupKey;
            MyMeetupKeySecret = myMeetupSecretKey;
        }

        /// <summary>
        /// Constructor initiating all keys
        /// </summary>
        /// <param name="myRTMkey"></param>
        /// <param name="myRTMsecret"></param>
        /// <param name="myMeetupKey"></param>
        /// <param name="myMeetupKeySecret"></param>
        /// <param name="myMeetupToken"></param>
        /// <param name="myMeetupCode"></param>
        public AuthKeys(string myRTMkey, string myRTMsecret, string myMeetupKey, string myMeetupKeySecret, 
                        string myMeetupToken, string myMeetupCode)
        {
            MyRTMkey = myRTMkey;
            MyRTMsecret = myRTMsecret;
            MyMeetupKey = myMeetupKey;
            MyMeetupKeySecret = myMeetupKeySecret;
            MyMeetupToken = myMeetupToken;
            MyMeetupCode = myMeetupCode;
        }
    }
}
