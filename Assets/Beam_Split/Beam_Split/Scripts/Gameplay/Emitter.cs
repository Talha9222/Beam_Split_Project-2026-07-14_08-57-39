using BeamSplit.Data;
using UnityEngine;

namespace BeamSplit.Gameplay
{
    /// <summary>
    /// Pure data component, list-friendly (multi-emitter structurally supported).
    /// </summary>
    public class Emitter : MonoBehaviour
    {
        [SerializeField] private Vector2Int gridPosition;
        [SerializeField] private Direction initialDirection;

        public Vector2Int GridPosition => gridPosition;
        public Direction InitialDirection => initialDirection;

        public void Configure(Vector2Int position, Direction direction)
        {
            gridPosition = position;
            initialDirection = direction;
        }
    }
}
