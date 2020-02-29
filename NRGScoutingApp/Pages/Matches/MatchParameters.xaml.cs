using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace NRGScoutingApp {
    public partial class MatchParameters : ContentPage {
        protected override bool OnBackButtonPressed () {
            return true;
        }

        public static Boolean updateTeam = false;

        protected override void OnAppearing () {
            teamName = Preferences.Get ("teamStart", 0);
        }

        public int teamName = Preferences.Get ("teamStart", 0);
        public static MatchFormat.EntryParams Entry = new MatchFormat.EntryParams {
            team = Preferences.Get("teamStart", 0),
            matchNum = 0,
            side = 0,

            crossBaseline = false,

            deathAmt = 0,
            climbLvl = 0,

            didDefense = 0,
            gotDefended = 0,

            yellowCard = false,
            redCard = false,
            comments = ""
        };

        public MatchParameters () {
            InitializeComponent ();
            cacheCheck ();
            onParamUpdate ();
        }

        void teamChanged (object sender, System.EventArgs e) {
            Navigation.PushAsync (new MatchEntryStart (ConstantVars.TEAM_SELECTION_TYPES.teamSelection));
        }

        async void autoFillClicked(object sender, System.EventArgs e)
        {
            string currEvent = Preferences.Get("CurrentEvent", "");
            if (Convert.ToInt32(matchnum.Text) < 1)
            {
                await DisplayAlert("Oops", "please enter match num first", "ok");
                return;
            }
            JObject s = App.matchesList;
            s = (s == null) ? new JObject() : s;
            if (s.ContainsKey(currEvent))
            {
                setSide(currEvent);
            }
            else
            {
                var response = await DisplayAlert("Oops", "Looks Like you don't have this downloaded", "Download", "Don't Download");
                if (response)
                {
                    DataDownload.getEventMatches(currEvent);
                    setSide(currEvent);
                }
            }
        }

        async void setSide(string currEvent)
        {
            bool showError = true;
            if (App.matchesList.ContainsKey(currEvent))
            {
                var temp = App.matchesList[currEvent];
                if (((JObject)temp).ContainsKey(matchnum.Text))
                {
                    temp = temp[matchnum.Text];
                    Debug.WriteLine(temp);
                    if (temp["blue"].ToArray().Contains(teamName))
                    {
                        var list = temp["blue"].ToArray().ToList();
                        PositionPicker.SelectedIndex = 3 + list.IndexOf(teamName);
                        showError = false;
                    }
                    else if (temp["red"].ToArray().Contains(teamName))
                    {
                        var list = temp["red"].ToArray().ToList();
                        PositionPicker.SelectedIndex = list.IndexOf(teamName);
                        showError = false;
                    }
                }
                else
                {
                    Debug.WriteLine("hmm0");
                }
            }
            else
            {
                Debug.WriteLine("hmm1");
            }
            if (showError)
            {
                await DisplayAlert("Error", "Could not find side and competition for given team", "OK");
            }
        }

            //Confirms user action to go back and clears all data for next match
            async void backClicked (object sender, System.EventArgs e) {
            var text = await DisplayAlert ("Alert", "Do you want to discard progress?", "Yes", "No");
            if (text) {
                clearMatchItems ();
                Navigation.PopAsync ();
            }
        }

        //Checks if all neccesary Items exist, clears match data, and goes to Matches Page
        async void saveClicked (object sender, System.EventArgs e) {
            Entry.team = teamName;
            onParamUpdate ();
            if (popErrorsToScreen ()) { } else {
                await Task.Run (async () => {
                    Device.BeginInvokeOnMainThread (() => { //Disables save button so app doesn't crash when user taps many times
                        saveButton.IsEnabled = false;
                    });
                    //Gets and combines all of the match's events to a JObject
                    JObject events = MatchFormat.eventsListToJSONEvents (NewMatchStart.events);
                    events.Add ("timerValue", NewMatchStart.timerValue);
                    JObject parameters = JObject.FromObject (Entry);
                    parameters.Merge (events);

                    //Adds or creates new JObject to start all data in app cache
                    JObject dataMain = initializeEventsObject ();
                    JObject data = (JObject)dataMain[Preferences.Get(ConstantVars.CURRENT_EVENT_NAME,"")];
                    if (data.Count <= 0 || !data.ContainsKey ("Matches")) {
                        data.Add (new JProperty ("Matches", new JArray ()));
                        pushBackToHome (dataMain, data, new JArray (), parameters);
                    } else {
                        JArray temp = (JArray) data["Matches"];
                        if (temp.ToList ().Exists (x => x["matchNum"].Equals (parameters["matchNum"]) && x["side"].Equals (parameters["side"]))) {
                            var item = temp.ToList ().Find (x => x["matchNum"].Equals (parameters["matchNum"]) && x["side"].Equals (parameters["side"]));
                            if (!item["team"].Equals (parameters["team"])) {
                                Device.BeginInvokeOnMainThread (async () => {
                                    bool remove = await DisplayAlert ("Error", "Overwrite Old Match with New Data?", "No", "Yes");
                                    if (!remove) {
                                        temp.Remove (item);
                                        pushBackToHome (dataMain, data, temp, parameters);
                                    } else {
                                        saveButton.IsEnabled = true;
                                        return;
                                    }
                                });
                            } else {
                                temp.Remove (item);
                                pushBackToHome (dataMain,data, temp, parameters);
                            }
                        } else {
                            pushBackToHome (dataMain, data, temp, parameters);
                        }
                    }
                });
            }
        }

        void Handle_SelectedIndexChanged (object sender, System.EventArgs e) {
            Entry.side = PositionPicker.SelectedIndex;

            onParamUpdate ();
        }



        void Handle_Toggled(object sender, Xamarin.Forms.ToggledEventArgs e)
        {
            Entry.crossBaseline = e.Value;
            onParamUpdate();
        }

        void deathSelector (object sender, System.EventArgs e) {
            Entry.deathAmt = Math.Round(death.Value);
            onParamUpdate ();
        }

        void climbLvlSelector (object sender, System.EventArgs e) {
            Entry.climbLvl = climbLvl.SelectedIndex;
        }

        void Handle_Toggled_11 (object sender, Xamarin.Forms.ToggledEventArgs e) {
            Entry.yellowCard = e.Value;
            onParamUpdate ();
        }

        void Handle_Toggled_12 (object sender, Xamarin.Forms.ToggledEventArgs e) {
            Entry.redCard = e.Value;
            onParamUpdate ();
        }

        void Comment_Box_Updated (object sender, Xamarin.Forms.TextChangedEventArgs e) {
            Entry.comments = e.NewTextValue;
            onParamUpdate ();
        }

        void defenseSlider_Updated(System.Object sender, Xamarin.Forms.ValueChangedEventArgs e)
        {
            gotDefended.Value = Math.Round(gotDefended.Value);
            didDefense.Value = Math.Round(didDefense.Value);
            Entry.gotDefended = (int)gotDefended.Value;
            Entry.didDefense = (int)didDefense.Value;
            onParamUpdate();
        }

        void Match_Num_Updated (object sender, Xamarin.Forms.TextChangedEventArgs e) {
            try {
                Entry.matchNum = Convert.ToInt32 (e.NewTextValue);
                onParamUpdate ();
            } catch (FormatException) {
                if (!String.IsNullOrWhiteSpace (e.NewTextValue)) {
                    DisplayAlert ("Warning", "Match Number Contains Letters. Did Not Update Value", "OK");
                    matchnum.Text = "1";
                }
            }
        }


        //Returns Jobject based on wheter match events string is empty or not
        public static JObject initializeEventsObject () {
            JObject data;
            string eventName = Preferences.Get(ConstantVars.CURRENT_EVENT_NAME, "");
            if (!String.IsNullOrWhiteSpace (Preferences.Get (ConstantVars.APP_DATA_STORAGE, ""))) {
                data = JObject.Parse (Preferences.Get (ConstantVars.APP_DATA_STORAGE, ""));
                if (!data.ContainsKey(eventName))
                {
                    data.Add(eventName,new JObject());
                }
            } else {
                data = new JObject ();
                data.Add(eventName, new JObject());              
            }
            return data;
        }

        //Updates tempParam Data Cache every time Parameters are updated
        void onParamUpdate () {
            Preferences.Set ("tempParams", JsonConvert.SerializeObject (Entry));
        }

        //Checks if old data exists in app and sets all toggles to reflect the options
        void cacheCheck () {
            MatchFormat.EntryParams x;
            bool containsVal = false;
            if (!String.IsNullOrWhiteSpace (Preferences.Get ("tempParams", ""))) {
                try {
                    x = JsonConvert.DeserializeObject<MatchFormat.EntryParams> (Preferences.Get ("tempParams", ""));
                    containsVal = true;
                } catch (JsonException) {
                    containsVal = false;
                    x = Entry;
                }
            } else {
                containsVal = false;
                x = Entry;
            }
            if (containsVal) {
                MatchFormat.EntryParams entries = x;
                matchnum.Text = entries.matchNum.ToString ();
                PositionPicker.SelectedIndex = entries.side;

                crossbase.IsToggled = entries.crossBaseline;

                death.Value = entries.deathAmt;
                climbLvl.SelectedIndex = entries.climbLvl;


                yellow.IsToggled = entries.yellowCard;
                red.IsToggled = entries.redCard;
                comments.Text = entries.comments;
                Entry = entries;
            } else {
                death.Value = (int) MatchFormat.DEATH_TYPE.noDeath;
            }
        }

        //Clears all properties for use in next match
        public static void clearMatchItems () {
            Preferences.Set("teamStart", 0);
            Preferences.Set ("appState", 0);
            Preferences.Set ("timerValue", 0);
            Preferences.Set ("lastItemPicked", 0);
            Preferences.Set ("lastItemDropped", 0);
            Preferences.Set ("tempParams", "");
            Preferences.Set ("tempMatchEvents", "");
            NewMatchStart.timerValue = 0;
            MatchEvents.update = true;
            NewMatchStart.events.Clear ();
            Entry = new MatchFormat.EntryParams ();
        }

        //Takes all objects and adds items while returning the main page
        void pushBackToHome (JObject mainData, JObject data, JArray temp, JObject parameters) {
            temp.Add (new JObject (parameters));
            data["Matches"] = temp;
            Preferences.Set (ConstantVars.APP_DATA_STORAGE, JsonConvert.SerializeObject (mainData));
            clearMatchItems ();
            Device.BeginInvokeOnMainThread (() => {
                try {
                    Navigation.PopAsync ();
                } catch (System.InvalidOperationException) {

                }
            });
        }


        //Pops errors if fields are checked but their counterparts are not
        private bool popErrorsToScreen () {
            String errors = "";
            bool toPrint = false;
            if (string.IsNullOrWhiteSpace (matchnum.Text) || matchnum.Text.Substring (0, 1).Equals ("0")) {
                errors += "\n- Match Number";
                toPrint = true;
            }
            if (PositionPicker.SelectedIndex < 0) {
                errors += "\n- Position";
                toPrint = true;
            }
            if (Entry.team <= 0)
            {
                errors+= "\n- Select Team \"Change Team\"";
                toPrint = true;
            }
            if (toPrint) {
                DisplayAlert ("The Following Errors Occured", errors, "OK");
            }
            return toPrint;
        }
    }
}