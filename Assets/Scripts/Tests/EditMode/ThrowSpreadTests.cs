using FlameBlame.Firefighter.Simulation;
using NUnit.Framework;

namespace FlameBlame.Tests.EditMode
{
    public class ThrowSpreadTests
    {
        [Test]
        public void Weight_AtTheHeldParticle_IsFull()
        {
            Assert.AreEqual(1f, ThrowSpread.Weight(0, 6), 1e-5f);
        }

        [Test]
        public void Weight_FadesLinearlyWithDistance()
        {
            Assert.AreEqual(0.5f, ThrowSpread.Weight(3, 5), 1e-5f);
            Assert.AreEqual(ThrowSpread.Weight(-2, 5), ThrowSpread.Weight(2, 5), 1e-5f);
        }

        [Test]
        public void Weight_BeyondTheSpread_IsZero()
        {
            Assert.AreEqual(0f, ThrowSpread.Weight(10, 5), 1e-5f);
        }
    }
}
