using FlameBlame.Firefighter.Simulation;
using NUnit.Framework;

namespace FlameBlame.Tests.EditMode
{
    public class CarryLoadTests
    {
        private const float Solo = 0.4f;

        [Test]
        public void SpeedScale_SingleGrip_IsSoloSpeed()
        {
            Assert.That(CarryLoad.SpeedScale(1, false, Solo), Is.EqualTo(Solo));
        }

        [Test]
        public void SpeedScale_NozzleGrips_AreNotABuddy()
        {
            Assert.That(CarryLoad.SpeedScale(2, true, Solo), Is.EqualTo(Solo));
        }

        [Test]
        public void SpeedScale_SecondHolder_IsFullSpeed()
        {
            Assert.That(CarryLoad.SpeedScale(2, false, Solo), Is.EqualTo(1f));
            Assert.That(CarryLoad.SpeedScale(3, true, Solo), Is.EqualTo(1f));
        }
    }
}
