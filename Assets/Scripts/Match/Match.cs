
using System.Collections.Generic;

public class Match
{
    private readonly HashSet<Player> team1;
    private readonly HashSet<Player> team2;

    public Match(HashSet<Player> team1, HashSet<Player> team2)
    {
        this.team1 = team1;
        this.team2 = team2;
    }

    public HashSet<Player> GetTeam1()
    {
        return team1;
    }

    public HashSet<Player> GetTeam2()
    {
        return team2;
    }
}