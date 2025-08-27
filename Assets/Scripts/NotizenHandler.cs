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

    private string letzterGedr�ckterTag;

    public GameObject knopfPanel;
    public GameObject infoPanelOK;
    public GameObject infoPanelAbbruch;
    public GameObject infoPanelL�schen;
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

        letzterGedr�ckterTag = DateTime.Now.Day.ToString("D2"); // Tag des heutigen Datums im Format "DD"
    }

    private void zeigeNotizan()
    {
        // Formatieren des aktuell betrachteten Datums als String
        string datum = letzterGedr�ckterTag.PadLeft(2, '0') + "-" + kalenderLogik.betrachteterMonat.ToString("D2") + "-" + kalenderLogik.betrachtetesJahr;

        // Initialisiere einen StringBuilder f�r den Notiztext
        System.Text.StringBuilder notizBuilder = new System.Text.StringBuilder();

        // 1. Durchlaufe das wiederkehrendeNotizen.wiederkehrendeNotizen Array und f�ge passende Notizen hinzu
        foreach (string wiederkehrendeNotiz in wiederkehrendeNotizen.wiederkehrendeNotizen)
        {
            string gek�rztesDatum = datum.Substring(0, 5);
            if (wiederkehrendeNotiz.StartsWith(gek�rztesDatum))
            {
                notizBuilder.AppendLine(trimmNotiz(wiederkehrendeNotiz)); // F�ge die wiederkehrende Notiz hinzu
            }
        }

        // 2. Durchlaufe das normale notizen Array und f�ge passende Notizen hinzu
        foreach (string gespeicherteNotiz in notizen)
        {
            if (gespeicherteNotiz.StartsWith(datum))
            {
                notizBuilder.AppendLine(trimmNotiz(gespeicherteNotiz)); // F�ge die normale Notiz hinzu
            }
        }

        // �berpr�fe, ob Notizen gefunden wurden
        if (notizBuilder.Length > 0)
        {
            notizenInputField.text = notizBuilder.ToString(); // Zeige alle gefundenen Notizen an
        }
        else
        {
            notizenInputField.text = "Keine Notiz f�r dieses Datum."; // Falls keine Notizen existieren
        }
    }


    void zelleClicked(Button clickedButton)
    {
        if (clickedButton.GetComponentInChildren<TextMeshProUGUI>().text == "")
        {
            return;
        }
        letzterGedr�ckterTag = clickedButton.GetComponentInChildren<TextMeshProUGUI>().text;
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

    public void notizL�schenKnopf()
    {
        // Datum als String formatieren
        string datum = letzterGedr�ckterTag.PadLeft(2, '0') + "-" + kalenderLogik.betrachteterMonat.ToString("D2") + "-" + kalenderLogik.betrachtetesJahr;

        // Liste f�r die aktualisierten Notizen
        List<string> updatedNotizen = new List<string>();

        // Iteriere �ber alle Notizen und behalte nur die, die NICHT zum Datum passen
        foreach (string notiz in notizen)
        {
            // Pr�fe, ob die Notiz NICHT mit dem Datum beginnt
            if (!notiz.StartsWith(datum))
            {
                updatedNotizen.Add(notiz); // Behalte alle anderen Notizen
            }
        }

        // Aktualisiere das Notizen-Array
        notizen = updatedNotizen.ToArray();

        // entferne alle Zeilenumbr�che
        notizen = entferneZeilenumbr�che(notizen);

        // Speichere das aktualisierte Notizen-Array in die Datei, ohne leere Zeilen
        System.IO.File.WriteAllLines(DiscordBot.notizenFilePath, notizen);

        // Publishe die neuen Notizen zum Discord
        DiscordBot.SafeAndPublishTextDateiAnPfad(DiscordBot.notizenFilePath);

        // Zeige eine Erfolgsmeldung oder f�hre eine andere Aktion aus
        infoPanelL�schen.SetActive(true);
        kalenderLogik.generiereKalenderNeu();
        Invoke("l�schenPanelweg", 2f);
        knopfPanel.transform.SetParent(null); // Mache das Panel unsichtbar
    }


    private string[] entferneZeilenumbr�che(string[] stringArray)
    {
        string[] bearbeiteterString = stringArray;

        for (int i = 0; i < bearbeiteterString.Length; i++)
        {
            bearbeiteterString[i] = bearbeiteterString[i].Replace("\r\n", "");
        }

        string[] bereinigteNotizen = bearbeiteterString
        .Where(n => !string.IsNullOrWhiteSpace(n))  // Entfernt leere oder nur aus Leerzeichen/Zeilenumbr�chen bestehende Eintr�ge
        .ToArray();

        return bereinigteNotizen;
    }


    public void speicherNotizOKKNOPF()
    {
        // Hole den Text der neuen Notiz

        // Text trimmen, um �berfl�ssige Leerzeichen oder leere Notizen zu vermeiden
        string neueNotiz = notizenInputField.text.Trim();

        // Falls der Notiztext leer ist, speichere nichts
        if (string.IsNullOrEmpty(neueNotiz))
        {
            return; // Keine leeren Notizen speichern
        }

        // Datum als String formatieren
        string datum = letzterGedr�ckterTag.PadLeft(2, '0') + "-" + kalenderLogik.betrachteterMonat.ToString("D2") + "-" + kalenderLogik.betrachtetesJahr;

        // �berpr�fen, ob das Notizen-Array eine Notiz f�r dieses Datum enth�lt und ggf. l�schen
        List<string> updatedNotizen = new List<string>();

        foreach (string notiz in notizen)
        {
            // Pr�fe, ob die Notiz das aktuelle Datum enth�lt
            if (!notiz.StartsWith(datum))
            {
                updatedNotizen.Add(notiz); // Behalte alle anderen Notizen
            }
        }

        // F�ge die neue Notiz hinzu
        updatedNotizen.Add(datum + " : " + neueNotiz);

        // Array mit den neuen Notizen aktualisieren
        notizen = updatedNotizen.ToArray();

        // entferne alle Zeilenumbr�che
        notizen = entferneZeilenumbr�che(notizen);

        // Speichere das bereinigte Array in die Datei
        System.IO.File.WriteAllLines(DiscordBot.notizenFilePath, notizen);

        // Publishe die neuen Notizen zum Discord
        DiscordBot.SafeAndPublishTextDateiAnPfad(DiscordBot.notizenFilePath);

        infoPanelOK.SetActive(true);
        kalenderLogik.generiereKalenderNeu();
        Invoke("okPanelweg", 2f);
        knopfPanel.transform.SetParent(null); // Mache das Panel unsichtbar

        //TODO versende eine Email an das Datum der Notiz an die gew�nschte emailadresse (hier hard coded)
    }


    private void okPanelweg()
    {
        infoPanelOK.SetActive(false);
    }

    private void abbruchPanelweg()
    {
        infoPanelAbbruch.SetActive(false);
    }

    private void l�schenPanelweg()
    {
        infoPanelL�schen.SetActive(false);
    }


}
