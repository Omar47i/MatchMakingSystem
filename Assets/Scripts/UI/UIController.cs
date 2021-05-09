using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UIController : MonoBehaviour
{
    private List<Player> players;
    private Matchmaker matchmaker;
    private int totalPlayersCount;
    private bool runFindMatch = false;       // if true, matchmaking will be trying to match players continuously

    private float findMatchCounter = 0f;
    private float findMatchIntervals = .1f;

    private GameMode selectedGameMode = GameMode.OneVOne;

    // references to UI gameObjects that will populate the scene with stats texts about the match making system
    [SerializeField] Image[] selectedModeImages;
    [SerializeField] TMP_Text addedPlayersText;
    [SerializeField] TMP_Text matchmakerStatusText;
    [SerializeField] GameObject matchItemPrefab;
    [SerializeField] Transform[] matchesViewParent;

    private void Start()
    {
        // Initialize references to the matchmaking scripts
        players = GameManager.Instance.GetPlayersList();
        matchmaker = GameManager.Instance.GetMatchMaker();

        // .. we need to know when the player is leaving the queue and entering a team to show the stats to the player
        matchmaker.GetEnteringTeamEvent().AddListener(OnPlayerLeavingQueue);

        InitializeUI();
    }

    private void InitializeUI()
    {
        totalPlayersCount = players.Count;

        UpdateSelectedModeGfx();
        UpdateAddedPlayers();
        matchmakerStatusText.text = "Idle";
    }

    public void AddOnePlayer()
    {
        if (players.Count == 0)
        {
            Debug.Log("No players to add!");
            return;
        }

        Debug.Log(players[0].GetName() + " was added");

        // .. Add the player in the queue
        matchmaker.EnterMatchmaking(players[0], selectedGameMode);

        // .. Update the in queue stat text and remove the player from the parsed json data
        CreatePlayerItemInQueue(players[0]);
        players.Remove(players[0]);

        UpdateAddedPlayers();
    }

    public void AddAllPlayers()
    {
        if (players.Count == 0)
        {
            Debug.Log("No players to add!");
            return;
        }

        while (players.Count != 0)
        {
            matchmaker.EnterMatchmaking(players[0], selectedGameMode);

            Debug.Log(players[0].GetName() + " was added");

            // .. Update the in queue stat text and remove the player from the parsed json data
            CreatePlayerItemInQueue(players[0]);
            players.Remove(players[0]);
        }

        UpdateAddedPlayers();
    }

    // Tries to match players one time, however, this method should be called few times every second to create teams fast, one step is probably just for testing
    public void OnFindMatchOneStep()
    {
        Match match = matchmaker.FindMatch(selectedGameMode);

        // we found a match, create a prefab to visualize the two teams
        if (match != null)
        {
            GameObject matchItem = Instantiate(matchItemPrefab, matchesViewParent[(int)selectedGameMode]);
            string team1 = "";
            string team2 = "";

            foreach (Player p in match.GetTeam1())
            {
                team1 += p.GetName() + "(" + p.GetSR() + ")" + ",";
            }

            team1 = team1.Remove(team1.LastIndexOf(","), 1);

            foreach (Player p in match.GetTeam2())
            {
                team2 += p.GetName() + "(" + p.GetSR() + ")" + ",";
            }

            team2 = team2.Remove(team2.LastIndexOf(","), 1);

            matchItem.transform.GetChild(0).GetComponent<TMP_Text>().text = team1 + " vs " + team2;

            Debug.Log("Matched " + team1 + " vs " + team2);
        }
    }

    // Run matchmaking system continuously
    public void OnToggleFindMatch()
    {
        runFindMatch = !runFindMatch;

        matchmakerStatusText.text = runFindMatch ? "Running" : "Idle";
    }

    void CreatePlayerItemInQueue(Player player)
    {
        GameObject playerInQueueObj = Instantiate(matchItemPrefab, matchesViewParent[3]);
        playerInQueueObj.GetComponent<PlayerInQueueItem>().SetPlayer(player.GetName(), player.GetSR(), player.GetID());
    }

    void UpdateSelectedModeGfx()
    {
        if (selectedGameMode == GameMode.OneVOne)
        {
            selectedModeImages[0].color = Color.green;
            selectedModeImages[1].color = Color.white;
            selectedModeImages[2].color = Color.white;
        }
        else if (selectedGameMode == GameMode.TwoVTwo)
        {
            selectedModeImages[0].color = Color.white;
            selectedModeImages[1].color = Color.green;
            selectedModeImages[2].color = Color.white;
        }
        else if (selectedGameMode == GameMode.ThreeVThree)
        {
            selectedModeImages[0].color = Color.white;
            selectedModeImages[1].color = Color.white;
            selectedModeImages[2].color = Color.green;
        }
    }

    void UpdateAddedPlayers()
    {
        int added = totalPlayersCount - players.Count;
        addedPlayersText.text = added + "/" + totalPlayersCount + " players are added.";
    }

    public void OnChangeMode(int i)
    {
        if ((int)selectedGameMode == i)
            return;

        selectedGameMode = (GameMode)i;

        UpdateSelectedModeGfx();
    }

    private void OnPlayerLeavingQueue(string id)
    {
        foreach (Transform tr in matchesViewParent[3])
        {
            PlayerInQueueItem item = tr.GetComponent<PlayerInQueueItem>();

            if (item.GetID().Equals(id))
            {
                Destroy(item.gameObject);
                break;
            }
        }
    }

    private void Update()
    {
        if (runFindMatch)
        {
            if (findMatchCounter <= 0)
            {
                OnFindMatchOneStep();

                findMatchCounter += findMatchIntervals;
            }
            else
                findMatchCounter -= Time.deltaTime;
        }
    }

    private void OnDestroy()
    {
        matchmaker.GetEnteringTeamEvent().RemoveListener(OnPlayerLeavingQueue);
    }
}
