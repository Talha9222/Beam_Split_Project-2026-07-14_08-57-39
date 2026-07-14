using System.Collections.Generic;
using UnityEngine;

namespace BeamSplit.Data
{
    [CreateAssetMenu(fileName = "LevelData", menuName = "Beam Split/Level Data")]
    public class LevelData : ScriptableObject
    {
        public int levelNumber;
        public int gridWidth = 8;
        public int gridHeight = 8;
        public int parMirrorCount;

        public List<EmitterData> emitters = new List<EmitterData>();
        public List<ReceiverData> receivers = new List<ReceiverData>();
        public List<Vector2Int> walls = new List<Vector2Int>();

        public List<BeamColor> availableFilterColors = new List<BeamColor>();
        public int mirrorBudget;

        // --- Objective ---
        public ObjectiveType objectiveType = ObjectiveType.MoveLimit;
        public int moveLimit = 10;            // used when objectiveType == MoveLimit
        public float timeLimitSeconds = 90f;  // used when objectiveType == TimeLimit
        public string objectiveDescription = ""; // author-facing text shown on Objectives panel

        // --- Economy ---
        public int coinReward = 50;

        // --- Hint solution ---
        public List<SolutionStepData> solution = new List<SolutionStepData>();

        // --- Tutorial (per-level info panel content override) ---
        public string tutorialText = ""; // empty = use the shared default tutorial body
    }
}
