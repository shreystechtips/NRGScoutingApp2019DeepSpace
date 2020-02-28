using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace NRGScoutingApp {
    public class Ranker {
        // not used because switch case only accept constant variables
        private readonly int CLIMB_LVL_1_INDEX = 0;
        private readonly int CLIMB_LVL_2_INDEX = 1;
        private readonly int CLIMB_LVL_3_INDEX = 2;
        private readonly int GIVE_CLIMB_LVL_2_INDEX = 0;
        private readonly int GIVE_CLIMB_LVL_3_INDEX = 1;

        private JArray fullData;
        private Dictionary<int, double> overallData = new Dictionary<int, double>();
        private Dictionary<int, double> cargoData = new Dictionary<int, double>();
        private Dictionary<int, double> hatchData = new Dictionary<int, double>();
        private Dictionary<int, double> climbData = new Dictionary<int, double>();
        private Dictionary<int, double> drop1Data = new Dictionary<int, double>(); // low rocket
        private Dictionary<int, double> drop2Data = new Dictionary<int, double>();
        private Dictionary<int, double> drop3Data = new Dictionary<int, double>();
        private Dictionary<int, double> drop4Data = new Dictionary<int, double>();
        private Dictionary<int, double> drop1_4Data = new Dictionary<int, double>();
        private Dictionary<int, int> dropAmount = new Dictionary<int, int>();
        private Dictionary<int, Color> colorData = new Dictionary<int, Color>();

        //PRE: data is in JSON Format
        public Ranker (String data) {
            fullData = getJSON (data);
            refresh ();
        }

        public void setData (String data) {
            fullData = getJSON (data);
            refresh ();
        }

        public JArray getDataAsJArray () {
            return fullData;
        }

        /*
         * This is the order in which the array is ordered
         * overall, cargoTime, hatchTime, climb, lvl1, lvl2, lvl3
         */
        public String[] returnTeamTimes (int team) {
            String[] retValues = new String[ConstantVars.numRankTypes];
            if (cargoData.ContainsKey (team)) {
                retValues[1] = timeAdaptiveString ((int) cargoData[team]);
            } else {
                retValues[1] = ConstantVars.noVal;
            }
            if (hatchData.ContainsKey (team)) {
                retValues[2] = timeAdaptiveString ((int) hatchData[team]);
            } else {
                retValues[2] = ConstantVars.noVal;
            }
            if (climbData.ContainsKey (team)) {
                retValues[3] = climbData[team].ToString ();
            } else {
                retValues[3] = ConstantVars.noVal;
            }
            if (drop1Data.ContainsKey (team)) {
                retValues[4] = timeAdaptiveString ((int) drop1Data[team]);
            } else {
                retValues[4] = ConstantVars.noVal;
            }
            if (drop2Data.ContainsKey (team)) {
                retValues[5] = timeAdaptiveString ((int) drop2Data[team]);
            } else {
                retValues[5] = ConstantVars.noVal;
            }
            if (drop3Data.ContainsKey (team)) {
                retValues[6] = timeAdaptiveString ((int) drop3Data[team]);
            } else {
                retValues[6] = ConstantVars.noVal;
            }
            if (drop4Data.ContainsKey (team)) {
                retValues[7] = timeAdaptiveString ((int) drop4Data[team]);
            } else {
                retValues[7] = ConstantVars.noVal;
            }
            retValues[0] = overallData[team].ToString ();
            return retValues;
        }

        public static string timeAdaptiveString (int timeValue) {
            int minutes = 0;
            int seconds = 0;
            int milliseconds = 0;
            minutes = timeValue / (int) ConstantVars.MIN_MS;
            timeValue %= (int) ConstantVars.MIN_MS;
            seconds = timeValue / (int) ConstantVars.SEC_MS;
            timeValue %= (int) ConstantVars.SEC_MS;
            milliseconds = timeValue;
            if (minutes == 0) {
                return seconds.ToString ("D2") + "." + (milliseconds / 10).ToString ("D2");
            } else {
                return minutes + ":" + seconds.ToString ("D2") + "." + (milliseconds / 10).ToString ("D2");
            }
        }

        public Dictionary<int, Color> getColors () {
            return colorData;
        }

        /*
         * Switchboard operator for getting match Ranks
         * PRE: Rank type is provided
         * POST: Dictionary<String,double> type
         * Used for populating the listView in Rankings Page
         */
        public Dictionary<int, double> getRank (MatchFormat.CHOOSE_RANK_TYPE x) {
            refresh ();
            switch (x) {
                case MatchFormat.CHOOSE_RANK_TYPE.pick1: //hatch
                    return timeFixer (hatchData);
                case MatchFormat.CHOOSE_RANK_TYPE.drop1:
                    return timeFixer (drop1_4Data);
                case MatchFormat.CHOOSE_RANK_TYPE.drop2:
                    return timeFixer (drop2Data);
                case MatchFormat.CHOOSE_RANK_TYPE.drop3:
                    return timeFixer (drop3Data);
                case MatchFormat.CHOOSE_RANK_TYPE.climb:
                    return climbData;
                case MatchFormat.CHOOSE_RANK_TYPE.overallRank:
                    return overallData;
                default:
                    Console.WriteLine ("ERROR: WRONG RANK TYPE");
                    return new Dictionary<int, double> ();
            }
        }

        public Dictionary<int, double> timeFixer (Dictionary<int, double> input) {
            Dictionary<int, double> temp = new Dictionary<int, double> (input);
            foreach (var s in input.ToList ()) {
                temp[s.Key] = Math.Round (ConstantVars.MATCH_SPAN_MS / input[s.Key], 2);
            }
            return temp;
        }

        public Dictionary<string, string> returnDataAsTime (Dictionary<string, double> input) {
            Dictionary<string, string> result = new Dictionary<string, string> ();
            foreach (var x in input) {
                result.Add (x.Key, timeAdaptiveString ((int) x.Value * (int) ConstantVars.TIME_NERF));
            }
            return result;
        }
        public void refresh () {
            hatchData = getPickAvgData ((int) MatchFormat.ACTION.pick1);
            drop1Data = getDropData ((int) MatchFormat.ACTION.drop1);
            drop2Data = getDropData ((int) MatchFormat.ACTION.drop2);
            drop3Data = getDropData ((int) MatchFormat.ACTION.drop3);
            colorData = cardColor ();
            drop1_4Data = getDrop1_4Data ();
            climbData = getClimbData ();
            overallData = getOverallData ();
        }

        private Dictionary<int, double> getDrop1_4Data () {
            Dictionary<int, double> returnData = new Dictionary<int, double> (drop1Data);
            foreach (KeyValuePair<int, double> y in drop4Data) {
                if (returnData.ContainsKey (y.Key)) {
                    returnData[y.Key] = (returnData[y.Key] + y.Value);
                } else {
                    returnData.Add (y.Key, y.Value);
                }
            }
            return returnData;
        }

        public Dictionary<int, Color> cardColor () {
            Dictionary<int, Color> teamCards = new Dictionary<int, Color> ();
            foreach (var match in fullData) {
                if (teamCards.ContainsKey ((int)match["team"])) {
                    Color temp = Color.Transparent;
                    if ((bool) match["redCard"]) {
                        temp = Color.Red;
                    } else if ((bool) match["yellowCard"]) {
                        temp = Color.Yellow;
                        if (teamCards[(int)match["team"]].Equals (Color.Yellow)) {
                            temp = Color.Red;
                        }
                    }
                    teamCards[(int)match["team"]] = mainColor (teamCards[(int)match["team"]], temp);
                } else {
                    Color temp = Color.Transparent;
                    if ((bool) match["redCard"]) {
                        temp = Color.Red;
                    } else if ((bool) match["yellowCard"]) {
                        temp = Color.Yellow;
                    }
                    teamCards[(int)match["team"]] = temp;
                }
            }
            return teamCards;
        }

        private Color mainColor (Color old, Color current) {
            if (old.Equals (Color.Red) || current.Equals (Color.Red)) {
                return Color.Red;
            } else if (old.Equals (Color.Yellow) || current.Equals (Color.Yellow)) {
                return Color.Yellow;
            } else {
                return Color.Transparent;
            }
        }

        // Pre: climbData returns a dictionary that consist of every team appeared
        public Dictionary<int, double> getOverallData () {
            Dictionary<int, double> data = new Dictionary<int, double> ();
            Dictionary<int, double> dropData1 = getDropData ((int) MatchFormat.CHOOSE_RANK_TYPE.drop1);
            Dictionary<int, double> dropData2 = getDropData ((int) MatchFormat.CHOOSE_RANK_TYPE.drop2);
            Dictionary<int, double> dropData3 = getDropData ((int) MatchFormat.CHOOSE_RANK_TYPE.drop3);
            Dictionary<int, double> cargoData = getPickAvgData ((int) MatchFormat.CHOOSE_RANK_TYPE.pick1);
            Dictionary<int, double> climbData = getClimbData ();
            foreach (KeyValuePair<int, double> entry in climbData) {
                double point = 0;
                if (dropData1.ContainsKey (entry.Key) && drop1Data[entry.Key] > 0) {
                    point += ConstantVars.DROP_1_MULTIPLIER / dropData1[entry.Key];
                }
                if (dropData2.ContainsKey (entry.Key) && drop2Data[entry.Key] > 0) {
                    point += ConstantVars.DROP_2_MULTIPLIER / dropData2[entry.Key];
                }
                if (dropData3.ContainsKey (entry.Key) && drop3Data[entry.Key] > 0) {
                    point += ConstantVars.DROP_3_MULTIPLIER / dropData3[entry.Key];
                }
                //if (dropData4.ContainsKey (entry.Key) && drop4Data[entry.Key] > 0) {
                //    point += ConstantVars.DROP_4_MULTIPLIER / dropData4[entry.Key];
                //}
                if (cargoData.ContainsKey (entry.Key) && cargoData[entry.Key] > 0) {
                    point += ConstantVars.CARGO_MULTIPLIER / cargoData[entry.Key];
                }
                //if (hatcherData.ContainsKey (entry.Key) && hatcherData[entry.Key] > 0) {
                //    point += ConstantVars.HATCHER_MULTIPLIER / hatcherData[entry.Key];
                //}
                if (dropAmount.ContainsKey (entry.Key) && dropAmount[entry.Key] > 0) {
                    point += ConstantVars.DROP_AMOUNT_MULTIPLIER * dropAmount[entry.Key];
                }
                point += (climbData[entry.Key] * ConstantVars.CLIMB_MULTIPLIER);
                data.Add (entry.Key, Math.Round (point, 2)); //* ConstantVars.OVERALL_MULT
            }
            return data;
        }

        //Returns average data for drop level passed through (enum int is passed through sortType)
        public Dictionary<int, double> getDropData (int levelEnum) {
            Dictionary<int, double> totalData = new Dictionary<int, double> ();
            Dictionary<int, int> numsData = new Dictionary<int, int> ();
            try {
                foreach (var match in fullData) {
                    int reps;
                    try {
                        reps = (int) match["numEvents"];
                    } catch (JsonException) {
                        reps = 0;
                    }
                    for (int i = 1; i < reps; i++) {
                        if ((int) match["TE" + i + "_1"] == levelEnum) {
                            if (((int) match["TE" + (i - 1) + "_1"] != (int) MatchFormat.ACTION.startClimb) && ((int) match["TE" + (i - 1) + "_1"] != (int) MatchFormat.ACTION.dropNone)) {
                                int doTime = (int) match["TE" + (i) + "_0"] - (int) match["TE" + (i - 1) + "_0"];
                                if ((int) match["TE" + (i) + "_0"] <= ConstantVars.AUTO_LENGTH && !(bool) match["autoOTele"]) {
                                    doTime /= 2;
                                }
                                if (totalData.ContainsKey ((int)match["team"])) {
                                    totalData[(int)match["team"]] += doTime;
                                    numsData[(int)match["team"]]++;
                                } else {
                                    totalData.Add ((int)match["team"], doTime);
                                    numsData.Add ((int)match["team"], 1);
                                }
                            }
                        }
                    }
                }
            } catch (System.NullReferenceException) {

            }
            Dictionary<int, double> pushData = new Dictionary<int, double> ();
            foreach (var data in totalData) {
                pushData.Add (data.Key, Math.Round (data.Value / numsData[data.Key], 2));
            }
            dropAmount = numsData;
            return pushData;
        }

        //Returns average data for the climb parameters
        public Dictionary<int, double> getClimbData () {
            Dictionary<int, double> totalPoint = new Dictionary<int, double> ();
            Dictionary<int, double> amountOfMatch = new Dictionary<int, double> ();
            try {
                foreach (var match in fullData) {
                    int point = 0;
                    switch ((int) match["climbLvl"]) {
                        case 0:
                            point += (int) ConstantVars.PTS_SELF_LVL_0;
                            break;
                        case 1:
                            point += (int) ConstantVars.PTS_SELF_LVL_1;
                            break;
                        case 2:
                            point += (int) ConstantVars.PTS_SELF_LVL_2;
                            break;
                        case 3:
                            point += (int)ConstantVars.PTS_SELF_LVL_3;
                            break;
                        default:
                            point += 0;
                            break;
                    }

                    if (totalPoint.ContainsKey ((int)match["team"])) {
                        totalPoint[(int)match["team"]] += point;
                        amountOfMatch[(int)match["team"]] += 1;
                    } else {
                        totalPoint.Add ((int)match["team"], point);
                        amountOfMatch.Add ((int)match["team"], 1);
                    }
                }
            } catch (System.NullReferenceException) {

            }
            Dictionary<int, double> data = new Dictionary<int, double> ();
            foreach (KeyValuePair<int, double> entry in totalPoint) {
                data.Add (entry.Key, Math.Round (entry.Value / amountOfMatch[entry.Key], 2));
            }
            return data;
        }

        //Returns average data for pick type passed through (enum int is passed through sortType)
        public Dictionary<int, double> getPickAvgData (int sortType) {
            Dictionary<int, double> totalData = new Dictionary<int, double> ();
            Dictionary<int, int> numsData = new Dictionary<int, int> ();
            Dictionary<int, double> pushData = new Dictionary<int, double> ();
            try {
                foreach (var match in fullData) {
                    int reps;
                    try {
                        reps = (int) match["numEvents"];
                    } catch (JsonException) {
                        reps = 0;
                    }
                    for (int i = 0; i < reps; i++) {
                        if ((int) match["TE" + i + "_1"] == sortType && i != reps - 1) {
                            if (((int) match["TE" + (i + 1) + "_1"] != (int) MatchFormat.ACTION.startClimb) && ((int) match["TE" + (i + 1) + "_1"] != (int) MatchFormat.ACTION.dropNone)) {
                                int doTime = (int) match["TE" + (i + 1) + "_0"] - (int) match["TE" + i + "_0"];
                                if ((int) match["TE" + (i + 1) + "_0"] <= ConstantVars.AUTO_LENGTH && !(bool) match["autoOTele"]) {
                                    doTime /= 2;
                                }
                                if (totalData.ContainsKey (Convert.ToInt32(match["team"]))) {
                                    totalData[(int)match["team"]] += doTime;
                                    numsData[(int)match["team"]]++;
                                } else {
                                    totalData.Add ((int)match["team"], doTime);
                                    numsData.Add ((int)match["team"], 1);
                                }
                            }
                        }
                    }
                }
            } catch (System.NullReferenceException) {

            }
            foreach (var data in totalData) {
                pushData.Add (data.Key, Math.Round (data.Value / numsData[data.Key], 2));
            }
            return pushData;
        }

        //Checks if the JObject contains any values
        public bool isRankNeeded () {
            return fullData.Count > 0;
        }

        //Returns JObject for matches input string
        private JArray getJSON (String input) {
            JObject tempJSON;
            if (!String.IsNullOrWhiteSpace (input)) {
                try {
                    tempJSON = JObject.Parse (input);
                } catch (NullReferenceException) {
                    System.Diagnostics.Debug.WriteLine ("Caught NullRepEx for ranker JObject");
                    tempJSON = new JObject ();
                }
            } else {
                tempJSON = new JObject (new JProperty ("Matches"));
            }
            if (tempJSON.ContainsKey ("Matches")) {
                return (JArray) tempJSON["Matches"];
            } else {
                return new JArray ();
            }
        }
    }
}