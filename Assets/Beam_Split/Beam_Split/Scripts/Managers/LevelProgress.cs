using BeamSplit.Data;

namespace BeamSplit.Managers
{
    /// <summary>
    /// Static hand-off for which LevelData to load next time CoreSimTest.unity starts.
    /// Static fields survive a same-session SceneManager.LoadScene (only a domain reload
    /// or exiting Play mode clears them), so this is enough to carry the next level across
    /// the Win -> Next Level scene reload without a DontDestroyOnLoad object. Not meant to
    /// survive an app restart - level progress isn't persisted, only coins are (SaveManager).
    /// </summary>
    public static class LevelProgress
    {
        public static LevelData PendingLevel;
    }
}
