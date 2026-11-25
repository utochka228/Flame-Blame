using FlameBlame.Hose.Simulation;
using NUnit.Framework;

namespace FlameBlame.Tests.EditMode
{
    public class BurnTimerTests
    {
        private const float Seconds = 2f;
        private const float Step = 0.1f;

        private BurnTimer _timer;

        [SetUp]
        public void SetUp()
        {
            _timer = new BurnTimer(Seconds);
        }

        private bool Run(float seconds, bool isHeated)
        {
            var burned = false;
            for (var index = 0; index < (int)(seconds / Step + 0.5f); index++)
                burned |= _timer.Step(isHeated, Step);

            return burned;
        }

        [Test]
        public void Heat_RaisesProgressInProportion()
        {
            Run(1f, true);

            Assert.That(_timer.Progress, Is.EqualTo(0.5f).Within(1e-4f));
        }

        [Test]
        public void SustainedHeat_BurnsOnceThenStartsOver()
        {
            var burned = Run(2.5f, true);

            Assert.That(burned, Is.True);
            Assert.That(_timer.Progress, Is.EqualTo(0.25f).Within(1e-3f));
        }

        [Test]
        public void ShortOfTheDuration_DoesNotBurn()
        {
            Assert.That(Run(1.5f, true), Is.False);
        }

        [Test]
        public void NoHeat_NeverBurns_AndProgressStaysAtZero()
        {
            Assert.That(Run(5f, false), Is.False);
            Assert.That(_timer.Progress, Is.EqualTo(0f));
        }

        [Test]
        public void Interruption_CoolsDownInsteadOfAccumulating()
        {
            Run(1.2f, true);
            Run(2f, false);

            Assert.That(Run(1.2f, true), Is.False);
        }
    }
}
