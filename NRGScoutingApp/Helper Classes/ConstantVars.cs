using System;
namespace NRGScoutingApp {
    public class ConstantVars {
        /*
         * IMPORTANT NOTE:
         * VARIABLES CONTAINING "LIVE" ARE USED IN the NewMatchStart page
         */

        /*
         * 
         * CubeDropDialog
         */

        public static readonly string SCOUTING_API_NAME = "NRG_API_KEY";
        public static readonly string SCOUTING_API_SITE = "http://api.nrg948.com";
        //https://nrg-scouting-api.herokuapp.com
        public static readonly int APP_YEAR = 2020;
        public static readonly String TEAM_LIST_STORAGE = "TeamsList";
        public static readonly String EVENT_LIST_STORAGE = "EventsList";
        public static readonly String CURRENT_EVENT_NAME = "CurrentEvent";
        public static readonly String COMPETITION_MATCHES_LIST = "CompMatchList";

        public static readonly String APP_DATA_STORAGE = "AllData";


        public static readonly int CYCLE_ADD_THRESHOLD = 5000; // # of milliseconds between last (same) event to add to the last event (not make a new one)
        public static readonly String DROP_1_DIALOG_TEXT = "Low";
        public static readonly String DROP_2_DIALOG_TEXT = "Medium";
        public static readonly String DROP_3_DIALOG_TEXT = "High";
        public static readonly String DROP_4_DIALOG_TEXT = "Ship";

        /*
         * Match Events Page
         */
        public static readonly String PICK_ITEM_1_IMAGE = "ic_picked_cube_yellow.png";
        public static readonly String PICK_ITEM_2_IMAGE = "ic_picked_cube_yellow.png";
        public static readonly String DROP_ITEM_IMAGE = "ic_dropped_cube_yellow.png";
        public static readonly String DROP_1_IMAGE = "ic_dropped_cube_yellow.png";
        public static readonly String DROP_2_IMAGE = "ic_dropped_cube_yellow.png";
        public static readonly String DROP_3_IMAGE = "ic_dropped_cube_yellow.png";
        public static readonly String DROP_4_IMAGE = "ic_dropped_cube_yellow.png";
        public static readonly String DROP_COLLECTOR_IMAGE = "ic_exchange.png";
        public static readonly String DROP_NONE_IMAGE = "ic_cancel.png";
        public static readonly String START_CLIMB_IMAGE = "ic_climb_yellow.png";

        public static readonly String PICK_ITEM_1_TEXT = "Picked Power Cell";
        //public static readonly String PICK_ITEM_2_TEXT = "Picked Cargo";
        public static readonly String DROP_ITEM_TEXT = "Dropped Item";
        public static readonly String DROP_1_TEXT = "Low Port";
        public static readonly String DROP_2_TEXT = "Outer Port";
        public static readonly String DROP_3_TEXT = "Inner Port";
        //public static readonly String DROP_4_TEXT = "Dropped Ship";
        public static readonly String DROP_NONE_TEXT = "Dropped None";
        public static readonly String START_CLIMB_TEXT = "Start Climb";
        public static readonly String DROP_KEYWORD = "Drop";
        public static readonly String PICK_KEYWORD = "Pick";

        /*
         * INTERNAL VARIABLES FOR SETTING IMPORTANT TIMER AND BUTTON VALUES (DO NOT CHANGE THIS)
         */
        public static readonly double MATCH_SPAN_MS = 150000;
        public static readonly double MIN_MS = 60000;
        public static readonly double SEC_MS = 1000;
        public static readonly String ITEM_PICKED_TEXT_LIVE = "Item Picked";
        public static readonly String ITEM_DROPPED_TEXT_LIVE = "Item Dropped";
        public static readonly String ITEM_DROPPED_IMAGE_LIVE = "ic_drop_cube.png";
        public static readonly String ITEM_PICKED_IMAGE_LIVE = "ic_picked_cube.png";
        public static readonly String TIMER_START = "Start Timer";
        public static readonly String TIMER_PAUSE = "Pause Timer";
        public static readonly String PICK_1_TEXT = "Pick Hatch";
        public static readonly String PICK_2_TEXT = "Pick Cargo";
        public static readonly String CANCEL = "Cancel";
        public static readonly String YES = "Yes";

        public static readonly String ITEM_PICK = "itemPick";
        public static readonly String ITEM_DROP = "itemDrop";
        public static readonly String ROBOT_CLIMB = "robotclimb";
        public static readonly int NUM_DROP_OPTIONS = 4;

