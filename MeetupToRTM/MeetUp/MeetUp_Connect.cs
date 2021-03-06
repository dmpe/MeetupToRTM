﻿using System;
using Flurl;
using Flurl.Http;
using Newtonsoft.Json;
using NLog;
using RestSharp;

namespace RememberTheMeetup.MeetUp
{
    class MeetUp_Connect
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly string authURL = "https://secure.meetup.com/oauth2/authorize";
        private readonly string accessURL = "https://secure.meetup.com/oauth2/access";
        private readonly string redirectURL = "https://dmpe.github.io/MeetupToRTM/";
        string authCompleteURL = string.Empty;
        public AuthKeys authKeys = null;
        public JsonMeetupAuth jma = null;

        /// <summary>
        /// Basic Meetup Connect constructor
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
        /// Having the authorization "code", use it in the URL which returns one-time access token.
        /// Use body from RequestAccessToken method and request JSON response
        /// </summary>
        /// <returns>JSON response which is mapped to the <c>JsonMeetupAuth</c> class</returns>
        public JsonMeetupAuth RequestAuthorizationAsync(string meetupApiKey, string meetupApiSecret, string authCode)
        {
            var client = new RestClient
            {
                BaseUrl = new Uri(accessURL)
            };
            var request = new RestRequest
            {
                Method = Method.POST
            };
            request.AddHeader("ContentType", "application/x-www-form-urlencoded");
            request.AddParameter("client_id", meetupApiKey);
            request.AddParameter("client_secret", meetupApiSecret);
            request.AddParameter("grant_type", "authorization_code");
            request.AddParameter("redirect_uri", redirectURL);
            request.AddParameter("code", authCode);

            logger.Info(client.BuildUri(request));
            var response = client.Execute(request);
            try
            {
                var content = response.Content;
                Console.WriteLine(content);
                jma = JsonConvert.DeserializeObject<JsonMeetupAuth>(content);
            }
            catch (JsonException ex)
            {
                // any other exception
                logger.Error(ex.Message);
            }

            return jma;
        }
    }
}
