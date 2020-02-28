using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace NRGScoutingApp {

    /* NOTICE: REFERENCE 
     * resetTimer when changing reset Timer text
     * startTimer when changing start timer text
     * climbStart when changing climb timer text
     * cubePicked when changing cube picked/dropped text
     * To change images for buttons, use the android names above, and use iosCubeImage.Source for the ios cube image
     */
    public partial class NewMatchStart : ContentPage {
        protected override bool OnBackButtonPressed() {
            return true;
        }

        public NewMatchStart() {
            BindingContext = this;
            InitializeComponent();
            timeSlider.Maximum = ConstantVars.MATCH_SPAN_MS;
            Preferences.Set("appState", 1);
            NavigationPage.SetHasBackButton(this, false);
            timerValueSetter();
            setEventButtons(isTimerRunning);
            setPickAndDropButtons();
        }

        public static int timerValue = 0;
        private bool firstTimerStart = true;
        public static int pickedTime = 0;
        public static int droppedTime = 0;
        int climbTime = 0;
        public static bool cubeSetDrop;
        public static bool isTimerRunning;
        public static bool setItemToDefault;
        public static List<MatchFormat.Data> events = new List<MatchFormat.Data>();
        public MatchFormat.Data lastEvent = null;

        private DateTime timeStartDate;
        private int timerStartVal;

        protected override void OnAppearing () {

            if (cubeSetDrop) {
                cubeSetDrop = false;
            }
            if (setItemToDefault) {
                setItemToDefault = false;
            }
            setClimbButton ();
            setPickAndDropButtons();
        }

        async void resetClicked (object sender, System.EventArgs e) {
            var ensure = await DisplayActionSheet ("Are you sure you want to reset everything about this match?", ConstantVars.CANCEL, null, ConstantVars.YES);
            if (ensure == ConstantVars.YES) {
                events.Clear ();
                saveEvents ();
                timer.Reset();
                timeSlider.Value = 0;
                isTimerRunning = false;
                climbTime = 0;
            }
        }

        async void startClicked (object sender, System.EventArgs e) {
            if (!isTimerRunning) {
                if (!Preferences.ContainsKey ("timerValue")) {
                    Preferences.Set ("timerValue", (int) 0);
                } else if (Preferences.ContainsKey ("timerValue") && firstTimerStart == true) {
                    timerValue = Convert.ToInt32 (Preferences.Get ("timerValue", 0));
                    timerText.Text = timeToString ((int) timerValue);
                    firstTimerStart = false;
                }
                isTimerRunning = true;
                startTimer.Text = ConstantVars.TIMER_PAUSE;
                setEventButtons (true);
                setClimbButton ();
                timeStartDate = DateTime.Now;
                timerStartVal = 0;
                //setCubeButton ();
                timer.Start();
                await Task.Run (() => {
                        Device.StartTimer (TimeSpan.FromMilliseconds (50), () => {
                            if (timerValue >= ConstantVars.MATCH_SPAN_MS || !isTimerRunning) {
                                Device.BeginInvokeOnMainThread (() => {
                                    startTimer.Text = ConstantVars.TIMER_START;
                                    isTimerRunning = false;
                                    setEventButtons (isTimerRunning);
                                });
                                return false;
                            }
                            Timer_Elapsed();
                            return true;
                        });
                });

            } else if (isTimerRunning) {
                startTimer.Text = ConstantVars.TIMER_START;
                isTimerRunning = false;
                timer.Reset();
                lastTime = 0;
            }
            setEventButtons (isTimerRunning);
            setClimbButton ();
        }

        Stopwatch timer = new Stopwatch();
        int lastTime = 0;
        private void Timer_Elapsed () {
            setPickAndDropButtons(calculateCurrentBalls());

            //if (timerStartVal >= ConstantVars.TIMER_CHECK_INTERVAL) {
            //    Console.WriteLine (DateTime.Now.Subtract (timeStartDate).TotalMilliseconds - timerStartVal);
            //    timerValue += (int) DateTime.Now.Subtract (timeStartDate).TotalMilliseconds -
            //        timerStartVal;
            //    timerStartVal = 0;
            //    timeStartDate = DateTime.Now;
            //}
            int time = (int)timer.ElapsedMilliseconds;
            timerValue += ((int)time - lastTime);
            Device.BeginInvokeOnMainThread (() => {
                timeSlider.Value = timerValue;
                timerText.Text = timeToString ((int)timerValue);
            });
            lastTime = time;
            Preferences.Set ("timerValue", (int) timerValue);
        }

        void timerValueChanged (object sender, Xamarin.Forms.ValueChangedEventArgs e) {
            if (!Preferences.ContainsKey ("timerValue")) {
                Preferences.Set ("timerValue", (int) 0);
            } else if (firstTimerStart) {
                timerValue = (int) Preferences.Get ("timerValue", 0);
                firstTimerStart = false;
            } else {
                Preferences.Set ("timerValue", (int) timeSlider.Value);
            }
            double value = e.NewValue;
            timerText.Text = timeToString ((int) e.NewValue);
            timerValue = (int) (e.NewValue);
            if (!isTimerRunning) {
                setEventButtons (isTimerRunning);
            }
            setPickAndDropButtons();
        }

        void climbClicked (object sender, System.EventArgs e) {
            if (!isTimerRunning) {
                DisplayAlert ("Error", "Timer not Started", "OK");
            } else if (climbTime == 0) {
                //Adds info to to JSON about climb
                climbTime = (int) timerValue;
                events.Add (new MatchFormat.Data { time = climbTime, type = (int) MatchFormat.ACTION.startClimb });
                saveEvents ();
                setClimbButton ();
            }
        }

        public static void saveEvents()
        {
            Preferences.Set("tempMatchEvents", JsonConvert.SerializeObject(MatchFormat.eventsListToJSONEvents(NewMatchStart.events)));
            MatchEvents.update = true;
        }

        public int calculateCurrentBalls()
        {
            int total = 0;
            //Debug.WriteLine(events.Count + "count"); 
            foreach (MatchFormat.Data data in events)
            {
                if (data.type == (int)MatchFormat.ACTION.pick1)
                {
                    total+= data.num;
                }
                else
                {
                    total-= data.num;
                }
                //Debug.WriteLine(data);
            }
            //Debug.WriteLine(total);
            return total;
        }

        MatchFormat.Data findEventAtTime(int time)
        {
            for (int i = 0; i < events.Count; i++)
            {
                if (events[i].time == time)
                {
                    return events[i];
                }
            }
            return null;
        }

        void pickClicked(object sender, System.EventArgs e)
        {
            pickedTime = (int)timerValue;
            if (lastEvent != null)
            {
                MatchFormat.Data initEvent = findEventAtTime(0);
                Debug.WriteLine(lastEvent.num);
                if (pickedTime == 0 &&  initEvent != null && initEvent.type == (int)MatchFormat.ACTION.pick1)
                {
                    initEvent.num++;
                }
                else if (Math.Abs(lastEvent.time - pickedTime) <= ConstantVars.CYCLE_ADD_THRESHOLD && lastEvent.type == (int)MatchFormat.ACTION.pick1)
                {
                    lastEvent.num++;
                }
                else
                {
                    lastEvent = new MatchFormat.Data { time = (int)pickedTime, type = (int)MatchFormat.ACTION.pick1 };
                    events.Add(lastEvent);
                }
            }
            else
            {
                lastEvent = new MatchFormat.Data { time = (int)pickedTime, type = (int)MatchFormat.ACTION.pick1 };
                events.Add(lastEvent);
            }
            saveEvents();
            int balls = calculateCurrentBalls();
            currentCellAmt.Text = balls.ToString();
            setPickAndDropButtons(balls);
        }

        void undoClicked(object sender, System.EventArgs e)
        {
            //Debug.WriteLine("reee"+ lastEvent.ToString());
            if (lastEvent != null)
            {
                //Debug.WriteLine(lastEvent.num);
                bool removed = false;
                if(lastEvent.num <= 1)
                {
                    events.Remove(lastEvent);
                    removed = true;
                }
                else
                {
                    lastEvent.num--;
                }
                if (events.Count > 0 && removed)
                {
                    lastEvent = events[events.Count - 1];
                }
            }
            saveEvents();
            int balls = calculateCurrentBalls();
            currentCellAmt.Text = balls.ToString();
            setPickAndDropButtons(balls);
        }

            void dropClicked(object sender, System.EventArgs e)
        {
            Button pressed = (Button)sender;
            pickedTime = (int)timerValue;
            MatchFormat.Data item = new MatchFormat.Data { time = pickedTime, type = (int)MatchFormat.ACTION.drop1, num = 1 };
            switch (pressed.Text.ToLower())
            {
                case "none":
                    break;
                case "low":
                    item.type = (int)MatchFormat.ACTION.drop1;
                    break;
                case "outside":
                    item.type = (int)MatchFormat.ACTION.drop2;
                    break;
                case "inside":
                    item.type = (int)MatchFormat.ACTION.drop3;
                    break;
                case "":
                    item = null;
                    break;

            }
            if (item != null)
            {
                if (lastEvent != null && item.type == lastEvent.type && Math.Abs(lastEvent.time - pickedTime) <= ConstantVars.CYCLE_ADD_THRESHOLD)
                {
                    lastEvent.num++;
                }
                else
                {
                    lastEvent = item;
                    events.Add(lastEvent);
                }
            }
            saveEvents();
            int balls = calculateCurrentBalls();
            currentCellAmt.Text = balls.ToString();
            setPickAndDropButtons(balls);
        }



        public void setSlider (double value) {
            timeSlider.Value = value;
        }

        //Sets buttons to not clickable if timer is/not running.
        public void setEventButtons (bool setter) {
            //if (setter && isTimerRunning) {
            //    dropInside.BackgroundColor = Color.FromHex ("fdad13");
            //    dropOutside.BackgroundColor = Color.FromHex("fdad13");
            //    dropLow.BackgroundColor = Color.FromHex ("fdad13");
            //    dropNone.BackgroundColor = Color.FromHex("fdad13");
            //} else {
            //    climbStart.BackgroundColor = Color.FromHex ("ffcc6b");
            //    cubePicked.BackgroundColor = Color.FromHex ("ffcc6b");
            //}
            setDropButtons(setter);
            timeSlider.IsEnabled = !isTimerRunning;
            if (timerValue >= ConstantVars.MATCH_SPAN_MS) {
                timerValue = (int) ConstantVars.MATCH_SPAN_MS;
                startTimer.IsEnabled = false;
            } else {
                startTimer.IsEnabled = true;
            }
        }

        //Sets the value of the time if app crashed or match was restored
        private void timerValueSetter () {
            if (!Preferences.ContainsKey ("lastItemPicked")) {
                Preferences.Set ("lastItemPicked", 0);
                Preferences.Set ("lastItemDroppped", 0);
                Preferences.Set ("tempEventString", "");
                Preferences.Set ("tempMatchEvents", "");
            }
            //else if (Preferences.Get ("lastItemPicked", 0) == 0 || Preferences.Get ("lastItemDropped", 0) == 0) { } else if (Preferences.Get ("lastItemDroppped", 0) > Preferences.Get ("lastItemDropped", 0)) {
            //    cubePicked.Text = ConstantVars.ITEM_DROPPED_TEXT_LIVE;
            //}

            if (!Preferences.ContainsKey ("timerValue")) {
                Preferences.Set ("timerValue", (int) timerValue);
            } else if (Preferences.ContainsKey ("timerValue") && firstTimerStart == true) {
                timerValue = Preferences.Get ("timerValue", 0);
                timeSlider.Value = timerValue;
                timerText.Text = timeToString ((int) timerValue);
                firstTimerStart = false;
            }
            try {
                try {
                    events = MatchFormat.JSONEventsToObject (JObject.Parse (Preferences.Get ("tempMatchEvents", "")));
                    if (events != null && events.Count > 0)
                    {
                        lastEvent = events[events.Count - 1];
                    }
                } catch (JsonReaderException) { }
                if (Object.ReferenceEquals (events, null)) {
                    events = new List<MatchFormat.Data>(); 
                }
            } catch (InvalidCastException) { }
            setEventButtons (isTimerRunning);
            int balls = calculateCurrentBalls();
            currentCellAmt.Text = balls.ToString();
            //setCubeButton ();
        }

        //sets robot action buttons based on climb start
        private void setClimbButton () {
            if (events.Exists (x => x.type == (int) MatchFormat.ACTION.startClimb)) {
                setEventButtons (false);
            } else {
                setEventButtons (true);
                climbTime = 0;
            }
        }

        public static string timeToString (int timeValue) {
            int minutes = 0;
            int seconds = 0;
            int milliseconds = 0;
            minutes = timeValue / (int) ConstantVars.MIN_MS;
            timeValue %= (int) ConstantVars.MIN_MS;
            seconds = timeValue / (int) ConstantVars.SEC_MS;
            timeValue %= (int) ConstantVars.SEC_MS;
            milliseconds = timeValue;
            return minutes + ":" + seconds.ToString ("D2") + "." + (milliseconds / 10).ToString ("D2");
        }

        private void setDropButtons(Boolean setter)
        {
            dropLow.IsEnabled = setter;
            dropInside.IsEnabled = setter;
            dropOutside.IsEnabled = setter;
            dropNone.IsEnabled = setter;
        }

        private void setPickAndDropButtons(int currentBalls)
        {
            if (isTimerRunning)
            {
                if (currentBalls >= 5)
                {
                    pickItem.IsEnabled = false;
                }
                else
                {
                    pickItem.IsEnabled = true;
                }
            }
            else if (timerValue == 0 & !isTimerRunning && currentBalls < 5)
            {
                pickItem.IsEnabled = true;
            }
            else
            {
                pickItem.IsEnabled = false;
            }
            if (isTimerRunning)
            {
                if (currentBalls <= 0)
                {
                    setDropButtons(false);
                }
                else
                {
                    setDropButtons(true);
                }
            }
            else
            {
                setDropButtons(false);
            }
        }
        private void setPickAndDropButtons()
        {
            int currentBalls = calculateCurrentBalls();
            setPickAndDropButtons(currentBalls);
        }
    }
}