using FlameBlame.Fire.Simulation;
using NUnit.Framework;

namespace FlameBlame.Tests.EditMode
{
    public class FireModelTests
    {
        private const float DousePerVolume = 2f;
        private const float HeatRadius = 4f;

        private FireModel _fire;
        private int _extinguishedCount;

        [SetUp]
        public void SetUp()
        {
            _fire = new FireModel(new FireSettings { DousePerVolume = DousePerVolume, HeatRadius = HeatRadius });
            _extinguishedCount = 0;
            _fire.Extinguished += () => _extinguishedCount++;
        }

        [Test]
        public void NewFire_BurnsAtFullIntensity()
        {
            Assert.That(_fire.Intensity, Is.EqualTo(1f));
            Assert.That(_fire.IsOut, Is.False);
        }

        [Test]
        public void Douse_ReducesIntensityByVolumeTimesEfficiency()
        {
            _fire.Douse(0.25f);

            Assert.That(_fire.Intensity, Is.EqualTo(0.5f).Within(1e-5f));
            Assert.That(_fire.IsOut, Is.False);
        }

        [Test]
        public void Douse_NoVolume_DoesNothing()
        {
            _fire.Douse(0f);
            _fire.Douse(-1f);

            Assert.That(_fire.Intensity, Is.EqualTo(1f));
        }

        [Test]
        public void Douse_Enough_PutsTheFireOutOnce()
        {
            _fire.Douse(0.4f);
            _fire.Douse(0.4f);
            _fire.Douse(5f);

            Assert.That(_fire.Intensity, Is.EqualTo(0f));
            Assert.That(_fire.IsOut, Is.True);
            Assert.That(_extinguishedCount, Is.EqualTo(1));
        }

        [Test]
        public void HeatFallsOffLinearlyWithDistance()
        {
            Assert.That(_fire.HeatAt(0f), Is.EqualTo(1f).Within(1e-5f));
            Assert.That(_fire.HeatAt(1f), Is.EqualTo(0.75f).Within(1e-5f));
            Assert.That(_fire.HeatAt(HeatRadius), Is.EqualTo(0f));
            Assert.That(_fire.HeatAt(HeatRadius + 3f), Is.EqualTo(0f));
        }

        [Test]
        public void Heat_ScalesWithIntensity_AndVanishesWhenOut()
        {
            _fire.Douse(0.25f);
            Assert.That(_fire.HeatAt(0f), Is.EqualTo(0.5f).Within(1e-5f));

            _fire.Douse(5f);
            Assert.That(_fire.HeatAt(0f), Is.EqualTo(0f));
        }
    }
}
