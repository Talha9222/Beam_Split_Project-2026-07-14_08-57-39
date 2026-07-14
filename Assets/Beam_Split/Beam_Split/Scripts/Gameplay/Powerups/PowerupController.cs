using BeamSplit.Data;
using BeamSplit.Gameplay.Economy;
using BeamSplit.Gameplay.Objective;
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

            if (!economyManager.TrySpend(PowerupCosts.AddTimeCost))
            {
                notificationManager?.ShowMessage("Not enough coins");
                return;
            }

            objectiveController.TryAddTime(PowerupCosts.AddTimeSeconds);
            notificationManager?.ShowMessage("+15s added");
        }

        public void TryUseAddMoves()
        {
            if (currentLevel == null || currentLevel.objectiveType != ObjectiveType.MoveLimit)
            {
                notificationManager?.ShowMessage("Only available on move-limited levels");
                return;
            }

            if (!economyManager.TrySpend(PowerupCosts.AddMovesCost))
            {
                notificationManager?.ShowMessage("Not enough coins");
                return;
            }

            objectiveController.TryAddMoves(PowerupCosts.AddMovesGrant);
            notificationManager?.ShowMessage("+3 moves added");
        }

        public void TryUseHint()
        {
            if (!economyManager.TrySpend(PowerupCosts.HintCost))
            {
                notificationManager?.ShowMessage("Not enough coins");
                return;
            }

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
                    notificationManager?.ShowMessage(
                        $"Hint: place {step.kind} at ({step.gridPosition.x},{step.gridPosition.y})");
                    return;
                }
            }

            notificationManager?.ShowMessage("No more hints - you are on the solution!");
        }

        public void TryUseUndo()
        {
            if (!placementController.HasHistory())
            {
                notificationManager?.ShowMessage("Nothing to undo");
                return;
            }

            if (!economyManager.TrySpend(PowerupCosts.UndoCost))
            {
                notificationManager?.ShowMessage("Not enough coins");
                return;
            }

            placementController.UndoLastPlacement();
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
