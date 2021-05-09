using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private int OneVOneThreshold = 850;
    [SerializeField] private int TwoVTwoThreshold = 850;
    [SerializeField] private int ThreeVThreeThreshold = 850;

    private Matchmaker matchmaker;

    private SampleDataParser dataParser;

    private List<Player> players;          // contains all the players listed in the sample json

    private void Awake()
    {
        // The game manager is responsible for creating the match maker interface that will be used to try and match players together
        matchmaker = new Matchmaker();

        dataParser = new SampleDataParser();

        players = dataParser.PlayerJsonDataToPlayers();
    }

    // For each game mode, there is a threshold that when the sr difference is below it, then we can match the two players together
    public int GetMatchingThreshold(GameMode gameMode)
    {
        if (gameMode == GameMode.OneVOne)
            return OneVOneThreshold;
        else if (gameMode == GameMode.TwoVTwo)
            return TwoVTwoThreshold;
        else if (gameMode == GameMode.ThreeVThree)
            return ThreeVThreeThreshold;
        else
            return 0;
    }

    // Return the players that have been read from the json sample
    public List<Player> GetPlayersList()
    {
        return players;
    }

    public Matchmaker GetMatchMaker()
    {
        return matchmaker;
    }
}

// Define the different game modes that a player can pick from
public enum GameMode
{
    OneVOne,
    TwoVTwo,
    ThreeVThree
}