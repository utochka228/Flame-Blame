using FlameBlame.Hose.Simulation;
using NUnit.Framework;

namespace FlameBlame.Tests.EditMode
{
    public class NozzleModelTests
    {
        private const float MaxJetLength = 6f;
        private const float BaseSpread = 6f;
        private const float ThumbSpread = 40f;
        private const float Recoil = 25f;

        private NozzleModel _model;

        [SetUp]
        public void SetUp()
        {
            _model = new NozzleModel(MaxJetLength, BaseSpread, ThumbSpread, Recoil);
        }

        [Test]
        public void Evaluate_NoPressure_NoFlowNoRecoil()
        {
            var output = _model.Evaluate(0f, 0f);

            Assert.AreEqual(0f, output.FlowFraction);
            Assert.AreEqual(0f, output.JetLength);
            Assert.AreEqual(0f, output.RecoilAcceleration);
        }

        [Test]
        public void Evaluate_JetLengthAndRecoil_GrowWithPressure()
        {
            var low = _model.Evaluate(0.25f, 0f);
            var high = _model.Evaluate(1f, 0f);

            Assert.AreEqual(MaxJetLength, high.JetLength);
            Assert.Greater(high.JetLength, low.JetLength);
            Assert.Greater(high.RecoilAcceleration, low.RecoilAcceleration);
        }

        [Test]
        public void Evaluate_OverPressure_CapsJetButKeepsRecoilGrowing()
        {
            var output = _model.Evaluate(1.5f, 0f);

            Assert.AreEqual(MaxJetLength, output.JetLength);
            Assert.AreEqual(Recoil * 1.5f, output.RecoilAcceleration, 1e-4f);
        }

        [Test]
        public void Evaluate_Thumb_WidensTheFan()
        {
            Assert.AreEqual(BaseSpread, _model.Evaluate(1f, 0f).SpreadDegrees);
            Assert.AreEqual(ThumbSpread, _model.Evaluate(1f, 1f).SpreadDegrees);
        }
    }
}
