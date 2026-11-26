using FlameBlame.Firefighter.Simulation;
using NUnit.Framework;

namespace FlameBlame.Tests.EditMode
{
    public class CameraFeelTests
    {
        private const float Tick = 1f / 60f;
        private const float AngularFrequency = 18f;
        private const float DampingRatio = 0.6f;
        private const float Frequency = 25f;

        [Test]
        public void Spring_AfterKick_DipsThenReturnsExactlyToRest()
        {
            var spring = new CameraSpring();
            spring.Kick(-3f);

            spring.Step(Tick, AngularFrequency, DampingRatio);
            Assert.That(spring.Offset, Is.LessThan(0f));

            for (var step = 0; step < 120; step++)
                spring.Step(Tick, AngularFrequency, DampingRatio);
            Assert.That(spring.Offset, Is.EqualTo(0f));
        }

        [Test]
        public void Spring_LongFrameHitch_StaysBounded()
        {
            var spring = new CameraSpring();
            spring.Kick(-3f);

            spring.Step(0.5f, AngularFrequency, DampingRatio);

            Assert.That(spring.Offset, Is.InRange(-1f, 1f));
        }

        [Test]
        public void Trauma_ClampsToOne()
        {
            var trauma = new CameraTrauma();

            trauma.Add(0.8f);
            trauma.Add(0.8f);

            Assert.That(trauma.Trauma, Is.EqualTo(1f));
        }

        [Test]
        public void Trauma_DecaysToZeroWithinOneOverDecay()
        {
            const float Decay = 1.5f;
            var trauma = new CameraTrauma();
            trauma.Add(1f);

            for (var elapsed = 0f; elapsed < 1f / Decay + Tick; elapsed += Tick)
                trauma.Step(Tick, Decay, Frequency);

            Assert.That(trauma.Trauma, Is.EqualTo(0f));
            Assert.That(trauma.Wave(1.7f), Is.EqualTo(0f));
        }

        [Test]
        public void Trauma_ShakeIsTraumaSquared()
        {
            var trauma = new CameraTrauma();

            trauma.Add(0.5f);

            Assert.That(trauma.Shake, Is.EqualTo(0.25f));
        }
    }
}
