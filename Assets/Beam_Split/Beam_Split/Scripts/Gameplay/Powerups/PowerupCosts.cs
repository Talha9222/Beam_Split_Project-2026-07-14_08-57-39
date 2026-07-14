namespace BeamSplit.Gameplay.Powerups
{
    /// <summary>Tunable powerup costs/grants. Kept scoped to just costs — see Utilities/GameConstants for UI timing.</summary>
    public static class PowerupCosts
    {
        public const int AddTimeCost = 15;
        public const float AddTimeSeconds = 15f;

        public const int AddMovesCost = 15;
        public const int AddMovesGrant = 3;

        public const int HintCost = 20;
        public const int UndoCost = 10;
    }
}
