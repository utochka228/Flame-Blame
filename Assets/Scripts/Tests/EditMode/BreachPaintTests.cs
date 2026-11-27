using System.Collections.Generic;
using FlameBlame.Hose.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace FlameBlame.Tests.EditMode
{
    public class BreachPaintTests
    {
        private static readonly Color Base = Color.white;
        private static readonly Color Burned = Color.black;
        private static readonly Color Taped = Color.blue;

        private BreachPaint _paint;

        [SetUp]
        public void SetUp()
        {
            _paint = new BreachPaint(Burned, Taped, 1f, 1f);
        }

        private Color[] Paint(int texelCount, int particleCount, params Breach[] breaches)
        {
            var texels = new Color[texelCount];
            _paint.Fill(texels, new List<Breach>(breaches), particleCount, Base);
            return texels;
        }

        private static void AssertColor(Color actual, Color expected)
        {
            Assert.That(actual.r, Is.EqualTo(expected.r).Within(1e-4f));
            Assert.That(actual.g, Is.EqualTo(expected.g).Within(1e-4f));
            Assert.That(actual.b, Is.EqualTo(expected.b).Within(1e-4f));
        }

        [Test]
        public void NoBreaches_PaintsTheBaseColorEverywhere()
        {
            var texels = Paint(11, 11);

            foreach (var texel in texels)
                AssertColor(texel, Base);
        }

        [Test]
        public void OpenLeak_PaintsItsStretchBurned_AndNothingElse()
        {
            var texels = Paint(41, 11, new Breach(3, 5, 40f, false));

            AssertColor(texels[16], Burned);
            AssertColor(texels[20], Burned);
            AssertColor(texels[22], Burned);
            AssertColor(texels[0], Base);
            AssertColor(texels[40], Base);
        }

        [Test]
        public void TapedLeak_PaintsTapeAroundItsCentre()
        {
            var texels = Paint(41, 11, new Breach(2, 6, 8f, true));

            AssertColor(texels[16], Taped);
            AssertColor(texels[0], Base);
        }

        [Test]
        public void TapedLeak_RestoresTheBaseColorOverTheRestOfItsStretch()
        {
            var texels = Paint(41, 11, new Breach(2, 6, 8f, true));

            AssertColor(texels[8], Base);
            AssertColor(texels[24], Base);
        }

        [Test]
        public void Edges_BlendSoftlyOverTheFeatherDistance()
        {
            var texels = Paint(41, 11, new Breach(4, 4, 40f, false));

            AssertColor(texels[14], Burned);
            AssertColor(texels[13], Color.Lerp(Base, Burned, 0.75f));
            AssertColor(texels[10], Base);
            AssertColor(texels[19], Color.Lerp(Base, Burned, 0.75f));
        }

        [Test]
        public void LeakAtTheStartOfTheHose_PaintsTheFirstTexel()
        {
            var texels = Paint(41, 11, new Breach(0, 1, 40f, false));

            AssertColor(texels[0], Burned);
        }

        [Test]
        public void TwoLeaks_PaintBothStretches()
        {
            var texels = Paint(41, 11, new Breach(1, 1, 40f, false), new Breach(8, 9, 8f, true));

            AssertColor(texels[4], Burned);
            AssertColor(texels[34], Taped);
            AssertColor(texels[20], Base);
        }
    }
}
