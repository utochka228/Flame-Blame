using FlameBlame.Firefighter.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace FlameBlame.Tests.EditMode
{
    public class TieDragTests
    {
        private const float Step = 0.02f;

        private static TieDrag Drag() => new TieDrag(new TieDragSettings
        {
            MinTension = 500f,
            SpeedMultiplier = 0.001f,
            MaxSpeed = 3f,
            Acceleration = 100f
        });

        private static Vector3 Settle(TieDrag drag, Vector3 tension, bool isPulled, Vector3 start)
        {
            var velocity = start;
            for (var index = 0; index < 200; index++)
                velocity = drag.Step(tension, isPulled, velocity, Step);

            return velocity;
        }

        [Test]
        public void Step_TensionBelowTheDeadZone_DoesNotMove()
        {
            var velocity = Settle(Drag(), new Vector3(300f, 0f, 0f), true, Vector3.zero);

            Assert.AreEqual(0f, velocity.magnitude, 1e-4f);
        }

        [Test]
        public void Step_PullAboveTheDeadZone_ScalesWithTheExcess()
        {
            var velocity = Settle(Drag(), new Vector3(1500f, 0f, 0f), true, Vector3.zero);

            Assert.AreEqual(1f, velocity.x, 0.01f);
        }

        [Test]
        public void Step_StrongPull_IsCappedAtMaxSpeed()
        {
            var velocity = Settle(Drag(), new Vector3(0f, 0f, 50000f), true, Vector3.zero);

            Assert.AreEqual(3f, velocity.z, 0.01f);
        }

        [Test]
        public void Step_NobodyHoldingTheHose_BrakesTheBox()
        {
            var velocity = Settle(Drag(), new Vector3(5000f, 0f, 0f), false, new Vector3(2f, 0f, 0f));

            Assert.AreEqual(0f, velocity.x, 1e-4f);
        }

        [Test]
        public void Step_KeepsTheVerticalVelocity()
        {
            var velocity = Drag().Step(new Vector3(5000f, 0f, 0f), true, new Vector3(0f, -4f, 0f), Step);

            Assert.AreEqual(-4f, velocity.y, 1e-4f);
        }

        [Test]
        public void Step_LimitsAccelerationPerStep()
        {
            var velocity = Drag().Step(new Vector3(50000f, 0f, 0f), true, Vector3.zero, Step);

            Assert.LessOrEqual(velocity.x, 100f * Step + 1e-4f);
        }
    }
}
