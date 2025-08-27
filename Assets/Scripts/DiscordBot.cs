using Discord;
using Discord.WebSocket;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class DiscordBot : MonoBehaviour
{
    private DiscordSocketClient _client;

    public string notizenFilePath;
    public bool botIsReady;

    private async void Start()
    {
        notizenFilePath = Path.Combine(Application.persistentDataPath, "NOTIZEN.txt");
        Debug.Log(notizenFilePath);

        // Beispiel: Überprüfen, ob die Datei existiert und gegebenenfalls erstellen
        if (!File.Exists(notizenFilePath))
        {
            File.WriteAllText(notizenFilePath, ""); // Erstellt die Datei, falls sie nicht existiert
        }

        // Konfiguration des Clients mit aktivierten Intents
        var config = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.MessageContent
        };

        _client = new DiscordSocketClient(config);

        // Event-Handler registrieren
        _client.Log += Log;
        _client.Ready += OnReady;

        try
        {
            await _client.LoginAsync(TokenType.Bot, "YOUR_BOT_TOKEN_HERE");
            await _client.StartAsync();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Fehler beim Anmelden: {ex.Message}");
            return;
        }

        // Warte auf Ereignisse
        await Task.Delay(-1);
    }


    public async Task<string[]> GetColorstyleInLines()
    {
        string text = await FetchAndReadTxtFileFromChannel("COLORSTYLE.txt");

        if (string.IsNullOrEmpty(text))
        {
            return new string[0];
        }

        return text.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
    }

    public async Task<string[]> GetNotizenInLines()
    {
        string text = await FetchAndReadTxtFileFromChannel("NOTIZEN.txt");

        if (string.IsNullOrEmpty(text))
        {
            return new string[0];
        }

        return text.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
    }

    public async Task<string[]> GetNotizenWiederkehrendInLines()
    {
        string text = await FetchAndReadTxtFileFromChannel("NOTIZEN_WIEDERKEHREND.txt");

        if (string.IsNullOrEmpty(text))
        {
            return new string[0];
        }

        return text.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
    }

    public void SafeAndPublishTextDateiAnPfad(string path)
    {
        //methode zum löschen einer nachricht aufrufen um alte NOTIZEN.txt aus chatt zu löschen
        SendTextFileToChannel(path);
    }


    private Task OnReady()
    {
        Debug.Log($"Bot ist eingeloggt als: {_client.CurrentUser.Username}");
        botIsReady = true;
        return Task.CompletedTask;
    }


    private async Task<string> FetchAndReadTxtFileFromChannel(string dateiName)
    {
        ulong channelId = YOUR_CHANNEL_ID_HERE; // <-- Dummy
        var channel = _client.GetChannel(channelId) as SocketTextChannel;

        if (channel != null)
        {
            var messages = await channel.GetMessagesAsync(10).FlattenAsync();

            foreach (var message in messages)
            {
                if (message.Attachments.Any())
                {
                    foreach (var attachment in message.Attachments)
                    {
                        // Überprüfen, ob der Anhang eine .txt-Datei ist und der Anhangname gleich dem dateiName ist
                        if (attachment.Filename.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) && attachment.Filename == dateiName)
                        {
                            Debug.Log($"TXT-Datei gefunden: {attachment.Filename}");

                            // Datei herunterladen
                            using (HttpClient httpClient = new HttpClient())
                            {
                                try
                                {
                                    // Lade die Datei als Byte-Array herunter
                                    var fileData = await httpClient.GetByteArrayAsync(attachment.Url);

                                    // Konvertiere das Byte-Array in eine UTF-8-kodierte Zeichenfolge
                                    string content = System.Text.Encoding.UTF8.GetString(fileData);

                                    Debug.Log($"Inhalt der Datei {attachment.Filename}:");
                                    Debug.Log(content);

                                    // Entferne überflüssige Zeilenumbrüche und leere Zeilen
                                    string bereinigterInhalt = string.Join(Environment.NewLine,
                                        content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
                                                .Select(line => line.Trim()) // Entfernt überflüssige Whitespaces an den Zeilenenden
                                                .Where(line => !string.IsNullOrWhiteSpace(line)) // Filtert leere Zeilen
                                    );

                                    return bereinigterInhalt; // gibt den bereinigten Inhalt der Datei zurück
                                }
                                catch (Exception ex)
                                {
                                    Debug.LogError($"Fehler beim Herunterladen der Datei: {ex.Message}");
                                }
                            }
                        }
                    }
                }
            }
        }
        else
        {
            Debug.LogError("Kanal nicht gefunden.");
        }
        return null; // oder eine leere Zeichenfolge, falls die Datei nicht gefunden wurde
    }


    private async Task SendMessageToChannel(string messageContent)
    {
        ulong channelId = YOUR_CHANNEL_ID_HERE; // <-- Dummy
        var channel = _client.GetChannel(channelId) as SocketTextChannel;

        if (channel != null)
        {
            try
            {
                await channel.SendMessageAsync(messageContent);
                Debug.Log($"Nachricht gesendet: {messageContent}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Fehler beim Senden der Nachricht: {ex.Message}");
            }
        }
        else
        {
            Debug.LogError("Kanal nicht gefunden.");
        }
    }

    private async Task SendTextFileToChannel(string filePath)
    {
        ulong channelId = YOUR_CHANNEL_ID_HERE; // <-- Dummy
        var channel = _client.GetChannel(channelId) as SocketTextChannel;

        if (channel != null)
        {
            try
            {
                // Überprüfen, ob die Datei existiert
                if (File.Exists(filePath))
                {
                    // Discord API-URL für das Hochladen von Nachrichten in einen Kanal
                    var apiUrl = $"https://discord.com/api/v10/channels/{channelId}/messages";

                    // Erstelle einen HttpClient zum Senden des HTTP-Requests
                    using (var client = new HttpClient())
                    {
                        // Setze den Bot-Token in den Authorization-Header
                        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bot", "YOUR_BOT_TOKEN_HERE");

                        // Erstelle ein Multipart-Formular für den Upload der Datei
                        using (var form = new MultipartFormDataContent())
                        {
                            // Lies die Datei als Byte-Array
                            byte[] fileBytes = File.ReadAllBytes(filePath);

                            // Erstelle einen ByteArrayContent und füge den Dateinamen hinzu
                            var fileContent = new ByteArrayContent(fileBytes);
                            fileContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
                            {
                                Name = "\"file\"",
                                FileName = "\"" + Path.GetFileName(filePath) + "\""
                            };
                            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

                            // Füge die Datei zum Multipart-Formular hinzu
                            form.Add(fileContent);

                            // lösche die alte datei
                            deleteOldTextDatei(Path.GetFileName(filePath));

                            // Führe den POST-Request aus
                            var response = await client.PostAsync(apiUrl, form);

                            if (response.IsSuccessStatusCode)
                            {
                                Debug.Log($"Datei erfolgreich gesendet: {filePath}");
                            }
                            else
                            {
                                Debug.LogError($"Fehler beim Senden der Datei. Statuscode: {response.StatusCode}");
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogError($"Die Datei wurde nicht gefunden: {filePath}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Fehler beim Senden der Datei: {ex.Message}\nStacktrace: {ex.StackTrace}");
            }
        }
        else
        {
            Debug.LogError("Kanal nicht gefunden.");
        }
    }

    private async Task deleteOldTextDatei(string dateiName)
    {
        ulong channelId = YOUR_CHANNEL_ID_HERE; // <-- Dummy
        var channel = _client.GetChannel(channelId) as SocketTextChannel;

        if (channel != null)
        {
            try
            {
                // Hole die letzten Nachrichten im Kanal
                var messages = await channel.GetMessagesAsync(15).FlattenAsync();

                foreach (var message in messages)
                {
                    // Überprüfe, ob die Nachricht Anhänge hat
                    if (message.Attachments.Count > 0)
                    {
                        // Überprüfe, ob der Name des Anhangs mit dem gesuchten Dateinamen übereinstimmt
                        foreach (var attachment in message.Attachments)
                        {
                            if (attachment.Filename.Equals(dateiName, StringComparison.OrdinalIgnoreCase))
                            {
                                // Lösche die Nachricht
                                await message.DeleteAsync();
                                Debug.Log($"Nachricht mit der Datei '{dateiName}' wurde gelöscht.");
                                return; // Beende die Methode, nachdem die Nachricht gelöscht wurde
                            }
                        }
                    }
                }

                Debug.Log($"Keine Nachricht mit der Datei '{dateiName}' gefunden.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Fehler beim Löschen der Nachricht: {ex.Message}\nStacktrace: {ex.StackTrace}");
            }
        }
        else
        {
            Debug.LogError("Kanal nicht gefunden.");
        }
    }


    private Task Log(LogMessage arg)
    {
        Debug.Log(arg);
        return Task.CompletedTask;
    }
}
