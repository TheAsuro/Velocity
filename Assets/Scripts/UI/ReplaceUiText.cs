using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ReplaceUiText : MonoBehaviour
{
    private string originalText;

    void Awake()
    {
        originalText = GetComponent<Text>().text;
    }

    void Update()
    {
        //$currentplayer
        SaveData sd = GameInfo.info.getCurrentSave();
        if (sd == null)
            originalText.Replace("$currentplayer", "No player selected!");
        else
            originalText.Replace("$currentplayer", sd.getPlayerName());
    }
}
