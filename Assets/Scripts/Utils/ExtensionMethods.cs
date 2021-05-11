using System;
using System.Collections.Generic;

public static class ExtensionMethods
{
    public static Player FindClosest(this SortedList<Player, int> players, Player targetPlayer)
    {
        int target = targetPlayer.GetSR();
        // Check to see if we need to search the list.
        //bool samePlayer = players.Count == 1 && players.Keys[0].GetHashCode().Equals(targetPlayer.GetHashCode());

        if (players == null || players.Count <= 0 /*|| samePlayer*/) { return null; }
        if (players.Count == 1) { return players.Keys[0]; }

        // remove the inspected target player from the sorted list to prevent comparing the target player with themselves
        bool isRemoved = players.Remove(targetPlayer);

        // Setup the variables needed to find the closest index
        int lower = 0;
        int upper = players.Count - 1;
        int index = (lower + upper) / 2;

        // Find the closest index (rounded down)
        while (lower <= upper)
        {
            int comparisonResult = decimal.Compare(target, players.Values[index]);
            if (comparisonResult == 0) { return players.Keys[index]; }

            if (comparisonResult < 0) { upper = index - 1; }
            else { lower = index + 1; }

            index = (lower + upper) / 2;
        }


        // Check to see if we are under or over the max values.
        if (index >= players.Count - 1) { return players.Keys[players.Count - 1]; }
        if (index < 0) { return players.Keys[0]; }

        // Check to see if we should have rounded up instead
        if (players.Values[index + 1] - target < target - players.Values[index]) { index++; }

        // Return the correct/closest player
        return players.Keys[index];
    }

    public static Team FindClosestInTeam(this SortedList<Team, int> players, Player targetPlayer)
    {
        int target = targetPlayer.GetSR();
        // Check to see if we need to search the list.
        //bool samePlayer = players.Count == 1 && players.Keys[0].GetHashCode().Equals(targetPlayer.GetHashCode());

        if (players == null || players.Count <= 0 /*|| samePlayer*/) { return null; }
        if (players.Count == 1) { return players.Keys[0]; }

        // Setup the variables needed to find the closest index
        int lower = 0;
        int upper = players.Count - 1;
        int index = (lower + upper) / 2;

        // Find the closest index (rounded down)
        while (lower <= upper)
        {
            int comparisonResult = decimal.Compare(target, players.Values[index]);
            if (comparisonResult == 0) { return players.Keys[index]; }

            if (comparisonResult < 0) { upper = index - 1; }
            else { lower = index + 1; }

            index = (lower + upper) / 2;
        }


        // Check to see if we are under or over the max values.
        if (index >= players.Count - 1) { return players.Keys[players.Count - 1]; }
        if (index < 0) { return players.Keys[0]; }

        // Check to see if we should have rounded up instead
        if (players.Values[index + 1] - target < target - players.Values[index]) { index++; }

        // Return the correct/closest player
        return players.Keys[index];
    }
}

// .. Sort the players based on their SR value
public class BySR : IComparer<Player>
{
    public int Compare(Player p1, Player p2)
    {
        if (p1.GetHashCode().Equals(p2.GetHashCode()))
        {
            //UnityEngine.Debug.Log("Removing " + p1.GetName());
            return 0;
        }
        else if (p1.GetSR() > p2.GetSR())
            return 1;

        return -1;
    }
}

// .. Sort the players based on their SR value
public class ByTeamSR : IComparer<Team>
{
    public int Compare(Team t1, Team t2)
    {
        if (t1.GetHashCode().Equals(t2.GetHashCode()))
        {
            //UnityEngine.Debug.Log("Removing " + p1.GetName());
            return 0;
        }
        else if (t1.TeamSR > t2.TeamSR)
            return 1;

        return -1;
    }
}