using System.Collections.Generic;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public static class ExtensionMethods
{
    // .. Find closest value in the players list to the target sr value
    public static string thisName;
    public static Player FindClosest(this SortedSet<Player> players, int target, Player removeTemporarily)
    {
        thisName = removeTemporarily.GetName();
        players.RemoveWhere(Exists);

        int n = players.Count;

        // Corner cases
        if (target <= players.ElementAt(0).GetSR())
        {
            Player returnValue = players.ElementAt(0);
            players.Add(removeTemporarily);
            return returnValue;
        }

        if (target >= players.ElementAt(n - 1).GetSR())
        {
            Player returnValue = players.ElementAt(n - 1);
            players.Add(removeTemporarily);
            return returnValue;
        }

        // Doing binary search
        int i = 0, j = n, mid = 0;
        while (i < j)
        {
            mid = (i + j) / 2;

            if (players.ElementAt(mid).GetSR() == target)
            {
                Player returnValue = players.ElementAt(mid);
                players.Add(removeTemporarily);

                return returnValue;
            }

            /* If target is less
            than array element,
            then search in left */
            if (target < players.ElementAt(mid).GetSR())
            {

                // If target is greater
                // than previous to mid,
                // return closest of two
                if (mid > 0 && target > players.ElementAt(mid - 1).GetSR())
                {
                    Player returnValue = getClosest(players.ElementAt(mid - 1),
                                 players.ElementAt(mid), target);
                    players.Add(removeTemporarily);

                    return returnValue;
                }

                /* Repeat for left half */
                j = mid;
            }

            // If target is
            // greater than mid
            else
            {
                if (mid < n - 1 && target < players.ElementAt(mid + 1).GetSR())
                {
                    Player returnValue = getClosest(players.ElementAt(mid),
                         players.ElementAt(mid + 1), target);
                    players.Add(removeTemporarily);

                    return returnValue;
                }
                i = mid + 1; // update i
            }
        }

        // Only single element
        // left after search
        Player retValue = players.ElementAt(mid);

        players.Add(removeTemporarily);

        return retValue;
    }

    // Method to compare which one
    // is the more close We find the
    // closest by taking the difference
    // between the target and both
    // values. It assumes that val2 is
    // greater than val1 and target
    // lies between these two.
    public static Player getClosest(Player val1, Player val2,
                                 int target)
    {
        if (target - val1.GetSR() >= val2.GetSR() - target)
            return val2;
        else
            return val1;
    }

    // Defines a predicate delegate to use
    // for the SortedSet.RemoveWhere method.
    private static bool Exists(Player p)
    {
        return p.GetName().StartsWith("Ka");
    }

}
