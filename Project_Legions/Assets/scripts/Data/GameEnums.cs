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

    public enum UnitClass
    {
        None,
        射手,
        战士,
        先锋,
        守卫,
        斥候
    }

    public enum UnitAction
    {
        Idle,
        Moving,
        Attacking,
        Dead
    }
}
