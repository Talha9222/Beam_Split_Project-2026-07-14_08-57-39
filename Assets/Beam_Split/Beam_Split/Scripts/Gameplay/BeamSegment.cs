using BeamSplit.Data;
using UnityEngine;

namespace BeamSplit.Gameplay
{
    public struct BeamSegment
    {
        public Vector2Int startCell;
        public Vector2Int endCell;
        public Vector3 startWorld;
        public Vector3 endWorld;
        public BeamColor color;

        public BeamSegment(Vector2Int startCell, Vector2Int endCell, Vector3 startWorld, Vector3 endWorld, BeamColor color)
        {
            this.startCell = startCell;
            this.endCell = endCell;
            this.startWorld = startWorld;
            this.endWorld = endWorld;
            this.color = color;
        }
    }
}
