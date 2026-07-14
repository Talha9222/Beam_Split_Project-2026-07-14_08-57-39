namespace BeamSplit.Utilities
{
    /// <summary>Cross-cutting UI timing tunables not scoped to powerups (see Gameplay/Powerups/PowerupCosts.cs for those).</summary>
    public static class GameConstants
    {
        public const float ToastFadeInDuration = 0.15f;
        public const float ToastHoldDuration = 1.2f;
        public const float ToastFadeOutDuration = 0.35f;
        public const float ToastStackOffsetY = 60f;
        public const int ToastPoolSize = 4;

        public const float PanelFadeDuration = 0.2f;

        public const float HintHighlightDuration = 3f;

        public const string TimeFormat = "mm\\:ss";
    }
}
