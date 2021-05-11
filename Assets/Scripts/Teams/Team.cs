using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Team
{
    public int TeamSR { get; private set; }

    public int TeamMembersCount { get; private set; }

    private List<Player> players = new List<Player>();

    public Team() { }

    public Team(Player p1, Player p2)
    {
        players.Add(p1);
        players.Add(p2);

        TeamSR = (p1.GetSR() + p2.GetSR()) / 2;
        TeamMembersCount = 2;
    }

    public void AddPlayer(Player p)
    {
        players.Add(p);

        RecalculateTeamSR();

        TeamMembersCount++;
    }

    private void RecalculateTeamSR()
    {
        int sum = 0;

        for (int i = 0; i < players.Count; i++)
            sum += players[i].GetSR();

        TeamSR = sum / players.Count;
    }

    public List<Player> GetPlayers()
    {
        return players;
    }
}
