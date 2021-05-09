using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UIController : MonoBehaviour
{
    private List<Player> players;
    private Matchmaker matchmaker;
    private int totalPlayersCount;

    private GameMode selectedGameMode = GameMode.OneVOne;

    [SerializeField] Image[] selectedModeImages;
    [SerializeField] TMP_Text addedPlayersText;
    [SerializeField] GameObject matchItemPrefab;

    private void Start()
    {
        // Initialize references to the matchmaking scripts
        players = GameManager.Instance.GetPlayersList();
        matchmaker = GameManager.Instance.GetMatchMaker();

        totalPlayersCount = players.Count;

        UpdateSelectedModeGfx();
        UpdateAddedPlayers();
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

        // .. Remove it from the parsed json data
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

            // .. Remove it from the parsed json data
            players.Remove(players[0]);
        }

        UpdateAddedPlayers();
    }

    public void OnFindMatchOneStep()
    {
        Match match = matchmaker.FindMatch(selectedGameMode);

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

            Debug.Log("Matched " + team1 + " with " + team2);
        }
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
}
