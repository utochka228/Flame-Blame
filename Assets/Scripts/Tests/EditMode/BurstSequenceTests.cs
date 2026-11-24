using FlameBlame.Hose.Simulation;
using NUnit.Framework;

namespace FlameBlame.Tests.EditMode
{
    public class BurstSequenceTests
    {
        private const float OverloadSeconds = 2f;
        private const float RecoverSeconds = 1f;
        private const float Step = 0.1f;
        private const float Tolerance = 1e-3f;

        private BurstSequence _sequence;
        private int _popCount;

        [SetUp]
        public void SetUp()
        {
            _popCount = 0;
            _sequence = new BurstSequence(OverloadSeconds, RecoverSeconds);
            _sequence.Popped += () => _popCount++;
        }

        private void Run(float pressureFraction, float seconds)
        {
            var stepCount = (int)(seconds / Step + 0.5f);
            for (var index = 0; index < stepCount; index++)
                _sequence.Update(pressureFraction, Step);
        }

        [Test]
        public void PressureAtOrBelowMax_NeverBursts()
        {
            Run(1f, 30f);

            Assert.IsFalse(_sequence.HasBurst);
            Assert.AreEqual(0f, _sequence.ProgressFraction, Tolerance);
        }

        [Test]
        public void Overpressure_BuildsProgressTowardsBurst()
        {
            Run(1.5f, 1f);

            Assert.AreEqual(0.5f, _sequence.ProgressFraction, Tolerance);
            Assert.IsFalse(_sequence.HasBurst);
        }

        [Test]
        public void SustainedOverpressure_BurstsOnceAndClearsProgress()
        {
            Run(1.5f, 5f);

            Assert.IsTrue(_sequence.HasBurst);
            Assert.AreEqual(1, _popCount);
            Assert.AreEqual(0f, _sequence.ProgressFraction, Tolerance);
        }

        [Test]
        public void DroppingBelowMax_DeflatesProgress()
        {
            Run(1.5f, 1f);

            Run(0.5f, 0.3f);

            Assert.AreEqual(0.2f, _sequence.ProgressFraction, Tolerance);
        }

        [Test]
        public void Progress_NeverGoesBelowZero()
        {
            Run(0.5f, 5f);

            Assert.AreEqual(0f, _sequence.ProgressFraction, Tolerance);
        }

        [Test]
        public void AfterBursting_StaysBurstWhateverThePressure()
        {
            Run(1.5f, 5f);

            Run(0f, 5f);

            Assert.IsTrue(_sequence.HasBurst);
            Assert.AreEqual(1, _popCount);
        }
    }
}
