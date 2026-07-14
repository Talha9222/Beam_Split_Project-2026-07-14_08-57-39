using UnityEngine;

namespace BeamSplit.Gameplay.Objective
{
    /// <summary>
    /// Tiny seam so TimeLimitState's countdown arithmetic is unit-testable without relying
    /// on real Time.deltaTime in EditMode.
    /// </summary>
    public interface IObjectiveClock
    {
        float GetDeltaTime();
    }

    /// <summary>
    /// Production implementation — wraps UnityEngine.Time.deltaTime.
    /// </summary>
    public class UnityDeltaTimeClock : IObjectiveClock
    {
        public float GetDeltaTime()
        {
            return Time.deltaTime;
        }
    }
}
