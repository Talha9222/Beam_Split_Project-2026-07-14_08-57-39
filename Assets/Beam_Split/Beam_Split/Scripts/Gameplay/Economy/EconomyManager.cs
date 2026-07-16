using System;
using BeamSplit.Managers;
using UnityEngine;

namespace BeamSplit.Gameplay.Economy
{
    /// <summary>
    /// Scene singleton (static Instance, same pattern as GridManager). Owns a CoinLedger,
    /// loads it via SaveManager.Load() in Awake(), persists immediately on every
    /// balance-changing call (not batched).
    /// </summary>
    public class EconomyManager : MonoBehaviour
    {
        public static EconomyManager Instance { get; private set; }

        private CoinLedger ledger;

        public event Action<int> OnCoinsChanged;

        public int Coins => ledger.Coins;

        private void Awake()
        {
            Instance = this;

            var save = SaveManager.Load();
            ledger = new CoinLedger(save.coins);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public bool TrySpend(int cost)
        {
            if (!ledger.TrySpend(cost))
            {
                return false;
            }

            Persist();
            OnCoinsChanged?.Invoke(ledger.Coins);
            return true;
        }

        public void Award(int amount)
        {
            ledger.Add(amount);
            Persist();
            OnCoinsChanged?.Invoke(ledger.Coins);
        }

        private void Persist()
        {
            var save = SaveManager.Load();
            save.coins = ledger.Coins;
            save.schemaVersion = 1;
            SaveManager.Save(save);
        }
    }
}
