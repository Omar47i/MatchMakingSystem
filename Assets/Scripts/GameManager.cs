using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private int oneVoneThreshold = 250;
    [SerializeField] private int twoVtwoThreshold = 250;
    [SerializeField] private int threeVthreeThreshold = 250;

    private Matchmaker matchmaker;

    private SampleDataParser dataParser;

    private List<Player> players;          // contains all the players listed in the sample json

    private void Start()
    {
        matchmaker = new Matchmaker();

        dataParser = new SampleDataParser();

        players = dataParser.PlayerDataToPlayers();
    }

    public int GetMatchingThreshold(GameMode gameMode)
    {
        if (gameMode == GameMode.OneVOne)
            return oneVoneThreshold;
        else if (gameMode == GameMode.TwoVTwo)
            return twoVtwoThreshold;
        else if (gameMode == GameMode.ThreeVThree)
            return threeVthreeThreshold;
        else
            return 0;
    }

    //@Debug
    public void OnAddPlayer()
    {
        if (players.Count == 0)
        {
            print("no more players");
            return;
        }

        Debug.Log(players[0].GetName() + " was added");

        matchmaker.EnterMatchmaking(players[0], GameMode.ThreeVThree);

        players.Remove(players[0]);
    }

    //@Debug
    public void OnAddAllPlayers()
    {
        if (players.Count == 0)
        {
            print("no more players");
            return;
        }

        while (players.Count != 0)
        {
            matchmaker.EnterMatchmaking(players[0], GameMode.ThreeVThree);
            Debug.Log(players[0].GetName() + " was added");

            players.Remove(players[0]);
        }
    }

    //@Debug
    public void OnFindMatchOnce()
    {
        Match match = matchmaker.FindMatch(GameMode.ThreeVThree);

        if (match != null)
        {
            string team1 = "";
            string team2 = "";

            foreach (Player p in match.GetTeam1())
            {
                team1 += p.GetName() + ",";
            }

            foreach (Player p in match.GetTeam2())
            {
                team2 += p.GetName() + ",";
            }

            Debug.Log("matched " + team1 + " with " + team2);
        }
    }

    //@Debug
    bool runFindMatch = false;
    float counter = 0f;
    public void OnFindMatch()
    {
        runFindMatch = !runFindMatch;
    }

    //@Debug
    private void Update()
    {
        if (runFindMatch)
        {
            if (counter <= 0)
            {
                OnFindMatchOnce();

                counter += .2f;
            }
            else
                counter -= Time.deltaTime;
        }
    }
}

// Define the different game modes that a player can pick from
public enum GameMode
{
    OneVOne,
    TwoVTwo,
    ThreeVThree
}