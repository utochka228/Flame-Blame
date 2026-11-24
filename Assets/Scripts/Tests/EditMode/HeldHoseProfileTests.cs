using FlameBlame.Hose.Simulation;
using NUnit.Framework;

namespace FlameBlame.Tests.EditMode
{
    public class HeldHoseProfileTests
    {
        private const int ParticleCount = 11;

        private HeldHoseProfile _profile;
        private float[] _caps;
        private float[] _damping;

        [SetUp]
        public void SetUp()
        {
            _profile = new HeldHoseProfile(new HeldHoseSettings
            {
                UpstreamRigidityFraction = 0.5f,
                FreeRigidityFraction = 0.1f,
                UpstreamDampingMultiplier = 3f,
                FreeDampingMultiplier = 0.5f
            });
            _caps = new float[ParticleCount];
            _damping = new float[ParticleCount];
        }

        [Test]
        public void Apply_HeldAtTip_WholeHoseIsUpstream()
        {
            _profile.Apply(ParticleCount - 1, _caps, _damping);

            Assert.That(_caps, Is.All.EqualTo(0.5f));
            Assert.That(_damping, Is.All.EqualTo(3f));
        }

        [Test]
        public void Apply_HeldMidHose_UpstreamIsStableAndTailIsFreer()
        {
            _profile.Apply(4, _caps, _damping);

            Assert.AreEqual(0.5f, _caps[0]);
            Assert.AreEqual(0.5f, _caps[4]);
            Assert.Less(_caps[ParticleCount - 1], _caps[4]);
            Assert.Less(_damping[ParticleCount - 1], _damping[4]);
        }

        [Test]
        public void Apply_TailEase_IsMonotonicTowardTheTip()
        {
            _profile.Apply(4, _caps, _damping);

            for (var index = 5; index < ParticleCount; index++)
                Assert.LessOrEqual(_caps[index], _caps[index - 1]);
        }

        [Test]
        public void Apply_LongerFreeTail_GivesAFloppierTip()
        {
            var shortTail = new float[ParticleCount];
            _profile.Apply(8, shortTail, new float[ParticleCount]);
            _profile.Apply(2, _caps, _damping);

            Assert.Less(_caps[ParticleCount - 1], shortTail[ParticleCount - 1]);
        }

        [Test]
        public void Clear_RestoresAFullyUnrestrictedHose()
        {
            _profile.Apply(4, _caps, _damping);

            HeldHoseProfile.Clear(_caps, _damping);

            Assert.That(_caps, Is.All.EqualTo(1f));
            Assert.That(_damping, Is.All.EqualTo(1f));
        }
    }
}
