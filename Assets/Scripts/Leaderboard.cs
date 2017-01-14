using System;
using System.Collections.Generic;
using Api;
using Demos;
using Newtonsoft.Json;

internal static class Leaderboard
{
    private const string LEADERBOARD_URL = "https://theasuro.de/Velocity/Api/leaderboard.php";

    //Request leaderboard entries from the server
    public static void GetEntries(string map, int index, int count, Action<LeaderboardEntry[]> callback)
    {
        HttpApi.StartRequest(LEADERBOARD_URL + "?level=" + map + "&index=" + index + "&count=" + count, "GET", (result) => callback(ParseEntries(result.text, index, map)));
    }

    private static LeaderboardEntry[] ParseEntries(string entryJson, int rankOffset = 0, string specificMap = "")
    {
        if (entryJson == "")
            return new LeaderboardEntry[0];

        LeaderboardResult[] results = JsonConvert.DeserializeObject<LeaderboardResult[]>(entryJson);
        LeaderboardEntry[] entries = new LeaderboardEntry[results.Length];
        for (int i = 0; i < results.Length; i++)
        {
            entries[i] = results[i];
        }

        for (int i = 0; i < entries.Length; i++)
        {
            if (specificMap != "")
                entries[i].map = specificMap;
            entries[i].rank = rankOffset + i + 1;
        }

        return entries;
    }

    public static void SendEntry(string player, decimal time, string map, string token, Demo demo)
    {
        //TODO: upload demo when needed
        Dictionary<string, string> data = new Dictionary<string, string>();
        data.Add("player", player);
        data.Add("time", time.ToString());
        data.Add("level", map);
        data.Add("token", token);

        // TODO: Display when entry was successful
        HttpApi.StartRequest(LEADERBOARD_URL, "POST", null, data);
    }

    public static void GetRecord(string map, Action<LeaderboardEntry> callback)
    {
        // TODO: Fix map with bad characters
        HttpApi.StartRequest(LEADERBOARD_URL + "?level=" + map, "GET", (result) =>
        {
            var entries = ParseEntries(result.text);
            if (entries.Length > 0)
                callback(entries[0]);
            else
                callback(null);
        });
    }

    public static void GetDemo(int entryId, Action<Demo> callback)
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
    public string playerName;
    public string map;
    public decimal time;
    public int rank;

    public static implicit operator LeaderboardEntry(LeaderboardResult v)
    {
        return new LeaderboardEntry() { id = v.id, playerName = v.playerName, map = v.mapName, time = v.gameTime, rank = -1 };
    }
}

public struct LeaderboardResult
{
    public string mapName;
    public int id;
    public string playerName;
    public decimal gameTime;
}