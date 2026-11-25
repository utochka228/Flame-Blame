using System.Collections.Generic;
using FlameBlame.Hose.Simulation;
using NUnit.Framework;

namespace FlameBlame.Tests.EditMode
{
    public class HoseBreachesTests
    {
        private const int ParticleCount = 10;
        private const int WeakPoint = 5;
        private const float BurstLoss = 40f;
        private const float TapeLoss = 8f;
        private const float Step = 0.1f;

        private HoseBreaches _breaches;
        private float[] _heat;
        private int _popCount;
        private readonly List<int> _burned = new List<int>();

        [SetUp]
        public void SetUp()
        {
            _breaches = new HoseBreaches(new BreachSettings
            {
                ParticleCount = ParticleCount,
                WeakPointIndex = WeakPoint,
                OverloadSeconds = 1f,
                RecoverSeconds = 1f,
                HeatThreshold = 0.5f,
                BurnSeconds = 1f,
                BurstLeakLoss = BurstLoss,
                TapeLeakLoss = TapeLoss
            });
            _heat = new float[ParticleCount];
            _popCount = 0;
            _burned.Clear();
            _breaches.Popped += () => _popCount++;
            _breaches.Burned += index => _burned.Add(index);
        }

        private void Heat(int particle, float value)
        {
            _heat[particle] = value;
        }

        private void Run(float seconds, float pressureFraction)
        {
            for (var index = 0; index < (int)(seconds / Step + 0.5f); index++)
                _breaches.Step(pressureFraction, _heat, Step);
        }

        [Test]
        public void NoOverloadNoHeat_HasNoBreaches()
        {
            Run(5f, 0.8f);

            Assert.That(_breaches.Breaches, Is.Empty);
        }

        [Test]
        public void SustainedOverload_PopsAnOpenLeakAtTheWeakPoint()
        {
            Run(1.5f, 1.5f);

            Assert.That(_breaches.Breaches.Count, Is.EqualTo(1));
            Assert.That(_breaches.Breaches[0].Index, Is.EqualTo(WeakPoint));
            Assert.That(_breaches.Breaches[0].Loss, Is.EqualTo(BurstLoss));
            Assert.That(_breaches.Breaches[0].IsTaped, Is.False);
            Assert.That(_popCount, Is.EqualTo(1));
        }

        [Test]
        public void Tape_OnABurst_KeepsASmallerPermanentLeak()
        {
            Run(1.5f, 1.5f);

            var changed = _breaches.Tape(WeakPoint);

            Assert.That(changed, Is.True);
            Assert.That(_breaches.Breaches.Count, Is.EqualTo(1));
            Assert.That(_breaches.Breaches[0].IsTaped, Is.True);
            Assert.That(_breaches.Breaches[0].Loss, Is.EqualTo(TapeLoss));
        }

        [Test]
        public void Tape_WhereThereIsNoBreach_DoesNothing()
        {
            Assert.That(_breaches.Tape(2), Is.False);
            Assert.That(_breaches.Breaches, Is.Empty);
        }

        [Test]
        public void Tape_ThenOverloadAgain_PopsOpenAgain()
        {
            Run(1.5f, 1.5f);
            _breaches.Tape(WeakPoint);

            Run(1.5f, 1.5f);

            Assert.That(_popCount, Is.EqualTo(2));
            Assert.That(_breaches.Breaches.Count, Is.EqualTo(1));
            Assert.That(_breaches.Breaches[0].IsTaped, Is.False);
            Assert.That(_breaches.Breaches[0].Loss, Is.EqualTo(BurstLoss));
        }

        [Test]
        public void SustainedHeat_BurnsAnOpenLeakAtThatParticle()
        {
            Heat(2, 0.8f);

            Run(1.5f, 0.5f);

            Assert.That(_breaches.Breaches.Count, Is.EqualTo(1));
            Assert.That(_breaches.Breaches[0].Index, Is.EqualTo(2));
            Assert.That(_breaches.Breaches[0].Loss, Is.EqualTo(BurstLoss));
            Assert.That(_breaches.Breaches[0].IsTaped, Is.False);
            Assert.That(_burned, Is.EqualTo(new[] { 2 }));
            Assert.That(_popCount, Is.EqualTo(0));
        }

        [Test]
        public void HeatBelowTheThreshold_NeverBurns()
        {
            Heat(2, 0.4f);

            Run(5f, 0.5f);

            Assert.That(_breaches.Breaches, Is.Empty);
        }

        [Test]
        public void InterruptedHeat_CoolsDownInsteadOfAccumulating()
        {
            Heat(2, 0.8f);
            Run(0.6f, 0.5f);
            Heat(2, 0f);
            Run(1f, 0.5f);
            Heat(2, 0.8f);
            Run(0.6f, 0.5f);

            Assert.That(_breaches.Breaches, Is.Empty);
        }

