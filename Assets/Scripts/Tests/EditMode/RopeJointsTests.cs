using FlameBlame.Hose.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace FlameBlame.Tests.EditMode
{
    public class RopeJointsTests
    {
        private const int Substeps = 8;
        private const int ParticleCount = 10;
        private const float Spacing = 0.2f;
        private const float BreakDistance = 0.3f;
        private const float FixedStep = 1f / 60f;

        private RopeWorld _world;
        private RopeJoints _joints;
        private RopeEnd _firstTail;
        private RopeEnd _secondStart;
        private RopeEnd _secondTail;
        private RopeEnd _thirdStart;

        private static int AddRope(RopeWorld world, float x)
        {
            var settings = new RopeSettings { ParticleCount = ParticleCount, Spacing = Spacing };
            return world.AddRope(new XpbdRope(settings, new Vector3(x, 10f, 0f), Vector3.right, new PlaneRopeCollider(-1000f)) { IsPinned = false });
        }

        [SetUp]
        public void SetUp()
        {
            _world = new RopeWorld(Substeps);
            var first = AddRope(_world, 0f);
            var second = AddRope(_world, 2f);
            var third = AddRope(_world, 4f);
            _firstTail = new RopeEnd(first, RopeEndKind.Tail);
            _secondStart = new RopeEnd(second, RopeEndKind.Start);
            _secondTail = new RopeEnd(second, RopeEndKind.Tail);
            _thirdStart = new RopeEnd(third, RopeEndKind.Start);
            _joints = new RopeJoints(_world, BreakDistance, 2);
        }

        private int Join(RopeEnd first, RopeEnd second) => _joints.Add(new[] { first, second });

        [Test]
        public void IsEndFree_UntilJoined()
        {
            Assert.IsTrue(_joints.IsEndFree(_firstTail));

            Join(_firstTail, _secondStart);

            Assert.IsFalse(_joints.IsEndFree(_firstTail));
            Assert.IsFalse(_joints.IsEndFree(_secondStart));
            Assert.IsTrue(_joints.IsEndFree(_secondTail));
        }

        [Test]
        public void Add_ReservedEnd_IsRefused()
        {
            _joints.Reserve(_secondStart);

            Assert.IsFalse(_joints.IsEndFree(_secondStart));
            Assert.AreEqual(RopeJoints.NoJoint, Join(_firstTail, _secondStart));
        }

        [Test]
        public void Add_EndAlreadyJoined_IsRefused()
        {
            Join(_firstTail, _secondStart);

            Assert.AreEqual(RopeJoints.NoJoint, Join(_secondStart, _thirdStart));
        }

        [Test]
        public void Add_PastTheCap_IsRefused()
        {
            Assert.AreNotEqual(RopeJoints.NoJoint, Join(_firstTail, _secondStart));
            Assert.AreNotEqual(RopeJoints.NoJoint, Join(_secondTail, _thirdStart));

            var extraFirst = new RopeEnd(0, RopeEndKind.Start);
            var extraThird = new RopeEnd(2, RopeEndKind.Tail);
            Assert.AreEqual(RopeJoints.NoJoint, Join(extraFirst, extraThird));
        }

        [Test]
        public void Remove_FreesTheEndsAndTheSlot()
        {
            var joint = Join(_firstTail, _secondStart);

            _joints.Remove(joint);

            Assert.IsFalse(_joints.IsLive(joint));
            Assert.IsTrue(_joints.IsEndFree(_firstTail));
            Assert.AreEqual(joint, Join(_firstTail, _secondTail));
        }

        [Test]
        public void Remove_KeepsTheLastPositionFinite()
        {
            var joint = Join(_firstTail, _secondStart);
            var before = _joints.Position(joint);

            _joints.Remove(joint);

            Assert.AreEqual(before, _joints.Position(joint));
        }

        [Test]
        public void StretchBrokenJoint_FreesItsEnds()
        {
            var farStart = new RopeEnd(AddRope(_world, 8f), RopeEndKind.Start);
            var joint = Join(_firstTail, farStart);

            for (var index = 0; index < 5; index++)
                _world.Step(FixedStep);

            Assert.IsFalse(_joints.IsLive(joint));
            Assert.IsTrue(_joints.IsEndFree(_firstTail));
        }

        [Test]
        public void FindAlongRay_PicksTheAimedJoint_AndIgnoresRemovedOnes()
        {
            var joint = Join(_firstTail, _secondStart);
            var point = _joints.Position(joint);
            var origin = point + new Vector3(0f, 0f, -2f);

            Assert.AreEqual(joint, _joints.FindAlongRay(origin, Vector3.forward, 5f, 0.3f));

            _joints.Remove(joint);

            Assert.AreEqual(RopeJoints.NoJoint, _joints.FindAlongRay(origin, Vector3.forward, 5f, 0.3f));
        }
    }
}
