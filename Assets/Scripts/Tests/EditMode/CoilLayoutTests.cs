using FlameBlame.Hose.Simulation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools.Utils;

namespace FlameBlame.Tests.EditMode
{
    public class CoilLayoutTests
    {
        private const int ParticleCount = 61;
        private const float Spacing = 0.2f;

        private static readonly CoilShape Shape = new CoilShape(0.25f, 0.23f, Spacing);
        private static readonly Vector3 Center = new Vector3(3f, 0.1f, -4f);

        private Vector3[] _positions;

        [SetUp]
        public void SetUp()
        {
            _positions = new Vector3[ParticleCount];
            CoilLayout.Fill(_positions, Center, Vector3.forward, Shape);
        }

        [Test]
        public void Fill_NeighboursSitAtRopeSpacing()
        {
            for (var index = 1; index < ParticleCount; index++)
                Assert.That(Vector3.Distance(_positions[index - 1], _positions[index]), Is.EqualTo(Spacing).Within(0.01f));
        }

        [Test]
        public void Fill_LiesFlatAtCenterHeight()
        {
            foreach (var position in _positions)
                Assert.That(position.y, Is.EqualTo(Center.y).Within(1e-5f));
        }

        [Test]
        public void Fill_StartsInsideAlongHeadingAndGrowsOutward()
        {
            Assert.That(_positions[0], Is.EqualTo(Center + Vector3.forward * Shape.InnerRadius).Using(Vector3EqualityComparer.Instance));
            Assert.That(Flat(_positions[ParticleCount - 1]), Is.GreaterThan(Flat(_positions[ParticleCount / 2])));
            Assert.That(Flat(_positions[ParticleCount / 2]), Is.GreaterThan(Flat(_positions[0])));
        }

        [Test]
        public void OuterRadius_MatchesTheLastParticle()
        {
            Assert.That(Flat(_positions[ParticleCount - 1]), Is.EqualTo(CoilLayout.OuterRadius(ParticleCount, Shape)).Within(0.05f));
        }

        private static float Flat(Vector3 position)
        {
            var offset = position - Center;
            return new Vector2(offset.x, offset.z).magnitude;
        }
    }
}
