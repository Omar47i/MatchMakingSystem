
public interface Matchmaker
{
    /**
     * <p>
     * Find a match with the given number of players per team.
     * </p>
     * 
     * @param playersPerTeam
     *            the number of players required in each team
     * @return an appropriate match or null if there is no appropriate match
     */
    Match FindMatch(int playersPerTeam);

    /**
     * <p>
     * Add a player for matching.
     * </p>
     */
    void EnterMatchmaking(Player player);

}
