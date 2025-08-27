using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using System.Text;

public class ColorstyleHandler : MonoBehaviour
{
    private string leereZellenHintergrundFarbe;
    private string volleZellenHintergrundFarbeANDnotizHintergrundFarbe;
    private string heutigerTagFarbe;
    private string markierterTagFarbe;
    private string markierterWiederkehrenderTagFarbe;

    private string kalenderHintergrundFarbeANDtimeLineColor;
    private string dunkleKalenderFarbe;
    private string okKnopfFarbe;
    private string abbruchKnopfFarbe;
    private string deleteKnopfFarbe;
    private string textBoxFarbe;

    private string allgemeineBackgroundColor;
    private string textColor;
    private string zeitLinienFarbe;

    public DiscordBot DiscordBot;
    public KalenderLogik kalenderLogik;
    public bool colorsHaveBeenSet;
    private bool alleNotizenArtenWurdenVerarbeitet;
    public NotizenHandler notizenHandler;
    public WiederkehrendeNotizenHandler wiederkehrendeNotizenHandler;

    private string[] colorData;

    private bool startedExtracting;

    public GameObject loadingScreen;

    private async void Update()
    {
        if (!startedExtracting && DiscordBot.botIsReady && !colorsHaveBeenSet && notizenHandler.notizenHaveBeenSet && wiederkehrendeNotizenHandler.wiederkehrendeNotizenHaveBeenSet)
        {
            startedExtracting = true;
            colorData = await DiscordBot.GetColorstyleInLines();
            colorsHaveBeenSet = true;
        }

        //führe erst die farbzuweisung aus wenn alle notizen extrahiert wurden
        if (colorsHaveBeenSet  && !alleNotizenArtenWurdenVerarbeitet)
        {
            alleNotizenArtenWurdenVerarbeitet = true;
            ProcessColors(colorData);
        }
    }

    private void ProcessColors(string[] colorData)
    {
        foreach (string line in colorData)
        {
            // Prüfen, ob der Variablenname in der Zeile enthalten ist
            if (line.Contains("leereZellenHintergrundFarbe"))
            {
                leereZellenHintergrundFarbe = GetColorValue(line);
            }
            else if (line.Contains("volleZellenHintergrundFarbeANDnotizHintergrundFarbe"))
            {
                volleZellenHintergrundFarbeANDnotizHintergrundFarbe = GetColorValue(line);
            }
            else if (line.Contains("heutigerTagFarbe"))
            {
                heutigerTagFarbe = GetColorValue(line);
            }
            else if (line.Contains("markierterTagFarbe"))
            {
                markierterTagFarbe = GetColorValue(line);
            }
            else if (line.Contains("markierterWiederkehrenderTagFarbe"))
            {
                markierterWiederkehrenderTagFarbe = GetColorValue(line);
            }
            else if (line.Contains("kalenderHintergrundFarbeANDtimeLineColor"))
            {
                kalenderHintergrundFarbeANDtimeLineColor = GetColorValue(line);
            }
            else if (line.Contains("dunkleKalenderFarbe"))
            {
                dunkleKalenderFarbe = GetColorValue(line);
            }
            else if (line.Contains("okKnopfFarbe"))
            {
                okKnopfFarbe = GetColorValue(line);
            }
            else if (line.Contains("abbruchKnopfFarbe"))
            {
                abbruchKnopfFarbe = GetColorValue(line);
            }
            else if (line.Contains("deleteKnopfFarbe"))
            {
                deleteKnopfFarbe = GetColorValue(line);
            }
            else if (line.Contains("textBoxFarbe"))
            {
                textBoxFarbe = GetColorValue(line);
            }
            else if (line.Contains("allgemeineBackgroundColor"))
            {
                allgemeineBackgroundColor = GetColorValue(line);
            }
            else if (line.Contains("textColor"))
            {
                textColor = GetColorValue(line);
            }
            else if (line.Contains("zeitLinienFarbe"))
            {
                zeitLinienFarbe = GetColorValue(line);
            }
        }

        kalenderLogik.gibZellenFarbTags();//WICHTIG MUSS FOR SET COLORS KOMMEN
        setColors();
        Destroy(loadingScreen);
    }

    private string GetColorValue(string line)
    {
        // Hier wird angenommen, dass die Zeile im Format "varname: value" vorliegt
        string[] parts = line.Split(new[] { ':' }, 2);
        return parts.Length > 1 ? parts[1].Trim() : string.Empty;
    }

