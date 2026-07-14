namespace BeamSplit.Gameplay.Economy
{
    /// <summary>
    /// Pure C# coin balance holder — no Unity API usage, fully unit-testable.
    /// Owned by EconomyManager (MonoBehaviour), which persists it via SaveManager.
    /// </summary>
    public class CoinLedger
    {
        public int Coins { get; private set; }

        public CoinLedger(int startingCoins = 0)
        {
            Coins = startingCoins < 0 ? 0 : startingCoins;
        }

        public bool CanAfford(int cost)
        {
            return cost >= 0 && Coins >= cost;
        }

        /// <summary>
        /// Returns false and no-ops if cost is negative or balance is insufficient.
        /// </summary>
        public bool TrySpend(int cost)
        {
            if (!CanAfford(cost))
            {
                return false;
            }

            Coins -= cost;
            return true;
        }

        public void Add(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            Coins += amount;
        }
    }
}
