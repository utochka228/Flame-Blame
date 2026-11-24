using System.Collections.Generic;
using FlameBlame.Hose.Simulation;
using NUnit.Framework;

namespace FlameBlame.Tests.EditMode
{
    public class PinchRunsTests
    {
        private const float Tolerance = 1e-4f;

        private readonly List<float> _positions = new List<float>();

        [Test]
        public void CollectPositions_ReportsBothEdgesOfEachRunAlongTheHose()
        {
            var pinched = new[] { false, true, true, false, true };

            PinchRuns.CollectPositions(pinched, _positions);

            Assert.AreEqual(3, _positions.Count);
            Assert.AreEqual(0.25f, _positions[0], Tolerance);
            Assert.AreEqual(0.5f, _positions[1], Tolerance);
            Assert.AreEqual(1f, _positions[2], Tolerance);
        }

        [Test]
        public void CollectPositions_WithNothingPinched_IsEmpty()
        {
            _positions.Add(0.5f);

            PinchRuns.CollectPositions(new[] { false, false, false }, _positions);

            Assert.AreEqual(0, _positions.Count);
        }
    }
}
