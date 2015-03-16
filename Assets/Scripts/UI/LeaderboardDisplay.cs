using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class LeaderboardDisplay : MonoBehaviour
{
    public InputField mapNameInput;
    public List<LeaderboardPanel> entryPanels; //Must always have 10 elements!

    private int startIndex = 0;

    void Awake()
    {
        if (mapNameInput)
            mapNameInput.onEndEdit.AddListener(LoadMap);
    }

    public void LoadMap(string mapName)
    {
        StartCoroutine(Leaderboard.GetEntries(mapName, startIndex, DisplayData));
    }

    private void DisplayData(string data)
    {
        if (entryPanels.Count != 10)
            throw new InvalidOperationException("Data can only be displayed if entryPanels has exactly 10 elements!");

        string[] lines = data.Split('\n');
        EntryRow[] rows = new EntryRow[10];

        for(int i = 0; i < 10; i++)
        {
            if (i >= lines.Length || lines[i].Equals(""))
            {
                rows[i] = new EntryRow();
                rows[i].isEmpty = true;
            }
            else
            {
                rows[i] = EntryRow.Parse((startIndex + i + 1) + "|" + lines[i]);
            }
        }

        for(int i = 0; i < Math.Min(10, rows.Length); i++)
        {
            if(rows[i].isEmpty)
            {
                entryPanels[i].time = "";
                entryPanels[i].player = "";
                entryPanels[i].rank = "";
                entryPanels[i].SetButtonAction(delegate { });
                entryPanels[i].SetButtonActive(false);
            }
            else
            {
                entryPanels[i].time = rows[i].time.ToString("0.0000");
                entryPanels[i].player = rows[i].player;
                entryPanels[i].rank = rows[i].rank;
                entryPanels[i].SetButtonAction(delegate { }); //TODO: Set this to download demo with demoID
                entryPanels[i].SetButtonActive(true);
            }
        }
    }

    public void AddIndex(int add)
    {
        startIndex += add;
        if (startIndex < 0)
            startIndex = 0;
        LoadMap(mapNameInput.text);
    }
}

public struct EntryRow
{
    public string rank;
    public string player;
    public decimal time;
    public int demoId;
    public bool isEmpty;

    /// <summary>
    /// Creates an Entry row from a string
    /// </summary>
    /// <param name="data">Format: "rank|player|time" or "rank|player|time|demoID"</param>
    /// <returns>The parsed Entry row</returns>
    public static EntryRow Parse(string data)
    {
        EntryRow row = new EntryRow();

        if(!data.Contains("|"))
            throw new InvalidCastException("Can't parse the string!");

        string[] parts = data.Split('|');

        if (parts.Length < 3 || parts.Length > 4)
        { Debug.Log(data); throw new InvalidCastException("Can't parse the string!"); }

        row.rank = parts[0];
        row.player = parts[1];

        if(!decimal.TryParse(parts[2], out row.time))
            throw new InvalidCastException("Can't parse the string!");

        if (parts.Length == 4)
            if(!int.TryParse(parts[3], out row.demoId))
                throw new InvalidCastException("Can't parse the string!");

        row.isEmpty = false;

        return row;
    }
}