using System;
using Flurl;
using Flurl.Http;
using Newtonsoft.Json;
using NLog;
using RestSharp;

namespace MeetupToRTM.Meetup_Helpers
{
    class JsonMeetupAuth : MeetUp_Connect
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public string expires_in { get; set; }
        public string refresh_token { get; set; }
    }
    class MeetUp_Connect
    {
        string auth = "https://secure.meetup.com/oauth2/authorize";
        string acc_token = "https://secure.meetup.com/oauth2/access";
        string redirect_url = "https://dmpe.github.io/MeetupToRTM/"; 
        string auth_complete_url = string.Empty;
        string acc_token_complete_url = string.Empty;
        string error_message = string.Empty;
        public AuthKeys authKeys = null;
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        public MeetUp_Connect()
        {

        }

        /// <summary>
        /// Creates Flurl-based URL which can access MeetUp data
        /// </summary>
        /// <param name="meetup_api_key">MeetUp Key provided by the user in the GUI</param>
        /// <returns>URL which is called for the JSON output</returns>
        public string PrepareRequestingAuthURL(string meetup_api_key)
        {
            try
            {
                auth_complete_url = auth.SetQueryParams(new
                {
                    client_id = meetup_api_key,
                    response_type = "code",
                    redirect_uri = redirect_url
                });
            } catch (Exception e) when (e is FlurlHttpException || e is null)
            {
                logger.Error(e);
            }
            return auth_complete_url;
        }

        public string RequestAccessToken(string meetup_api_key, string meetup_api_secret_key, string auth_code)
        {
            string request_body = "".SetQueryParams(new
            {
                client_id = meetup_api_key,
                client_secret = meetup_api_secret_key,
                grant_type = "authorization_code",
                redirect_uri = redirect_url,
                code = auth_code,
            });
            logger.Info(request_body.Remove(0, 1));
            return request_body.Remove(0, 1);
        }

        public string RequestAuthorization(string body_url)
        {
            string codeToken = string.Empty;
            try
            {
                dynamic JsonString = acc_token.WithHeaders(new { ContentType = "application/x-www-form-urlencoded" }).PostUrlEncodedAsync(body_url).ReceiveJson();
                if (JsonString.ErrorException != null)
                {
                    error_message = "Error retrieving response. Check inner details for more info.";
                    logger.Error(error_message);
                    var MeetUpConnectException = new ApplicationException(error_message, JsonString.ErrorException);
                    throw MeetUpConnectException;
                }
                JsonMeetupAuth deserializedProduct = JsonConvert.DeserializeObject<JsonMeetupAuth>(JsonString);
                codeToken = deserializedProduct.access_token;

                logger.Info(codeToken);

            }
            catch
            {
                // any other exception
                logger.Error(error_message);
            }
            
            return codeToken;
        }
    }
}
