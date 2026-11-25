using FlameBlame.Hose.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace FlameBlame.Tests.EditMode
{
    public class HeatSourcesTests
    {
        private HeatSources _sources;

        private class FixedHeat : IHeatSource
        {
            private readonly float _heat;

            public FixedHeat(float heat)
            {
                _heat = heat;
            }

            public float HeatAt(Vector3 point) => _heat;
        }

        [SetUp]
        public void SetUp()
        {
            _sources = new HeatSources();
        }

        [Test]
        public void NoSources_MeansNoHeat()
        {
            Assert.That(_sources.Sample(Vector3.zero), Is.EqualTo(0f));
        }

        [Test]
        public void Sample_SumsEverySource()
        {
            _sources.Register(new FixedHeat(0.25f));
            _sources.Register(new FixedHeat(0.5f));

            Assert.That(_sources.Sample(Vector3.zero), Is.EqualTo(0.75f).Within(1e-5f));
        }

        [Test]
        public void Sample_IsClampedToOne()
        {
            _sources.Register(new FixedHeat(0.8f));
            _sources.Register(new FixedHeat(0.8f));

            Assert.That(_sources.Sample(Vector3.zero), Is.EqualTo(1f));
        }

        [Test]
        public void Unregister_StopsASourceCounting()
        {
            var source = new FixedHeat(0.5f);
            _sources.Register(source);

            _sources.Unregister(source);

            Assert.That(_sources.Sample(Vector3.zero), Is.EqualTo(0f));
        }

        [Test]
        public void Register_TheSameSourceTwice_CountsItOnce()
        {
            var source = new FixedHeat(0.3f);
            _sources.Register(source);
            _sources.Register(source);

            Assert.That(_sources.Sample(Vector3.zero), Is.EqualTo(0.3f).Within(1e-5f));
        }
    }
}
