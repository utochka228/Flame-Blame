using FlameBlame.Core.Extensions;
using FlameBlame.Hose.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace FlameBlame.Tests.EditMode
{
    public class RopeWorldTests
    {
        private const float FixedStep = 1f / 60f;
        private const float DeepFloor = -1000f;
        private const float AnchorHeight = 10f;
        private const int Substeps = 8;
        private const int ParticleCount = 10;
        private const float Spacing = 0.2f;
        private const float GapTolerance = 0.05f;
        private const float BreakDistance = 0.5f;
        private const float StretchTolerance = 0.05f;

        private static readonly float TailX = (ParticleCount - 1) * Spacing;

        private static RopeSettings CreateSettings() => new RopeSettings
        {
            ParticleCount = ParticleCount,
            Spacing = Spacing,
            ParticleRadius = 0.05f
        };

        private static XpbdRope CreateRope(float anchorX, bool pinned)
        {
            return new XpbdRope(CreateSettings(), new Vector3(anchorX, AnchorHeight, 0f), Vector3.right, new PlaneRopeCollider(DeepFloor))
            {
                IsPinned = pinned
            };
        }

        private static void Simulate(RopeWorld world, float seconds)
        {
            var stepCount = Mathf.RoundToInt(seconds / FixedStep);
            for (var index = 0; index < stepCount; index++)
                world.Step(FixedStep);
        }

        private static float TotalLength(XpbdRope rope)
        {
            var length = 0f;
            for (var index = 1; index < rope.Positions.Count; index++)
                length += Vector3.Distance(rope.Positions[index], rope.Positions[index - 1]);
            return length;
        }

        private static Vector3 EndPosition(XpbdRope rope, RopeEndKind kind) =>
            rope.Positions[kind == RopeEndKind.Start ? 0 : rope.Positions.Count - 1];

        [Test]
        public void JoinedRopes_KeepTheirEndsTogether()
        {
            var world = new RopeWorld(Substeps);
            var first = CreateRope(0f, true);
            var second = CreateRope(TailX, false);
            var junction = world.AddJunction(BreakDistance);
            world.Attach(junction, new RopeEnd(world.AddRope(first), RopeEndKind.Tail));
            world.Attach(junction, new RopeEnd(world.AddRope(second), RopeEndKind.Start));

            Simulate(world, 3f);

            Assert.Less(Vector3.Distance(EndPosition(first, RopeEndKind.Tail), EndPosition(second, RopeEndKind.Start)), GapTolerance);
            Assert.IsTrue(world.IsIntact(junction));
        }

        [Test]
        public void JoinedRopes_StayFiniteAndUnstretched()
        {
            var world = new RopeWorld(Substeps);
            var first = CreateRope(0f, true);
            var second = CreateRope(TailX, false);
            var junction = world.AddJunction(BreakDistance);
            world.Attach(junction, new RopeEnd(world.AddRope(first), RopeEndKind.Tail));
            world.Attach(junction, new RopeEnd(world.AddRope(second), RopeEndKind.Start));
            var restLength = (ParticleCount - 1) * Spacing;

            Simulate(world, 5f);

            foreach (var rope in new[] { first, second })
            {
                Assert.AreEqual(restLength, TotalLength(rope), restLength * StretchTolerance);
                foreach (var position in rope.Positions)
                    Assert.IsTrue(position.IsFinite());
            }
        }

        [Test]
        public void ThreeEndJunction_BringsAllEndsToOnePoint()
        {
            var world = new RopeWorld(Substeps);
            var feed = CreateRope(0f, true);
            var left = CreateRope(TailX, false);
            var right = CreateRope(TailX, false);
            var junction = world.AddJunction(BreakDistance);
            world.Attach(junction, new RopeEnd(world.AddRope(feed), RopeEndKind.Tail));
            world.Attach(junction, new RopeEnd(world.AddRope(left), RopeEndKind.Start));
            world.Attach(junction, new RopeEnd(world.AddRope(right), RopeEndKind.Start));

            Simulate(world, 3f);

            var feedTail = EndPosition(feed, RopeEndKind.Tail);
            Assert.Less(Vector3.Distance(feedTail, EndPosition(left, RopeEndKind.Start)), GapTolerance);
            Assert.Less(Vector3.Distance(feedTail, EndPosition(right, RopeEndKind.Start)), GapTolerance);
        }

        [Test]
        public void Junction_BreaksWhenEndsArePulledApart()
        {
            var world = new RopeWorld(Substeps);
            var first = CreateRope(0f, true);
            var second = CreateRope(TailX, true);
            var junction = world.AddJunction(BreakDistance);
            world.Attach(junction, new RopeEnd(world.AddRope(first), RopeEndKind.Tail));
            world.Attach(junction, new RopeEnd(world.AddRope(second), RopeEndKind.Start));
            Simulate(world, 0.5f);
            Assert.IsTrue(world.IsIntact(junction));

            second.SetAnchor(new Vector3(TailX + 5f, AnchorHeight, 0f), Vector3.right);
            Simulate(world, 0.5f);

            Assert.IsFalse(world.IsIntact(junction));
        }

        [Test]
        public void JunctionPosition_FollowsTheJoinedEnds()
        {
            var world = new RopeWorld(Substeps);
            var first = CreateRope(0f, true);
            var second = CreateRope(TailX, false);
            var junction = world.AddJunction(BreakDistance);
            world.Attach(junction, new RopeEnd(world.AddRope(first), RopeEndKind.Tail));
            world.Attach(junction, new RopeEnd(world.AddRope(second), RopeEndKind.Start));

            Simulate(world, 2f);

            Assert.Less(Vector3.Distance(world.JunctionPosition(junction), EndPosition(first, RopeEndKind.Tail)), GapTolerance);
        }

        [Test]
        public void RigidJoinedRopes_KeepTheirEndsTogetherEveryStep()
        {
            var world = new RopeWorld(Substeps);
            var first = CreateRope(0f, true);
            var second = CreateRope(TailX, false);
            first.RigidityFraction = 1f;
            second.RigidityFraction = 1f;
            var junction = world.AddJunction(BreakDistance);
            world.Attach(junction, new RopeEnd(world.AddRope(first), RopeEndKind.Tail));
            world.Attach(junction, new RopeEnd(world.AddRope(second), RopeEndKind.Start));
            var worstGap = 0f;

            for (var step = 0; step < 300; step++)
            {
                world.Step(FixedStep);
                worstGap = Mathf.Max(worstGap, Vector3.Distance(EndPosition(first, RopeEndKind.Tail), EndPosition(second, RopeEndKind.Start)));
            }

            Assert.Less(worstGap, GapTolerance);
        }

        [Test]
        public void AddJunction_AfterTheWorldHasStepped_SurvivesItsFirstStepWhenEndsStartApart()
        {
            var first = CreateRope(0f, true);
            var second = CreateRope(TailX + 0.5f, false);
            var world = new RopeWorld(Substeps);
            var firstIndex = world.AddRope(first);
            var secondIndex = world.AddRope(second);
            world.Step(FixedStep);

            var junction = world.AddJunction(0.25f);
            world.Attach(junction, new RopeEnd(firstIndex, RopeEndKind.Tail));
            world.Attach(junction, new RopeEnd(secondIndex, RopeEndKind.Start));
            world.Step(FixedStep);

            Assert.IsTrue(world.IsIntact(junction));
        }
    }
}
