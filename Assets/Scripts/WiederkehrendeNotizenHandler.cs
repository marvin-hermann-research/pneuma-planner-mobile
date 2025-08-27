using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WiederkehrendeNotizenHandler : MonoBehaviour
{
    public DiscordBot DiscordBot;
    public bool wiederkehrendeNotizenHaveBeenSet;
    public string[] wiederkehrendeNotizen;
    private bool waitedForBot;


    private async void Update()
    {
        if (DiscordBot.botIsReady && !waitedForBot)
        {
            waitedForBot = true;
            wiederkehrendeNotizen = await DiscordBot.GetNotizenWiederkehrendInLines();  
        }

        if (wiederkehrendeNotizen.Length != 0 && !wiederkehrendeNotizenHaveBeenSet)
        {
            wiederkehrendeNotizenHaveBeenSet = true;
        }
    }

    public bool checkIfDateHasWiederkehrendeNotiz(DateTime zellenDatum)
    {
        string zellenDatumString = zellenDatum.ToString("dd-MM");
        foreach (string notiz in wiederkehrendeNotizen)
        {
            if (notiz.Contains(zellenDatumString))
            {
                return true;
            }
        }
        return false;
    }

    public string getWiederkehrendeNotizOfDate(DateTime zellenDatum)
    {
        string zellenDatumString = zellenDatum.ToString("dd-MM");
        foreach (string notiz in wiederkehrendeNotizen)
        {
            if (notiz.Contains(zellenDatumString))
            {
                return trimmNotiz(notiz);
            }
        }
        return null;
    }

    private string trimmNotiz(string notiz)
    {
        int colonIndex = notiz.IndexOf(':');
        if (colonIndex != -1)
        {
            return notiz.Substring(colonIndex + 1).Trim();
        }
        return notiz;
    }
}
