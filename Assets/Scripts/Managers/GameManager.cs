using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private int OneVOneThreshold = 250;
    [SerializeField] private int TwoVTwoThreshold = 250;
    [SerializeField] private int ThreeVThreeThreshold = 250;

    private Matchmaker matchmaker;

    private SampleDataParser dataParser;

    private List<Player> players;          // contains all the players listed in the sample json

    private void Awake()
    {
        matchmaker = new Matchmaker();

        dataParser = new SampleDataParser();

        players = dataParser.PlayerDataToPlayers();
    }

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