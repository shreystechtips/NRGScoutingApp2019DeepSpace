using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Essentials;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace NRGScoutingApp {
    public partial class App : Application {
        public App () {
            InitializeComponent ();
            DataDownload.initClient();
            DataDownload.populateAllData();
            MainPage = new NavigationPage (new NavTab ());
            Application.Current.MainPage = MainPage;
        }

        public static Dictionary<int, String> teamsList = new Dictionary<int, String>();
        public static Dictionary<string, string> eventsList = new Dictionary<string, string>();
        public static JObject matchesList = new JObject();


        /// <summary>
        /// Returns json string of a certain competition instead of the whole event
        /// </summary>
        /// <param name="totalData">all the app data</param>
        /// <param name="eventKey">the event key for which the data to return</param>
        /// <returns></returns>
        public static string getCompJson(string totalData, string eventKey)
        {
            return JsonConvert.SerializeObject(getCompObject(totalData, eventKey));
        }

        /// <summary>
        /// Returns json string of a certain competition instead of the whole event
        /// </summary>
        /// <param name="totalData">all the app data</param>
        /// <param name="eventKey">the event key for which the data to return</param>
        /// <returns></returns>
        public static JObject getCompObject(string totalData, string eventKey)
        {
            if (String.IsNullOrWhiteSpace(totalData))
            {
                totalData = "{}";
            }
            try
            {
                JObject data = JObject.Parse(totalData);
                if (data.ContainsKey(eventKey) && data[eventKey] != null)
                {
                    return (JObject) data[eventKey];
                }
            }
            catch {}
            return new JObject();
        }
    }
}