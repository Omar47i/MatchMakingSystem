using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TeamsBuilder
{
    Dictionary<GameMode, List<Player>> playersInQueue = new Dictionary<GameMode, List<Player>>();

    // .. All the players that are of similar criteria will be matched in teams, except for the 1v1 players as they are not within a team
    Dictionary<GameMode, List<List<Player>>> playersWaitingInTeams = new Dictionary<GameMode, List<List<Player>>>();

    Dictionary<GameMode, List<List<Player>>> playersInTeamsAndMatched = new Dictionary<GameMode, List<List<Player>>>();

    private GameMode activeMode;    // the mode that we are trying the matchmaking for

    // .. Fired when a player is removed from the queue and found a team, or found a match in case of 1v1, used for UI stats 
    public PlayerLeavingQueueBaseEvent PlayerLeavingQueueEvent = new PlayerLeavingQueueBaseEvent();

    public TeamsBuilder()
    {
        playersInQueue[GameMode.OneVOne] = new List<Player>();
        playersInQueue[GameMode.TwoVTwo] = new List<Player>();
        playersInQueue[GameMode.ThreeVThree] = new List<Player>();

        playersWaitingInTeams[GameMode.TwoVTwo] = new List<List<Player>>();
        playersWaitingInTeams[GameMode.ThreeVThree] = new List<List<Player>>();
    }

    public void InsertPlayerInQueue(Player player, GameMode gameMode)
    {
        playersInQueue[gameMode].Add(player);
    }

    /// <summary>
    /// try creating a match by adding individual players with similar SR to teams or match individual players in 1v1
    /// </summary>
    /// <param name="gameMode"></param>
    /// <returns></returns>
    public Match TryFindingMatch(GameMode gameMode)
    {
        activeMode = gameMode;

        if (gameMode == GameMode.OneVOne)
            return TryMatching1v1Players();
        else
            return TryMatching2v2_3v3Players();
    }

    /// <summary>
    /// Matching players with similar SR together and create a match for them to play
    /// </summary>
    /// <returns>the created match</returns>
    private Match TryMatching1v1Players()
    {
        int count = playersInQueue[GameMode.OneVOne].Count;

        // .. Do nothing if there is not enough players searching for a game
        if (count == 0 || count == 1)
            return null;

        List<Player> onevonePlayers = playersInQueue[GameMode.OneVOne];

        for (int i = 0; i < count - 1; i++)
        {
            Player p1 = onevonePlayers[i];

            for (int j = i + 1; j < count; j++)
            {
                Player p2 = onevonePlayers[j];

                // .. Get the SR difference between the two players searching for a game
                int srDiff = GetSRDifference(p1.GetSR(), p2.GetSR());

                if (CanMatch(srDiff, GameMode.OneVOne))
                {
                    // we got a match, remove the matched players so that we don't include them in future match making
                    playersInQueue[GameMode.OneVOne].Remove(p1);
                    playersInQueue[GameMode.OneVOne].Remove(p2);
                    PlayerLeavingQueueEvent.Invoke(p1.GetID());
                    PlayerLeavingQueueEvent.Invoke(p2.GetID());

                    List<Player> players = new List<Player>();
                    players.Add(p1);
                    players.Add(p2);
                    return CreateMatch(players);
                }
            }
        }

        return null;
    }

    /// <summary>
    /// add potential players to the same team in the 3v3 mode
    /// </summary>
    /// <returns>a match if one exist</returns>
    private Match TryMatching2v2_3v3Players()
    {
        int playerInQueueCount = playersInQueue[activeMode].Count;

        // .. Do nothing if there is not enough players searching for a game
        if (playerInQueueCount == 0 || playerInQueueCount == 1)
            return null;

        List<Player> playersInQueueList = playersInQueue[activeMode];

        for (int i = 0; i < playerInQueueCount; i++)
        {
            Player p1 = playersInQueueList[i];

            Match match = TryMatching2v2_3v3PlayersInTeam(p1, ref playerInQueueCount);

            if (match != null)
                return match;
        }

        // .. If no match for queing players with teams due to SR difference, then try to create new teams with close SR values from individual players

        //playerInQueueCount = playersInQueue[GameMode.TwoVTwo].Count;

        for (int i = 0; i < playerInQueueCount - 1; i++)
        {
            Player p1 = playersInQueueList[i];

            for (int j = i + 1; j < playerInQueueCount; j++)
            {
                Player p2 = playersInQueueList[j];

                // .. Get the SR difference between the two players searching for a game
                int srDiff = GetSRDifference(p1.GetSR(), p2.GetSR());

                if (CanMatch(srDiff, activeMode))
                {
                    // we got a match, create a team out of the two players and remove them from the queue
                    CreateTeam(p1, p2);
                    PlayerLeavingQueueEvent.Invoke(p1.GetID());
                    PlayerLeavingQueueEvent.Invoke(p2.GetID());
                    // return null as we don't have enough players yet to start a game
                    return null;
                }
            }
        }

        return null;
    }

    private Match TryMatching2v2_3v3PlayersInTeam(Player p, ref int playerInQueueCount)
    {
        List<List<Player>> playersInTeams = playersWaitingInTeams[activeMode];

        for (int j = 0; j < playersInTeams.Count; j++)
        {
            int teamSR = GetTeamSR(playersInTeams[j]);

            // .. Get the SR difference between the inspected player and the team SR
            int srDiff = GetSRDifference(p.GetSR(), teamSR);

            if (CanMatch(srDiff, activeMode))
            {
                // we got a match, add the player to the team and remove them from the queue
                playersInTeams[j].Add(p);
                playersInQueue[activeMode].Remove(p);
                PlayerLeavingQueueEvent.Invoke(p.GetID());
                playerInQueueCount--;

                // .. we got a full match
                if (playersInTeams[j].Count == GetMatchPlayersCount(activeMode))
                    return CreateMatch(playersInTeams[j]);
                else
                    return null;
            }
        }

        return null;
    }

    /// <summary>
    /// Create a new team from two players of similar SR
    /// </summary>
    /// <param name="p1">Player 1</param>
    /// <param name="p2">Player 2</param>
    private void CreateTeam(Player p1, Player p2)
    {
        playersInQueue[activeMode].Remove(p1);
        playersInQueue[activeMode].Remove(p2);

        List<Player> newTeam = new List<Player>();
        newTeam.Add(p1);
        newTeam.Add(p2);

        playersWaitingInTeams[activeMode].Add(newTeam);
    }

    public int GetSRDifference(int p1, int p2)
    {
        return Mathf.Abs(p1 - p2);
    }

    public bool CanMatch(int difference, GameMode mode)
    {
        return difference < GameManager.Instance.GetMatchingThreshold(mode);
    }

    private Match CreateMatch(List<Player> players)
    {
        HashSet<Player> team1 = new HashSet<Player>();
        HashSet<Player> team2 = new HashSet<Player>();

        for (int i = 0; i < GetMatchPlayersCount(activeMode); i++)
        {
            if (i < GetMatchPlayersCount(activeMode) * .5f)
            {
                team1.Add(players[i]);
            }
            else
            {
                team2.Add(players[i]);
            }
        }

        return new Match(team1, team2);
    }

    private int GetMatchPlayersCount(GameMode gameMode)
    {
        if (gameMode == GameMode.OneVOne)
            return 2;
        else if (gameMode == GameMode.TwoVTwo)
            return 4;
        else if (gameMode == GameMode.ThreeVThree)
            return 6;

        return 0;
    }

    public int GetTeamSR(List<Player> players)
    {
        int teamSR = 0;

        for (int i = 0; i < players.Count; i++)
        {
            teamSR += players[i].GetSR();
        }

        return teamSR / players.Count;
    }
}

public class PlayerLeavingQueueBaseEvent : UnityEvent<string>
{ }