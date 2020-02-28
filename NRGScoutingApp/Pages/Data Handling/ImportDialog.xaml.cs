using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rg.Plugins.Popup.Services;
using Xamarin.Essentials;
using System.Net;
using System.Collections.Generic;
using System.Diagnostics;

namespace NRGScoutingApp {
    public partial class ImportDialog {
        public ImportDialog () {
            InitializeComponent ();
        }

        void cancelClicked (object sender, System.EventArgs e) {
            PopupNavigation.Instance.PopAsync (true);
        }

        void Handle_TextChanged (object sender, Xamarin.Forms.TextChangedEventArgs e) {
            importButton.IsEnabled = !String.IsNullOrWhiteSpace (e.NewTextValue);
        }

        async void importClicked (object sender, System.EventArgs e) {
            JObject data = MatchParameters.initializeEventsObject ();
            if (data.ContainsKey(""))
            {
                data.Remove("");
            }
            if (importData.Text.ToLower().Contains("https://pastebin.com/raw/")) {
                try
                {
                    String response = new WebClient().DownloadString(importData.Text);
                    importData.Text = response;
                }
                catch {
                    await DisplayAlert("Error", "No Internet!", "OK");
                    return;
                }
            }
            try {
                JObject import = (JObject) JsonConvert.DeserializeObject (importData.Text);
                foreach (JProperty temp in import.Properties()) {
                    Debug.WriteLine(temp);
                    JObject importJSON = new JObject();
                    //importJSON = importJSON;
                    try
                    {
                        importJSON = temp.Value.ToObject<JObject>();
                        if (importJSON.ContainsKey("Matches") || importJSON.ContainsKey("PitNotes"))
                        {
                            if (importJSON.ContainsKey("Matches"))
                            {
                                await addMatchItemsChecker(data, importJSON, (String)temp.Name); //TODO
                            }
                            if (importJSON.ContainsKey("PitNotes"))
                            {
                                await addPitItemsChecker(data, importJSON, (String)temp.Name); //TODO
                            }
                            Debug.WriteLine("final" + data);
                            Preferences.Set(ConstantVars.APP_DATA_STORAGE, JsonConvert.SerializeObject(data));
                            PopupNavigation.Instance.PopAsync();
                        }
                        else
                        {
                            await DisplayAlert("Alert", "Error in Data", "OK");
                        }
                    }
                    catch
                    {
                        
                    }
                }
            } catch (JsonReaderException ex) {
                Debug.WriteLine(ex);
                await DisplayAlert ("Alert", "Error in Data", "OK");
            }
        }

        private async Task addPitItemsChecker (JObject data, JObject importJSON, String comp) {
            //JArray temp = (JArray) data["PitNotes"];
            JArray importTemp = (JArray)importJSON["PitNotes"];
            List<JToken> tempList = new List<JToken>();
            if(data.ContainsKey(comp) && (JArray)data[comp]["PitNotes"] != null)
            {
                tempList = ((JArray)data[comp]["PitNotes"]).ToList();
            }
            else if (!data.ContainsKey(comp))
            {
                data[comp] = new JObject();
                data[comp]["PitNotes"] = new JArray();
            }
            else
            {
                data[comp]["PitNotes"] = new JArray();
            }
            JArray temp = (JArray)data[comp]["PitNotes"];
            Debug.WriteLine("yet" + temp);
            Debug.WriteLine("data" + data);
            foreach (var match in importTemp.ToList ()) {
                if (tempList.Exists (x => x["team"].Equals (match["team"]))) {
                    var item = tempList.Find (x => x["team"].Equals (match["team"]));
                    temp.Remove (item);
                    for (int i = 0; i < ConstantVars.QUESTIONS.Length; i++) {
                        try
                        {
                            List<String> vals = new List<String>();
                                String[] import = { match["q" + i].ToString() };
                                String[] existing = { item["q" + i].ToString() };
                                if (match["q" + i].ToString().Contains(ConstantVars.entrySeparator))
                                {
                                    try
                                    {
                                        import = match["q" + i].ToString().Split(ConstantVars.entrySeparator);
                                    }
                                    catch { }
                                }
                                if (item["q" + i].ToString().Contains(ConstantVars.entrySeparator))
                                {
                                    try
                                    {
                                        existing = item["q" + i].ToString().Split(ConstantVars.entrySeparator);
                                    }
                                    catch { }
                                }
                                foreach (String input in import)
                                {
                                    String replaced = input.Replace("&", "and");
                                    if (!vals.Contains(replaced)) {
                                        vals.Add(replaced);
                                    }
                                }
                                foreach (String input in existing)
                                {
                                    String replaced = input.Replace("&", "and");
                                    if (!vals.Contains(replaced))
                                    {
                                        vals.Add(replaced);
                                    }
                                }
                                String total = vals[0];
                                for(int j = 1; j < vals.Count; j++) {
                                    total += ConstantVars.entrySeparator + vals[j];
                                }
                                Console.WriteLine(total);
                                item["q" + i] = total;
                            }
                        catch
                        {
                            Console.WriteLine("oof");
                        }
                    }
                    temp.Add (item);
                } else {
                    temp.Add (match);
                }
            }
        }

