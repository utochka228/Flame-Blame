using FlameBlame.Hose.Simulation;
using NUnit.Framework;

namespace FlameBlame.Tests.EditMode
{
    public class BurstRepairTests
    {
        private const float OverloadSeconds = 1f;
        private const float RecoverSeconds = 1f;
        private const float Step = 0.1f;

        private BurstSequence _sequence;

        [SetUp]
        public void SetUp()
        {
            _sequence = new BurstSequence(OverloadSeconds, RecoverSeconds);
        }

        private void Overload(float seconds)
        {
            for (var index = 0; index < (int)(seconds / Step + 0.5f); index++)
                _sequence.Update(1.5f, Step);
        }

        [Test]
        public void Repair_ClearsTheBurst()
        {
            Overload(2f);
            Assert.IsTrue(_sequence.HasBurst);

            _sequence.Repair();

            Assert.IsFalse(_sequence.HasBurst);
            Assert.AreEqual(0f, _sequence.ProgressFraction);
        }

        [Test]
        public void Repair_AllowsBurstingAgainUnderOverload()
        {
            var popCount = 0;
            _sequence.Popped += () => popCount++;
            Overload(2f);
            _sequence.Repair();

            Overload(2f);

            Assert.AreEqual(2, popCount);
        }
    }
}
