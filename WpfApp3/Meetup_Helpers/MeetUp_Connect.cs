using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using NLog;
using RestSharp;

namespace MeetupToRTM.Meetup_Helpers
{
    class MeetUp_Connect
    {
        string auth = "https://secure.meetup.com/oauth2/authorize";
        string acc_token = "https://secure.meetup.com/oauth2/access";
        string auth_complete_url = string.Empty;
        string acc_token_complete_url = string.Empty;
        public string error_message = string.Empty;

        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        public string AccessToken { get; set; }

        public MeetUp_Connect()
        {

        }

        /// <summary>
        /// Creates Flurl-based URL which can access MeetUp data
        /// </summary>
        /// <param name="meetup_api_key">MeetUp Key provided by the user in the GUI</param>
        /// <returns>URL which is called for the JSON output</returns>
        public string SetMeetupFirstURL(string meetup_api_key)
        {
            try
            {
                auth_complete_url = auth.SetQueryParams(new
                {
                    client_id = meetup_api_key,
                    response_type = "code",
                    redirect_uri = "https://github.com/dmpe/MeetupToRTM"

                });
            } catch (Exception e) when (e is FlurlHttpException || e is Exception)
            {
                logger.Error(e);
            }
            return auth_complete_url;
        }

        public string RequestAuthorization(string url)
        {
            //string code = string.Empty;
            string code = "code string";
            var client = new RestClient
            {
                BaseUrl = new Uri(url)
            };
            var request = new RestRequest();

            try
            {
                IRestResponse response = client.Execute(request);
                if (response.ErrorException != null)
                {
                    error_message = "Error retrieving response. Check inner details for more info.";
                    logger.Error(error_message);
                    var MeetUpConnectException = new ApplicationException(error_message, response.ErrorException);
                    throw MeetUpConnectException;
                }
                var content = response.Content;
                logger.Info(content.ToString());
            } catch
            {
                // any other exception
                logger.Error(error_message);
            }

            return code;
        }

        public string RequestAccessToken(string meetup_api_key, string meetup_api_secret_key, string auth_code)
        {
            try
            {
                acc_token_complete_url = acc_token.SetQueryParams(new
                {
                    client_id = meetup_api_key,
                    client_secret = meetup_api_secret_key,
                    grant_type = "authorization_code",
                    code = auth_code,
                });
            }
            catch (Exception e) when (e is FlurlHttpException || e is Exception)
            {
                logger.Error(e);
            }
            return acc_token_complete_url;
        }


    }
}
