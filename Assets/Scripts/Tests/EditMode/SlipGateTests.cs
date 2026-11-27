using FlameBlame.Firefighter.Simulation;
using NUnit.Framework;

namespace FlameBlame.Tests.EditMode
{
    public class SlipGateTests
    {
        private const float MinSpeed = 3f;
        private const float FallSeconds = 1f;
        private const float ImmunitySeconds = 1f;
        private const float Tick = 0.02f;
        private const float Fast = 5f;

        private SlipGate _gate;

        [SetUp]
        public void SetUp()
        {
            _gate = new SlipGate(MinSpeed, FallSeconds, ImmunitySeconds);
        }

        [Test]
        public void Dry_AtSpeed_NeverSlips()
        {
            Assert.That(_gate.Step(false, Fast, Tick), Is.False);
            Assert.That(_gate.IsFallen, Is.False);
        }

        [Test]
        public void Wet_BelowMinSpeed_NeverSlips()
        {
            Assert.That(_gate.Step(true, MinSpeed - 0.1f, Tick), Is.False);
            Assert.That(_gate.IsFallen, Is.False);
        }

        [Test]
        public void Wet_AtSpeed_SlipsOnceAndFalls()
        {
            Assert.That(_gate.Step(true, Fast, Tick), Is.True);
            Assert.That(_gate.IsFallen, Is.True);
            Assert.That(_gate.Step(true, Fast, Tick), Is.False);
        }

        [Test]
        public void Fall_EndsAfterFallSeconds()
        {
            _gate.Step(true, Fast, Tick);

            StepFor(FallSeconds - Tick, false);
            Assert.That(_gate.IsFallen, Is.True);

            StepFor(2f * Tick, false);
            Assert.That(_gate.IsFallen, Is.False);
        }

        [Test]
        public void Immunity_BlocksSlipUntilItEnds()
        {
            _gate.Step(true, Fast, Tick);
            StepFor(FallSeconds + Tick, false);

            Assert.That(_gate.Step(true, Fast, Tick), Is.False);

            StepFor(ImmunitySeconds, false);
            Assert.That(_gate.Step(true, Fast, Tick), Is.True);
        }

        private void StepFor(float seconds, bool isWet)
        {
            for (var elapsed = 0f; elapsed < seconds - 1e-4f; elapsed += Tick)
                _gate.Step(isWet, Fast, Tick);
        }
    }
}
