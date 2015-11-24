using System;
using System.Collections.Generic;
using System.Text;

namespace Api
{
    static class Leaderboard
    {
        private const string LEADERBOARD_URL = "http://theasuro.de/Velocity/Test/leaderboard.php";

        //Request leaderboard entries from the server
        public static void GetEntries(string map, int index, int count, Action<List<LeaderboardEntry>> callback)
        {
            HttpApi.StartRequest(LEADERBOARD_URL + "?level=" + map + "&index=" + index + "&count=" + count, "GET", (result) => callback(ParseEntries(result.text, index)));
        }

        private static List<LeaderboardEntry> ParseEntries(string entryJSON, int rankOffset = 0)
        {
            // TODO: parse
            return new List<LeaderboardEntry>();
        }

        public static void SendEntry(string player, decimal time, string map, string hash, Demo demo)
        {
            //TODO: only upload demo when needed
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("Player", player);
            data.Add("Time", time.ToString());
            data.Add("Map", map);
            data.Add("Hash", hash);
            data.Add("Demo", Encoding.ASCII.GetString(demo.GetBinaryData()));

            // TODO: Display when entry was successful
            HttpApi.StartRequest(LEADERBOARD_URL, "POST", null, data);
        }

        public static void GetRecord(string map, Action<LeaderboardEntry> callback)
        {
            // TODO: Fix map with bad characters
            // TODO: Fix if result has no entries
            HttpApi.StartRequest(LEADERBOARD_URL + "?map=" + map, "GET", (result) => callback(ParseEntries(result.text)[0]));
        }

        public static void GetDemo(int entryID, Action<Demo> callback)
        {
            /*byte[] data = System.Text.Encoding.ASCII.GetBytes(demoText);
            MemoryStream stream = new MemoryStream(data);
            Demo downloadedDemo = new Demo(new BinaryReader(stream));*/
            // TODO: everything
        }
    }

    public class LeaderboardEntry
    {
        public int id;
        public int rank;
        public string map;
        public string playerName;
        public decimal time;
    }
}