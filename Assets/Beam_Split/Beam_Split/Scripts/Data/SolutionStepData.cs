using UnityEngine;

namespace BeamSplit.Data
{
    [System.Serializable]
    public class SolutionStepData
    {
        public Vector2Int gridPosition;
        public PlacementKind kind;       // mirrors PlacementController.PlacementMode's "kind" axis
        public BeamColor filterColor;    // only meaningful when kind == Filter
    }

    public enum PlacementKind
    {
        Mirror,
        Filter
    }
}
