using FlameBlame.Fire.Simulation;
using NUnit.Framework;

namespace FlameBlame.Tests.EditMode
{
    public class FireModelTests
    {
        private const float DousePerVolume = 2f;
        private const float HeatRadius = 4f;
        private const float RegrowDelaySeconds = 2f;
        private const float RegrowPerSecond = 0.1f;
        private const float MinWaterPerSecond = 0.2f;
        private const float StepSeconds = 0.5f;

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

        [Test]
        public void Tick_WithinRegrowDelay_DoesNotRegrow()
        {
            var fire = CreateRegrowingFire();
            fire.Douse(0.25f);

            fire.Tick(RegrowDelaySeconds - 0.5f);

            Assert.That(fire.Intensity, Is.EqualTo(0.5f).Within(1e-5f));
        }

        [Test]
        public void Tick_AfterRegrowDelay_RegrowsAtRate()
        {
            var fire = CreateRegrowingFire();
            fire.Douse(0.25f);

            fire.Tick(RegrowDelaySeconds);
            fire.Tick(1f);

            Assert.That(fire.Intensity, Is.EqualTo(0.5f + RegrowPerSecond).Within(1e-5f));
        }

        [Test]
        public void Douse_ResetsRegrowDelay()
        {
            var fire = CreateRegrowingFire();
            fire.Douse(0.25f);
            fire.Tick(RegrowDelaySeconds - 0.5f);

            fire.Douse(0.1f);
            fire.Tick(RegrowDelaySeconds - 0.5f);

            Assert.That(fire.Intensity, Is.EqualTo(0.3f).Within(1e-5f));
        }

        [Test]
        public void Tick_NeverRegrowsPastFullIntensity()
        {
            var fire = CreateRegrowingFire();
            fire.Douse(0.25f);

            fire.Tick(RegrowDelaySeconds);
            fire.Tick(100f);

            Assert.That(fire.Intensity, Is.EqualTo(1f));
        }

        [Test]
        public void Tick_OnExtinguishedFire_StaysOut()
        {
            var fire = CreateRegrowingFire();
            fire.Douse(5f);

            fire.Tick(RegrowDelaySeconds);
            fire.Tick(100f);

            Assert.That(fire.Intensity, Is.EqualTo(0f));
            Assert.That(fire.IsOut, Is.True);
        }

        [Test]
        public void Tick_WaterBelowMinimum_IsIgnored_AndFireRegrows()
        {
            var fire = CreateRegrowingFire(MinWaterPerSecond);
            fire.Douse(0.25f);
            fire.Tick(RegrowDelaySeconds);

            fire.AddWater(MinWaterPerSecond * StepSeconds * 0.5f);
            fire.Tick(StepSeconds);

            Assert.That(fire.Intensity, Is.EqualTo(0.5f + RegrowPerSecond * StepSeconds).Within(1e-5f));
        }

        [Test]
        public void Tick_WaterAtMinimum_DousesTheFire()
        {
            var fire = CreateRegrowingFire(MinWaterPerSecond);
            var water = MinWaterPerSecond * StepSeconds;

            fire.AddWater(water);
            fire.Tick(StepSeconds);

            Assert.That(fire.Intensity, Is.EqualTo(1f - water * DousePerVolume).Within(1e-5f));
        }

        [Test]
        public void Tick_TwoJetsBelowMinimumEach_AddUpPastIt()
        {
            var fire = CreateRegrowingFire(MinWaterPerSecond);
            var halfJet = MinWaterPerSecond * StepSeconds * 0.6f;

            fire.AddWater(halfJet);
            fire.AddWater(halfJet);
            fire.Tick(StepSeconds);

            Assert.That(fire.Intensity, Is.EqualTo(1f - 2f * halfJet * DousePerVolume).Within(1e-5f));
        }

        private static FireModel CreateRegrowingFire(float minWaterPerSecond = 0f) => new FireModel(new FireSettings
        {
            DousePerVolume = DousePerVolume,
            HeatRadius = HeatRadius,
            RegrowDelaySeconds = RegrowDelaySeconds,
            RegrowPerSecond = RegrowPerSecond,
            MinWaterPerSecond = minWaterPerSecond
        });
    }
}
