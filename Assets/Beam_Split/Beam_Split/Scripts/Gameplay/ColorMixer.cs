using BeamSplit.Data;

namespace BeamSplit.Gameplay
{
    /// <summary>
    /// Ordered set of distinct base filter colors (Red/Blue/Yellow) hit so far along a beam
    /// path, capped at 2 members. This is the source of truth for beam color during tracing —
    /// NOT raw RGB float addition — since the spec defines a discrete 7-value lattice.
    /// Immutable value type; Mix returns a new instance.
    /// </summary>
    public readonly struct ColorSet
    {
        public readonly bool hasRed;
        public readonly bool hasBlue;
        public readonly bool hasYellow;
        public readonly int count;

        private ColorSet(bool hasRed, bool hasBlue, bool hasYellow, int count)
        {
            this.hasRed = hasRed;
            this.hasBlue = hasBlue;
            this.hasYellow = hasYellow;
            this.count = count;
        }

        public static readonly ColorSet Empty = new ColorSet(false, false, false, 0);

        public ColorSet WithColor(BeamColor filterColor)
        {
            bool r = hasRed;
            bool b = hasBlue;
            bool y = hasYellow;

            bool alreadyPresent =
                (filterColor == BeamColor.Red && r) ||
                (filterColor == BeamColor.Blue && b) ||
                (filterColor == BeamColor.Yellow && y);

            if (alreadyPresent)
            {
                // Repeat color is a no-op.
                return this;
            }

            if (count >= 2)
            {
                // Cap: a third distinct filter once the set has 2 members is a no-op.
                return this;
            }

            switch (filterColor)
            {
                case BeamColor.Red: r = true; break;
                case BeamColor.Blue: b = true; break;
                case BeamColor.Yellow: y = true; break;
                default:
                    // Non-base filter colors are not valid filter inputs; no-op.
                    return this;
            }

            return new ColorSet(r, b, y, count + 1);
        }
    }

    public static class ColorMixer
    {
        /// <summary>
        /// Pure function: mixes a filter color into the current color set.
        /// </summary>
        public static ColorSet Mix(ColorSet current, BeamColor filterColor)
        {
            return current.WithColor(filterColor);
        }

        public static BeamColor ToBeamColor(ColorSet set)
        {
            if (set.hasRed && set.hasBlue) return BeamColor.Magenta;
            if (set.hasBlue && set.hasYellow) return BeamColor.Green;
            if (set.hasRed && set.hasYellow) return BeamColor.Orange;
            if (set.hasRed) return BeamColor.Red;
            if (set.hasBlue) return BeamColor.Blue;
            if (set.hasYellow) return BeamColor.Yellow;
            return BeamColor.White;
        }
    }
}