        [Test]
        public void HeatAfterTheBurn_DoesNotBurnAgainWhileTheLeakIsOpen()
        {
            Heat(2, 0.8f);

            Run(5f, 0.5f);

            Assert.That(_burned.Count, Is.EqualTo(1));
        }

        [Test]
        public void TapedLeak_BurnsOpenAgainUnderHeat()
        {
            Heat(2, 0.8f);
            Run(1.5f, 0.5f);
            _breaches.Tape(2);

            Run(1.5f, 0.5f);

            Assert.That(_burned.Count, Is.EqualTo(2));
            Assert.That(_breaches.Breaches[0].IsTaped, Is.False);
        }

        [Test]
        public void HeatOverAStretch_BurnsOneLeakCoveringIt()
        {
            for (var particle = 2; particle <= 5; particle++)
                Heat(particle, 0.8f);

            Run(1.5f, 0.5f);

            Assert.That(_breaches.Breaches.Count, Is.EqualTo(1));
            Assert.That(_breaches.Breaches[0].FirstIndex, Is.EqualTo(2));
            Assert.That(_breaches.Breaches[0].LastIndex, Is.EqualTo(5));
            Assert.That(_breaches.Breaches[0].Index, Is.EqualTo(3));
            Assert.That(_breaches.Breaches[0].Loss, Is.EqualTo(BurstLoss));
            Assert.That(_burned.Count, Is.EqualTo(1));
        }

        [Test]
        public void SeparateHotSpots_BurnSeparateLeaks()
        {
            Heat(1, 0.8f);
            Heat(6, 0.8f);

            Run(1.5f, 0.5f);

            Assert.That(_breaches.Breaches.Count, Is.EqualTo(2));
        }

        [Test]
        public void HeatNextToAnOpenLeak_GrowsItInsteadOfStartingAnother()
        {
            Heat(2, 0.8f);
            Run(1.5f, 0.5f);

            Heat(3, 0.8f);
            Run(1.5f, 0.5f);

            Assert.That(_breaches.Breaches.Count, Is.EqualTo(1));
            Assert.That(_breaches.Breaches[0].FirstIndex, Is.EqualTo(2));
            Assert.That(_breaches.Breaches[0].LastIndex, Is.EqualTo(3));
            Assert.That(_burned.Count, Is.EqualTo(1));
        }

        [Test]
        public void HeatBridgingTwoLeaks_JoinsThemIntoOne()
        {
            Heat(2, 0.8f);
            Heat(4, 0.8f);
            Run(1.5f, 0.5f);

            Heat(3, 0.8f);
            Run(1.5f, 0.5f);

            Assert.That(_breaches.Breaches.Count, Is.EqualTo(1));
            Assert.That(_breaches.Breaches[0].FirstIndex, Is.EqualTo(2));
            Assert.That(_breaches.Breaches[0].LastIndex, Is.EqualTo(4));
        }

        [Test]
        public void Tape_AnywhereOnTheStretch_RepairsTheWholeLeak()
        {
            for (var particle = 2; particle <= 5; particle++)
                Heat(particle, 0.8f);
            Run(1.5f, 0.5f);

            var changed = _breaches.Tape(5);

            Assert.That(changed, Is.True);
            Assert.That(_breaches.Breaches.Count, Is.EqualTo(1));
            Assert.That(_breaches.Breaches[0].IsTaped, Is.True);
            Assert.That(_breaches.Breaches[0].Loss, Is.EqualTo(TapeLoss));
            Assert.That(_breaches.Breaches[0].FirstIndex, Is.EqualTo(2));
            Assert.That(_breaches.Breaches[0].LastIndex, Is.EqualTo(5));
        }

        [Test]
        public void Tape_JustOutsideTheStretch_DoesNothing()
        {
            for (var particle = 2; particle <= 5; particle++)
                Heat(particle, 0.8f);
            Run(1.5f, 0.5f);

            Assert.That(_breaches.Tape(6), Is.False);
            Assert.That(_breaches.Breaches[0].IsTaped, Is.False);
        }

        [Test]
        public void TapedStretch_BurnsOpenAgainWhenAnyPartOfItIsHot()
        {
            for (var particle = 2; particle <= 5; particle++)
                Heat(particle, 0.8f);
            Run(1.5f, 0.5f);
            _breaches.Tape(3);
            for (var particle = 2; particle <= 5; particle++)
                Heat(particle, 0f);
            Heat(4, 0.8f);

            Run(1.5f, 0.5f);

            Assert.That(_breaches.Breaches.Count, Is.EqualTo(1));
            Assert.That(_breaches.Breaches[0].IsTaped, Is.False);
            Assert.That(_breaches.Breaches[0].FirstIndex, Is.EqualTo(2));
            Assert.That(_breaches.Breaches[0].LastIndex, Is.EqualTo(5));
            Assert.That(_burned.Count, Is.EqualTo(2));
        }
    }
}
