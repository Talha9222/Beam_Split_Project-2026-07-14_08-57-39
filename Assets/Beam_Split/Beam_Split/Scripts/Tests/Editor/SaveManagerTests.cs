using BeamSplit.Data;
using BeamSplit.Managers;
using NUnit.Framework;
using UnityEngine;

namespace BeamSplit.Tests
{
    /// <summary>
    /// Caveat (plan Section 2): SaveManager touches PlayerPrefs, which EditMode tests can
    /// exercise but will pollute the real Editor's PlayerPrefs unless cleaned up. Every test
    /// here deletes the "BeamSplit.Save" key in TearDown to leave no residue.
    /// </summary>
    public class SaveManagerTests
    {
        private const string SaveKey = "BeamSplit.Save";

        [TearDown]
        public void TearDown()
        {
            PlayerPrefs.DeleteKey(SaveKey);
        }

        [Test]
        public void EconomySaveData_JsonRoundTrip_PreservesFields()
        {
            var original = new EconomySaveData { coins = 123, schemaVersion = 1 };
            string json = JsonUtility.ToJson(original);
            var roundTripped = JsonUtility.FromJson<EconomySaveData>(json);

            Assert.AreEqual(original.coins, roundTripped.coins);
            Assert.AreEqual(original.schemaVersion, roundTripped.schemaVersion);
        }

        [Test]
        public void Load_KeyAbsent_ReturnsZeroedDefault()
        {
            PlayerPrefs.DeleteKey(SaveKey);

            var data = SaveManager.Load();

            Assert.AreEqual(0, data.coins);
            Assert.AreEqual(1, data.schemaVersion);
        }

        [Test]
        public void SaveThenLoad_RoundTripsCoins()
        {
            SaveManager.Save(new EconomySaveData { coins = 77, schemaVersion = 1 });

            var loaded = SaveManager.Load();

            Assert.AreEqual(77, loaded.coins);
        }
    }
}
