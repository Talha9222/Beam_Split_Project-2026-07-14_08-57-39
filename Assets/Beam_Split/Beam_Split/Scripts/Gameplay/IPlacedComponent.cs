using UnityEngine;

namespace BeamSplit.Gameplay
{
    public enum PlacedComponentType
    {
        Mirror,
        Filter
    }

    /// <summary>
    /// Common interface for player-placed grid objects (mirrors, filters) so BeamSimulator
    /// and GridManager can treat them uniformly for occupancy checks.
    /// </summary>
    public interface IPlacedComponent
    {
        Vector2Int GridPosition { get; }
        PlacedComponentType ComponentType { get; }
    }
}
