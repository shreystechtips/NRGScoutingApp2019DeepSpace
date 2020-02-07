using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using Xamarin.Essentials;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace NRGScoutingApp
{
    public class Teams
    {
        public Teams()
        {
            
        }
        // TODO: Add var for lowest team amt
        public static void populateTeamList(String json, Dictionary<int, String> list)
        {
            Debug.WriteLine(json);
            Dictionary<int, String> temp = new Dictionary<int, String>(list);
            JArray repsonse = JArray.Parse(json);
            list.Clear();
            foreach (JObject s in repsonse)
            {
                list.Add(Convert.ToInt32(s.GetValue("team_number")), (String)s.GetValue("nickname"));
            }
            if(list.Count <= 500)
            {
                list = temp;
            }
        }

            public static void refreshTeams()
        {
            String key = Environment.GetEnvironmentVariable(ConstantVars.SCOUTING_API_NAME);
            key = ConstantVars.SCOUTING_API_KEY;
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(ConstantVars.SCOUTING_API_SITE + "/teams");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Accept = "application/json";
            httpWebRequest.Method = "POST";
            httpWebRequest.Headers.Add("API-Key", key);
            JArray repsonse = new JArray();
            try
            {
                using (StreamWriter streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    JObject json = new JObject();
                    json.Add("year", ConstantVars.APP_YEAR);
                    streamWriter.Write(json);
                }
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var responseText = streamReader.ReadToEnd();
                    populateTeamList(responseText, App.teamsList);
                    Preferences.Set(ConstantVars.TEAM_LIST_STORAGE, responseText);
                }
            }
            catch (System.Net.WebException ex)
            {
               throw new Exception("Incorrect API Key or server URL. Please contact IT.");
            }
        }
    }
}
