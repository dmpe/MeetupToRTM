using System;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using MeetupToRTM.MeetupJSONHelpers;
using Newtonsoft.Json;
using NLog;

namespace MeetupToRTM.Meetup_Helpers
{
    class MeetUp_Connect
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        readonly string authURL = "https://secure.meetup.com/oauth2/authorize";
        string accessURL = "https://secure.meetup.com/oauth2/access";
        string redirectURL = "https://dmpe.github.io/MeetupToRTM/";
        string authCompleteURL = string.Empty;
        string accessTokenCompleteURL = string.Empty;
        string error_message = string.Empty;
        public AuthKeys authKeys = null;

        /// <summary>
        /// Basic constructor
        /// </summary>
        public MeetUp_Connect()
        {

        }

        /// <summary>
        /// Create Flurl-based URL which can initiate Authorization flow for MeetUp data
        /// </summary>
        /// <param name="meetupApiKey">MeetUp Key provided by the user in the GUI</param>
        /// <returns>URL which is called for the requesting access token</returns>
        public string PrepareRequestingAuthURL(string meetupApiKey)
        {
            try
            {
                authCompleteURL = authURL.SetQueryParams(new
                {
                    client_id = meetupApiKey,
                    response_type = "code",
                    redirect_uri = redirectURL
                });
            }
            catch (Exception e) when (e is FlurlHttpException || e is null)
            {
                logger.Error(e);
            }
            return authCompleteURL;
        }

        /// <summary>
        /// Having the autherization "code", use it in the URL which returns one-time access token.
        /// Use body from RequestAccessToken method and request JSON response
        /// </summary>
        /// <returns>JSON response which is mapped to the <c>JsonMeetupAuth</c> class</returns>
        public async Task<JsonMeetupAuth> RequestAuthorizationAsync(string meetupApiKey, string meetupApiSecret, string authCode)
        {
            var PostJSONcall = accessURL
                .WithHeaders(new { ContentType = "application/x-www-form-urlencoded" })
                .PostUrlEncodedAsync(new
                {
                    client_id = meetupApiKey,
                    client_secret = meetupApiSecret,
                    grant_type = "authorization_code",
                    redirect_uri = redirectURL,
                    code = authCode
                });
            return await PostJSONcall.ReceiveJson<JsonMeetupAuth>();
        }
    }
}



//    logger.Info(JsonString);

//                if (JsonString.ErrorException != null)
//                {
//                    error_message = "Error retrieving response. Check inner details for more info.";
//                    logger.Error(error_message);
//                    var MeetUpConnectException = new ApplicationException(error_message, JsonString.ErrorException);
//                    throw MeetUpConnectException;
//                }
//JsonMeetupAuth deserializedProduct = JsonConvert.DeserializeObject<JsonMeetupAuth>(JsonString);
//accssToken = deserializedProduct.access_token;

//                logger.Info(accssToken);

//            }
//            catch
//            {
//                // any other exception
//                logger.Error(error_message);
//            }










