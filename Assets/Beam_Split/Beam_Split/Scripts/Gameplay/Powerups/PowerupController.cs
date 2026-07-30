using BeamSplit.Data;
using BeamSplit.Gameplay.Economy;
using BeamSplit.Gameplay.Objective;
using BeamSplit.Managers;
using BeamSplit.UI;
using UnityEngine;

namespace BeamSplit.Gameplay.Powerups
{
    /// <summary>
    /// One script owning all 4 powerup actions — they share the same
    /// spend-then-apply-then-notify pipeline and references. See plan Section 5.
    /// </summary>
    public class PowerupController : MonoBehaviour
    {
        [SerializeField] private EconomyManager economyManager;
        [SerializeField] private ObjectiveController objectiveController;
        [SerializeField] private NotificationManager notificationManager;
        [SerializeField] private PlacementController placementController;
        [SerializeField] private BeamSimulator beamSimulator;
        [SerializeField] private HintHighlighter hintHighlighter;

        [Header("Powerup Costs")]
        [SerializeField] private int addTimeCost = 15;
        [SerializeField] private float addTimeSeconds = 15f;
        [SerializeField] private int addMovesCost = 15;
        [SerializeField] private int addMovesGrant = 3;
        [SerializeField] private int hintCost = 20;
        [SerializeField] private int undoCost = 10;

        private LevelData currentLevel;

        public void Configure(LevelData level)
        {
            currentLevel = level;
        }

        public void TryUseAddTime()
        {
            if (currentLevel == null || currentLevel.objectiveType != ObjectiveType.TimeLimit)
            {
                notificationManager?.ShowMessage("Only available on timed levels");
                return;
            }

            void Grant()
            {
                objectiveController.TryAddTime(addTimeSeconds);
                AudioManager.Instance?.PlayPowerup();
                notificationManager?.ShowMessage("+15s added");
            }

            if (economyManager.TrySpend(addTimeCost))
            {
                Grant();
                return;
            }

            notificationManager?.ShowMessage("Not enough coins");
        }

        public void TryUseAddMoves()
        {
            if (currentLevel == null || currentLevel.objectiveType != ObjectiveType.MoveLimit)
            {
                notificationManager?.ShowMessage("Only available on move-limited levels");
                return;
            }

            void Grant()
            {
                objectiveController.TryAddMoves(addMovesGrant);
                AudioManager.Instance?.PlayPowerup();
                notificationManager?.ShowMessage("+3 moves added");
            }

            if (economyManager.TrySpend(addMovesCost))
            {
                Grant();
                return;
            }

            notificationManager?.ShowMessage("Not enough coins");
        }

        public void TryUseHint()
        {
            void Grant()
            {
                if (currentLevel == null)
                {
                    notificationManager?.ShowMessage("No more hints - you are on the solution!");
                    return;
                }

                foreach (var step in currentLevel.solution)
                {
                    if (!IsStepSatisfied(step))
                    {
                        hintHighlighter?.Show(step.gridPosition);
                        AudioManager.Instance?.PlayPowerup();
                        notificationManager?.ShowMessage(
                            $"Hint: place {step.kind} at ({step.gridPosition.x},{step.gridPosition.y})");
                        return;
                    }
                }

                notificationManager?.ShowMessage("No more hints - you are on the solution!");
            }

            if (economyManager.TrySpend(hintCost))
            {
                Grant();
                return;
            }

            notificationManager?.ShowMessage("Not enough coins");
        }

        public void TryUseUndo()
        {
            if (!placementController.HasHistory())
            {
                notificationManager?.ShowMessage("Nothing to undo");
                return;
            }

            void Grant()
            {
                placementController.UndoLastPlacement();
                AudioManager.Instance?.PlayPowerup();
            }

            if (economyManager.TrySpend(undoCost))
            {
                Grant();
                return;
            }

            notificationManager?.ShowMessage("Not enough coins");
        }

        private bool IsStepSatisfied(SolutionStepData step)
        {
            if (beamSimulator == null || !beamSimulator.TryGetPlacement(step.gridPosition, out var component))
            {
                return false;
            }

            if (step.kind == PlacementKind.Mirror)
            {
                return component.ComponentType == PlacedComponentType.Mirror;
            }

            return component.ComponentType == PlacedComponentType.Filter
                   && component is FilterTile filterTile
                   && filterTile.FilterColor == step.filterColor;
        }
    }
}
