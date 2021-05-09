using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Matchmaker : IMatchmaker
{
    private TeamsBuilder teamsBuilder;

    public Matchmaker()
    {
        teamsBuilder = new TeamsBuilder();
    }

    public Match FindMatch(GameMode gameMode)
    {
        return teamsBuilder.TryFindingMatch(gameMode);
    }

    // This is where player enters the matchmaking individually
    public void EnterMatchmaking(Player player, GameMode gameMode)
    {
        teamsBuilder.InsertPlayerInQueue(player, gameMode);
    }

    // return the even that will be fired as the player is removed from the queue
    public PlayerLeavingQueueBaseEvent GetEnteringTeamEvent()
    {
        return teamsBuilder.PlayerLeavingQueueEvent;
    }
}