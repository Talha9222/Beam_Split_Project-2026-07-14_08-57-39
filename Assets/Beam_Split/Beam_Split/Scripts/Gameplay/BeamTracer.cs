using System.Collections.Generic;
using BeamSplit.Data;
using UnityEngine;

namespace BeamSplit.Gameplay
{
    public enum CellContentType
    {
        Empty,
        Wall,
        Mirror,
        Filter,
        Receiver
    }

    /// <summary>
    /// Occupancy lookup value for a single grid cell, as seen by BeamTracer. Multiple cell
    /// content types cannot coexist on the same cell in this phase (single-occupancy grid).
    /// </summary>
    public readonly struct CellContent
    {
        public readonly CellContentType type;
        public readonly BeamColor filterColor; // only meaningful when type == Filter

        public CellContent(CellContentType type, BeamColor filterColor = BeamColor.White)
        {
            this.type = type;
            this.filterColor = filterColor;
        }

        public static readonly CellContent Empty = new CellContent(CellContentType.Empty);
        public static CellContent Wall() => new CellContent(CellContentType.Wall);
        public static CellContent Mirror() => new CellContent(CellContentType.Mirror);
        public static CellContent Filter(BeamColor color) => new CellContent(CellContentType.Filter, color);
        public static CellContent Receiver() => new CellContent(CellContentType.Receiver);
    }

    /// <summary>
    /// Pure C# (no MonoBehaviour), unit-testable beam tracing algorithm. Grid-intersection
    /// walking (not continuous raycast) across an 8-direction compass.
    /// </summary>
    public static class BeamTracer
    {
        public static (List<BeamSegment> segments, Dictionary<Vector2Int, BeamColor> receiverHits) Trace(
            int gridWidth,
            int gridHeight,
            Dictionary<Vector2Int, CellContent> occupancy,
            List<(Vector2Int pos, Vector2Int dir)> emitters)
        {
            var segments = new List<BeamSegment>();
            var receiverHits = new Dictionary<Vector2Int, BeamColor>();
            var visited = new HashSet<(Vector2Int cell, Vector2Int dir)>();

            foreach (var emitter in emitters)
            {
                TraceRay(emitter.pos, emitter.dir, ColorSet.Empty, gridWidth, gridHeight, occupancy, segments, receiverHits, visited);
            }

            return (segments, receiverHits);
        }

        private static bool InBounds(Vector2Int cell, int gridWidth, int gridHeight)
        {
            return cell.x >= 0 && cell.x < gridWidth && cell.y >= 0 && cell.y < gridHeight;
        }

        private static CellContent GetContent(Dictionary<Vector2Int, CellContent> occupancy, Vector2Int cell)
        {
            return occupancy != null && occupancy.TryGetValue(cell, out var content) ? content : CellContent.Empty;
        }

        private static void TraceRay(
            Vector2Int currentCell,
            Vector2Int direction,
            ColorSet colorSet,
            int gridWidth,
            int gridHeight,
            Dictionary<Vector2Int, CellContent> occupancy,
            List<BeamSegment> segments,
            Dictionary<Vector2Int, BeamColor> receiverHits,
            HashSet<(Vector2Int cell, Vector2Int dir)> visited)
        {
            Vector2Int segmentStart = currentCell;

            while (true)
            {
                if (!visited.Add((currentCell, direction)))
                {
                    // Cycle guard: repeat (cell, direction) state — terminate this branch.
                    EmitSegment(segments, segmentStart, currentCell, colorSet);
                    return;
                }

                Vector2Int nextCell = currentCell + direction;

                if (!InBounds(nextCell, gridWidth, gridHeight))
                {
                    // Out of bounds — terminate harmlessly at the last in-bounds cell.
                    EmitSegment(segments, segmentStart, currentCell, colorSet);
                    return;
                }

                CellContent content = GetContent(occupancy, nextCell);

                switch (content.type)
                {
                    case CellContentType.Wall:
                        EmitSegment(segments, segmentStart, nextCell, colorSet);
                        return;

                    case CellContentType.Mirror:
                    {
                        EmitSegment(segments, segmentStart, nextCell, colorSet);
                        var (dirA, dirB) = GridDirection.Reflect(direction);
                        TraceRay(nextCell, dirA, colorSet, gridWidth, gridHeight, occupancy, segments, receiverHits, visited);
                        TraceRay(nextCell, dirB, colorSet, gridWidth, gridHeight, occupancy, segments, receiverHits, visited);
                        return;
                    }

                    case CellContentType.Filter:
                    {
                        ColorSet newColorSet = ColorMixer.Mix(colorSet, content.filterColor);
                        EmitSegment(segments, segmentStart, nextCell, colorSet);
                        segmentStart = nextCell;
                        colorSet = newColorSet;
                        currentCell = nextCell;
                        continue;
                    }

                    case CellContentType.Receiver:
                    {
                        // Pass-through: record the hit but do not break the segment or stop.
                        BeamColor arrivingColor = ColorMixer.ToBeamColor(colorSet);
                        receiverHits[nextCell] = arrivingColor;
                        currentCell = nextCell;
                        continue;
                    }

                    case CellContentType.Empty:
                    default:
                        currentCell = nextCell;
                        continue;
                }
            }
        }

        private static void EmitSegment(List<BeamSegment> segments, Vector2Int startCell, Vector2Int endCell, ColorSet colorSet)
        {
            // World-space positions are filled in by the caller (BeamSimulator) which has
            // access to GridManager; here we store grid-space only and mirror into world
            // fields via GridManager-less zero vectors, resolved by BeamSimulator.
            var segment = new BeamSegment(startCell, endCell, Vector3.zero, Vector3.zero, ColorMixer.ToBeamColor(colorSet));
            segments.Add(segment);
        }
    }
}
