using System.Globalization;
using System.IO;
using Demos;
using Game;
using UI;
using UnityEngine;

namespace Api
{
    public static class Leaderboard
    {
        private const string MAP_PREFIX = "?Map=";
        private const string OFFSET_PREFIX = "&Offset=";
        private const string LIMIT_PREFIX = "&Limit=";
        private const string VERSION_STRING = "&Version=4";
        private const string GET = "GET";
        private const string POST = "POST";
        private const decimal TICKS_PER_SECOND = 10000000m;

        //Request leaderboard entries from the server
        public static LeaderboardRequest GetEntries(MapData map, int offset, int limit)
        {
            string url = Url.HIGHSCORES + MAP_PREFIX + map.id + OFFSET_PREFIX + offset + LIMIT_PREFIX + limit + VERSION_STRING;
            return new LeaderboardRequest(new ApiRequest(url, GET), map, offset);
        }

        public static LeaderboardRequest GetRecord(MapData map)
        {
            return new LeaderboardRequest(new ApiRequest(Url.HIGHSCORES + MAP_PREFIX + map.id + LIMIT_PREFIX + "1", GET), map, 0);
        }

        public static ApiRequest SendEntry(PlayerSave player, int mapID, long time, Demo demo)
        {
            StringRequestData data = new StringRequestData
            {
                {"User", player.ID.ToString()},
                {"Time", ((decimal)time / TICKS_PER_SECOND).ToString(CultureInfo.InvariantCulture)},
                {"Map", mapID.ToString()},
                {"Token", player.Token}
            };

            // TODO: Display when entry was successful
            ApiRequest rq = new ApiRequest(Url.HIGHSCORES, POST, data);
            rq.OnDone += (sender, args) =>
            {
                if (args.Error)
                    GameMenu.SingletonInstance.ShowError(args.ErrorText);
                else if (int.Parse(args.StringResult) != 0)
                {
                    using (MemoryStream stream = new MemoryStream())
                    {
                        demo.SaveToStream(stream);
                        RequestData demoData = new BinaryRequestData(stream.GetBuffer());
                        // TODO: get run id from highscore request
                        ApiRequest demoRq = new ApiRequest(Url.DEMOS + "/" + 50 + "/" + 7, POST, demoData);
                        demoRq.OnDone += (o, eventArgs) =>
                        {
                            if (demoRq.Error)
                                GameMenu.SingletonInstance.ShowError(demoRq.ErrorText);
                            else
                                Debug.Log("Demo upload finished!");
                        };
                        demoRq.StartRequest();
                    }
                }
            };
            rq.StartRequest();
            return rq;
        }

        public static ApiRequest GetDemo(int playerID, int mapID)
        {
            ApiRequest rq = new ApiRequest(Url.DEMOS + "/" + playerID + "/" + mapID, "GET");
            rq.StartRequest();
            return rq;
        }
    }

    public struct LeaderboardEntry
    {
        public int PlayerID { get; set; }
        public string PlayerName { get; set; }
        public decimal Time { get; set; }
        public int MapID { get; set; }
        public int Rank { get; set; }
    }
}