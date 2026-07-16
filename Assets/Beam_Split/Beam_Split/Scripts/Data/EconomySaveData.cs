using System;

namespace BeamSplit.Data
{
    /// <summary>
    /// Plain serializable shape of the persisted save blob (JsonUtility, not a
    /// ScriptableObject). See Managers/SaveManager.cs for read/write.
    /// </summary>
    [Serializable]
    public class EconomySaveData
    {
        public int coins;
        public int schemaVersion;
        public int currentLevelIndex;
    }
}
