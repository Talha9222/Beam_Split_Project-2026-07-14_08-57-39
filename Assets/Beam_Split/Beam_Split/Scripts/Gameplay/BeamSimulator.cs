using System;
using System.Collections.Generic;
using BeamSplit.Data;
using UnityEngine;

namespace BeamSplit.Gameplay
{
    /// <summary>
    /// Central simulation authority. Holds the current placement dictionary, emitter/receiver
    /// lists, and drives BeamTracer. Raises OnBeamsUpdated (consumed by BeamRenderer) and
    /// OnAllReceiversActive (win-check).
    /// </summary>
    public class BeamSimulator : MonoBehaviour
    {
        [SerializeField] private GridManager gridManager;
        [SerializeField] private LevelData currentLevel;

        private readonly Dictionary<Vector2Int, IPlacedComponent> placements = new Dictionary<Vector2Int, IPlacedComponent>();
        private readonly List<Emitter> emitters = new List<Emitter>();
        private readonly List<Receiver> receivers = new List<Receiver>();
        private readonly HashSet<Vector2Int> wallCells = new HashSet<Vector2Int>();
        private readonly HashSet<Vector2Int> beamCrossedCells = new HashSet<Vector2Int>();

        public event Action<List<BeamSegment>> OnBeamsUpdated;
        public event Action OnAllReceiversActive;

        public LevelData CurrentLevel => currentLevel;

        public void Configure(GridManager grid, LevelData level)
        {
            gridManager = grid;
            currentLevel = level;

            wallCells.Clear();
            if (level != null)
            {
                foreach (var wall in level.walls)
                {
                    wallCells.Add(wall);
                }
            }
        }

        public void RegisterEmitter(Emitter emitter)
        {
            if (!emitters.Contains(emitter))
            {
                emitters.Add(emitter);
            }
        }

        public void RegisterReceiver(Receiver receiver)
        {
            if (!receivers.Contains(receiver))
            {
                receivers.Add(receiver);
            }
        }

        public bool TryPlace(Vector2Int cell, IPlacedComponent component)
        {
            if (placements.ContainsKey(cell))
            {
                return false;
            }

            placements[cell] = component;
            return true;
        }

        public bool RemovePlacement(Vector2Int cell)
        {
            return placements.Remove(cell);
        }

        public bool HasPlacement(Vector2Int cell)
        {
            return placements.ContainsKey(cell);
        }

        public bool TryGetPlacement(Vector2Int cell, out IPlacedComponent component)
        {
            return placements.TryGetValue(cell, out component);
        }

        public bool IsValidPlacementPoint(Vector2Int cell)
        {
            // Not a receiver cell.
            foreach (var receiver in receivers)
            {
                if (receiver.GridPosition == cell)
                {
                    return false;
                }
            }

            // Not a wall cell or already-occupied cell.
            if (wallCells.Contains(cell) || placements.ContainsKey(cell))
            {
                return false;
            }

            // Must have an active beam crossing this cell.
            return beamCrossedCells.Contains(cell);
        }

        public void Recalculate()
        {
            var occupancy = BuildOccupancyMap();
            var emitterList = new List<(Vector2Int pos, Vector2Int dir)>();
            foreach (var emitter in emitters)
            {
                emitterList.Add((emitter.GridPosition, GridDirection.FromDirection(emitter.InitialDirection)));
            }

            int width = currentLevel != null ? currentLevel.gridWidth : (gridManager != null ? gridManager.GridWidth : 0);
            int height = currentLevel != null ? currentLevel.gridHeight : (gridManager != null ? gridManager.GridHeight : 0);

            var (segments, receiverHits) = BeamTracer.Trace(width, height, occupancy, emitterList);

            // Resolve world-space positions now that we have GridManager access.
            beamCrossedCells.Clear();
            for (int i = 0; i < segments.Count; i++)
            {
                var segment = segments[i];
                if (gridManager != null)
                {
                    segment.startWorld = gridManager.GetWorldPosition(segment.startCell);
                    segment.endWorld = gridManager.GetWorldPosition(segment.endCell);
                }

                segments[i] = segment;
                MarkCrossedCells(segment.startCell, segment.endCell);
            }

            bool allActive = true;
            foreach (var receiver in receivers)
            {
                BeamColor? arriving = receiverHits.TryGetValue(receiver.GridPosition, out var color)
                    ? (BeamColor?)color
                    : null;
                receiver.ApplyBeamColor(arriving);
                if (!receiver.IsActive)
                {
                    allActive = false;
                }
            }

            OnBeamsUpdated?.Invoke(segments);

            if (allActive && receivers.Count > 0)
            {
                OnAllReceiversActive?.Invoke();
            }
        }

        private void MarkCrossedCells(Vector2Int start, Vector2Int end)
        {
            Vector2Int step = new Vector2Int(Math.Sign(end.x - start.x), Math.Sign(end.y - start.y));
            Vector2Int cell = start;
            beamCrossedCells.Add(cell);

            // Bounded walk: segments are grid-aligned or 45-degree diagonal, so this always terminates.
            int guard = 0;
            int maxSteps = (currentLevel != null ? currentLevel.gridWidth + currentLevel.gridHeight : 64) + 4;
            while (cell != end && guard < maxSteps)
            {
                cell += step;
                beamCrossedCells.Add(cell);
                guard++;
            }
        }

        private Dictionary<Vector2Int, CellContent> BuildOccupancyMap()
        {
            var occupancy = new Dictionary<Vector2Int, CellContent>();

            foreach (var wall in wallCells)
            {
                occupancy[wall] = CellContent.Wall();
            }

            foreach (var receiver in receivers)
            {
                occupancy[receiver.GridPosition] = CellContent.Receiver();
            }

            foreach (var kvp in placements)
            {
                var cell = kvp.Key;
                var component = kvp.Value;
                if (component.ComponentType == PlacedComponentType.Mirror)
                {
                    occupancy[cell] = CellContent.Mirror();
                }
                else if (component is FilterTile filterTile)
                {
                    occupancy[cell] = CellContent.Filter(filterTile.FilterColor);
                }
            }

            return occupancy;
        }
    }
}
