using UnityEngine;

namespace BeamSplit.Gameplay
{
    /// <summary>
    /// Simple static-Instance singleton MonoBehaviour. Holds grid dimensions/cell size and
    /// world<->grid conversion helpers. Bounds are set from LevelData on level load.
    /// </summary>
    public class GridManager : MonoBehaviour
    {
        public static GridManager Instance { get; private set; }

        [SerializeField] private float cellSize = 1f;
        [SerializeField] private Vector3 origin = Vector3.zero;

        public float CellSize => cellSize;
        public Vector3 Origin => origin;
        public int GridWidth { get; private set; }
        public int GridHeight { get; private set; }

        [SerializeField] private BeamSimulator beamSimulator;

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void Configure(int gridWidth, int gridHeight)
        {
            GridWidth = gridWidth;
            GridHeight = gridHeight;
        }

        public Vector3 GetWorldPosition(Vector2Int gridPosition)
        {
            return origin + new Vector3(gridPosition.x * cellSize, gridPosition.y * cellSize, 0f);
        }

        public Vector2Int? GetGridPosition(Vector3 worldPosition)
        {
            Vector3 local = worldPosition - origin;
            int x = Mathf.RoundToInt(local.x / cellSize);
            int y = Mathf.RoundToInt(local.y / cellSize);
            var cell = new Vector2Int(x, y);
            return IsInBounds(cell) ? cell : (Vector2Int?)null;
        }

        public bool IsInBounds(Vector2Int cell)
        {
            return cell.x >= 0 && cell.x < GridWidth && cell.y >= 0 && cell.y < GridHeight;
        }

        /// <summary>
        /// Delegates to BeamSimulator for "has active beam crossing" + "not a receiver cell".
        /// </summary>
        public bool IsValidPlacementPoint(Vector2Int cell)
        {
            if (!IsInBounds(cell))
            {
                return false;
            }

            if (beamSimulator == null)
            {
                return false;
            }

            return beamSimulator.IsValidPlacementPoint(cell);
        }
    }
}
