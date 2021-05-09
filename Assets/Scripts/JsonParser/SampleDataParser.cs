using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class SampleDataParser
{
    private string jsonFileName = "sample-data";

    private List<PlayerData> playerData;

    public SampleDataParser()
    {
        // Load the sample data json file from Resources folder and deserialize it
        var jsonTextFile = Resources.Load<TextAsset>(jsonFileName);

        playerData = JsonConvert.DeserializeObject<List<PlayerData>>(jsonTextFile.text);
    }

    public List<PlayerData> GetPlayerData()
    {
        return playerData;
    }

    public List<Player> PlayerDataToPlayers()
    {
        List<Player> players = new List<Player>();

        for (int i = 0; i < playerData.Count; i++)
        {
            players.Add(new Player(playerData[i].ID, playerData[i].Name, playerData[i].Wins, playerData[i].Losses, playerData[i].WinStreak, playerData[i].SR));
        }

        return players;
    }
}