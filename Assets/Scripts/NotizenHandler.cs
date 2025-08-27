using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;


public class NotizenHandler : MonoBehaviour
{
    public KalenderLogik kalenderLogik;
    public WiederkehrendeNotizenHandler wiederkehrendeNotizen;
    public DiscordBot DiscordBot;
    public bool notizenHaveBeenSet;
    private bool waitedForBot;
    public string[] notizen;

    private string letzterGedrückterTag;

    public GameObject knopfPanel;
    public GameObject infoPanelOK;
    public GameObject infoPanelAbbruch;
    public GameObject infoPanelLöschen;
    public GameObject canvas;

    public TMP_InputField notizenInputField;
    void Start()
    {
        // Durch alle Buttons iterieren und das Click-Event dynamisch zuweisen
        foreach (GameObject zelle in kalenderLogik.Zellen)
        {
            // Dynamische Zuweisung des Click-Events ohne den Text im Voraus zu kennen
            zelle.GetComponent<Button>().onClick.AddListener(() => zelleClicked(zelle.GetComponent<Button>()));
        }

        letzterGedrückterTag = DateTime.Now.Day.ToString("D2"); // Tag des heutigen Datums im Format "DD"
    }

    private void zeigeNotizan()
    {
        // Formatieren des aktuell betrachteten Datums als String
        string datum = letzterGedrückterTag.PadLeft(2, '0') + "-" + kalenderLogik.betrachteterMonat.ToString("D2") + "-" + kalenderLogik.betrachtetesJahr;

        // Initialisiere einen StringBuilder für den Notiztext
        System.Text.StringBuilder notizBuilder = new System.Text.StringBuilder();

        // 1. Durchlaufe das wiederkehrendeNotizen.wiederkehrendeNotizen Array und füge passende Notizen hinzu
        foreach (string wiederkehrendeNotiz in wiederkehrendeNotizen.wiederkehrendeNotizen)
        {
            string gekürztesDatum = datum.Substring(0, 5);
            if (wiederkehrendeNotiz.StartsWith(gekürztesDatum))
            {
                notizBuilder.AppendLine(trimmNotiz(wiederkehrendeNotiz)); // Füge die wiederkehrende Notiz hinzu
            }
        }

        // 2. Durchlaufe das normale notizen Array und füge passende Notizen hinzu
        foreach (string gespeicherteNotiz in notizen)
        {
            if (gespeicherteNotiz.StartsWith(datum))
            {
                notizBuilder.AppendLine(trimmNotiz(gespeicherteNotiz)); // Füge die normale Notiz hinzu
            }
        }

        // Überprüfe, ob Notizen gefunden wurden
        if (notizBuilder.Length > 0)
        {
            notizenInputField.text = notizBuilder.ToString(); // Zeige alle gefundenen Notizen an
        }
        else
        {
            notizenInputField.text = "Keine Notiz für dieses Datum."; // Falls keine Notizen existieren
        }
    }


    void zelleClicked(Button clickedButton)
    {
        if (clickedButton.GetComponentInChildren<TextMeshProUGUI>().text == "")
        {
            return;
        }
        letzterGedrückterTag = clickedButton.GetComponentInChildren<TextMeshProUGUI>().text;
        knopfPanel.transform.SetParent(canvas.transform); //mache knopf panel sichtbar
        zeigeNotizan();
    }

    private async void Update()
    {
        if (DiscordBot.botIsReady && !waitedForBot && wiederkehrendeNotizen.wiederkehrendeNotizenHaveBeenSet)
        {
            waitedForBot = true;
            notizen = await DiscordBot.GetNotizenInLines();
        }

        if (notizen.Length !=0 && !notizenHaveBeenSet)
        {
            notizenHaveBeenSet = true;
            zeigeNotizan();
        }
    }

    public bool checkIfDateHasNotiz(DateTime zellenDatum)
    {
        string zellenDatumString = zellenDatum.ToString("dd-MM-yyyy");
        foreach (string notiz in notizen)
        {
            if (notiz.Contains(zellenDatumString))
            {
                return true;
            }
        }
        return false;
    }

