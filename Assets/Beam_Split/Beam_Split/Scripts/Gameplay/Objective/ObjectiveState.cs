namespace BeamSplit.Gameplay.Objective
{
    /// <summary>
    /// Pure C# arithmetic for the MoveLimit objective kind — no Unity dependency beyond int.
    /// </summary>
    public class MoveLimitState
    {
        public int MovesRemaining { get; private set; }

        public bool IsExhausted => MovesRemaining <= 0;

        public MoveLimitState(int startingMoves)
        {
            MovesRemaining = startingMoves < 0 ? 0 : startingMoves;
        }

        /// <summary>
        /// Decrements movesRemaining (clamped at 0) and returns the new value.
        /// </summary>
        public int ConsumeMove()
        {
            if (MovesRemaining > 0)
            {
                MovesRemaining--;
            }

            return MovesRemaining;
        }

        public void AddMoves(int n)
        {
            if (n <= 0)
            {
                return;
            }

            MovesRemaining += n;
        }
    }

    /// <summary>
    /// Pure C# arithmetic for the TimeLimit objective kind — no Unity dependency beyond float.
    /// </summary>
    public class TimeLimitState
    {
        public float SecondsRemaining { get; private set; }

        public bool IsExpired => SecondsRemaining <= 0f;

        public TimeLimitState(float startingSeconds)
        {
            SecondsRemaining = startingSeconds < 0f ? 0f : startingSeconds;
        }

        /// <summary>
        /// Decrements secondsRemaining by deltaTime, clamped at 0 (never negative).
        /// </summary>
        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return;
            }

            SecondsRemaining -= deltaTime;
            if (SecondsRemaining < 0f)
            {
                SecondsRemaining = 0f;
            }
        }

        public void AddSeconds(float n)
        {
            if (n <= 0f)
            {
                return;
            }

            SecondsRemaining += n;
        }
    }
}
