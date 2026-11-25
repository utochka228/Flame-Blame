using FlameBlame.Level.Simulation;
using NUnit.Framework;

namespace FlameBlame.Tests.EditMode
{
    public class LevelProgressTests
    {
        private const int TotalFires = 4;

        private LevelProgress _progress;
        private int _completedCount;

        [SetUp]
        public void SetUp()
        {
            _progress = new LevelProgress(TotalFires);
            _completedCount = 0;
            _progress.Completed += () => _completedCount++;
        }

        [Test]
        public void NewLevel_HasEveryFireLeft_AndIsNotComplete()
        {
            Assert.That(_progress.FiresLeft, Is.EqualTo(TotalFires));
            Assert.That(_progress.IsComplete, Is.False);
        }

        [Test]
        public void Report_CountsFiresLeft()
        {
            _progress.Report(3);

            Assert.That(_progress.FiresLeft, Is.EqualTo(1));
            Assert.That(_progress.IsComplete, Is.False);
            Assert.That(_completedCount, Is.EqualTo(0));
        }

        [Test]
        public void Report_AllFiresOut_CompletesOnce()
        {
            _progress.Report(TotalFires);
            _progress.Report(TotalFires);

            Assert.That(_progress.FiresLeft, Is.EqualTo(0));
            Assert.That(_progress.IsComplete, Is.True);
            Assert.That(_completedCount, Is.EqualTo(1));
        }

        [Test]
        public void Report_MoreThanTheTotal_NeverGoesNegative()
        {
            _progress.Report(TotalFires + 3);

            Assert.That(_progress.FiresLeft, Is.EqualTo(0));
        }

        [Test]
        public void LevelWithNoFires_IsNeverComplete()
        {
            var empty = new LevelProgress(0);

            empty.Report(0);

            Assert.That(empty.IsComplete, Is.False);
        }
    }
}
