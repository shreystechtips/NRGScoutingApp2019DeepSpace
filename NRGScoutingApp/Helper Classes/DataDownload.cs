using System;
using RestSharp;
using System.Collections.Generic;
using Xamarin.Essentials;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace NRGScoutingApp
{
    public class DataDownload
    {
        private static RestClient client = new RestClient(ConstantVars.SCOUTING_API_SITE);

        public static void populateAllData()
        {
            DataDownload.populateTeamList(Preferences.Get(ConstantVars.TEAM_LIST_STORAGE, "[]"), App.teamsList);
            DataDownload.populateEventList(Preferences.Get(ConstantVars.EVENT_LIST_STORAGE, "[]"), App.eventsList);
            DataDownload.populateMatchList(Preferences.Get(ConstantVars.COMPETITION_MATCHES_LIST, "[]"),App.matchesList, Preferences.Get(ConstantVars.CURRENT_EVENT_NAME,""));
        }

        public static void initClient()
        {
            client.AddDefaultHeader("API-Key", HideAPIKey.SCOUTING_API_KEY);
            client.AddDefaultHeader("Content-Type", "application/json");
        }

        /// <summary>
        /// populates a given int,String Dictionary with a given teamList Json
        /// </summary>
        /// <param name="json">the json string of teams</param>
        /// <param name="list">list object to populate</param>
        public static void populateTeamList(string json, Dictionary<int, String> list)
        {
            Dictionary<int, String> temp = new Dictionary<int, String>(list);
            try
            {
                JArray repsonse = JArray.Parse(json);
                list.Clear();
                foreach (var s in repsonse)
                {
                    JObject v = s.ToObject<JObject>();
                    list.Add((int)v["team_number"], (String)v["nickname"]);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                list = temp;
            }
            //if(list.Count <= 500)
            //{
            //    list = temp;
            //}
        }

        /// <summary>
        /// refreshes list of teams from NRG API
        /// </summary>
        /// <returns></returns>
        public static void refreshTeams()
        {
            try
            {
                string response;
                do {
                RestRequest request = new RestRequest("/teams");
                JObject data = new JObject();
                data.Add(new JProperty("year", ConstantVars.APP_YEAR));
                request.AddJsonBody(data.ToString());
                response = client.Post(request).Content;
               } while (String.IsNullOrEmpty(response));

                // POPULATE data points
                populateTeamList(response, App.teamsList);
                Preferences.Set(ConstantVars.TEAM_LIST_STORAGE, response);
            }
            catch (Exception ex)
            {
                throw new Exception("Incorrect API Key or server URL. Please contact IT." + ex.ToString());
            }

        }

        public static void populateMatchList(String response, JObject data, string eventKey)
        {
            try
            {
                JObject temp = JObject.Parse(response);
                if (temp != null && temp.Count > 0)
                {
                    data[eventKey] = temp;
                }

            } catch
            {
                data = new JObject();
            }
        }

        public static void getEventMatches(String eventKey)
        {
            try
            {
                string response;
                do
                {
                    RestRequest request = new RestRequest("/event/matches");
                    JObject data = new JObject();
                    data.Add(new JProperty("event_key", eventKey));
                    data.Add(new JProperty("comp_level", "qm"));
                    data.Add(new JProperty("uses_sets", false));
                    request.AddJsonBody(data.ToString());
                    response = client.Post(request).Content;
                } while (String.IsNullOrEmpty(response));

                // POPULATE data points
                populateMatchList(response, App.matchesList, eventKey);
                Preferences.Set(ConstantVars.TEAM_LIST_STORAGE, response);
            }
            catch (Exception ex)
            {
                throw new Exception("Please contact IT. Match not exist or server error" + ex.ToString());
            }
        }


        public static void populateEventList(string json, Dictionary<string,string> list)
        {
            Debug.WriteLine(json);
            Dictionary<string, string> temp = new Dictionary<string, string>(list);
            JArray repsonse = JArray.Parse(json);
            list.Clear();
            foreach (var s in repsonse)
            {
                JObject v = s.ToObject<JObject>();
                list.Add((string)v["key"], (string)v["name"]);
            }
            if (list.Count < temp.Count)
            {
                list = temp;
            }
        }

        /// <summary>
        /// Update the list of competition names, mainly for AddCompetition page, pulling from Shrey's server
        /// </summary>
        public static void refreshEvents()
        {

            try
            {
                string response;
                do
                {
                    RestRequest request = new RestRequest("/events");
                    JObject data = new JObject();
                    data.Add(new JProperty("year", ConstantVars.APP_YEAR));
                    request.AddJsonBody(data.ToString());
                    response = client.Post(request).Content;
                } while (String.IsNullOrEmpty(response));


                populateEventList(response, App.eventsList);
                Preferences.Set(ConstantVars.EVENT_LIST_STORAGE, response);
            }
            catch (Exception e)
            {
                throw e.InnerException;
            }
        }
    }
}
