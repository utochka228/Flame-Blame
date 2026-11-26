using FlameBlame.Firefighter.Simulation;
using NUnit.Framework;

namespace FlameBlame.Tests.EditMode
{
    public class SwapTimerTests
    {
        [Test]
        public void Step_LowestRoll_FiresAtHalfTheMax()
        {
            var timer = new SwapTimer(60f, 5f);
            timer.Restart(0f);

            Assert.IsFalse(timer.Step(29.9f, true));
            Assert.IsTrue(timer.Step(0.2f, true));
        }

        [Test]
        public void Step_HighestRoll_FiresAtTheMax()
        {
            var timer = new SwapTimer(60f, 5f);
            timer.Restart(1f);

            Assert.IsFalse(timer.Step(59.9f, true));
            Assert.IsTrue(timer.Step(0.2f, true));
        }

        [Test]
        public void Step_CannotFire_WaitsUntilItCan()
        {
            var timer = new SwapTimer(30f, 5f);
            timer.Restart(0f);

            Assert.IsFalse(timer.Step(30f, false));
            Assert.IsFalse(timer.Step(1f, false));
            Assert.IsTrue(timer.Step(0.1f, true));
        }

        [Test]
        public void Step_FiresOnceUntilRestarted()
        {
            var timer = new SwapTimer(30f, 5f);
            timer.Restart(0f);

            Assert.IsTrue(timer.Step(20f, true));
            Assert.IsFalse(timer.Step(20f, true));
        }

        [Test]
        public void Step_Off_NeverFires()
        {
            var timer = new SwapTimer(0f, 5f);
            timer.Restart(0f);

            Assert.IsFalse(timer.Step(1000f, true));
            Assert.AreEqual(0f, timer.WarningFraction);
        }

        [Test]
        public void Step_Stopped_NeverFires()
        {
            var timer = new SwapTimer(30f, 5f);
            timer.Restart(0f);
            timer.Stop();

            Assert.IsFalse(timer.Step(1000f, true));
        }

        [Test]
        public void WarningFraction_RampsOverTheWarningSeconds()
        {
            var timer = new SwapTimer(60f, 4f);
            timer.Restart(0f);

            timer.Step(25f, true);
            Assert.AreEqual(0f, timer.WarningFraction);

            timer.Step(3f, true);
            Assert.AreEqual(0.5f, timer.WarningFraction, 1e-4f);
        }

        [Test]
        public void WarningFraction_HeldAtFullWhileWaiting()
        {
            var timer = new SwapTimer(30f, 4f);
            timer.Restart(0f);

            timer.Step(30f, false);

            Assert.AreEqual(1f, timer.WarningFraction);
        }
    }
}
