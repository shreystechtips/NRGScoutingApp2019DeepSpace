using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace NRGScoutingApp
{
    public class AdapterMethods
    {
        public AdapterMethods()
        {
        }

        public static string getTeamString(int teamnum)
        {
            try
            {
                return teamnum + " - " + App.teamsList[teamnum];
            }
            catch (Exception ex) { }
            return teamnum.ToString();
        }


        public static int getTeamInt(string input, Dictionary<int, string> teams)
        {
            Debug.WriteLine(input);
            try
            {
                var value = input.Split(" - ");
                return Convert.ToInt32(value[0]);
            }
            catch (Exception ex)
            {
                throw new Exception("");
            }

            return 0;
        }
    }
}