    private void setColors()
    {
        //methode zum setzen der starren farben

        // Wir gehen davon aus, dass die GameObjects ein Tag haben, das dem Namen der Variablen entspricht
        SetColorForGameObject("leereZellenHintergrundFarbe", leereZellenHintergrundFarbe);
        SetColorForGameObject("volleZellenHintergrundFarbeANDnotizHintergrundFarbe", volleZellenHintergrundFarbeANDnotizHintergrundFarbe);
        SetColorForGameObject("kalenderHintergrundFarbeANDtimeLineColor", kalenderHintergrundFarbeANDtimeLineColor);
        SetColorForGameObject("okKnopfFarbe", okKnopfFarbe);
        SetColorForGameObject("abbruchKnopfFarbe", abbruchKnopfFarbe);
        SetColorForGameObject("deleteKnopfFarbe", deleteKnopfFarbe);
        SetColorForGameObject("textBoxFarbe", textBoxFarbe);
        SetColorForGameObject("allgemeineBackgroundColor", allgemeineBackgroundColor);
        SetColorForGameObject("textColor", textColor);
        SetColorForGameObject("heutigerTagFarbe", heutigerTagFarbe);
        SetColorForGameObject("markierterTagFarbe", markierterTagFarbe);
        SetColorForGameObject("markierterWiederkehrenderTagFarbe", markierterWiederkehrenderTagFarbe);
        SetColorForGameObject("dunkleKalenderFarbe", dunkleKalenderFarbe);
    }

    public void refresKalenderFarben()
    {
        SetColorForGameObject("leereZellenHintergrundFarbe", leereZellenHintergrundFarbe);
        SetColorForGameObject("volleZellenHintergrundFarbeANDnotizHintergrundFarbe", volleZellenHintergrundFarbeANDnotizHintergrundFarbe);
        SetColorForGameObject("heutigerTagFarbe", heutigerTagFarbe);
        SetColorForGameObject("markierterTagFarbe", markierterTagFarbe);
        SetColorForGameObject("markierterWiederkehrenderTagFarbe", markierterWiederkehrenderTagFarbe);
        SetColorForGameObject("dunkleKalenderFarbe", dunkleKalenderFarbe);
    }

    private void SetColorForGameObject(string tagName, string colorValue)
    {
        // Finde alle GameObjects mit dem angegebenen Tag
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag(tagName);

        // Durchlaufe alle GameObjects und setze die Farbe des SpriteRenderers oder der Image-Komponente
        foreach (GameObject obj in gameObjects)
        {
            //priorität nach orden (welche elemente gibt es häufiger) diese oben nach unten
            
            TextMeshProUGUI textKomponent = obj.GetComponent<TextMeshProUGUI>();
            if (textKomponent != null)
            {
                textKomponent.color = ConvertStringToColor(colorValue);
                continue;
            }

            Image imageComponent = obj.GetComponent<Image>();
            if (imageComponent != null)
            {
                imageComponent.color = ConvertStringToColor(colorValue);
                continue;
            }


            Camera camera = obj.GetComponent<Camera>();
            if (camera != null)
            {
                camera.backgroundColor = ConvertStringToColor(colorValue);
                continue;
            }

        }
    }

    private Color ConvertStringToColor(string colorValue)
    {
        StringBuilder gefilterteFarbwerte = new StringBuilder();

        // Durchlaufe den String und filtere unerwünschte Zeichen
        for (int i = 0; i < colorValue.Length; i++)
        {
            char c = colorValue[i];
            if (c != ' ' && c != '(' && c != ')' && c != '"')
            {
                gefilterteFarbwerte.Append(c);
            }
        }

        // Teile den gefilterten String in die RGB-Werte auf
        string[] rgbValues = gefilterteFarbwerte.ToString().Split(new[] { ',' });

        // Konvertiere die Strings in Floats und skaliere sie auf den Wertebereich 0-1
        float r = Mathf.Clamp01(float.Parse(rgbValues[0]) / 255f);
        float g = Mathf.Clamp01(float.Parse(rgbValues[1]) / 255f);
        float b = Mathf.Clamp01(float.Parse(rgbValues[2]) / 255f);

        // Erstelle und gebe die Color zurück
        return new Color(r, g, b);
    }


}
