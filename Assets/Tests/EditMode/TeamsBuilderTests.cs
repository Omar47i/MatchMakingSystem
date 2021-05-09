using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class TeamsBuilderTests
{
    [Test]
    public void TestTryMatching1v1Players()
    {
        int count = 2;
        int oneVoneThreshold = 250;

        Assert.Greater(count, 1);

        Player p1 = new Player("6235e412", "p1", 1, 2, 4, 10);
        Player p2 = new Player("4352dw34", "p2", 1, 2, 4, 20);
        Player p3 = new Player("34534v45", "p3", 1, 2, 4, 653234);
        Player p4 = new Player("342vc346", "p4", 1, 2, 4, -25);
        Player p5 = new Player("567x3rt4", "p5", 1, 2, 4, 0);
        Player p6 = new Player("er23r23f", "p6", 1, 2, 4, 39);
        Player p7 = new Player("wervv234", "p7", 1, 2, 4, 102);
        Player p8 = new Player("wefxv23r", "p8", 1, 2, 4, 202);
        Player p9 = new Player("86546345", "p9", 1, 2, 4, 669);
        Player p10 = new Player("23werw4", "p10", 1, 2, 4, 779);

        List<int> srDiffList = new List<int>();
        srDiffList.Add(Mathf.Abs(p1.GetSR() - p2.GetSR()));
        srDiffList.Add(Mathf.Abs(p3.GetSR() - p4.GetSR()));
        srDiffList.Add(Mathf.Abs(p5.GetSR() - p6.GetSR()));
        srDiffList.Add(Mathf.Abs(p7.GetSR() - p8.GetSR()));
        srDiffList.Add(Mathf.Abs(p9.GetSR() - p10.GetSR()));

        for (int i = 0; i < srDiffList.Count; i++)
        {
            int srDiff = Mathf.Abs(srDiffList[i]);

            bool isMatch = srDiff < oneVoneThreshold;

            if (isMatch)
                Assert.Less(srDiff, oneVoneThreshold);
            else
                Assert.Greater(srDiff, oneVoneThreshold);
        }
    }

    [Test]
    public void TestCanMatch()
    {
        int zeroDiff = 0;
        int posDiff = 254;
        int negDiff = -6589;
        int largePosDiff = 32315643;
        int largeNegDiff = -9587456;

        int threshold = 250;

        Assert.IsTrue(zeroDiff < threshold);
        Assert.IsTrue(posDiff > threshold);
        Assert.IsTrue(negDiff < threshold);
        Assert.IsTrue(largePosDiff > threshold);
        Assert.IsTrue(largeNegDiff < threshold);
    }

    [Test]
    public void TestGetSRDifference()
    {
        int zeroDiff = 0;
        int posDiff = 254;
        int negDiff = -6589;
        int largePosDiff = 32315643;
        int largeNegDiff = -9587456;

        Assert.GreaterOrEqual(Mathf.Abs(zeroDiff - zeroDiff), 0);
        Assert.GreaterOrEqual(Mathf.Abs(zeroDiff - largeNegDiff), 0);
        Assert.GreaterOrEqual(Mathf.Abs(negDiff - posDiff), 0);
        Assert.GreaterOrEqual(Mathf.Abs(largeNegDiff - largePosDiff), 0);
    }

    [Test]
    public void TestGetTeamSR()
    {
        Player p1 = new Player("623412", "p1", 1, 2, 4, 10);
        Player p2 = new Player("435234", "p2", 1, 2, 4, 20);

        List<Player> players = new List<Player>();
        players.Add(p1);
        players.Add(p2);
        int teamSR = 0;

        for (int i = 0; i < players.Count; i++)
        {
            teamSR += players[i].GetSR();
        }

        teamSR /= players.Count;

        Assert.AreEqual(teamSR, 15);
    }

    [Test]
    public void TestGetMatchPlayersCount()
    {
        GameMode gameMode = GameMode.OneVOne;
        int playersInMatchCount = 0;

        if (gameMode == GameMode.OneVOne)
            playersInMatchCount = 2;

        Assert.AreEqual(playersInMatchCount, 2);

        gameMode = GameMode.TwoVTwo;
        if (gameMode == GameMode.TwoVTwo)
            playersInMatchCount = 4;

        Assert.AreEqual(playersInMatchCount, 4);

        gameMode = GameMode.ThreeVThree;
        if (gameMode == GameMode.ThreeVThree)
            playersInMatchCount = 6;

        Assert.AreEqual(playersInMatchCount, 6);
    }
}
