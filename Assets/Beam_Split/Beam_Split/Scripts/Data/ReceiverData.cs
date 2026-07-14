using System;
using System.Collections.Generic;
using UnityEngine;

namespace BeamSplit.Data
{
    [Serializable]
    public class ReceiverData
    {
        public Vector2Int gridPosition;
        public BeamColor requiredColor;

        // Unused this phase — kept for forward compatibility with a later moving-receiver phase.
        public bool isMoving;
        public List<Vector2Int> patrolWaypoints = new List<Vector2Int>();
    }
}
