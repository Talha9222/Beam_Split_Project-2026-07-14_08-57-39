namespace BeamSplit.Data
{
    /// <summary>
    /// Discrete 7-value color lattice for beams. Not a continuous RGB representation —
    /// see ColorMixer for the flag-set based mixing rules that produce these values.
    /// </summary>
    public enum BeamColor
    {
        White,
        Red,
        Blue,
        Yellow,
        Magenta,
        Green,
        Orange
    }
}
