using UnityEngine;

namespace BeamSplit.Gameplay
{
    /// <summary>
    /// No sim logic; SetGridPosition called by PlacementController on spawn. Placement
    /// uses grid math, not physics raycasts — no collider required.
    /// </summary>
    public class MirrorTile : MonoBehaviour, IPlacedComponent
    {
        [SerializeField] private Vector2Int gridPosition;

        public Vector2Int GridPosition => gridPosition;
        public PlacedComponentType ComponentType => PlacedComponentType.Mirror;

        public void SetGridPosition(Vector2Int position)
        {
            gridPosition = position;
        }
    }
}