        public static readonly int TIMER_INTERVAL_ANDROID = 100;
        public static readonly int TIMER_INTERVAL_IOS = 10;
        public static readonly int TIMER_CHECK_INTERVAL = 1000;

        public static readonly String RED_1_TEXT = "Red 1";
        public static readonly String RED_2_TEXT = "Red 2";
        public static readonly String RED_3_TEXT = "Red 3";
        public static readonly String BLUE_1_TEXT = "Blue 1";
        public static readonly String BLUE_2_TEXT = "Blue 2";
        public static readonly String BLUE_3_TEXT = "Blue 3";

        //PARAMETERS PAGE
        public static readonly String LVL_1_CLIMB = "Level 1";
        public static readonly String LVL_2_CLIMB = "Level 2";
        public static readonly String LVL_3_CLIMB = "Level 3";

        /*
         * RANKER VALUES
         * NOTE: THE LOWER THE VALUES FOR RANK, THE BETTER
         */
        //Autonomous
        public static readonly int AUTO_LENGTH = 15000;
        public static readonly double MULT_SANDSTORM_MANUAL = 1;
        public static readonly double MULT_SANDSTORM_AUTO = 0.5;
        public static readonly double PTS_BASELINE_NONE = 0;
        public static readonly double PTS_BASELINE_LVL_1 = 1;
        public static readonly double PTS_BASELINE_LVL_2 = 3;

        //Game Piece Manipulation
        public static readonly double PTS_GAME_PIECE = 1;
        public static readonly double PTS_DROP_LVL_1 = 1;
        public static readonly double PTS_DROP_LVL_2 = 2;
        public static readonly double PTS_DROP_LVL_3 = 3;

        // This is just examples of multiplier, should be changed soon
        public static readonly double TIME_NERF = 10000;
        public static readonly double OVERALL_MULT = 10;
        public static readonly double CARGO_MULTIPLIER = 3 * MATCH_SPAN_MS;
        public static readonly double HATCHER_MULTIPLIER = 2 * MATCH_SPAN_MS;
        public static readonly double CLIMB_MULTIPLIER = 5;
        public static readonly double DROP_1_MULTIPLIER = 1 * MATCH_SPAN_MS;
        public static readonly double DROP_2_MULTIPLIER = 2 * MATCH_SPAN_MS;
        public static readonly double DROP_3_MULTIPLIER = 3 * MATCH_SPAN_MS;
        public static readonly double DROP_4_MULTIPLIER = 3 * MATCH_SPAN_MS;
        public static readonly double DROP_AMOUNT_MULTIPLIER = 3;

        //Climb
        public static readonly double PTS_NEED_HELP_LVL_2 = 1;
        public static readonly double PTS_NEED_HELP_LVL_3 = 1;
        public static readonly double PTS_SELF_LVL_0 = 0;
        public static readonly double PTS_SELF_LVL_1 = 5;
        public static readonly double PTS_SELF_LVL_2 = 30;
        public static readonly double PTS_SELF_LVL_3 = 30; //TODO:
        public static readonly double PTS_HELPED_LVL_2 = 1;
        public static readonly double PTS_HELPED_LVL_3 = 2;
        public static readonly int PTS_LVL_1_CLIMB = 3;
        public static readonly int PTS_LVL_2_CLIMB = 6;
        public static readonly int PTS_LVL_3_CLIMB = 12;

        /*
         * Rankings Detail View Page
         */
        public static readonly String[] scoreBaseVals = { "Overall: ", "Cargo: ", "Hatch: ", "Climb: ", "Low: ", "Medium: ", "High: ", "Ship: " };
        public static readonly int numRankTypes = scoreBaseVals.Length;
        public static readonly String noVal = "Empty";

        /*
         * PIT SCOUTING
         */
        //Separates the entries if same team was scouted twice
        public static readonly String entrySeparator = "\n:::::::\n";
        public static readonly String[] QUESTIONS = {
            "What drive base do you use?",
            "How much does your robot weigh",
            "Drive practice hours? (estimate)",
            "Can you climb? if so where on the bar and at what level can you climb?",
            "Low goal or high goal?",
            "Inner port capability?",
            "Trench run capability?",
            "How many power cells per match? (estimate)",
            "What does auto do?",
            "Anything else you would like to tell us?",
            "Other?"

        };

        public enum TEAM_SELECTION_TYPES {
            match,
            pit,
            teamSelection
        }

        /*
         * Export Dialog Text
         */
        public static readonly String[] exportTypes = { "Cancel", "All", "Match", "Pit", "Online Paste Bin" };
    }
}