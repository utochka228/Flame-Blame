using FlameBlame.Firefighter.Simulation;
using NUnit.Framework;

namespace FlameBlame.Tests.EditMode
{
    public class ClimbMantleTests
    {
        [Test]
        public void Step_FeetBelowTheTop_KeepsRising()
        {
            var mantle = new ClimbMantle();
            mantle.Begin(3f);

            Assert.IsFalse(mantle.Step(2.5f, 0.02f));
        }

        [Test]
        public void Step_FeetClearTheTop_Finishes()
        {
            var mantle = new ClimbMantle();
            mantle.Begin(3f);

            Assert.IsTrue(mantle.Step(3.3f, 0.02f));
        }

        [Test]
        public void Step_GivesUpAfterTheTimeLimit()
        {
            var mantle = new ClimbMantle();
            mantle.Begin(3f);

            Assert.IsFalse(mantle.Step(0f, 0.5f));
            Assert.IsTrue(mantle.Step(0f, 0.6f));
        }
    }
}
