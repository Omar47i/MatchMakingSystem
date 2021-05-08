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
    private readonly string name;
    private readonly long wins;
    private readonly long losses;

    public Player(string name, long wins, long losses)
    {
        this.name = name;
        this.wins = wins;
        this.losses = losses;
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
}
