namespace PPCorps
{
    public enum GameState
    {
        Deploy,
        Battle,
        Win,
        Lose
    }

    public enum UnitType
    {
        Melee,
        Ranged
    }

    public enum UnitAction
    {
        Idle,
        Moving,
        Attacking,
        Dead
    }
}