        private async Task addMatchItemsChecker (JObject data, JObject importJSON, string comp) {
            int tooMuch = 0;
            int mode = 0; // 1 for overwite all, 2 for ignore all
            JArray importTemp = (JArray) importJSON["Matches"];
            List<JToken> tempList = new List<JToken>();
            if (data.ContainsKey(comp) && (JArray)data[comp]["Matches"] != null)
            {
                tempList = ((JArray)data[comp]["Matches"]).ToList();
            }
            else if (!data.ContainsKey(comp))
            {
                data[comp] = new JObject();
                data[comp]["Matches"] = new JArray();
            }
            else
            {
                data[comp]["Matches"] = new JArray();
            }
            JArray temp = (JArray)data[comp]["Matches"];
            foreach (var match in importTemp.ToList ()) {
                Debug.WriteLine("hi" + match);
                if (tempList.Exists (x => x["matchNum"].Equals (match["matchNum"]) && x["side"].Equals (match["side"]))) {
                    var item = tempList.Find (x => x["matchNum"].Equals (match["matchNum"]) && x["side"].Equals (match["side"]));
                    if (!item["team"].Equals (match["team"])) {
                        if (mode == 1)
                        {
                            temp.Remove(item);
                            temp.Add(match);
                        }
                        else if (mode == 0)
                        {
                            tooMuch++;
                            if (tooMuch <= 1)
                            {
                                var add = await DisplayAlert ("Warning!",
                                    "\nEvent: " + AdapterMethods.getEventName(comp) +
                                    "Match: " + item["matchNum"] +
                                    "\nTeam: " + item["team"] +
                                    "\nSide: " + MatchFormat.matchSideFromEnum (Convert.ToInt32 (item["side"])) +
                                    "\nConflicts with Existing Match", "Overwite", "Ignore");
                                if (add) {
                                    temp.Remove (item);
                                    temp.Add (match);
                                }
                            }
                            else
                            {
                                var add = await DisplayActionSheet ("Warning!",
                                    "Event: " + AdapterMethods.getEventName(comp) +
                                    "\nMatch: " + item["matchNum"] +
                                    "\nTeam: " + item["team"] +
                                    "\nSide: " + MatchFormat.matchSideFromEnum(Convert.ToInt32(item["side"])) +
                                    "\nConflicts with Existing Match", null, null, "Overwite", "Ignore", "Overwite All", "Ignore All");
                                if (add.Equals("Overwite"))
                                {
                                    temp.Remove(item);
                                    temp.Add(match);
                                }
                                else if (add.Equals("Overwite All"))
                                {
                                    temp.Remove(item);
                                    temp.Add(match);
                                    mode = 1;
                                }
                                else if (add.Equals("Ignore All"))
                                {
                                    mode = 2;
                                }
                            }
                        }
                    }
                } else {
                    temp.Add (match);
                }
            }
            //return data;
        }
    }
}