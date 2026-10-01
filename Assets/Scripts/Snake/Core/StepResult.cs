namespace SnakeJam
{
    /// What happened during one tick of the game.
    public enum StepResult
    {
        Moved,               // snake moved into an empty cell
        Ate,                 // snake moved onto the normal food and grew
        AteBonus,            // snake moved onto the bonus food (no growth)
        AteBonusAndNormal,   // rare: same tick landed on both
        Won,                 // snake ate the last food and the board is full
        Died                 // snake hit a wall or itself
    }
}