using BeamSplit.Gameplay.Economy;
using NUnit.Framework;

namespace BeamSplit.Tests
{
    public class CoinLedgerTests
    {
        [Test]
        public void CanAfford_ExactBalance_ReturnsTrue()
        {
            var ledger = new CoinLedger(50);
            Assert.IsTrue(ledger.CanAfford(50));
        }

        [Test]
        public void CanAfford_OverBalanceByOne_ReturnsFalse()
        {
            var ledger = new CoinLedger(49);
            Assert.IsFalse(ledger.CanAfford(50));
        }

        [Test]
        public void TrySpend_ExactBalance_SucceedsAndZeroesBalance()
        {
            var ledger = new CoinLedger(50);
            Assert.IsTrue(ledger.TrySpend(50));
            Assert.AreEqual(0, ledger.Coins);
        }

        [Test]
        public void TrySpend_InsufficientFunds_FailsAndDoesNotChangeBalance()
        {
            var ledger = new CoinLedger(10);
            Assert.IsFalse(ledger.TrySpend(11));
            Assert.AreEqual(10, ledger.Coins);
        }

        [Test]
        public void TrySpend_NegativeCost_Guarded_ReturnsFalse()
        {
            var ledger = new CoinLedger(10);
            Assert.IsFalse(ledger.TrySpend(-5));
            Assert.AreEqual(10, ledger.Coins);
        }

        [Test]
        public void Add_AccumulatesAcrossMultipleCalls()
        {
            var ledger = new CoinLedger(0);
            ledger.Add(30);
            ledger.Add(20);
            Assert.AreEqual(50, ledger.Coins);
        }

        [Test]
        public void Add_NonPositiveAmount_NoOps()
        {
            var ledger = new CoinLedger(10);
            ledger.Add(0);
            ledger.Add(-5);
            Assert.AreEqual(10, ledger.Coins);
        }

        [Test]
        public void Constructor_NegativeStartingCoins_ClampsToZero()
        {
            var ledger = new CoinLedger(-20);
            Assert.AreEqual(0, ledger.Coins);
        }
    }
}
