namespace BeamSplit.Utilities
{
    /// <summary>Cross-cutting UI timing tunables not scoped to powerups (see PowerupController's Inspector fields for those).</summary>
    public static class GameConstants
    {
        public const float ToastFadeInDuration = 0.15f;
        public const float ToastHoldDuration = 1.2f;
        public const float ToastFadeOutDuration = 0.35f;
        public const float ToastStackOffsetY = 60f;
        public const int ToastPoolSize = 4;

        // Anchored to the top of the screen, not the bottom — a bottom banner ad
        // would otherwise sit on top of toasts anchored there.
        public const float ToastTopMargin = 150f;

        public const float PanelFadeDuration = 0.2f;

        public const float HintHighlightDuration = 3f;

        public const string TimeFormat = "mm\\:ss";

        public const int CoinFlyMaxCount = 8;
        public const float CoinFlyDuration = 0.5f;
        public const float CoinFlyStagger = 0.08f;
    }
}
