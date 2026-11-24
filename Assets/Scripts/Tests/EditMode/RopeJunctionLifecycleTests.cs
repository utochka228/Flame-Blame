using FlameBlame.Hose.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace FlameBlame.Tests.EditMode
{
    public class RopeJunctionLifecycleTests
    {
        private const float FixedStep = 1f / 60f;
        private const int Substeps = 8;
        private const int ParticleCount = 10;
        private const float Spacing = 0.2f;
        private const float BreakDistance = 5f;

        private RopeWorld _world;
        private XpbdRope _first;
        private XpbdRope _second;
        private int _firstId;
        private int _secondId;

        private static XpbdRope CreateRope(float anchorX, bool pinned)
        {
            var settings = new RopeSettings { ParticleCount = ParticleCount, Spacing = Spacing };
            return new XpbdRope(settings, new Vector3(anchorX, 10f, 0f), Vector3.right, new PlaneRopeCollider(-1000f)) { IsPinned = pinned };
        }

        [SetUp]
        public void SetUp()
        {
            _world = new RopeWorld(Substeps);
            _first = CreateRope(0f, true);
            _second = CreateRope((ParticleCount - 1) * Spacing, false);
            _firstId = _world.AddRope(_first);
            _secondId = _world.AddRope(_second);
        }

        private int Join()
        {
            var junction = _world.AddJunction(BreakDistance);
            _world.Attach(junction, new RopeEnd(_firstId, RopeEndKind.Tail));
            _world.Attach(junction, new RopeEnd(_secondId, RopeEndKind.Start));
            return junction;
        }

        private void Simulate(float seconds)
        {
            var stepCount = Mathf.RoundToInt(seconds / FixedStep);
            for (var index = 0; index < stepCount; index++)
                _world.Step(FixedStep);
        }

        private float Gap() => Vector3.Distance(_first.Positions[ParticleCount - 1], _second.Positions[0]);

        [Test]
        public void Break_LetsTheEndsSeparate()
        {
            var junction = Join();
            Simulate(1f);
            Assert.Less(Gap(), 0.05f);

            _world.Break(junction);
            Simulate(2f);

            Assert.IsFalse(_world.IsIntact(junction));
            Assert.Greater(Gap(), 0.3f);
        }

        [Test]
        public void RemoveJunction_FreesTheSlotForReuse()
        {
            var first = Join();
            _world.RemoveJunction(first);

            var second = Join();

            Assert.AreEqual(first, second);
            Assert.IsTrue(_world.IsIntact(second));
        }

        [Test]
        public void AddJunction_DoesNotReuseASlotThatOnlyBroke()
        {
            var first = Join();
            _world.Break(first);

            var second = _world.AddJunction(BreakDistance);

            Assert.AreNotEqual(first, second);
        }
    }
}
