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
        public void CurvedRopeOnFloor_AtPartialRigidity_ComesToRest()
        {
            const float Rigidity = 0.88f;
            const float CurveRadius = 3f;
            const float CurveRadiansPerParticle = 0.06f;
            const float MaxWanderMeters = 0.1f;
            var settings = CreateSettings();
            settings.Damping = 2.5f;
            settings.RigidDampingMultiplier = 3f;
            var rope = new XpbdRope(settings, new Vector3(0f, settings.ParticleRadius, 0f), Vector3.right, new PlaneRopeCollider(0f)) { IsPinned = false };
            for (var index = 0; index < rope.Positions.Count; index++)
            {
                var angle = index * CurveRadiansPerParticle;
                rope.SetPosition(index, new Vector3(Mathf.Sin(angle) * CurveRadius, settings.ParticleRadius, (1f - Mathf.Cos(angle)) * CurveRadius));
            }

            Simulate(rope, 2f);
            rope.RigidityFraction = Rigidity;
            Simulate(rope, 3f);
            var start = new Vector3[rope.Positions.Count];
            for (var index = 0; index < start.Length; index++)
                start[index] = rope.Positions[index];

            var wander = 0f;
            for (var step = 0; step < Mathf.RoundToInt(1f / FixedStep); step++)
            {
                rope.Step(FixedStep);
                for (var index = 0; index < start.Length; index++)
                    wander = Mathf.Max(wander, Vector3.Distance(rope.Positions[index], start[index]));
            }

            Assert.Less(wander, MaxWanderMeters);
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
        public void RigidRope_SpansGapHeldAtBothEnds()
        {
            const float Gap = 7.4f;
            const int Substeps = 8;
            const float EndCompliance = 1e-7f;
            const float AimCompliance = 1e-5f;
            var settings = CreateSettings();
            var rope = CreateRope(settings);
            rope.IsPinned = false;
            rope.RigidityFraction = 1f;
            var world = new RopeWorld(Substeps);
            world.AddRope(rope);
            var first = Mathf.RoundToInt((RestLength(settings) - Gap) * 0.5f / settings.Spacing);
            var last = settings.ParticleCount - 1 - first;
            var start = new Vector3(first * settings.Spacing, AnchorHeight, 0f);
            HoldAt(world, first, start, EndCompliance);
            HoldAt(world, first + 1, start + Vector3.right * settings.Spacing, AimCompliance);
            HoldAt(world, last, start + Vector3.right * Gap, EndCompliance);
            HoldAt(world, last - 1, start + Vector3.right * (Gap - settings.Spacing), AimCompliance);

            for (var step = 0; step < Mathf.RoundToInt(5f / FixedStep); step++)
                world.Step(FixedStep);

            var lowest = AnchorHeight;
            for (var index = first; index <= last; index++)
                lowest = Mathf.Min(lowest, rope.Positions[index].y);
            Assert.Less(AnchorHeight - lowest, Gap * 0.05f);
        }

        private static void HoldAt(RopeWorld world, int index, Vector3 target, float compliance)
        {
            var grip = world.AddGrip(new RopeParticle(0, index), compliance);
            world.SetGripTarget(grip, target);
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

        [Test]
        public void RigidityProfile_StiffensOnlyTheParticlesItNames()
        {
            var settings = CreateSettings();
            var rope = CreateRope(settings);
            var halfCount = settings.ParticleCount / 2;
            var profile = new float[settings.ParticleCount];
            for (var index = 0; index < halfCount; index++)
                profile[index] = 1f;
            rope.SetRigidityProfile(profile);
            var floppyRope = CreateRope(settings);

            Simulate(rope, 5f);
            Simulate(floppyRope, 5f);

            var rigidHalfDrop = AnchorHeight - rope.Positions[halfCount - 1].y;
            var tipDrop = AnchorHeight - rope.Positions[rope.Positions.Count - 1].y;
            var rigidHalfReach = rope.Positions[halfCount - 1].x;
            var floppyReach = floppyRope.Positions[halfCount - 1].x;
            Assert.Greater(rigidHalfReach, floppyReach + RestLength(settings) * 0.1f);
            Assert.Greater(tipDrop, rigidHalfDrop + RestLength(settings) * 0.2f);
        }

        [Test]
        public void FrozenParticle_HoldsUnderItsOwnHangingWeight()
        {
            var settings = CreateSettings();
            var rope = CreateRope(settings);
            var frozen = FrozenAt(settings, settings.ParticleCount / 2);
            var start = rope.Positions[settings.ParticleCount / 2];

            for (var step = 0; step < Mathf.RoundToInt(2f / FixedStep); step++)
            {
                rope.SetFrozenProfile(frozen);
                rope.Step(FixedStep);
            }

            Assert.AreEqual(start, rope.Positions[settings.ParticleCount / 2]);
        }

        [Test]
        public void FrozenParticle_SlipsFreeWhenPulledHard()
        {
            var settings = CreateSettings();
            var rope = CreateRope(settings);
            var frozen = FrozenAt(settings, settings.ParticleCount / 2);
            var start = rope.Positions[settings.ParticleCount / 2];

            var tip = rope.Positions[settings.ParticleCount - 1];
            for (var step = 0; step < Mathf.RoundToInt(1f / FixedStep); step++)
            {
                tip += Vector3.right * (2f * FixedStep);
                rope.SetFrozenProfile(frozen);
                rope.SetPosition(settings.ParticleCount - 1, tip);
                rope.Step(FixedStep);
            }

            Assert.Greater(Vector3.Distance(start, rope.Positions[settings.ParticleCount / 2]), settings.Spacing);
        }

        private static bool[] FrozenAt(RopeSettings settings, int index)
        {
            var frozen = new bool[settings.ParticleCount];
            frozen[index] = true;
            return frozen;
        }
    }
}
