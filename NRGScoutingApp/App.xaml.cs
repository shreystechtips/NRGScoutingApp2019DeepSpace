using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Essentials;
using Newtonsoft.Json.Linq;

namespace NRGScoutingApp {
    public partial class App : Application {
        public App () {
            InitializeComponent ();
            DataDownload.initClient();
            populateLists();
            MainPage = new NavigationPage (new NavTab ());
            Application.Current.MainPage = MainPage;
        }

        public void populateLists()
        {
            DataDownload.populateTeamList(Preferences.Get(ConstantVars.TEAM_LIST_STORAGE, "[]"), teamsList);
            DataDownload.populateEventList(Preferences.Get(ConstantVars.EVENT_LIST_STORAGE, "[]"), eventsList);
        }

        public static Dictionary<int, String> teamsList = new Dictionary<int, String>();
        public static Dictionary<string, string> eventsList = new Dictionary<string, string>();


        /// <summary>
        /// Returns json string of a certain competition instead of the whole event
        /// </summary>
        /// <param name="totalData">all the app data</param>
        /// <param name="eventKey">the event key for which the data to return</param>
        /// <returns></returns>
        public static string getCompJson(string totalData, string eventKey)
        {
            try
            {
                JObject data = JObject.Parse(totalData);
                if (data.ContainsKey(eventKey))
                {
                    return data[eventKey].ToString();
                }
            }
            catch
            {
                return "";
            }
            return "";
        }
    }
}