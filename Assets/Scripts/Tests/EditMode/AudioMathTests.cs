using System;
using FlameBlame.Audio.Simulation;
using NUnit.Framework;

namespace FlameBlame.Tests.EditMode
{
    public class AudioMathTests
    {
        private const int Takes = 4;
        private const int Draws = 500;

        [Test]
        public void ToDecibels_ZeroIsSilent()
        {
            Assert.That(VolumeCurve.ToDecibels(0f), Is.EqualTo(VolumeCurve.SilentDecibels));
        }

        [Test]
        public void ToDecibels_FullIsUnity()
        {
            Assert.That(VolumeCurve.ToDecibels(1f), Is.EqualTo(0f).Within(0.0001f));
        }

        [Test]
        public void ToDecibels_HalfSliderIsSquaredGain()
        {
            Assert.That(VolumeCurve.ToDecibels(0.5f), Is.EqualTo(-12.04f).Within(0.01f));
        }

        [Test]
        public void ToDecibels_ClampsOutOfRange()
        {
            Assert.That(VolumeCurve.ToDecibels(2f), Is.EqualTo(0f).Within(0.0001f));
            Assert.That(VolumeCurve.ToDecibels(-1f), Is.EqualTo(VolumeCurve.SilentDecibels));
        }

        [Test]
        public void Pick_NeverRepeatsLastTake()
        {
            var random = new Random(7);
            var last = TakePicker.NoTake;
            for (var draw = 0; draw < Draws; draw++)
            {
                var take = TakePicker.Pick(Takes, last, random);
                Assert.That(take, Is.InRange(0, Takes - 1));
                Assert.That(take, Is.Not.EqualTo(last));
                last = take;
            }
        }

        [Test]
        public void Pick_ReachesEveryTake()
        {
            var random = new Random(7);
            var seen = new bool[Takes];
            var last = TakePicker.NoTake;
            for (var draw = 0; draw < Draws; draw++)
            {
                last = TakePicker.Pick(Takes, last, random);
                seen[last] = true;
            }

            Assert.That(seen, Is.All.True);
        }

        [Test]
        public void Pick_SingleTakeRepeats()
        {
            Assert.That(TakePicker.Pick(1, 0, new Random(7)), Is.EqualTo(0));
        }

        [Test]
        public void Draw_DealsEveryTakeOncePerRound()
        {
            var random = new Random(7);
            var bag = new ShuffleBag(Takes);
            for (var round = 0; round < Draws / Takes; round++)
            {
                var seen = new bool[Takes];
                for (var draw = 0; draw < Takes; draw++)
                    seen[bag.Draw(random)] = true;

                Assert.That(seen, Is.All.True);
            }
        }

        [Test]
        public void Draw_NeverRepeatsAcrossRounds()
        {
            var random = new Random(7);
            var bag = new ShuffleBag(Takes);
            var last = bag.Draw(random);
            for (var draw = 1; draw < Draws; draw++)
            {
                var take = bag.Draw(random);
                Assert.That(take, Is.Not.EqualTo(last));
                last = take;
            }
        }

        [Test]
        public void Draw_SingleTakeRepeats()
        {
            var bag = new ShuffleBag(1);
            Assert.That(bag.Draw(new Random(7)), Is.EqualTo(0));
            Assert.That(bag.Draw(new Random(7)), Is.EqualTo(0));
        }
    }
}
