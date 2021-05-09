using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public class TeamsBuilder
{
    // .. Holds all the players that enters match making and are waiting to be added to teams or matched against a player in 1v1
    Dictionary<GameMode, Queue<Player>> playersInQueue = new Dictionary<GameMode, Queue<Player>>();

    // .. Holds the players that haven't found a match due to large difference in the SR, we will handle them first as new players join
    Dictionary<GameMode, Queue<Player>> priorityPlayersInQueue = new Dictionary<GameMode, Queue<Player>>();

    // .. Holds all the players sorted based on their SR ascendingly 
    Dictionary<GameMode, SortedSet<Player>> playersSortedRanked = new Dictionary<GameMode, SortedSet<Player>>();

    // .. Players of close SR values will be matched together in teams, except for the 1v1 players as they are not within a team
    Dictionary<GameMode, List<List<Player>>> playersInTeams = new Dictionary<GameMode, List<List<Player>>>();

    // .. 
    //Dictionary<GameMode, List<List<Player>>> playersInTeamsAndMatched = new Dictionary<GameMode, List<List<Player>>>();

    private GameMode activeMode;    // the mode that we are trying the matchmaking for

    public TeamsBuilder()
    {
        playersInQueue[GameMode.OneVOne] = new Queue<Player>();
        playersInQueue[GameMode.TwoVTwo] = new Queue<Player>();
        playersInQueue[GameMode.ThreeVThree] = new Queue<Player>();

        priorityPlayersInQueue[GameMode.OneVOne] = new Queue<Player>();
        priorityPlayersInQueue[GameMode.TwoVTwo] = new Queue<Player>();
        priorityPlayersInQueue[GameMode.ThreeVThree] = new Queue<Player>();

        playersSortedRanked[GameMode.OneVOne] = new SortedSet<Player>(new BySR());
        playersSortedRanked[GameMode.TwoVTwo] = new SortedSet<Player>(new BySR());
        playersSortedRanked[GameMode.ThreeVThree] = new SortedSet<Player>(new BySR());

        playersInTeams[GameMode.TwoVTwo] = new List<List<Player>>();
        playersInTeams[GameMode.ThreeVThree] = new List<List<Player>>();
    }

    public void InsertPlayerInQueue(Player player, GameMode gameMode)
    {
        // .. Add new players to the priority queue to priotorize the first in queue,
        // .. also them to a sorted list based on their SR to efficiently find the closest player with SR
        playersInQueue[gameMode].Enqueue(player);

        playersSortedRanked[gameMode].Add(player);
        //Debug.Log("sorted list count: " + playersSortedRanked[GameMode.OneVOne].Count);
    }

    //@Debug
    public void PrintSortedPlayers()
    {
        //int i = 0;
        //foreach(Player p in playersSortedRanked[GameMode.OneVOne])
        //{
        //    Debug.Log("player " + i++ + ": " + p.GetName());
        //}

        //Player kate = playersInQueue[GameMode.OneVOne].Dequeue();
        //Debug.Log("closest player to " + kate.GetName() + " is " + 
        //    playersSortedRanked[GameMode.OneVOne].FindClosest(kate.GetSR(), kate).GetName());
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
        activeMode = GameMode.OneVOne;
        // Check the priority queue first for possible matches with newly joined players
        //int priorityCount = priorityPlayersInQueue[GameMode.OneVOne].Count;

        //Queue<Player> newlyJoinedPlayers = playersInQueue[GameMode.OneVOne];

        // used to hold the dequeued players that havent found a match yet, we will insert them again in the 
        // priority queue
        Queue<Player> temporaryQueue = new Queue<Player>();

        // Loop through all the priority players and check if there is a close SR player in the ranked list
        while (priorityPlayersInQueue[GameMode.OneVOne].Count != 0)
        {
            Player priorityPlayer = priorityPlayersInQueue[GameMode.OneVOne].Dequeue();

            Player closestPlayer = playersSortedRanked[GameMode.OneVOne].FindClosest(priorityPlayer.GetSR(), priorityPlayer);

            // .. Get the SR difference between the two players searching for a game
            int srDiff = GetSRDifference(closestPlayer.GetSR(), priorityPlayer.GetSR());

            if (CanMatch(srDiff, GameMode.OneVOne))
            {
                // we got a match, remove the matched players so that we don't include them in future match making
                playersSortedRanked[GameMode.OneVOne].Remove(closestPlayer);
                playersSortedRanked[GameMode.OneVOne].Remove(priorityPlayer);

                List<Player> players = new List<Player>();
                players.Add(priorityPlayer);
                players.Add(closestPlayer);

                // .. insert any removed players to the priority queue again
                InsertInPlayersPriorityQueue(ref temporaryQueue);

                return CreateMatch(players);
            }
            else
            {
                temporaryQueue.Enqueue(priorityPlayer);
            }
        }

        // Insert the dequeued priority players into the priority queue again to give them advatage when searching
        // for a game with newly joined players
        InsertInPlayersPriorityQueue(ref temporaryQueue);

        // .. If the priority queue players couldn't find a match yet, try to match other players until they
        // .. could be matched with newly joined players
        while (playersInQueue[GameMode.OneVOne].Count != 0)
        {
            Player player = playersInQueue[GameMode.OneVOne].Dequeue();

            // .. Don't process the player if it's already got matched
            if (player.IsDirty())
                continue;

            Player closestPlayer = playersSortedRanked[GameMode.OneVOne].FindClosest(player.GetSR(), player);

            // .. Get the SR difference between the two players searching for a game
            int srDiff = GetSRDifference(closestPlayer.GetSR(), player.GetSR());

            if (CanMatch(srDiff, GameMode.OneVOne))
            {
                // .. Set the player that got matched to be Dirty which means they cannot be matched again until
                // .. they get removed from the playersInQueue
                closestPlayer.SetDirty();

                // .. We got a match, remove the matched players so that we don't include them in future match making
                playersSortedRanked[GameMode.OneVOne].Remove(closestPlayer);
                playersSortedRanked[GameMode.OneVOne].Remove(player);

                // .. Create a match
                List<Player> players = new List<Player>();
                players.Add(player);
                players.Add(closestPlayer);

                return CreateMatch(players);
            }
            else
            {
                priorityPlayersInQueue[GameMode.OneVOne].Enqueue(player);
            }
        }

        //int count = playersInQueue[GameMode.OneVOne].Count;

        //// .. Do nothing if there is not enough players searching for a game
        //if (count == 0 || count == 1)
        //    return null;

        //List<Player> onevonePlayers = playersInQueue[GameMode.OneVOne];

        //for (int i = 0; i < count - 1; i++)
        //{
        //    Player p1 = onevonePlayers[i];

        //    for (int j = i + 1; j < count; j++)
        //    {
        //        Player p2 = onevonePlayers[j];

        //        // .. Get the SR difference between the two players searching for a game
        //        int srDiff = GetSRDifference(p1.GetSR(), p2.GetSR());

        //        if (CanMatch(srDiff, GameMode.OneVOne))
        //        {
        //            // we got a match, remove the matched players so that we don't include them in future match making
        //            playersInQueue[GameMode.OneVOne].Remove(p1);
        //            playersInQueue[GameMode.OneVOne].Remove(p2);

        //            List<Player> players = new List<Player>();
        //            players.Add(p1);
        //            players.Add(p2);
        //            return CreateMatch(players);
        //        }
        //    }
        //}

        return null;
    }

    // insert the dequeued players that couldn't find a match into the priority queue again
    private void InsertInPlayersPriorityQueue(ref Queue<Player> tempQueue)
    {
        while (tempQueue.Count != 0)
            priorityPlayersInQueue[activeMode].Enqueue(tempQueue.Dequeue());
    }

    /// <summary>
    /// add potential players to the same team in the 3v3 mode
    /// </summary>
    /// <returns>a match if one exist</returns>
    private Match TryMatching2v2_3v3Players()
    {
        Queue<Player> temporaryQueue = new Queue<Player>();
        bool isMatched = false;

        // .. Give the priority to created teams already that are down players
        // .. Loop through all the priority players and check if there is a close SR player alr in a team
        while (priorityPlayersInQueue[activeMode].Count != 0)
        {
            Player priorityPlayer = priorityPlayersInQueue[activeMode].Dequeue();

            Match match = TryMatching2v2_3v3PlayersInTeam(priorityPlayer, out isMatched);

            if (match != null)
            {
                InsertInPlayersPriorityQueue(ref temporaryQueue);

                return match;
            }
            else
            {
                if (!isMatched)
                {
                    temporaryQueue.Enqueue(priorityPlayer);
                }
            }
        }

        InsertInPlayersPriorityQueue(ref temporaryQueue);

        // .. If the priority queue players couldn't find a match yet, try to match other players into teams
        while (playersInQueue[activeMode].Count != 0)
        {
            Player player = playersInQueue[activeMode].Dequeue();

            // .. Don't process the player if it's already got matched
            if (player.IsDirty())
                continue;

            Player closestPlayer = playersSortedRanked[activeMode].FindClosest(player.GetSR(), player);

            // .. Get the SR difference between the two players searching for a game
            int srDiff = GetSRDifference(closestPlayer.GetSR(), player.GetSR());

            if (CanMatch(srDiff, activeMode))
            {
                // we got a match, create a team out of the two players and remove them from the queue
                CreateTeam(player, closestPlayer);

                // .. insert any removed players to the priority queue again
                InsertInPlayersPriorityQueue(ref temporaryQueue);

                // return null as we don't have enough players yet to start a game
                return null;
            }
            else
            {
                temporaryQueue.Enqueue(player);
            }
        }

        // .. insert any removed players to the priority queue again
        InsertInPlayersPriorityQueue(ref temporaryQueue);

        return null;

        // .. If no match for queing players with teams due to SR difference, then try to create new teams
        // .. with close SR values from individual players

        //// Loop through all the priority players and check if there is a close SR player in the ranked list
        //while (priorityPlayersInQueue[activeMode].Count != 0)
        //{
        //    Player priorityPlayer = priorityPlayersInQueue[activeMode].Dequeue();

        //    Player closestPlayer = playersSortedRanked[GameMode.OneVOne].FindClosest(priorityPlayer.GetSR(), priorityPlayer);

        //    // .. Get the SR difference between the two players searching for a game
        //    int srDiff = GetSRDifference(closestPlayer.GetSR(), priorityPlayer.GetSR());

        //    if (CanMatch(srDiff, GameMode.OneVOne))
        //    {
        //        // we got a match, remove the matched players so that we don't include them in future match making
        //        playersSortedRanked[GameMode.OneVOne].Remove(closestPlayer);
        //        playersSortedRanked[GameMode.OneVOne].Remove(priorityPlayer);

        //        List<Player> players = new List<Player>();
        //        players.Add(priorityPlayer);
        //        players.Add(closestPlayer);

        //        // .. insert any removed players to the priority queue again
        //        InsertInPlayersPriorityQueue(ref temporaryQueue);

        //        return CreateMatch(players);
        //    }
        //    else
        //    {
        //        temporaryQueue.Enqueue(priorityPlayer);
        //    }
        //}

        //int playerInQueueCount = playersInQueue[activeMode].Count;

        //// .. Do nothing if there is not enough players searching for a game
        //if (playerInQueueCount == 0 || playerInQueueCount == 1)
        //    return null;

        //List<Player> playersInQueueList = playersInQueue[activeMode];

        //for (int i = 0; i < playerInQueueCount; i++)
        //{
        //    Player p1 = playersInQueueList[i];

        //    Match match = TryMatching2v2_3v3PlayersInTeam(p1, ref playerInQueueCount);

        //    if (match != null)
        //        return match;
        //}

        //// .. If no match for queing players with teams due to SR difference, then try to create new teams with close SR values from individual players

        ////playerInQueueCount = playersInQueue[GameMode.TwoVTwo].Count;

        //for (int i = 0; i < playerInQueueCount - 1; i++)
        //{
        //    Player p1 = playersInQueueList[i];

        //    for (int j = i + 1; j < playerInQueueCount; j++)
        //    {
        //        Player p2 = playersInQueueList[j];

        //        // .. Get the SR difference between the two players searching for a game
        //        int srDiff = GetSRDifference(p1.GetSR(), p2.GetSR());

        //        if (CanMatch(srDiff, activeMode))
        //        {
        //            // we got a match, create a team out of the two players and remove them from the queue
        //            CreateTeam(p1, p2);

        //            // return null as we don't have enough players yet to start a game
        //            return null;
        //        }
        //    }
        //}

        //return null;
    }

    private Match TryMatching2v2_3v3PlayersInTeam(Player p, out bool isMatched)
    {
        List<List<Player>> teams = playersInTeams[activeMode];
        isMatched = false;

        for (int j = 0; j < teams.Count; j++)
        {
            int teamSR = GetTeamSR(teams[j]);

            // .. Get the SR difference between the inspected player and the team SR
            int srDiff = GetSRDifference(p.GetSR(), teamSR);

            if (CanMatch(srDiff, activeMode))
            {
                isMatched = true;
                // we got a match, add player to the team and remove them from the queue
                teams[j].Add(p);
                //playersInQueue[activeMode].Remove(p);
                //playerInQueueCount--;

                // .. we got a full match
                if (teams[j].Count == GetMatchPlayersCount(activeMode))
                    return CreateMatch(teams[j]);
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
        // .. Set the player that got matched to be Dirty which means they cannot be matched again until
        // .. they get removed from the playersInQueue
        p2.SetDirty();

        // we got a match, remove the matched players so that we don't include them in future match making
        playersSortedRanked[activeMode].Remove(p1);
        playersSortedRanked[activeMode].Remove(p2);

        List<Player> newTeam = new List<Player>();
        newTeam.Add(p1);
        newTeam.Add(p2);

        playersInTeams[activeMode].Add(newTeam);
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

// Defines a comparer to create a sorted set
// that is sorted by the SR
public class BySR : IComparer<Player>
{
    int p1SR, p2SR;

    public int Compare(Player p1, Player p2)
    {
        p1SR = p1.GetSR();
        p2SR = p2.GetSR();

        // Compare the two SRs and sort them in ascending order
        if (p1SR > p2SR)
            return 1;
        else
            return -1;
    }
}