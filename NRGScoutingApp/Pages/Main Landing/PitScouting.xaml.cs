using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace NRGScoutingApp {
    public partial class PitScouting : ContentPage {
        public PitScouting () {
            InitializeComponent ();
            eventName = Preferences.Get(ConstantVars.CURRENT_EVENT_NAME, "");
            setListView (Preferences.Get (ConstantVars.APP_DATA_STORAGE, ""));
        }

        public static List<string> pitItems = new List<string> ();
        string eventName;
        protected override void OnAppearing () {
            setListView (Preferences.Get (ConstantVars.APP_DATA_STORAGE, ""));
            eventName = Preferences.Get(ConstantVars.CURRENT_EVENT_NAME, "");
        }

        void newPit (object sender, System.EventArgs e) {
            if (String.IsNullOrEmpty(Preferences.Get(ConstantVars.CURRENT_EVENT_NAME, "")))
            {
                Navigation.PushAsync(new ChangeEventPage());
            }
            else
            {
                Navigation.PushAsync(new MatchEntryStart(ConstantVars.TEAM_SELECTION_TYPES.pit));
            }
        }

        void SearchBar_OnTextChanged (object sender, Xamarin.Forms.TextChangedEventArgs e) {
            if (string.IsNullOrWhiteSpace (e.NewTextValue)) {
                listView.ItemsSource = pitItems;
            } else {
                listView.ItemsSource = pitItems.Where (pitItems => pitItems.ToLower ().Contains (e.NewTextValue.ToLower ()));
            }
        }

        async void teamClicked (object sender, Xamarin.Forms.ItemTappedEventArgs e) {
            String teamName = e.Item.ToString ();
            int teamnum = AdapterMethods.getTeamInt(teamName, App.teamsList);
            JArray pitValues = (JArray) JObject.Parse (Preferences.Get (ConstantVars.APP_DATA_STORAGE, "")) ["PitNotes"];
            Preferences.Set ("teamStart", teamnum);
            await Navigation.PushAsync (new PitEntry (false,  teamnum, true) { Title = teamName });
        }

        /*
         * Sets the visibility of the list based on boolean and the sad error opposite
         * So if list.IsVisible = true, then sadNoMatch.IsVisible = false
         */
        private void setListVisibility (int setList) {
            listView.IsVisible = setList > 0;
            sadNoPit.IsVisible = !listView.IsVisible;
        }

        public static List<string> getListVals (JObject input) {
            List<string> teamsInclude = new List<string> ();
            if (input.ContainsKey ("PitNotes")) {
                JArray pits = (JArray) input["PitNotes"];
                foreach (var x in pits) {
                    teamsInclude.Add (AdapterMethods.getTeamString((int)x["team"]));
                }
            }
            return teamsInclude;
        }
        void setListView (String json) {
            JObject input;
            if (!String.IsNullOrWhiteSpace (json)) {
                try {
                    input = JObject.Parse (json);
                    Debug.WriteLine(eventName);
                    if (input.ContainsKey(eventName))
                    {
                        input = (JObject)input[eventName];
                    }
                } catch (JsonException) {
                    Debug.WriteLine("mission failed, we'll get them next time");
                    input = new JObject ();
                }
                pitItems = getListVals (input == null ? new JObject() : input);
                scoutView.IsVisible = true;
                sadNoPit.IsVisible = !scoutView.IsVisible;
            } else {
                pitItems = new List<string> ();
            }
            listView.ItemsSource = pitItems;
            scoutView.IsVisible = pitItems.Count > 0;
            sadNoPit.IsVisible = !scoutView.IsVisible;
        }

        async void deleteClicked(object sender, System.EventArgs e)
        {
            await DisplayAlert("Hold it", "Make sure export to data first", "OK");
            var del = await DisplayAlert("Notice", "Do you want to delete all Pit Notes? Data CANNOT be recovered.", "Yes", "No");
            if (del)
            {
                JObject s = JObject.Parse(Preferences.Get (ConstantVars.APP_DATA_STORAGE, ""));
                if (s.ContainsKey(eventName) && s[eventName].ToObject<JObject>().ContainsKey("PitNotes"))
                {
                    JObject temp = (JObject)s[eventName];
                    temp.Remove("PitNotes");
                }
                Preferences.Set(ConstantVars.APP_DATA_STORAGE, JsonConvert.SerializeObject(s));
                setListView(Preferences.Get (ConstantVars.APP_DATA_STORAGE, ""));
            }
        }
    }
}