using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace NRGScoutingApp {
    /*ADD Ranking Chooser Replacement for iOS 
     *like a picker acts as the distribution center to choose the type
     */
    public partial class Rankings : INotifyPropertyChanged {
        public Rankings () {
            InitializeComponent ();
            eventName = Preferences.Get(ConstantVars.CURRENT_EVENT_NAME, "");
            rankPicker.SelectedIndex = 0;
        }

        public static int teamSend;
        private List<RankStruct> rankList;
        public static List<string> pitTeams;
        string eventName =  Preferences.Get(ConstantVars.CURRENT_EVENT_NAME, "");
        MatchFormat.CHOOSE_RANK_TYPE rankChoice;

        //Initializes the ranking object
        Ranker mainRank = new Ranker (Preferences.Get (ConstantVars.APP_DATA_STORAGE, ""));

        //void settingsClicked (object sender, System.EventArgs e) {
        //    Navigation.PushAsync (new Settings ());
        //}

        void rankTypeDelta (object sender, System.EventArgs e) {
            switch (rankPicker.SelectedIndex) {
                case 0:
                    rankChoice = MatchFormat.CHOOSE_RANK_TYPE.overallRank;
                    break;
                case 1:
                    rankChoice = MatchFormat.CHOOSE_RANK_TYPE.pick1; //Hatch
                    break;
                //case 2:
                //    rankChoice = MatchFormat.CHOOSE_RANK_TYPE.pick2; //Cargo
                //    break;
                case 3:
                    rankChoice = MatchFormat.CHOOSE_RANK_TYPE.climb; //Climb
                    break;
                case 4:
                    rankChoice = MatchFormat.CHOOSE_RANK_TYPE.drop1; //Lvl1
                    break;
                case 5:
                    rankChoice = MatchFormat.CHOOSE_RANK_TYPE.drop2; //lvl2
                    break;
                case 6:
                    rankChoice = MatchFormat.CHOOSE_RANK_TYPE.drop3; //lvl3
                    break;
                case 7:
                    rankChoice = MatchFormat.CHOOSE_RANK_TYPE.overallRank; // teamNum
                    break;
                default:
                    rankChoice = MatchFormat.CHOOSE_RANK_TYPE.overallRank;
                    break;
            }
            updateEvents ();
        }

        protected override void OnAppearing () {
            eventName = Preferences.Get(ConstantVars.CURRENT_EVENT_NAME, "");
            updateEvents ();
            setPitTeams ();

        }

        //Updates events with given enum
        private void updateEvents () {
            //Updates string data from matches
            Debug.WriteLine(Preferences.Get(ConstantVars.APP_DATA_STORAGE, "reee"));
            //JObject temp = JObject.Parse(Preferences.Get(ConstantVars.APP_DATA_STORAGE, "{}"));
            //Debug.WriteLine(temp);
            //if (temp.ContainsKey(eventName))
            //{
            //    temp = (JObject)temp[eventName];
            //}
            //else
            //{
            //    temp = new JObject();
            //}
            mainRank.setData (App.getCompJson(Preferences.Get(ConstantVars.APP_DATA_STORAGE, "{}"),eventName));
            //Gets all data and sets it into ascending order based on each team's average time
            Dictionary<int, double> x = mainRank.getRank (rankChoice);
            Dictionary<int, double> y = new Dictionary<int, double>(x);
            List<RankStruct> ranks = new List<RankStruct> ();
            if (!(rankPicker.SelectedIndex == 7)) {
                y = (from pair in x orderby pair.Value descending select pair).ToDictionary (pair => pair.Key, pair => pair.Value);
            } else {
                try {
                    y = (from pair in x orderby pair.Key ascending select pair).ToDictionary (pair => pair.Key, pair => pair.Value);
                } catch {
                    y = new Dictionary<int, double> ();
                }
            }

            foreach (var s in y) {
                ranks.Add (new RankStruct { Key = AdapterMethods.getTeamString(s.Key), Value = s.Value, color = getTeamColor (s.Key) });
            }
            listView.ItemsSource = ranks;
            rankList = ranks;
            setListVisibility (y.Count ());
        }

        public class RankStruct {
            public string Key { get; set; }
            public double Value { get; set; }
            public Color color { get; set; }
        }

        private Color getTeamColor (int team) {
            return mainRank.getColors () [team];
        }

        /*
         * Sets the visibility of the list based on boolean and the sad error opposite
         * So if list.IsVisible = true, then sadNoMatch.IsVisible = false
         */
        private void setListVisibility (int setList) {
            rankingsView.IsVisible = setList > 0;
            sadNoMatch.IsVisible = !rankingsView.IsVisible;
        }

        void SearchBar_TextChanged (object sender, Xamarin.Forms.TextChangedEventArgs e) {
            if (string.IsNullOrWhiteSpace (e.NewTextValue)) {
                listView.ItemsSource = rankList;
            } else {
                listView.ItemsSource = rankList.Where (rankList => rankList.Key.ToLower ().Contains (e.NewTextValue.ToLower ()) ||
                    rankList.Key.ToLower().Contains (e.NewTextValue.ToLower ()) ||
                    getColorString (rankList.color).ToLower ().Contains (e.NewTextValue.ToLower ()));
            }
        }

        private String getColorString (Color input) {
            if (input.Equals (Color.Red)) {
                return "Red";
            } else if (input.Equals (Color.Yellow)) {
                return "Yellow";
            } else {
                return "None";
            }
        }

        async void teamClicked (object sender, Xamarin.Forms.ItemTappedEventArgs e) {
            var x = (listView.ItemsSource as IEnumerable<RankStruct>).ToList ();
            int teamnum = AdapterMethods.getTeamInt(((RankStruct)e.Item).Key, App.teamsList);
            //String item = x.Find (y => y.Equals (teamnum)).Key;
            teamSend = teamnum;
            Debug.WriteLine("befrore send" + ((RankStruct)e.Item).Key);
            await Navigation.PushAsync (new RankingsDetailView (mainRank.returnTeamTimes (teamnum)) { Title = ((RankStruct)e.Item).Key });
        }

        private void setPitTeams () {
            JObject input;
            try {
                input = JObject.Parse (Preferences.Get (ConstantVars.APP_DATA_STORAGE, ""));
                input = (JObject)input[eventName];
            } catch (Newtonsoft.Json.JsonException) {
                input = new JObject ();
            }
            pitTeams = PitScouting.getListVals (input ==null ? new JObject() : input);
        }

        void allianceClicked (object sender, System.EventArgs e) {

        }
    }
}