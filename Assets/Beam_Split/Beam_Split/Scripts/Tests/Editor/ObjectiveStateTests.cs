using BeamSplit.Gameplay.Objective;
using NUnit.Framework;

namespace BeamSplit.Tests
{
    public class ObjectiveStateTests
    {
        [Test]
        public void MoveLimitState_ConsumeMove_DownToZero_IsExhausted()
        {
            var state = new MoveLimitState(2);
            Assert.AreEqual(1, state.ConsumeMove());
            Assert.IsFalse(state.IsExhausted);
            Assert.AreEqual(0, state.ConsumeMove());
            Assert.IsTrue(state.IsExhausted);
        }

        [Test]
        public void MoveLimitState_ConsumeMove_ClampsAtZero_DoesNotGoNegative()
        {
            var state = new MoveLimitState(0);
            Assert.AreEqual(0, state.ConsumeMove());
            Assert.AreEqual(0, state.MovesRemaining);
        }

        [Test]
        public void MoveLimitState_AddMovesAfterExhaustion_UnExhaustsIt()
        {
            var state = new MoveLimitState(1);
            state.ConsumeMove();
            Assert.IsTrue(state.IsExhausted);

            state.AddMoves(2);
            Assert.IsFalse(state.IsExhausted);
            Assert.AreEqual(2, state.MovesRemaining);
        }

        [Test]
        public void TimeLimitState_Tick_ClampsAtZero_NeverNegative()
        {
            var state = new TimeLimitState(1f);
            state.Tick(5f);
            Assert.AreEqual(0f, state.SecondsRemaining);
            Assert.IsTrue(state.IsExpired);
        }

        [Test]
        public void TimeLimitState_Tick_DecrementsNormally()
        {
            var state = new TimeLimitState(10f);
            state.Tick(3f);
            Assert.AreEqual(7f, state.SecondsRemaining, 0.0001f);
            Assert.IsFalse(state.IsExpired);
        }

        [Test]
        public void TimeLimitState_AddSecondsAfterExpiry_UnExpiresIt()
        {
            var state = new TimeLimitState(1f);
            state.Tick(5f);
            Assert.IsTrue(state.IsExpired);

            state.AddSeconds(10f);
            Assert.IsFalse(state.IsExpired);
            Assert.AreEqual(10f, state.SecondsRemaining, 0.0001f);
        }
    }
}
