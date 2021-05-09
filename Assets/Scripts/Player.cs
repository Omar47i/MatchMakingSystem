using Newtonsoft.Json;
/**
 * <p>
 * Representation of a player.
 * </p>
 * <p>
 * As indicated in the challenge description, feel free to augment the Player
 * class in any way that you feel will improve your final matchmaking solution.
 * <strong>Do NOT remove the name, wins, or losses fields.</strong> Also note
 * that if you make any of these changes, you are responsible for updating the
 * {@link SampleData} such that it provides a useful data set to exercise your
 * solution.
 * </p>
 */
public class Player
{
    private readonly string id;
    private readonly string name;
    private readonly int wins;
    private readonly int losses;
    private readonly int winstreak;
    private readonly int sr;
    private bool isDirty = false;
    public Player(string id, string name, int wins, int losses, int winstreak, int sr)
    {
        this.id = id;
        this.name = name;
        this.wins = wins;
        this.losses = losses;
        this.winstreak = winstreak;
        this.sr = sr;
    }

    public string GetID()
    {
        return id;
    }

    public string GetName()
    {
        return name;
    }

    public long GetWins()
    {
        return wins;
    }

    public long GetLosses()
    {
        return losses;
    }

    public long GetWinStreak()
    {
        return winstreak;
    }

    public int GetSR()
    {
        return sr;
    }

    public bool IsDirty() => isDirty;

    public void SetDirty() => isDirty = true;

    public void SetUnDirty() => isDirty = false;
}