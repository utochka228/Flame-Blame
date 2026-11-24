using FlameBlame.Hose.Presentation;
using NUnit.Framework;

namespace FlameBlame.Tests.EditMode
{
    public class HoseRadiusProfileTests
    {
        private const float BaseRadius = 0.05f;
        private const float SwellFraction = 0.4f;
        private const float SquashFraction = 0.5f;
        private const float Tolerance = 1e-5f;

        [Test]
        public void Fill_SwellsWithPressureAndSquashesWherePinched()
        {
            var profile = new HoseRadiusProfile(BaseRadius, SwellFraction, SquashFraction);
            var radii = new float[3];

            profile.Fill(new[] { 0f, 0.5f, 0.5f }, new[] { false, false, true }, radii);

            Assert.AreEqual(0.05f, radii[0], Tolerance);
            Assert.AreEqual(0.06f, radii[1], Tolerance);
            Assert.AreEqual(0.03f, radii[2], Tolerance);
        }
    }
}
