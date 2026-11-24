using FlameBlame.Firefighter.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace FlameBlame.Tests.EditMode
{
    public class LookStateTests
    {
        private const float DegreesPerPixel = 0.5f;
        private const float PitchLimit = 80f;

        [Test]
        public void Apply_AccumulatesYaw_FromInitial()
        {
            var look = new LookState(DegreesPerPixel, PitchLimit, 90f);

            look.Apply(new Vector2(10f, 0f));
            look.Apply(new Vector2(4f, 0f));

            Assert.That(look.Yaw, Is.EqualTo(97f));
        }

        [Test]
        public void Apply_MovingMouseUp_LooksUp()
        {
            var look = new LookState(DegreesPerPixel, PitchLimit, 0f);

            look.Apply(new Vector2(0f, 20f));

            Assert.That(look.Pitch, Is.EqualTo(-10f));
        }

        [Test]
        public void Apply_ClampsPitchBothWays()
        {
            var look = new LookState(DegreesPerPixel, PitchLimit, 0f);

            look.Apply(new Vector2(0f, 1000f));
            Assert.That(look.Pitch, Is.EqualTo(-PitchLimit));

            look.Apply(new Vector2(0f, -5000f));
            Assert.That(look.Pitch, Is.EqualTo(PitchLimit));
        }
    }
}
