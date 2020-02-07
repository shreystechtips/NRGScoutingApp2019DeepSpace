using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Essentials;

namespace NRGScoutingApp {
    public partial class App : Application {
        public static bool UseMockDataStore = true;
        public static string BackendUrl = "https://localhost:5000";

        public App () {
            InitializeComponent ();
            MainPage = new NavigationPage (new NavTab ());
            Application.Current.MainPage = new NavigationPage (new NavTab ());
            if (UseMockDataStore)
                DependencyService.Register<MockDataStore> ();
            else
                DependencyService.Register<CloudDataStore> ();
            Teams.populateTeamList(Preferences.Get(ConstantVars.TEAM_LIST_STORAGE,"[]"), teamsList);
        }

        public static Dictionary<int, String> teamsList = new Dictionary<int, String>();
    }
}