using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TeamsBuilder
{
    // .. Holds all the players that have entered the searching for a game, sorted by their entering time
    Dictionary<GameMode, Queue<Player>> playersWaitingInQueue = new Dictionary<GameMode, Queue<Player>>();

    // .. Holds all the players that have entered the searching for a game but haven't found a match yet, prioritize them in searching
    Dictionary<GameMode, Queue<Player>> playersWaitingInPriorityQueue = new Dictionary<GameMode, Queue<Player>>();

    // .. all waiting players are sorted based on their SR value
    Dictionary<GameMode, SortedList<Player, int>> playersSortedRanked = new Dictionary<GameMode, SortedList<Player, int>>();

    // .. All the players that are of similar criteria will be matched in teams, except for the 1v1 players as they are solo
    Dictionary<GameMode, SortedList<Team, int>> teamsSortedRanked = new Dictionary<GameMode, SortedList<Team, int>>();

    // the mode that we are trying the matchmaking for at the moment
    private GameMode selectedMode;

    // .. Fired when a player is removed from the queue and found a team, or found a match in case of 1v1, used for UI stats 
    public PlayerLeavingQueueBaseEvent PlayerLeavingQueueEvent = new PlayerLeavingQueueBaseEvent();

    public TeamsBuilder()
    {
        playersWaitingInQueue[GameMode.OneVOne] = new Queue<Player>();
        playersWaitingInQueue[GameMode.TwoVTwo] = new Queue<Player>();
        playersWaitingInQueue[GameMode.ThreeVThree] = new Queue<Player>();

        playersWaitingInPriorityQueue[GameMode.OneVOne] = new Queue<Player>();
        playersWaitingInPriorityQueue[GameMode.TwoVTwo] = new Queue<Player>();
        playersWaitingInPriorityQueue[GameMode.ThreeVThree] = new Queue<Player>();

        playersSortedRanked[GameMode.OneVOne] = new SortedList<Player, int>(new BySR());
        playersSortedRanked[GameMode.TwoVTwo] = new SortedList<Player, int>(new BySR());
        playersSortedRanked[GameMode.ThreeVThree] = new SortedList<Player, int>(new BySR());

        teamsSortedRanked[GameMode.OneVOne] = new SortedList<Team, int>(new ByTeamSR());
        teamsSortedRanked[GameMode.TwoVTwo] = new SortedList<Team, int>(new ByTeamSR());
        teamsSortedRanked[GameMode.ThreeVThree] = new SortedList<Team, int>(new ByTeamSR());
    }

    public void InsertPlayerInQueue(Player player, GameMode gameMode)
    {
        playersWaitingInQueue[gameMode].Enqueue(player);
        playersSortedRanked[gameMode].Add(player, player.GetSR());
    }

    // try creating a match by adding individual players with similar SR to teams or match individual players in 1v1
    public Match TryFindingMatch(GameMode gameMode)
    {
        selectedMode = gameMode;

        if (gameMode == GameMode.OneVOne)
            return TryMatching1v1PlayersEnhanced();
        else
            return TryMatching2v2_3v3PlayersEnhanced();
    }

    /// <summary>
    /// Matching players with similar SR together and create a match for them to play
    /// </summary>
    /// <returns>the created match</returns>
    private Match TryMatching1v1PlayersEnhanced()
    {
        // used to hold the dequeued players that havent found a match yet, we will insert them again in the 
        // priority queue
        Queue<Player> temporaryQueue = new Queue<Player>();

        // Loop through all the priority players and check if there is a close SR player in the ranked list
        while (playersWaitingInPriorityQueue[selectedMode].Count != 0)
        {
            Player priorityPlayer = playersWaitingInPriorityQueue[GameMode.OneVOne].Dequeue();

            Player closestPlayer = playersSortedRanked[GameMode.OneVOne].FindClosest(priorityPlayer);

            // .. Get the SR difference between the two players searching for a game
            int srDiff = GetSRDifference(closestPlayer.GetSR(), priorityPlayer.GetSR());

            if (CanMatch(srDiff, GameMode.OneVOne))
            {
                // we got a match, remove the matched player so that we don't include them in future match making
                playersSortedRanked[GameMode.OneVOne].Remove(closestPlayer);

                List<Player> players = new List<Player>();
                players.Add(priorityPlayer);
                players.Add(closestPlayer);

                // .. insert any removed players to the priority queue again
                InsertInPlayersPriorityQueue(ref temporaryQueue);

                PlayerLeavingQueueEvent.Invoke(priorityPlayer.GetID());
                PlayerLeavingQueueEvent.Invoke(closestPlayer.GetID());

                return CreateMatch(players);
            }
            else
            {
                playersSortedRanked[selectedMode].Add(priorityPlayer, priorityPlayer.GetSR());
                temporaryQueue.Enqueue(priorityPlayer);
            }
        }

        // Insert the dequeued priority players into the priority queue again to give them advatage when searching
        // for a game with newly joined players
        InsertInPlayersPriorityQueue(ref temporaryQueue);

        // .. If the priority queue players couldn't find a match yet, try to match other players until they
        // .. could be matched with newly joined players
        while (playersWaitingInQueue[selectedMode].Count != 0)
        {
            Player player = playersWaitingInQueue[selectedMode].Dequeue();

            // .. Don't process the player if it's already got matched
            if (player.IsDirty())
            {
                continue;
            }

            Player closestPlayer = playersSortedRanked[selectedMode].FindClosest(player);

            // .. Get the SR difference between the two players searching for a game
            int srDiff = GetSRDifference(closestPlayer.GetSR(), player.GetSR());

            if (CanMatch(srDiff, selectedMode))
            {
                // .. Set the player that got matched to be Dirty which means they cannot be matched again until
                // .. they get removed from the playersWaitingInQueue
                closestPlayer.SetDirty();

                // .. We got a match, remove the matched player so that we don't include them in future match making
                playersSortedRanked[GameMode.OneVOne].Remove(closestPlayer);
                playersSortedRanked[GameMode.OneVOne].Remove(player);

                // .. Create a match
                List<Player> players = new List<Player>();
                players.Add(player);
                players.Add(closestPlayer);

                PlayerLeavingQueueEvent.Invoke(player.GetID());
                PlayerLeavingQueueEvent.Invoke(closestPlayer.GetID());

                return CreateMatch(players);
            }
            else
            {
                playersSortedRanked[selectedMode].Add(player, player.GetSR());
                playersWaitingInPriorityQueue[GameMode.OneVOne].Enqueue(player);
            }
        }

        return null;
    }

    // insert the dequeued players that couldn't find a match into the priority queue again
    private void InsertInPlayersPriorityQueue(ref Queue<Player> tempQueue)
    {
        while (tempQueue.Count != 0)
            playersWaitingInPriorityQueue[selectedMode].Enqueue(tempQueue.Dequeue());
    }

    /// <summary>
    /// add potential players to the same team in the 3v3 mode
    /// </summary>
    /// <returns>a match if one exist</returns>
    private Match TryMatching2v2_3v3PlayersEnhanced()
    {
        int playersWaitingCount = playersWaitingInQueue[selectedMode].Count + playersWaitingInPriorityQueue[selectedMode].Count;

        // .. Do nothing if there is not enough players searching for a game
        if (playersWaitingCount <= 1)
            return null;

        // used to hold the dequeued players that havent found a match yet, we will insert them again in the
        // priority queue
        Queue<Player> temporaryQueue = new Queue<Player>();

        // Loop through all the priority players and check if there is a close SR player in teams to match with
        while (playersWaitingInPriorityQueue[selectedMode].Count != 0 && teamsSortedRanked[selectedMode].Count > 0)
        {
            Player priorityPlayer = playersWaitingInPriorityQueue[selectedMode].Dequeue();

            if (priorityPlayer.IsDirty())
                continue;

            Match match = TryMatching2v2_3v3PlayersInTeamEnhanced(priorityPlayer, ref temporaryQueue);

            if (match != null)
                return match;
        }

        // Insert the dequeued priority players into the priority queue again to give them advatage when searching
        // for a game with newly joined players
        InsertInPlayersPriorityQueue(ref temporaryQueue);

        // Loop through all the non-priority players and check if there is a close SR player in teams to match with
        while (playersWaitingInQueue[selectedMode].Count != 0 && teamsSortedRanked[selectedMode].Count > 0)
        {
            Player player = playersWaitingInQueue[selectedMode].Dequeue();

            if (player.IsDirty())
                continue;

            Match match = TryMatching2v2_3v3PlayersInTeamEnhanced(player, ref temporaryQueue);

            if (match != null)
                return match;
        }

        // Insert the dequeued priority players into the priority queue again to give them advatage when searching
        // for a game with newly joined players
        InsertInPlayersPriorityQueue(ref temporaryQueue);

        // .. Try matching players together
        while (playersWaitingInPriorityQueue[selectedMode].Count != 0)
        {
            Player priorityPlayer = playersWaitingInPriorityQueue[selectedMode].Dequeue();

            // .. Don't process the player if it's already got matched
            if (priorityPlayer.IsDirty())
                continue;

            Player closestPlayer = playersSortedRanked[selectedMode].FindClosest(priorityPlayer);

            // .. Get the SR difference between the two players searching for a game
            int srDiff = GetSRDifference(closestPlayer.GetSR(), priorityPlayer.GetSR());

            if (CanMatch(srDiff, selectedMode))
            {
                closestPlayer.SetDirty();

                // .. Create a match
                CreateTeamEnhanced(closestPlayer, priorityPlayer);

                // .. insert any removed players to the priority queue again
                InsertInPlayersPriorityQueue(ref temporaryQueue);

                // Fire these events as the players leave the queue in order to show the stats UI
                PlayerLeavingQueueEvent.Invoke(priorityPlayer.GetID());
                PlayerLeavingQueueEvent.Invoke(closestPlayer.GetID());

                return null;
            }
            else
            {
                playersSortedRanked[selectedMode].Add(priorityPlayer, priorityPlayer.GetSR());
                temporaryQueue.Enqueue(priorityPlayer);
            }
        }

        // .. insert any removed players to the priority queue again
        InsertInPlayersPriorityQueue(ref temporaryQueue);

        // .. Try matching players together
        while (playersWaitingInQueue[selectedMode].Count != 0)
        {
            Player player = playersWaitingInQueue[selectedMode].Dequeue();

            // .. Don't process the player if it's already got matched
            if (player.IsDirty())
                continue;

            Player closestPlayer = playersSortedRanked[selectedMode].FindClosest(player);

            // .. Get the SR difference between the two players searching for a game
            int srDiff = GetSRDifference(closestPlayer.GetSR(), player.GetSR());

            if (CanMatch(srDiff, selectedMode))
            {
                closestPlayer.SetDirty();

                // .. Create a match
                CreateTeamEnhanced(closestPlayer, player);

                // Fire these events as the players leave the queue in order to show the stats UI
                PlayerLeavingQueueEvent.Invoke(player.GetID());
                PlayerLeavingQueueEvent.Invoke(closestPlayer.GetID());

                return null;
            }
            else
            {
                playersSortedRanked[selectedMode].Add(player, player.GetSR());
                playersWaitingInPriorityQueue[selectedMode].Enqueue(player);
            }
        }

        return null;
    }

    // Try to match the player p with an already created team that needs more players
    private Match TryMatching2v2_3v3PlayersInTeamEnhanced(Player player, ref Queue<Player> temporaryQueue)
    {
        Team closestSRTeam = teamsSortedRanked[selectedMode].FindClosestInTeam(player);

        if (closestSRTeam == null)
        {
            temporaryQueue.Enqueue(player);
            return null;
        }

        // .. Get the SR difference between the two players searching for a game
        int srDiff = GetSRDifference(closestSRTeam.TeamSR, player.GetSR());

        if (CanMatch(srDiff, selectedMode))
        {
            // .. Add the player that matches the team SR to the team
            closestSRTeam.AddPlayer(player);

            PlayerLeavingQueueEvent.Invoke(player.GetID());

            playersSortedRanked[selectedMode].Remove(player);

            // .. we got a full match
            if (closestSRTeam.TeamMembersCount == GetMatchPlayersCount(selectedMode))
            {
                // .. after matching a player to a team, add any removed player from the priority queue that failed to find a match to the priority queue again
                InsertInPlayersPriorityQueue(ref temporaryQueue);

                // .. remove the team if we have full 2 teams that can play together
                teamsSortedRanked[selectedMode].Remove(closestSRTeam);

                return CreateMatch(closestSRTeam);
            }
            else
                return null;
        }
        else
        {
            temporaryQueue.Enqueue(player);
        }

        return null;
    }

    /// <summary>
    /// Create a new team from two players of similar SR
    private void CreateTeamEnhanced(Player p1, Player p2)
    {
        // .. We got a team, remove the matched player so that we don't include them in future match making
        playersSortedRanked[selectedMode].Remove(p1);
        playersSortedRanked[selectedMode].Remove(p2);

        Team team = new Team(p1, p2);
        teamsSortedRanked[selectedMode].Add(team, team.TeamSR);
    }

    public int GetSRDifference(int p1, int p2)
    {
        return Mathf.Abs(p1 - p2);
    }

    // Players can match if their SR difference is lower than the threshold defined for the mode being selected
    public bool CanMatch(int difference, GameMode mode)
    {
        return difference < GameManager.Instance.GetMatchingThreshold(mode);
    }

    // Create a match of the selected players and return it
    private Match CreateMatch(List<Player> players)
    {
        HashSet<Player> team1 = new HashSet<Player>();
        HashSet<Player> team2 = new HashSet<Player>();

        for (int i = 0; i < GetMatchPlayersCount(selectedMode); i++)
        {
            if (i < GetMatchPlayersCount(selectedMode) * .5f)
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

    // Create a match of the selected players and return it
    private Match CreateMatch(Team team)
    {
        HashSet<Player> team1 = new HashSet<Player>();
        HashSet<Player> team2 = new HashSet<Player>();

        for (int i = 0; i < GetMatchPlayersCount(selectedMode); i++)
        {
            if (i < GetMatchPlayersCount(selectedMode) * .5f)
            {
                team1.Add(team.GetPlayers()[i]);
            }
            else
            {
                team2.Add(team.GetPlayers()[i]);
            }
        }

        return new Match(team1, team2);
    }

    // For each mode, get the total players number
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

    // Calculate the team SR by getting the median
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

// Base class for the player leaving queue even, defined to include the player id as an argument
public class PlayerLeavingQueueBaseEvent : UnityEvent<string>
{ }