using System.Collections.Generic;
using Demos;
using Game;
using UnityEngine;

namespace Api
{
    internal static class Leaderboard
    {
        private const string LEADERBOARD_URL = "https://api.theasuro.de/velocity/highscores";
        private const string MAP_PREFIX = "?Map=";
        private const string OFFSET_PREFIX = "&Offset=";
        private const string LIMIT_PREFIX = "&Limit=";
        private const string VERSION_STRING = "&Version=4";
        private const string GET = "GET";
        private const string POST = "POST";

        //Request leaderboard entries from the server
        public static LeaderboardRequest GetEntries(MapData map, int offset, int limit)
        {
            string url = LEADERBOARD_URL + MAP_PREFIX + map.id + OFFSET_PREFIX + offset + LIMIT_PREFIX + limit + VERSION_STRING;
            return new LeaderboardRequest(new ApiRequest(url, GET), map, offset);
        }

        public static void SendEntry(PlayerSave player, int mapID, long time, Demo demo)
        {
            //TODO: upload demo when needed
            Dictionary<string, string> data = new Dictionary<string, string> {{"User", player.ID.ToString()}, {"Time", time.ToString()}, {"Map", mapID.ToString()}, {"Token", player.Token}};

            // TODO: Display when entry was successful
            ApiRequest rq = new ApiRequest(LEADERBOARD_URL, POST, data);
            rq.OnDone += (sender, args) =>
            {
                if (rq.Error)
                    Debug.LogError(rq.ErrorText);
            };
            rq.StartRequest();
        }

        public static LeaderboardRequest GetRecord(MapData map)
        {
            return new LeaderboardRequest(new ApiRequest(LEADERBOARD_URL + MAP_PREFIX + map.id + LIMIT_PREFIX + "1", GET), map, 0);
        }
    }

    public class LeaderboardEntry
    {
        public int id;
        public string playerName;
        public string map;
        public long time;
        public int rank;

        public static implicit operator LeaderboardEntry(LeaderboardResult v)
        {
            return new LeaderboardEntry() { id = v.ID, playerName = v.Name, time = v.Time, rank = -1 };
        }
    }

    public struct LeaderboardResult
    {
        public int ID;
        public string Name;
        public long Time;
    }
}