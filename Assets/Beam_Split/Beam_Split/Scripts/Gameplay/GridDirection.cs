using System;
using BeamSplit.Data;
using UnityEngine;

namespace BeamSplit.Gameplay
{
    /// <summary>
    /// Static 8-directional grid vector helper. All beam travel directions are one of the
    /// 8 compass Vector2Int constants below (diagonals are true 45-degree corner-to-corner
    /// steps on the square grid).
    /// </summary>
    public static class GridDirection
    {
        public static readonly Vector2Int E = new Vector2Int(1, 0);
        public static readonly Vector2Int NE = new Vector2Int(1, 1);
        public static readonly Vector2Int N = new Vector2Int(0, 1);
        public static readonly Vector2Int NW = new Vector2Int(-1, 1);
        public static readonly Vector2Int W = new Vector2Int(-1, 0);
        public static readonly Vector2Int SW = new Vector2Int(-1, -1);
        public static readonly Vector2Int S = new Vector2Int(0, -1);
        public static readonly Vector2Int SE = new Vector2Int(1, -1);

        // Fixed compass order used to index the reflection table: E, NE, N, NW, W, SW, S, SE
        private static readonly Vector2Int[] Compass =
        {
            E, NE, N, NW, W, SW, S, SE
        };

        // For each compass index, the (+45 CCW, -45 CW) reflection outputs, same order as Compass.
        private static readonly (Vector2Int ccw, Vector2Int cw)[] ReflectionTable =
        {
            (NE, SE), // E
            (N, E),   // NE
            (NW, NE), // N
            (W, N),   // NW
            (SW, NW), // W
            (S, W),   // SW
            (SE, SW), // S
            (E, S)    // SE
        };

        public static Vector2Int FromDirection(Direction direction)
        {
            switch (direction)
            {
                case Direction.Right: return E;
                case Direction.Left: return W;
                case Direction.Up: return N;
                case Direction.Down: return S;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        private static int IndexOf(Vector2Int direction)
        {
            for (int i = 0; i < Compass.Length; i++)
            {
                if (Compass[i] == direction)
                {
                    return i;
                }
            }

            throw new ArgumentException($"Direction {direction} is not one of the 8 valid compass directions.", nameof(direction));
        }

        /// <summary>
        /// Given an incoming travel direction hitting a mirror, returns the two outgoing
        /// directions: rotated +45 degrees (CCW) and -45 degrees (CW) on the 8-direction compass.
        /// </summary>
        public static (Vector2Int a, Vector2Int b) Reflect(Vector2Int incoming)
        {
            int index = IndexOf(incoming);
            var (ccw, cw) = ReflectionTable[index];
            return (ccw, cw);
        }
    }
}
