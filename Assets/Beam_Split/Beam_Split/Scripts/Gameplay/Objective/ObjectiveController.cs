using System;
using BeamSplit.Data;
using UnityEngine;

namespace BeamSplit.Gameplay.Objective
{
    /// <summary>
    /// Thin MonoBehaviour wrapper around MoveLimitState/TimeLimitState. Owns exactly one of
    /// the two, constructed from LevelData.objectiveType at Configure(). Does not start
    /// ticking until BeginRunning() (called by LevelLoader once panels close).
    /// </summary>
    public class ObjectiveController : MonoBehaviour
    {
        private IObjectiveClock clock = new UnityDeltaTimeClock();

        private ObjectiveType objectiveType;
        private MoveLimitState moveLimitState;
        private TimeLimitState timeLimitState;

        private bool isRunning;

        public event Action OnObjectiveFailed;
        public event Action<int> OnMovesChanged;
        public event Action<float> OnTimeChanged;

        public bool IsRunning => isRunning;
        public ObjectiveType ObjectiveType => objectiveType;

        /// <summary>Test-only seam; production code uses the default UnityDeltaTimeClock.</summary>
        public void SetClock(IObjectiveClock customClock)
        {
            clock = customClock ?? new UnityDeltaTimeClock();
        }

        public void Configure(LevelData level)
        {
            isRunning = false;
            objectiveType = level.objectiveType;

            if (objectiveType == ObjectiveType.MoveLimit)
            {
                moveLimitState = new MoveLimitState(level.moveLimit);
                timeLimitState = null;
                OnMovesChanged?.Invoke(moveLimitState.MovesRemaining);
            }
            else
            {
                timeLimitState = new TimeLimitState(level.timeLimitSeconds);
                moveLimitState = null;
                OnTimeChanged?.Invoke(timeLimitState.SecondsRemaining);
            }
        }

        public void BeginRunning()
        {
            isRunning = true;
        }

        public void StopRunning()
        {
            isRunning = false;
        }

        /// <summary>Suspends Tick() without resetting secondsRemaining (used by PauseMenu).</summary>
        public void PauseRunning()
        {
            isRunning = false;
        }

        public void ResumeRunning()
        {
            isRunning = true;
        }

        private void Update()
        {
            if (!isRunning || objectiveType != ObjectiveType.TimeLimit || timeLimitState == null)
            {
                return;
            }

            timeLimitState.Tick(clock.GetDeltaTime());
            OnTimeChanged?.Invoke(timeLimitState.SecondsRemaining);

            if (timeLimitState.IsExpired)
            {
                isRunning = false;
                OnObjectiveFailed?.Invoke();
            }
        }

        /// <summary>
        /// Called by PlacementController after a successful placement, AFTER Recalculate()
        /// has already had the chance to synchronously fire a win. No-ops on TimeLimit levels.
        /// </summary>
        public void NotifyPlacementMade()
        {
            if (objectiveType != ObjectiveType.MoveLimit || moveLimitState == null)
            {
                return;
            }

            int remaining = moveLimitState.ConsumeMove();
            OnMovesChanged?.Invoke(remaining);

            if (moveLimitState.IsExhausted)
            {
                isRunning = false;
                OnObjectiveFailed?.Invoke();
            }
        }

        /// <summary>
        /// Called by PlacementController.UndoLastPlacement(). No-ops on TimeLimit levels
        /// (deliberately asymmetric with the move-consuming NotifyPlacementMade call).
        /// </summary>
        public void NotifyPlacementRemoved()
        {
            if (objectiveType != ObjectiveType.MoveLimit || moveLimitState == null)
            {
                return;
            }

            moveLimitState.AddMoves(1);
            OnMovesChanged?.Invoke(moveLimitState.MovesRemaining);
        }

        /// <summary>Returns false if called against the wrong objective type.</summary>
        public bool TryAddTime(float seconds)
        {
            if (objectiveType != ObjectiveType.TimeLimit || timeLimitState == null)
            {
                return false;
            }

            timeLimitState.AddSeconds(seconds);
            OnTimeChanged?.Invoke(timeLimitState.SecondsRemaining);
            return true;
        }

        /// <summary>Returns false if called against the wrong objective type.</summary>
        public bool TryAddMoves(int moves)
        {
            if (objectiveType != ObjectiveType.MoveLimit || moveLimitState == null)
            {
                return false;
            }

            moveLimitState.AddMoves(moves);
            OnMovesChanged?.Invoke(moveLimitState.MovesRemaining);
            return true;
        }

        public int MovesRemaining => moveLimitState?.MovesRemaining ?? 0;
        public float SecondsRemaining => timeLimitState?.SecondsRemaining ?? 0f;
    }
}
