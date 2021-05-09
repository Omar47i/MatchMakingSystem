using Newtonsoft.Json;

public class PlayerData
{
    [JsonProperty("id")]
    public string ID { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("wins")]
    public int Wins { get; set; }

    [JsonProperty("losses")]
    public int Losses { get; set; }

    [JsonProperty("win-streak")]
    public int WinStreak { get; set; }

    [JsonProperty("SR")]
    public int SR { get; set; }
}