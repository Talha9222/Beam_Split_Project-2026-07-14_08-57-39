using BeamSplit.Data;
using BeamSplit.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace BeamSplit.Tests
{
    public class GridDirectionTests
    {
        [Test]
        public void Reflect_E_ReturnsNE_SE()
        {
            var (a, b) = GridDirection.Reflect(GridDirection.E);
            Assert.AreEqual(GridDirection.NE, a);
            Assert.AreEqual(GridDirection.SE, b);
        }

        [Test]
        public void Reflect_NE_ReturnsN_E()
        {
            var (a, b) = GridDirection.Reflect(GridDirection.NE);
            Assert.AreEqual(GridDirection.N, a);
            Assert.AreEqual(GridDirection.E, b);
        }

        [Test]
        public void Reflect_N_ReturnsNW_NE()
        {
            var (a, b) = GridDirection.Reflect(GridDirection.N);
            Assert.AreEqual(GridDirection.NW, a);
            Assert.AreEqual(GridDirection.NE, b);
        }

        [Test]
        public void Reflect_NW_ReturnsW_N()
        {
            var (a, b) = GridDirection.Reflect(GridDirection.NW);
            Assert.AreEqual(GridDirection.W, a);
            Assert.AreEqual(GridDirection.N, b);
        }

        [Test]
        public void Reflect_W_ReturnsSW_NW()
        {
            var (a, b) = GridDirection.Reflect(GridDirection.W);
            Assert.AreEqual(GridDirection.SW, a);
            Assert.AreEqual(GridDirection.NW, b);
        }

        [Test]
        public void Reflect_SW_ReturnsS_W()
        {
            var (a, b) = GridDirection.Reflect(GridDirection.SW);
            Assert.AreEqual(GridDirection.S, a);
            Assert.AreEqual(GridDirection.W, b);
        }

        [Test]
        public void Reflect_S_ReturnsSE_SW()
        {
            var (a, b) = GridDirection.Reflect(GridDirection.S);
            Assert.AreEqual(GridDirection.SE, a);
            Assert.AreEqual(GridDirection.SW, b);
        }

        [Test]
        public void Reflect_SE_ReturnsE_S()
        {
            var (a, b) = GridDirection.Reflect(GridDirection.SE);
            Assert.AreEqual(GridDirection.E, a);
            Assert.AreEqual(GridDirection.S, b);
        }

        [TestCase(Direction.Right, 1, 0)]
        [TestCase(Direction.Left, -1, 0)]
        [TestCase(Direction.Up, 0, 1)]
        [TestCase(Direction.Down, 0, -1)]
        public void FromDirection_MapsCardinalsCorrectly(Direction direction, int expectedX, int expectedY)
        {
            Vector2Int result = GridDirection.FromDirection(direction);
            Assert.AreEqual(new Vector2Int(expectedX, expectedY), result);
        }
    }
}
