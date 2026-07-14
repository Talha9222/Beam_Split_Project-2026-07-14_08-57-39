using BeamSplit.Data;
using BeamSplit.Gameplay.Placement;
using NUnit.Framework;
using UnityEngine;

namespace BeamSplit.Tests
{
    public class PlacementHistoryTests
    {
        [Test]
        public void PushPop_LifoOrder()
        {
            var history = new PlacementHistory();
            history.Push(new PlacementRecord { cell = new Vector2Int(0, 0), kind = PlacementKind.Mirror });
            history.Push(new PlacementRecord { cell = new Vector2Int(1, 1), kind = PlacementKind.Filter, filterColor = BeamColor.Red });

            Assert.IsTrue(history.TryPop(out var first));
            Assert.AreEqual(new Vector2Int(1, 1), first.cell);

            Assert.IsTrue(history.TryPop(out var second));
            Assert.AreEqual(new Vector2Int(0, 0), second.cell);
        }

        [Test]
        public void TryPop_OnEmptyStack_ReturnsFalse_DoesNotThrow()
        {
            var history = new PlacementHistory();
            Assert.IsFalse(history.TryPop(out _));
        }

        [Test]
        public void Count_ReflectsPushesAndPops()
        {
            var history = new PlacementHistory();
            Assert.AreEqual(0, history.Count);

            history.Push(new PlacementRecord { cell = new Vector2Int(2, 2), kind = PlacementKind.Mirror });
            Assert.AreEqual(1, history.Count);

            history.TryPop(out _);
            Assert.AreEqual(0, history.Count);
        }
    }
}
