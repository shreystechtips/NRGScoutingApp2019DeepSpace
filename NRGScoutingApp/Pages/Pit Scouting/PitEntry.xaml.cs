using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace NRGScoutingApp {
    public partial class PitEntry : ContentPage {
        //Object for storing all the pit notes for JSON conversion
        public String[] vals = new string[ConstantVars.QUESTIONS.Length + 1];

        protected override bool OnBackButtonPressed () {
            return true;
        }
        string eventName = Preferences.Get(ConstantVars.CURRENT_EVENT_NAME, "");

        protected override void OnAppearing () {
            if (!teamName.Equals (Preferences.Get ("teamStart", 0))) {
                newName = Preferences.Get ("teamStart", 0);
                //this.Title = AdapterMethods.getTeamString(newName);
            }
            eventName = Preferences.Get(ConstantVars.CURRENT_EVENT_NAME, "");
        }

        private Editor[] inputs = new Editor[ConstantVars.QUESTIONS.Length];
        private Label[] questions = new Label[ConstantVars.QUESTIONS.Length];
        int teamName;
        int newName;

        //The boolean will hide the delete button if the entry is new
        public PitEntry (bool newCreation, int teamName, bool teamChange) {
            this.teamName = teamName;
            newName = teamName;
            NavigationPage.SetHasBackButton (this, false);
            InitializeComponent ();
            teamChanger.IsVisible = teamChange;
            vals[vals.Length - 1] = Preferences.Get ("teamStart", "memes not recieve");
            for (int i = 0; i < inputs.Length; i++) {
                questions[i] = new Label {
                    Text = ConstantVars.QUESTIONS[i],
                    MinimumWidthRequest = 1000,
                    MinimumHeightRequest = 300,
                    Margin = new Thickness(10, 5, 0, 0),
                    FontAttributes = FontAttributes.Bold,
                    //TextColor = (Color)App.Current.Resources["TextPrimaryColor"]
            };
                inputs[i] = new Editor {
                    Placeholder = "Type Here",
                    MinimumWidthRequest = 1000,
                    MinimumHeightRequest = 300,
                    Margin = new Thickness (10, 5, 0, 0),
                    Keyboard = Keyboard.Text,
                    AutoSize = EditorAutoSizeOption.TextChanges
                };
                inputs[i].TextChanged += new EventHandler<TextChangedEventArgs> (Comment_Box_Updated);
                mainLayout.Children.Add (questions[i]);
                mainLayout.Children.Add (inputs[i]);
            }
            deleteButton.IsVisible = !newCreation;
            Preferences.Set ("appState", 2);
            cacheCheck ();
        }

        protected void Comment_Box_Updated (object sender, Xamarin.Forms.TextChangedEventArgs e) {
            updateAllBoxes ();
            updateItems ();
        }

        void teamChanged (object sender, System.EventArgs e) {
            Navigation.PushAsync (new MatchEntryStart (ConstantVars.TEAM_SELECTION_TYPES.teamSelection));
        }

        void updateAllBoxes () {
            for (int i = 0; i < vals.Length - 1; i++) {
                vals[i] = inputs[i].Text;
            }
        }

        async void deleteClicked (object sender, System.EventArgs e) {
            bool text = await DisplayAlert ("Are you sure you want to delete??", "Data CANNOT be recovered", "No", "Yes");
            if (!text) {
                JObject data = JObject.Parse (Preferences.Get (ConstantVars.APP_DATA_STORAGE, ""));
                JArray pitNotes = (JArray) data[eventName]["PitNotes"];
                Debug.WriteLine("sd" + Preferences.Get("teamStart", 0));
                foreach (var s in pitNotes.ToList())
                {
                    Debug.WriteLine(s);
                }
                var delItem = pitNotes.ToList ().Find (x => (int)x["team"] == (Preferences.Get ("teamStart", 0)));
                pitNotes.Remove (delItem);
                if (pitNotes.Count <= 0) {
                    data.Remove ("PitNotes");
                }
                if (data.Count <= 0) {
                    Preferences.Set (ConstantVars.APP_DATA_STORAGE, "");
                }
                Preferences.Set (ConstantVars.APP_DATA_STORAGE, JsonConvert.SerializeObject (data));
                try {
                    await Navigation.PopAsync (true);
                } catch (System.NullReferenceException) { }
                clearMatchItems ();
            }
        }

        async void backClicked (object sender, System.EventArgs e) {
            bool text = await DisplayAlert ("Alert", "Do you want to discard progress?", "Yes", "No");
            if (text) {
                clearMatchItems ();
                try {
                    await Navigation.PopAsync ();
                } catch (System.NullReferenceException) {

                }
            }
        }

        void updateItems () {
            Dictionary<String, String> temp = new Dictionary<string, string> ();
            for (int i = 0; i < vals.Length - 1; i++) {
                temp.Add ("q" + i, vals[i]);
            }
            temp["team"] = vals[vals.Length - 1];
            Preferences.Set ("tempPitNotes", JsonConvert.SerializeObject (temp));
        }

        //Clears all properties for use in next match
        void clearMatchItems () {
            Preferences.Set ("teamStart", "");
            Preferences.Set ("appState", 0);
            Preferences.Set ("tempPitNotes", "");
        }

        void saveClicked (object sender, System.EventArgs e) {
            //Disables save button so app doesn't crash when user taps many times
            saveButton.IsEnabled = false;
            vals[vals.Length - 1] = newName.ToString();
            Dictionary<String, object> s = new Dictionary<String, object> ();
            for (int i = 0; i < vals.Length - 1; i++) {
                s.Add ("q" + i, vals[i]);
            }
            s.Add ("team", newName);
            JObject notes = JObject.FromObject (s);
            if (isAllEmpty (notes)) {
                try {
                    Navigation.PopAsync (true);
                } catch (InvalidOperationException) { }
                clearMatchItems ();
            } else {
                //Adds or creates new JObject to start all data in app cache
                JObject dataMain = MatchParameters.initializeEventsObject ();
                JObject data = (JObject)dataMain[Preferences.Get(ConstantVars.CURRENT_EVENT_NAME, "")];
                if (!data.ContainsKey ("PitNotes")) {
                    data.Add (new JProperty ("PitNotes", new JArray ()));
                    pushBackToHome (dataMain, data, new JArray (), notes);
                } else {
                    JArray temp = (JArray) data["PitNotes"];
                    if (temp.ToList ().Exists (x => x["team"].Equals (notes["team"]))) {
                        var item = temp.ToList ().Find (x => (int)x["team"] == (int)notes["team"]);
                        temp.Remove (item);
                        for (int i = 0; i < ConstantVars.QUESTIONS.Length; i++) {
                            try {
                                item["q" + (i)] = giveNewString (item["q" + i].ToString (), notes["q" + (i)].ToString ());
                            } catch { }
                        }
                    }
                    pushBackToHome (dataMain, data, temp, notes);
                }
            }
        }

        //calls all final methods to return to home as it updates all the data
        async void pushBackToHome (JObject dataMain, JObject data, JArray temp, JObject parameters) {
            temp.Add (new JObject (parameters));
            if (deleteButton.IsVisible && teamName != newName) {
                var delItem = data["PitNotes"].ToList ().Find (x => (int)x["team"] == (teamName));
                temp.Remove (delItem);
            }
            data["PitNotes"] = temp;
            Preferences.Set (ConstantVars.APP_DATA_STORAGE, JsonConvert.SerializeObject (dataMain));
            try {
                await Navigation.PopAsync (true);
            } catch (System.NullReferenceException) { }
            clearMatchItems ();
        }
        private string giveNewString (String old, String add) {
            if (String.IsNullOrWhiteSpace (add) && !String.IsNullOrWhiteSpace (old)) {
                DisplayAlert ("Alert", "Try deleting this entry instead", "ok");
                return old;
            }
            return add;
        }

        //Checks if all the question answers are empty
        private bool isAllEmpty (JObject valsIn) {
            bool total = true;
            for (int i = 0; i < ConstantVars.QUESTIONS.Length; i++) {
                total = String.IsNullOrWhiteSpace (valsIn["q" + i].ToString ()) && total;
            }
            return total;
        }

        //Populates and checks in case of app crash
        void cacheCheck () {
            int team = Preferences.Get ("teamStart", -1);
            Dictionary<String, String> temp = new Dictionary<string, string> ();
            String tempNotes = Preferences.Get ("tempPitNotes", "");
            if (!String.IsNullOrWhiteSpace (tempNotes)) {
                try {
                    temp = JsonConvert.DeserializeObject<Dictionary<String, String>> (tempNotes);
                } catch (JsonException e) {
                    Console.WriteLine (e.StackTrace);
                    temp = new Dictionary<String, String> ();
                }
            }

            if (temp.Count == 0) {
                JObject mainObject = new JObject ();
                if (!String.IsNullOrWhiteSpace (Preferences.Get (ConstantVars.APP_DATA_STORAGE, ""))) {
                    mainObject = JObject.Parse (Preferences.Get (ConstantVars.APP_DATA_STORAGE, ""));
                }
                JArray scoutArray = new JArray();
                if (mainObject.ContainsKey(eventName) && mainObject[eventName]["PitNotes"] != null) {
                    scoutArray = (JArray) ((JObject) mainObject[eventName])["PitNotes"];
                } else {
                    scoutArray = new JArray ();
                }
                Debug.WriteLine(scoutArray);
                try {
                    Debug.WriteLine(scoutArray.Count);
                    Debug.WriteLine(scoutArray.ToList().Exists(x => (int)x["team"] == (team)));
                    if (scoutArray.Count > 0 && scoutArray.ToList().Exists(x => (int)x["team"] == (team))) {
                        Debug.WriteLine("yret");
                        var final = scoutArray.ToList ().Find (x => (int)x["team"] == (team));
                        for (int i = 0; i < inputs.Length; i++) {
                            try {
                                inputs[i].Text = (final["q" + i].ToString ());
                            } catch {
                                inputs[i].Text = "";
                            }
                        }
                    }
                } catch (System.NullReferenceException) { }
            } else {
                try {
                    for (int i = 0; i < inputs.Length; i++) {
                        try {
                            inputs[i].Text = temp["q" + i];
                        } catch {
                            inputs[i].Text = "";
                        }
                    }
                } catch (NullReferenceException e) {
                    Console.WriteLine (e.StackTrace);
                }
            }
            updateAllBoxes ();
            updateItems ();
        }
    }
}