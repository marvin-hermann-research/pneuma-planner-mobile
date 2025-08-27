using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class KalenderLogik : MonoBehaviour
{
    public GameObject[] Zellen = new GameObject[42];
    public NotizenHandler notizenHandler;
    public WiederkehrendeNotizenHandler wiederkehrendeHandler;
    public ColorstyleHandler colorStyleHandler;

    public int betrachteterMonat;
    public int betrachtetesJahr;

    public TextMeshProUGUI monatsText;
    public TextMeshProUGUI jahrText;

    void Start()
    {
        DateTime aktuellesDatum = DateTime.Now;

        int aktuellerMonat = aktuellesDatum.Month;
        int aktuellesJahr = aktuellesDatum.Year;

        betrachteterMonat = aktuellerMonat;
        betrachtetesJahr = aktuellesJahr;
        GeneriereKalender(aktuellerMonat, aktuellesJahr);
    }

    void GeneriereKalender(int monat, int jahr)
    {
        DateTime ersterTagImMonat = new DateTime(jahr, monat, 1);
        int tageImMonat = DateTime.DaysInMonth(jahr, monat);

        SetzeJahrInZelle();
        SetzeMonatInZelle();

        // Berechne den Wochentag des ersten Tages (1 = Montag, 7 = Sonntag)
        int startWochentag = (int)ersterTagImMonat.DayOfWeek;
        if (startWochentag == 0) startWochentag = 7; // Sonntag zu 7 ändern, damit Montag = 1

        // Setze die Zellen auf leer (Standard: 0)
        for (int i = 0; i < Zellen.Length; i++)
        {
            SetzeTagInZelle(Zellen[i], 0); // Setze die Zelle auf 0 (leer)
        }

        // Tage im Monat in die entsprechenden Zellen verteilen
        for (int tag = 1; tag <= tageImMonat; tag++)
        {
            // Berechne den Index für die Kalenderzelle
            int zellenIndex = (tag + startWochentag - 2); // -2, um den Index zu berechnen (Mo=0, So=6)

            // Setze den Tag in der Zelle
            SetzeTagInZelle(Zellen[zellenIndex], tag);
        }
    }

    void SetzeTagInZelle(GameObject zelle, int tag)
    {
        // Hole das TextMeshProUGUI-Komponenten-Child der Zelle
        TextMeshProUGUI zellenText = zelle.GetComponentInChildren<TextMeshProUGUI>();

        // Setze den Text basierend auf dem Tag
        zellenText.text = tag > 0 ? tag.ToString() : ""; // Zeige den Tag an, wenn er > 0 ist
    }

    void SetzeMonatInZelle()
    {
        string[] monate = { "Ja", "Fe", "Mr", "Ap", "Ma", "Jn", "Jl", "Au", "Se", "Oc", "No", "De" };

        if (betrachteterMonat >= 1 && betrachteterMonat <= 12)
        {
            monatsText.text = monate[betrachteterMonat - 1];
        }
    }

    void SetzeJahrInZelle()
    {
        jahrText.text = betrachtetesJahr.ToString().Substring(betrachtetesJahr.ToString().Length - 2);
    }

    public void gibZellenFarbTags()
    {
        DateTime heute = DateTime.Now; // Holen Sie sich das aktuelle Datum

        for (int i = 0; i < Zellen.Length; i++)
        {
            // Hole den Tag aus dem Text der Zelle
            int tag;
            if (int.TryParse(Zellen[i].GetComponentInChildren<TextMeshProUGUI>().text, out tag))
            {
                DateTime zellenDatum = new DateTime(betrachtetesJahr, betrachteterMonat, tag);

                if (zellenDatum.Date == heute.Date)
                {
                    Zellen[i].tag = "heutigerTagFarbe";
                    continue;
                }

                if (wiederkehrendeHandler.checkIfDateHasWiederkehrendeNotiz(zellenDatum))
                {
                    Zellen[i].tag = "markierterWiederkehrenderTagFarbe";
                    continue;
                }

                if (notizenHandler.checkIfDateHasNotiz(zellenDatum))
                {
                    Zellen[i].tag = "markierterTagFarbe";
                    continue;
                }

                Zellen[i].tag = "volleZellenHintergrundFarbeANDnotizHintergrundFarbe";
                continue;
            }
            Zellen[i].tag = "leereZellenHintergrundFarbe";
        }
    }

    public void monatNachVorne()
    {
        betrachteterMonat++;
        if (betrachteterMonat > 12)
        {
            betrachteterMonat = 1;
            betrachtetesJahr++;
        }
        generiereKalenderNeu();
    }

    public void monatNachHinten()
    {
        betrachteterMonat--;
        if (betrachteterMonat < 1)
        {
            betrachteterMonat = 12;
            betrachtetesJahr--;
        }
        generiereKalenderNeu();
    }

    public void generiereKalenderNeu()
    {
        GeneriereKalender(betrachteterMonat, betrachtetesJahr);
        gibZellenFarbTags();
        colorStyleHandler.refresKalenderFarben();
    }
}
