using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

static class Leaderboard
{
    private const string leaderboardUrl = "http://theasuro.de/Velocity/mapleaderboardtemp.php";
    private const string recordUrl = "http://theasuro.de/Velocity/wr.php";
    private const string entryUrl = "http://theasuro.de/Velocity/newentry.php";

    public delegate void LeaderboardCallback(string entryData);

    //Request leaderboard entries from the server
    public static IEnumerator<WWW> GetEntries(string map, int index, LeaderboardCallback callback)
    {
        WWWForm form = new WWWForm();
        form.AddField("Map", map);
        form.AddField("Index", index);

        WWW www = new WWW(leaderboardUrl, form);
        return WaitForData(www, callback);
    }

    public static void SendEntry(string player, decimal time, string map, string hash)
    {
        WWWForm form = new WWWForm();

        form.AddField("Player", player);
        form.AddField("Time", time.ToString());
        form.AddField("Map", map);
        form.AddField("Hash", hash);

        new WWW(entryUrl, form);
    }

    public static IEnumerator<WWW> GetRecord(string map, LeaderboardCallback callback)
    {
        WWWForm form = new WWWForm();

        form.AddField("Map", map);

        WWW www = new WWW(recordUrl, form);
        return WaitForData(www, callback);
    }

    private static IEnumerator<WWW> WaitForData(WWW www, LeaderboardCallback callback)
    {
        yield return www;
        callback(www.text);
    }
}