    public string getNotizOfDate(DateTime zellenDatum)
    {
        string zellenDatumString = zellenDatum.ToString("dd-MM-yyyy");
        foreach (string notiz in notizen)
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
    
    public void abbruchKnopf()
    {
        infoPanelAbbruch.SetActive(true);
        Invoke("abbruchPanelweg", 2f);
        knopfPanel.transform.SetParent(null);
    }

    public void notizLöschenKnopf()
    {
        // Datum als String formatieren
        string datum = letzterGedrückterTag.PadLeft(2, '0') + "-" + kalenderLogik.betrachteterMonat.ToString("D2") + "-" + kalenderLogik.betrachtetesJahr;

        // Liste für die aktualisierten Notizen
        List<string> updatedNotizen = new List<string>();

        // Iteriere über alle Notizen und behalte nur die, die NICHT zum Datum passen
        foreach (string notiz in notizen)
        {
            // Prüfe, ob die Notiz NICHT mit dem Datum beginnt
            if (!notiz.StartsWith(datum))
            {
                updatedNotizen.Add(notiz); // Behalte alle anderen Notizen
            }
        }

        // Aktualisiere das Notizen-Array
        notizen = updatedNotizen.ToArray();

        // entferne alle Zeilenumbrüche
        notizen = entferneZeilenumbrüche(notizen);

        // Speichere das aktualisierte Notizen-Array in die Datei, ohne leere Zeilen
        System.IO.File.WriteAllLines(DiscordBot.notizenFilePath, notizen);

        // Publishe die neuen Notizen zum Discord
        DiscordBot.SafeAndPublishTextDateiAnPfad(DiscordBot.notizenFilePath);

        // Zeige eine Erfolgsmeldung oder führe eine andere Aktion aus
        infoPanelLöschen.SetActive(true);
        kalenderLogik.generiereKalenderNeu();
        Invoke("löschenPanelweg", 2f);
        knopfPanel.transform.SetParent(null); // Mache das Panel unsichtbar
    }


    private string[] entferneZeilenumbrüche(string[] stringArray)
    {
        string[] bearbeiteterString = stringArray;

        for (int i = 0; i < bearbeiteterString.Length; i++)
        {
            bearbeiteterString[i] = bearbeiteterString[i].Replace("\r\n", "");
        }

        string[] bereinigteNotizen = bearbeiteterString
        .Where(n => !string.IsNullOrWhiteSpace(n))  // Entfernt leere oder nur aus Leerzeichen/Zeilenumbrüchen bestehende Einträge
        .ToArray();

        return bereinigteNotizen;
    }


    public void speicherNotizOKKNOPF()
    {
        // Hole den Text der neuen Notiz

        // Text trimmen, um überflüssige Leerzeichen oder leere Notizen zu vermeiden
        string neueNotiz = notizenInputField.text.Trim();

        // Falls der Notiztext leer ist, speichere nichts
        if (string.IsNullOrEmpty(neueNotiz))
        {
            return; // Keine leeren Notizen speichern
        }

        // Datum als String formatieren
        string datum = letzterGedrückterTag.PadLeft(2, '0') + "-" + kalenderLogik.betrachteterMonat.ToString("D2") + "-" + kalenderLogik.betrachtetesJahr;

        // Überprüfen, ob das Notizen-Array eine Notiz für dieses Datum enthält und ggf. löschen
        List<string> updatedNotizen = new List<string>();

        foreach (string notiz in notizen)
        {
            // Prüfe, ob die Notiz das aktuelle Datum enthält
            if (!notiz.StartsWith(datum))
            {
                updatedNotizen.Add(notiz); // Behalte alle anderen Notizen
            }
        }

        // Füge die neue Notiz hinzu
        updatedNotizen.Add(datum + " : " + neueNotiz);

        // Array mit den neuen Notizen aktualisieren
        notizen = updatedNotizen.ToArray();

        // entferne alle Zeilenumbrüche
        notizen = entferneZeilenumbrüche(notizen);

        // Speichere das bereinigte Array in die Datei
        System.IO.File.WriteAllLines(DiscordBot.notizenFilePath, notizen);

        // Publishe die neuen Notizen zum Discord
        DiscordBot.SafeAndPublishTextDateiAnPfad(DiscordBot.notizenFilePath);

        infoPanelOK.SetActive(true);
        kalenderLogik.generiereKalenderNeu();
        Invoke("okPanelweg", 2f);
        knopfPanel.transform.SetParent(null); // Mache das Panel unsichtbar

        //TODO versende eine Email an das Datum der Notiz an die gewünschte emailadresse (hier hard coded)
    }


    private void okPanelweg()
    {
        infoPanelOK.SetActive(false);
    }

    private void abbruchPanelweg()
    {
        infoPanelAbbruch.SetActive(false);
    }

    private void löschenPanelweg()
    {
        infoPanelLöschen.SetActive(false);
    }


}
