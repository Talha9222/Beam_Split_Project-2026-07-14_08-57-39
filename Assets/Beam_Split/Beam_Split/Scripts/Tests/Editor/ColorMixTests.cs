using BeamSplit.Data;
using BeamSplit.Gameplay;
using NUnit.Framework;

namespace BeamSplit.Tests
{
    public class ColorMixTests
    {
        [Test]
        public void Empty_IsWhite()
        {
            Assert.AreEqual(BeamColor.White, ColorMixer.ToBeamColor(ColorSet.Empty));
        }

        [Test]
        public void Red_MixesToRed()
        {
            var set = ColorMixer.Mix(ColorSet.Empty, BeamColor.Red);
            Assert.AreEqual(BeamColor.Red, ColorMixer.ToBeamColor(set));
        }

        [Test]
        public void Blue_MixesToBlue()
        {
            var set = ColorMixer.Mix(ColorSet.Empty, BeamColor.Blue);
            Assert.AreEqual(BeamColor.Blue, ColorMixer.ToBeamColor(set));
        }

        [Test]
        public void Yellow_MixesToYellow()
        {
            var set = ColorMixer.Mix(ColorSet.Empty, BeamColor.Yellow);
            Assert.AreEqual(BeamColor.Yellow, ColorMixer.ToBeamColor(set));
        }

        [Test]
        public void RedThenBlue_MixesToMagenta()
        {
            var set = ColorMixer.Mix(ColorSet.Empty, BeamColor.Red);
            set = ColorMixer.Mix(set, BeamColor.Blue);
            Assert.AreEqual(BeamColor.Magenta, ColorMixer.ToBeamColor(set));
        }

        [Test]
        public void BlueThenYellow_MixesToGreen()
        {
            var set = ColorMixer.Mix(ColorSet.Empty, BeamColor.Blue);
            set = ColorMixer.Mix(set, BeamColor.Yellow);
            Assert.AreEqual(BeamColor.Green, ColorMixer.ToBeamColor(set));
        }

        [Test]
        public void RedThenYellow_MixesToOrange()
        {
            var set = ColorMixer.Mix(ColorSet.Empty, BeamColor.Red);
            set = ColorMixer.Mix(set, BeamColor.Yellow);
            Assert.AreEqual(BeamColor.Orange, ColorMixer.ToBeamColor(set));
        }

        [Test]
        public void RepeatColor_IsNoOp()
        {
            var set = ColorMixer.Mix(ColorSet.Empty, BeamColor.Red);
            var set2 = ColorMixer.Mix(set, BeamColor.Red);
            Assert.AreEqual(1, set2.count);
            Assert.AreEqual(BeamColor.Red, ColorMixer.ToBeamColor(set2));
        }

        [Test]
        public void ThirdDistinctColor_OnceCapped_IsNoOp()
        {
            var set = ColorMixer.Mix(ColorSet.Empty, BeamColor.Red);
            set = ColorMixer.Mix(set, BeamColor.Blue);
            var set2 = ColorMixer.Mix(set, BeamColor.Yellow);

            Assert.AreEqual(2, set2.count);
            Assert.AreEqual(BeamColor.Magenta, ColorMixer.ToBeamColor(set2));
        }
    }
}
