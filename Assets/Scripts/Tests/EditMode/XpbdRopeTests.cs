using FlameBlame.Core.Extensions;
using FlameBlame.Hose.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace FlameBlame.Tests.EditMode
{
    public class XpbdRopeTests
    {
        private const float FixedStep = 1f / 60f;
        private const float DeepFloor = -1000f;
        private const float AnchorHeight = 10f;
        private const float SettleSeconds = 8f;
        private const float LaunchMargin = 0.2f;
        private const float SettledSpeed = 0.1f;

        private static RopeSettings CreateSettings() => new RopeSettings
        {
            ParticleCount = 50,
            Spacing = 0.2f,
            ParticleRadius = 0.05f
        };

        private static XpbdRope CreateRope(RopeSettings settings, float floorHeight = DeepFloor)
        {
            return new XpbdRope(settings, new Vector3(0f, AnchorHeight, 0f), Vector3.right, new PlaneRopeCollider(floorHeight));
        }

        private static void Simulate(XpbdRope rope, float seconds, float step = FixedStep)
        {
            var stepCount = Mathf.RoundToInt(seconds / step);
            for (var index = 0; index < stepCount; index++)
                rope.Step(step);
        }

        private static float TotalLength(XpbdRope rope)
        {
            var length = 0f;
            for (var index = 1; index < rope.Positions.Count; index++)
                length += Vector3.Distance(rope.Positions[index], rope.Positions[index - 1]);
            return length;
        }

        private static float RestLength(RopeSettings settings) => (settings.ParticleCount - 1) * settings.Spacing;

        private static bool AllFinite(XpbdRope rope)
        {
            foreach (var position in rope.Positions)
            {
                if (!position.IsFinite()) return false;
            }

            return true;
        }

        [Test]
        public void NewRope_IsStraightAlongDirection()
        {
            var settings = CreateSettings();
            var rope = CreateRope(settings);

            Assert.AreEqual(settings.ParticleCount, rope.Positions.Count);
            Assert.AreEqual(RestLength(settings), rope.Positions[rope.Positions.Count - 1].x, 1e-3f);
            Assert.AreEqual(AnchorHeight, rope.Positions[rope.Positions.Count - 1].y, 1e-3f);
        }

        [Test]
        public void HangingRope_KeepsRestLength()
        {
            var settings = CreateSettings();
            var rope = CreateRope(settings);
            rope.RigidityFraction = 0f;

            Simulate(rope, 5f);

            Assert.AreEqual(RestLength(settings), TotalLength(rope), RestLength(settings) * 0.02f);
        }

        [Test]
        public void FloppyRope_HangsDownFromAnchor()
        {
            var settings = CreateSettings();
            var rope = CreateRope(settings);
            rope.RigidityFraction = 0f;

            Simulate(rope, 5f);

            var tipDrop = AnchorHeight - rope.Positions[rope.Positions.Count - 1].y;
            Assert.Greater(tipDrop, RestLength(settings) * 0.5f);
        }

        [Test]
        public void RigidRope_HoldsOutAsCantilever()
        {
            var settings = CreateSettings();
            var rope = CreateRope(settings);
            rope.RigidityFraction = 1f;

            Simulate(rope, 5f);

            var tipDrop = AnchorHeight - rope.Positions[rope.Positions.Count - 1].y;
            Assert.Less(tipDrop, RestLength(settings) * 0.05f);
        }

        [Test]
        public void MoreRigidity_MeansLessSag()
        {
            var settings = CreateSettings();
            var floppy = CreateRope(settings);
            var stiff = CreateRope(settings);
            floppy.RigidityFraction = 0.2f;
            stiff.RigidityFraction = 0.8f;

            Simulate(floppy, 5f);
            Simulate(stiff, 5f);

            Assert.Greater(stiff.Positions[stiff.Positions.Count - 1].y, floppy.Positions[floppy.Positions.Count - 1].y);
        }

        [Test]
        public void Sag_MatchesAcrossTimesteps()
        {
            var settings = CreateSettings();
            settings.Damping = 2f;
            var slowRope = CreateRope(settings);
            var fastRope = CreateRope(settings);
            slowRope.RigidityFraction = 0.4f;
            fastRope.RigidityFraction = 0.4f;

            Simulate(slowRope, 8f, 1f / 50f);
            Simulate(fastRope, 8f, 1f / 60f);

            var slowTip = slowRope.Positions[slowRope.Positions.Count - 1];
            var fastTip = fastRope.Positions[fastRope.Positions.Count - 1];
            Assert.Less(Vector3.Distance(slowTip, fastTip), RestLength(settings) * 0.05f);
        }

        [Test]
        public void TeleportedAnchor_RecoversWithoutNaN()
        {
            var settings = CreateSettings();
            var rope = CreateRope(settings);
            Simulate(rope, 1f);

            rope.SetAnchor(new Vector3(50f, AnchorHeight, 0f), Vector3.right);
            Simulate(rope, 2f);

            Assert.IsTrue(AllFinite(rope));
            Assert.LessOrEqual(TotalLength(rope), RestLength(settings) * (1f + settings.MaxStretchFraction) + 1e-2f);
            Assert.Less(Vector3.Distance(rope.Positions[rope.Positions.Count - 1], new Vector3(50f, AnchorHeight, 0f)), RestLength(settings));
        }

        [Test]
        public void FallingRope_NeverSinksIntoFloor()
        {
            var settings = CreateSettings();
            var rope = CreateRope(settings, 0f);
            rope.RigidityFraction = 0f;

            Simulate(rope, 6f);

            foreach (var position in rope.Positions)
                Assert.GreaterOrEqual(position.y, settings.ParticleRadius - 1e-3f);
        }

        [Test]
        public void PressureDrop_SettlesWithoutLaunchingTheHose()
        {
            var settings = CreateSettings();
            var rope = CreateRope(settings);
            rope.RigidityFraction = 1f;
            Simulate(rope, 3f);

            rope.RigidityFraction = 0f;
            var highestPoint = float.MinValue;
            var stepCount = Mathf.RoundToInt(SettleSeconds / FixedStep);
            for (var step = 0; step < stepCount; step++)
            {
                rope.Step(FixedStep);
                foreach (var position in rope.Positions)
                    highestPoint = Mathf.Max(highestPoint, position.y);
            }

            Assert.LessOrEqual(highestPoint, AnchorHeight + LaunchMargin);
            Assert.Less(MaxSpeed(rope), SettledSpeed);
        }

        private static float MaxSpeed(XpbdRope rope)
        {
            var maxSpeed = 0f;
            for (var index = 0; index < rope.Positions.Count; index++)
                maxSpeed = Mathf.Max(maxSpeed, (rope.Positions[index] - rope.PreviousPositions[index]).magnitude / FixedStep);
            return maxSpeed;
        }

        [Test]
        public void StepStartPositions_ReflectStateBeforeLatestStep()
        {
            var rope = CreateRope(CreateSettings());
            rope.RigidityFraction = 0f;
            Simulate(rope, 1f);
            var before = rope.Positions[rope.Positions.Count - 1];

            rope.Step(FixedStep);

            Assert.AreEqual(before, rope.PreviousPositions[rope.PreviousPositions.Count - 1]);
        }
    }
}
