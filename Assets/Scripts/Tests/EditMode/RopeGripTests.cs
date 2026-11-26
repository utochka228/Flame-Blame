using FlameBlame.Hose.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace FlameBlame.Tests.EditMode
{
    public class RopeGripTests
    {
        private const float FixedStep = 1f / 60f;
        private const int Substeps = 8;
        private const int ParticleCount = 10;
        private const float Spacing = 0.2f;
        private const float StiffCompliance = 1e-4f;
        private const float SoftCompliance = 1e-6f;

        private RopeWorld _world;
        private XpbdRope _rope;

        [SetUp]
        public void SetUp()
        {
            var settings = new RopeSettings { ParticleCount = ParticleCount, Spacing = Spacing };
            _rope = new XpbdRope(settings, new Vector3(0f, 10f, 0f), Vector3.right, new PlaneRopeCollider(-1000f)) { IsPinned = false };
            _world = new RopeWorld(Substeps);
            _world.AddRope(_rope);
        }

        private void Simulate(float seconds)
        {
            var stepCount = Mathf.RoundToInt(seconds / FixedStep);
            for (var index = 0; index < stepCount; index++)
                _world.Step(FixedStep);
        }

        [Test]
        public void ActiveHandGripCount_CountsOnlyActiveHandGrips()
        {
            _world.AddGrip(new RopeParticle(0, 0), StiffCompliance);
            _world.AddGrip(new RopeParticle(0, 4), StiffCompliance, true);
            var released = _world.AddGrip(new RopeParticle(0, 6), StiffCompliance, true);
            _world.Release(released);

            Assert.AreEqual(1, _world.ActiveHandGripCount(0));
        }

        [Test]
        public void Grip_HoldsParticleAtTarget()
        {
            var grip = _world.AddGrip(new RopeParticle(0, 4), StiffCompliance);
            var target = new Vector3(0.8f, 12f, 1f);
            _world.SetGripTarget(grip, target);

            Simulate(2f);

            Assert.Less(Vector3.Distance(_rope.Positions[4], target), 0.1f);
        }

        [Test]
        public void Grip_ReportsTension_TowardTheRope()
        {
            var grip = _world.AddGrip(new RopeParticle(0, 4), SoftCompliance);
            _world.SetGripTarget(grip, new Vector3(0.8f, 12f, 0f));

            Simulate(2f);

            Assert.Less(_world.GripTension(grip).y, 0f);
        }

        [Test]
        public void Release_LetsTheRopeFall_AndClearsTension()
        {
            var grip = _world.AddGrip(new RopeParticle(0, 4), StiffCompliance);
            _world.SetGripTarget(grip, new Vector3(0.8f, 10f, 0f));
            Simulate(1f);

            _world.Release(grip);
            Simulate(1f);

            Assert.Less(_rope.Positions[4].y, 8f);
            Assert.AreEqual(Vector3.zero, _world.GripTension(grip));
        }

        [Test]
        public void AddGrip_AfterRelease_ReusesTheSlot()
        {
            var first = _world.AddGrip(new RopeParticle(0, 4), StiffCompliance);
            var second = _world.AddGrip(new RopeParticle(0, 6), StiffCompliance);

            _world.Release(first);
            var third = _world.AddGrip(new RopeParticle(0, 8), StiffCompliance);

            Assert.AreEqual(first, third);
            Assert.AreNotEqual(second, third);
            Assert.AreEqual(8, _world.GripParticle(third).Index);
        }

        [Test]
        public void TwoGrips_OrientTheSegmentBetweenThem()
        {
            var tip = _world.AddGrip(new RopeParticle(0, ParticleCount - 1), StiffCompliance);
            var inner = _world.AddGrip(new RopeParticle(0, ParticleCount - 2), StiffCompliance);
            var direction = new Vector3(0f, 0.6f, 0.8f);
            var tipTarget = new Vector3(0f, 12f, 1f);
            _world.SetGripTarget(tip, tipTarget);
            _world.SetGripTarget(inner, tipTarget - direction * Spacing);

            Simulate(2f);

            var aimed = (_rope.Positions[ParticleCount - 1] - _rope.Positions[ParticleCount - 2]).normalized;
            Assert.Greater(Vector3.Dot(aimed, direction), 0.99f);
        }

        [Test]
        public void AddImpulse_MovesTheParticle()
        {
            var start = _rope.Positions[ParticleCount - 1];

            _world.AddImpulse(new RopeParticle(0, ParticleCount - 1), new Vector3(0f, 0f, 10f));
            Simulate(0.2f);

            Assert.Greater(_rope.Positions[ParticleCount - 1].z - start.z, 0.5f);
        }

        [Test]
        public void FindAlongRay_PicksTheParticleNearestTheAim()
        {
            var found = _world.FindAlongRay(new Vector3(0.6f, 10f, -3f), Vector3.forward, 5f, 0.3f);

            Assert.AreEqual(new RopeParticle(0, 3).Index, found.Index);
        }

        [Test]
        public void IsAlongRay_ReportsWhetherTheParticleIsAimedAt()
        {
            var origin = new Vector3(1.8f, 10f, -3f);

            Assert.IsTrue(_world.IsAlongRay(new RopeParticle(0, 9), origin, Vector3.forward, 5f, 0.3f));
            Assert.IsFalse(_world.IsAlongRay(new RopeParticle(0, 0), origin, Vector3.forward, 5f, 0.3f));
        }

        [Test]
        public void FindAlongRay_OutOfReachOrAim_FindsNothing()
        {
            Assert.IsTrue(_world.FindAlongRay(new Vector3(0.6f, 10f, -3f), Vector3.forward, 2f, 0.3f).IsNone);
            Assert.IsTrue(_world.FindAlongRay(new Vector3(0.6f, 13f, -3f), Vector3.forward, 5f, 0.3f).IsNone);
        }
    }
}
