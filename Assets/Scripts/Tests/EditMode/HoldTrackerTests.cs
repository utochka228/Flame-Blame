using FlameBlame.Firefighter.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace FlameBlame.Tests.EditMode
{
    public class HoldTrackerTests
    {
        private const float FixedStep = 1f / 60f;
        private const float FollowSeconds = 0.2f;
        private const float MaxDistance = 3f;

        private HoldTracker _tracker;

        [SetUp]
        public void SetUp()
        {
            _tracker = new HoldTracker(FollowSeconds, MaxDistance);
        }

        private void StepFor(float seconds, Vector3 holdPoint, Vector3 viewDirection)
        {
            var stepCount = Mathf.RoundToInt(seconds / FixedStep);
            for (var index = 0; index < stepCount; index++)
                _tracker.Step(holdPoint, viewDirection, FixedStep);
        }

        [Test]
        public void Begin_StartsWhereTheParticleIs()
        {
            _tracker.Begin(new Vector3(1f, 2f, 3f), Vector3.forward);

            Assert.AreEqual(new Vector3(1f, 2f, 3f), _tracker.Position);
            Assert.AreEqual(Vector3.forward, _tracker.AimDirection);
        }

        [Test]
        public void Step_EasesTowardTheHoldPoint_NotInstantly()
        {
            _tracker.Begin(Vector3.zero, Vector3.forward);

            _tracker.Step(new Vector3(0f, 0f, 2f), Vector3.forward, FixedStep);

            Assert.Greater(_tracker.Position.z, 0f);
            Assert.Less(_tracker.Position.z, 1f);
        }

        [Test]
        public void Step_ConvergesOnHoldPointAndViewDirection()
        {
            _tracker.Begin(Vector3.zero, Vector3.forward);

            StepFor(3f, new Vector3(0f, 1f, 2f), Vector3.up);

            Assert.Less(Vector3.Distance(_tracker.Position, new Vector3(0f, 1f, 2f)), 0.01f);
            Assert.Greater(Vector3.Dot(_tracker.AimDirection, Vector3.up), 0.999f);
        }

        [Test]
        public void IsTooFar_TripsOnlyPastTheMaxDistance()
        {
            Assert.IsFalse(_tracker.IsTooFar(Vector3.zero, new Vector3(0f, 0f, MaxDistance)));
            Assert.IsTrue(_tracker.IsTooFar(Vector3.zero, new Vector3(0f, 0f, MaxDistance + 0.01f)));
        }
    }
}
