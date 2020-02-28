using System;

namespace NRGScoutingApp
{
    public static class HideAPIKey
    {
        public static readonly string SCOUTING_API_KEY = Environment.GetEnvironmentVariable("SERVER_API_KEY");
    }
}
