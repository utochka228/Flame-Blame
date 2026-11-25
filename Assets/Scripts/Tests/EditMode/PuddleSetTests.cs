using FlameBlame.Water.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace FlameBlame.Tests.EditMode
{
    public class PuddleSetTests
    {
        private const float RadiusPerSqrtVolume = 0.5f;
        private const float MinTraction = 0.2f;

        private PuddleSet _puddles;

        [SetUp]
        public void SetUp()
        {
            _puddles = NewSet();
        }

        private static PuddleSet NewSet(float mergeRadius = 0f) => new PuddleSet(new PuddleSettings
        {
            Capacity = 3,
            RadiusPerSqrtVolume = RadiusPerSqrtVolume,
            MergeRadius = mergeRadius,
            EvaporationPerSecond = 1f,
            MinTraction = MinTraction
        });

        [Test]
        public void DryGround_HasFullTraction()
        {
            Assert.That(_puddles.Puddles, Is.Empty);
            Assert.That(_puddles.TractionAt(Vector3.zero), Is.EqualTo(1f));
        }

        [Test]
        public void Add_StartsAPuddleSizedByItsVolume()
        {
            _puddles.Add(new Vector3(1f, 0f, 2f), 4f);

            Assert.That(_puddles.Puddles.Count, Is.EqualTo(1));
            Assert.That(_puddles.Puddles[0].Position, Is.EqualTo(new Vector3(1f, 0f, 2f)));
            Assert.That(_puddles.Puddles[0].Volume, Is.EqualTo(4f));
            Assert.That(_puddles.Puddles[0].Radius, Is.EqualTo(1f).Within(1e-4f));
        }

        [Test]
        public void TractionIsLowInsideAPuddleAndFullOutside()
        {
            _puddles.Add(Vector3.zero, 4f);

            Assert.That(_puddles.TractionAt(new Vector3(0.9f, 0f, 0f)), Is.EqualTo(MinTraction));
            Assert.That(_puddles.TractionAt(new Vector3(1.1f, 0f, 0f)), Is.EqualTo(1f));
        }

        [Test]
        public void TractionIgnoresHeight()
        {
            _puddles.Add(Vector3.zero, 4f);

            Assert.That(_puddles.TractionAt(new Vector3(0.5f, 1.7f, 0f)), Is.EqualTo(MinTraction));
        }

        [Test]
        public void Add_InsideAnExistingPuddle_GrowsIt()
        {
            _puddles.Add(Vector3.zero, 4f);

            _puddles.Add(new Vector3(0.5f, 0f, 0f), 5f);

            Assert.That(_puddles.Puddles.Count, Is.EqualTo(1));
            Assert.That(_puddles.Puddles[0].Position, Is.EqualTo(Vector3.zero));
            Assert.That(_puddles.Puddles[0].Volume, Is.EqualTo(9f));
            Assert.That(_puddles.Puddles[0].Radius, Is.EqualTo(1.5f).Within(1e-4f));
        }

        [Test]
        public void Add_OutsideEveryPuddle_StartsAnotherOne()
        {
            _puddles.Add(Vector3.zero, 4f);

            _puddles.Add(new Vector3(3f, 0f, 0f), 1f);

            Assert.That(_puddles.Puddles.Count, Is.EqualTo(2));
        }

        [Test]
        public void Add_AtCapacity_MergesIntoTheNearestPuddle()
        {
            _puddles.Add(new Vector3(0f, 0f, 0f), 1f);
            _puddles.Add(new Vector3(10f, 0f, 0f), 1f);
            _puddles.Add(new Vector3(20f, 0f, 0f), 1f);

            _puddles.Add(new Vector3(12f, 0f, 0f), 2f);

            Assert.That(_puddles.Puddles.Count, Is.EqualTo(3));
            Assert.That(_puddles.Puddles[1].Volume, Is.EqualTo(3f));
        }

        [Test]
        public void Add_NoVolume_IsIgnored()
        {
            _puddles.Add(Vector3.zero, 0f);
            _puddles.Add(Vector3.one, -2f);

            Assert.That(_puddles.Puddles, Is.Empty);
        }

        [Test]
        public void Step_EvaporationShrinksThePuddle()
        {
            _puddles.Add(Vector3.zero, 4f);

            _puddles.Step(1f);

            Assert.That(_puddles.Puddles[0].Volume, Is.EqualTo(3f).Within(1e-4f));
            Assert.That(_puddles.Puddles[0].Radius, Is.EqualTo(0.5f * Mathf.Sqrt(3f)).Within(1e-4f));
        }

        [Test]
        public void Step_RemovesPuddlesThatDryUp_RestoringTraction()
        {
            _puddles.Add(Vector3.zero, 1f);
            _puddles.Add(new Vector3(10f, 0f, 0f), 5f);

            _puddles.Step(1.5f);

            Assert.That(_puddles.Puddles.Count, Is.EqualTo(1));
            Assert.That(_puddles.TractionAt(Vector3.zero), Is.EqualTo(1f));
            Assert.That(_puddles.TractionAt(new Vector3(10f, 0f, 0f)), Is.EqualTo(MinTraction));
        }

        [Test]
        public void Add_NearATinyPuddle_MergesWithinTheMergeRadius()
        {
            var puddles = NewSet(0.3f);
            puddles.Add(Vector3.zero, 0.01f);

            puddles.Add(new Vector3(0.25f, 0f, 0f), 0.01f);

            Assert.That(puddles.Puddles.Count, Is.EqualTo(1));
            Assert.That(puddles.Puddles[0].Volume, Is.EqualTo(0.02f).Within(1e-5f));
        }
    }
}
