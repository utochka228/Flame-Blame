using FlameBlame.Firefighter.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace FlameBlame.Tests.EditMode
{
    public class RopeClimbTests
    {
        private static Vector3[] HangingRope()
        {
            var positions = new Vector3[5];
            for (var index = 0; index < positions.Length; index++)
                positions[index] = new Vector3(0f, 4f - index * 0.5f, 0f);

            return positions;
        }

        [Test]
        public void Step_ForwardInput_ClimbsTowardHigherEnd()
        {
            var climb = new RopeClimb(1f);
            climb.Begin(3);

            var position = climb.Step(HangingRope(), 1f, 0.5f);

            Assert.AreEqual(2f, climb.Index, 1e-4f);
            Assert.AreEqual(3f, position.y, 1e-4f);
        }

        [Test]
        public void Step_BackwardInput_DescendsTowardLowerEnd()
        {
            var climb = new RopeClimb(1f);
            climb.Begin(1);

            climb.Step(HangingRope(), -1f, 0.5f);

            Assert.AreEqual(2f, climb.Index, 1e-4f);
        }

        [Test]
        public void Step_ClampsAtRopeEnds()
        {
            var climb = new RopeClimb(10f);
            climb.Begin(1);
            climb.Step(HangingRope(), 1f, 1f);
            Assert.AreEqual(0f, climb.Index, 1e-4f);

            climb.Step(HangingRope(), -1f, 10f);
            Assert.AreEqual(4f, climb.Index, 1e-4f);
        }

        [Test]
        public void Step_NoInput_StaysPut()
        {
            var climb = new RopeClimb(2f);
            climb.Begin(2);

            climb.Step(HangingRope(), 0f, 1f);

            Assert.AreEqual(2f, climb.Index, 1e-4f);
        }

        private static Vector3[] RopeOverEdge()
        {
            return new[]
            {
                new Vector3(0f, 0f, 0f), new Vector3(0f, 1f, 0f), new Vector3(0f, 2f, 0f), new Vector3(0f, 3f, 0f),
                new Vector3(1f, 2.9f, 0f), new Vector3(2f, 1.9f, 0f), new Vector3(2f, 0.9f, 0f)
            };
        }

        [Test]
        public void Step_ClimbingOverEdge_StopsAtThePeakInsteadOfTurningBack()
        {
            var climb = new RopeClimb(10f);
            climb.Begin(1);

            climb.Step(RopeOverEdge(), 1f, 1f);

            Assert.AreEqual(3f, climb.Index, 1e-4f);
            Assert.IsTrue(climb.IsAtTop);
        }

        [Test]
        public void Step_AtPeak_ForwardInputStaysAndBackwardInputDescends()
        {
            var climb = new RopeClimb(1f);
            climb.Begin(3);

            climb.Step(RopeOverEdge(), 1f, 0.5f);
            Assert.AreEqual(3f, climb.Index, 1e-4f);

            climb.Step(RopeOverEdge(), -1f, 0.5f);
            Assert.Less(climb.Index, 3f);
            Assert.IsFalse(climb.IsAtTop);
        }

        [Test]
        public void Step_FromTheFarSide_ClimbsBackTowardThePeak()
        {
            var climb = new RopeClimb(1f);
            climb.Begin(6);

            climb.Step(RopeOverEdge(), 1f, 0.5f);

            Assert.Less(climb.Index, 6f);
        }

        [Test]
        public void TopDirection_PointsAcrossThePeakAwayFromTheClimbSide()
        {
            var climb = new RopeClimb(10f);
            climb.Begin(1);
            climb.Step(RopeOverEdge(), 1f, 1f);

            var direction = climb.TopDirection(RopeOverEdge());

            Assert.AreEqual(1f, direction.x, 1e-4f);
            Assert.AreEqual(0f, direction.y, 1e-4f);
        }
    }
}
