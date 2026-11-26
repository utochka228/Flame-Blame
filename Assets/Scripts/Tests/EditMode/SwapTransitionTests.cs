using FlameBlame.Firefighter.Simulation;
using NUnit.Framework;

namespace FlameBlame.Tests.EditMode
{
    public class SwapTransitionTests
    {
        [Test]
        public void Step_NotBegun_DoesNothing()
        {
            var transition = new SwapTransition(0.2f, 0.4f);

            Assert.IsFalse(transition.Step(1f));
            Assert.IsFalse(transition.IsActive);
            Assert.AreEqual(0f, transition.VisorFraction);
        }

        [Test]
        public void Step_Dropping_LowersTheVisor()
        {
            var transition = new SwapTransition(0.2f, 0.4f);
            transition.Begin();

            Assert.IsFalse(transition.Step(0.1f));
            Assert.AreEqual(0.5f, transition.VisorFraction, 1e-4f);
        }

        [Test]
        public void Step_VisorDown_ReportsTheCutOnce()
        {
            var transition = new SwapTransition(0.2f, 0.4f);
            transition.Begin();

            Assert.IsFalse(transition.Step(0.1f));
            Assert.IsTrue(transition.Step(0.1f));
            Assert.AreEqual(1f, transition.VisorFraction, 1e-4f);
            Assert.IsFalse(transition.Step(0.1f));
        }

        [Test]
        public void Step_Lifted_Finishes()
        {
            var transition = new SwapTransition(0.2f, 0.4f);
            transition.Begin();

            transition.Step(0.2f);
            transition.Step(0.2f);
            Assert.AreEqual(0.5f, transition.VisorFraction, 1e-4f);
            Assert.IsTrue(transition.IsActive);

            transition.Step(0.3f);
            Assert.AreEqual(0f, transition.VisorFraction);
            Assert.IsFalse(transition.IsActive);
        }

        [Test]
        public void Step_OneLongFrame_StillReportsTheCut()
        {
            var transition = new SwapTransition(0.2f, 0.4f);
            transition.Begin();

            Assert.IsTrue(transition.Step(5f));
            Assert.IsFalse(transition.IsActive);
        }
    }
}
