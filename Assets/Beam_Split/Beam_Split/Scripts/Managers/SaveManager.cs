using BeamSplit.Data;
using UnityEngine;

namespace BeamSplit.Managers
{
    /// <summary>
    /// Static class — no scene presence needed. Single versioned JSON blob (EconomySaveData)
    /// under one PlayerPrefs key. Writes flush immediately via PlayerPrefs.Save() so a
    /// mid-session crash/force-quit on mobile doesn't lose a coin change.
    /// </summary>
    public static class SaveManager
    {
        private const string SaveKey = "BeamSplit.Save";

        public static EconomySaveData Load()
        {
            string json = PlayerPrefs.GetString(SaveKey, string.Empty);
            if (string.IsNullOrEmpty(json))
            {
                return new EconomySaveData { coins = 0, schemaVersion = 1 };
            }

            var data = JsonUtility.FromJson<EconomySaveData>(json);
            return data ?? new EconomySaveData { coins = 0, schemaVersion = 1 };
        }

        public static void Save(EconomySaveData data)
        {
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.Save();
        }
    }
}
