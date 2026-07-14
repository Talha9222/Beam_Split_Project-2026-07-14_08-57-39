namespace BeamSplit.Data
{
    /// <summary>
    /// Cardinal-only direction used for emitter initial facing. Diagonal travel
    /// (post-mirror) is represented separately by GridDirection's 8-way Vector2Int compass.
    /// </summary>
    public enum Direction
    {
        Right,
        Left,
        Up,
        Down
    }
}
