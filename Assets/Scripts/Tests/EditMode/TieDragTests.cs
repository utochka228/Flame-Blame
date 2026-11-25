using FlameBlame.Firefighter.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace FlameBlame.Tests.EditMode
{
    public class TieDragTests
    {
        private const float Step = 0.02f;
        private const float Mass = 10f;

        private static TieDrag Drag() => new TieDrag(new TieDragSettings
        {
            MinTension = 500f,
            ForceMultiplier = 0.1f,
            MaxForce = 200f,
            MaxSpeed = 2f,
            BrakeSeconds = 0.5f
        });

        private static Vector3 Settle(TieDrag drag, Vector3 tension, bool isPulled, Vector3 velocity)
        {
            var force = Vector3.zero;
            for (var index = 0; index < 200; index++)
                force = drag.Step(tension, isPulled, velocity, Mass, Step);

            return force;
        }

        [Test]
        public void Step_TensionBelowTheDeadZone_AppliesNoForce()
        {
            var force = Settle(Drag(), new Vector3(300f, 0f, 0f), true, Vector3.zero);

            Assert.AreEqual(0f, force.magnitude, 1e-4f);
        }

        [Test]
        public void Step_PullAboveTheDeadZone_ScalesWithTheExcess()
        {
            var force = Settle(Drag(), new Vector3(1500f, 0f, 0f), true, Vector3.zero);

            Assert.AreEqual(100f, force.x, 0.5f);
        }

        [Test]
        public void Step_StrongPull_IsCappedAtMaxForce()
        {
            var force = Settle(Drag(), new Vector3(0f, 0f, 50000f), true, Vector3.zero);

            Assert.AreEqual(200f, force.z, 0.5f);
        }

        [Test]
        public void Step_AlreadyAtMaxSpeed_AppliesNoMoreForce()
        {
            var force = Settle(Drag(), new Vector3(5000f, 0f, 0f), true, new Vector3(2f, 0f, 0f));

            Assert.AreEqual(0f, force.magnitude, 1e-4f);
        }

        [Test]
        public void Step_NobodyHoldingTheHose_BrakesAgainstTheMotion()
        {
            var force = Drag().Step(new Vector3(5000f, 0f, 0f), false, new Vector3(1f, 0f, 0f), Mass, Step);

            Assert.AreEqual(-Mass / 0.5f, force.x, 1e-3f);
        }

        [Test]
        public void Step_IgnoresTheVerticalPullAndVelocity()
        {
            var force = Drag().Step(new Vector3(0f, 5000f, 0f), true, new Vector3(0f, -4f, 0f), Mass, Step);

            Assert.AreEqual(0f, force.magnitude, 1e-4f);
        }
    }
}
