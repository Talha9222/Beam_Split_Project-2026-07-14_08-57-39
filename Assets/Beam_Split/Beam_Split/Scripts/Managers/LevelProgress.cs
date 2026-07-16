using BeamSplit.Data;

namespace BeamSplit.Managers
{
    /// <summary>
    /// Static hand-off for which LevelData to load next time CoreSimTest.unity starts.
    /// Static fields survive a same-session SceneManager.LoadScene (only a domain reload
    /// or exiting Play mode clears them), so this is enough to carry the next level across
    /// the Win -> Next Level and Lose -> Retry scene reloads without a DontDestroyOnLoad
    /// object. Not meant to survive an app restart itself - for that, LevelLoader.Start()
    /// falls back to SaveManager.Load().currentLevelIndex when this is null (see CLAUDE.md
    /// "Save").
    /// </summary>
    public static class LevelProgress
    {
        public static LevelData PendingLevel;
    }
}
