using System.Collections.Generic;
using BeamSplit.Data;
using UnityEngine;

namespace BeamSplit.Gameplay.Placement
{
    public struct PlacementRecord
    {
        public Vector2Int cell;
        public PlacementKind kind;
        public BeamColor filterColor;
    }

    /// <summary>
    /// Pure C# LIFO placement history — no Unity MonoBehaviour, unit-testable. Owned by
    /// PlacementController: pushed on every successful PlaceCurrentMode, popped by
    /// UndoLastPlacement. TryErase deliberately does not push or pop this stack.
    /// </summary>
    public class PlacementHistory
    {
        private readonly Stack<PlacementRecord> stack = new Stack<PlacementRecord>();

        public int Count => stack.Count;

        public void Push(PlacementRecord record)
        {
            stack.Push(record);
        }

        public bool TryPop(out PlacementRecord record)
        {
            if (stack.Count == 0)
            {
                record = default;
                return false;
            }

            record = stack.Pop();
            return true;
        }
    }
}
