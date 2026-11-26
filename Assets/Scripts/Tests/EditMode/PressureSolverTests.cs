using FlameBlame.Pressure.Simulation;
using NUnit.Framework;

namespace FlameBlame.Tests.EditMode
{
    public class PressureSolverTests
    {
        private const float Tolerance = 1e-4f;

        private readonly PressureGraph _graph = new PressureGraph();
        private readonly PressureResult _result = new PressureResult();

        [SetUp]
        public void SetUp()
        {
            _graph.Clear();
        }

        [Test]
        public void Hose_LosesPressureByLength()
        {
            var source = _graph.AddSource(100f);
            var hose = _graph.AddHose(10f, 2f);
            _graph.Link(source, PressureGraph.SourceOutlet, hose, PressureGraph.HoseStart);

            PressureSolver.Solve(_graph, _result);

            Assert.AreEqual(80f, _result.TipPressure(hose), Tolerance);
        }

        [Test]
        public void ChainedHoses_AccumulateLoss()
        {
            var source = _graph.AddSource(100f);
            var first = _graph.AddHose(10f, 2f);
            var second = _graph.AddHose(10f, 1f);
            _graph.Link(source, PressureGraph.SourceOutlet, first, PressureGraph.HoseStart);
            _graph.Link(first, PressureGraph.HoseEnd, second, PressureGraph.HoseStart);

            PressureSolver.Solve(_graph, _result);

            Assert.AreEqual(70f, _result.TipPressure(second), Tolerance);
        }

        [Test]
        public void ReversedHose_StillCarriesPressureToItsFreeEnd()
        {
            var source = _graph.AddSource(100f);
            var hose = _graph.AddHose(10f, 2f);
            _graph.Link(source, PressureGraph.SourceOutlet, hose, PressureGraph.HoseEnd);

            PressureSolver.Solve(_graph, _result);

            Assert.AreEqual(80f, _result.TipPressure(hose), Tolerance);
        }

        [Test]
        public void DisconnectedHose_HasNoPressure()
        {
            _graph.AddSource(100f);
            var hose = _graph.AddHose(10f, 2f);

            PressureSolver.Solve(_graph, _result);

            Assert.AreEqual(0f, _result.TipPressure(hose), Tolerance);
        }

        [Test]
        public void ClosedValve_GivesNoPressure()
        {
            var source = _graph.AddSource(0f);
            var hose = _graph.AddHose(10f, 2f);
            _graph.Link(source, PressureGraph.SourceOutlet, hose, PressureGraph.HoseStart);

            PressureSolver.Solve(_graph, _result);

            Assert.AreEqual(0f, _result.TipPressure(hose), Tolerance);
        }

        [Test]
        public void LoopedHoses_TerminateAndKeepFirstVisit()
        {
            var source = _graph.AddSource(100f);
            var first = _graph.AddHose(10f, 2f);
            var second = _graph.AddHose(10f, 1f);
            _graph.Link(source, PressureGraph.SourceOutlet, first, PressureGraph.HoseStart);
            _graph.Link(first, PressureGraph.HoseEnd, second, PressureGraph.HoseStart);
            _graph.Link(second, PressureGraph.HoseEnd, first, PressureGraph.HoseStart);

            PressureSolver.Solve(_graph, _result);

            Assert.AreEqual(80f, _result.TipPressure(first), Tolerance);
        }

        [Test]
        public void Splitter_DividesPressureAmongConnectedOutputs()
        {
            var source = _graph.AddSource(100f);
            var feed = _graph.AddHose(10f, 2f);
            var splitter = _graph.AddSplitter();
            var left = _graph.AddHose(5f, 2f);
            var right = _graph.AddHose(5f, 2f);
            _graph.Link(source, PressureGraph.SourceOutlet, feed, PressureGraph.HoseStart);
            _graph.Link(feed, PressureGraph.HoseEnd, splitter, 0);
            _graph.Link(splitter, 1, left, PressureGraph.HoseStart);
            _graph.Link(splitter, 2, right, PressureGraph.HoseStart);

            PressureSolver.Solve(_graph, _result);

            Assert.AreEqual(30f, _result.TipPressure(left), Tolerance);
            Assert.AreEqual(30f, _result.TipPressure(right), Tolerance);
        }

        [Test]
        public void Splitter_WithOneConnectedOutput_PassesFullPressure()
        {
            var source = _graph.AddSource(100f);
            var splitter = _graph.AddSplitter();
            var branch = _graph.AddHose(10f, 2f);
            _graph.Link(source, PressureGraph.SourceOutlet, splitter, 0);
            _graph.Link(splitter, 1, branch, PressureGraph.HoseStart);

            PressureSolver.Solve(_graph, _result);

            Assert.AreEqual(80f, _result.TipPressure(branch), Tolerance);
        }

        [Test]
        public void Splitter_EnteredThroughAnyPort_DividesAmongTheOthers()
        {
            var source = _graph.AddSource(100f);
            var splitter = _graph.AddSplitter();
            var left = _graph.AddHose(10f, 0f);
            var right = _graph.AddHose(10f, 0f);
            _graph.Link(source, PressureGraph.SourceOutlet, splitter, 2);
            _graph.Link(splitter, 0, left, PressureGraph.HoseStart);
            _graph.Link(splitter, 1, right, PressureGraph.HoseStart);

            PressureSolver.Solve(_graph, _result);

            Assert.AreEqual(50f, _result.TipPressure(left), Tolerance);
        }

        [Test]
        public void ChainedSplitters_QuarterThePressure()
        {
            var source = _graph.AddSource(100f);
            var first = _graph.AddSplitter();
            var second = _graph.AddSplitter();
            var direct = _graph.AddHose(10f, 0f);
            var nested = _graph.AddHose(10f, 0f);
            var other = _graph.AddHose(10f, 0f);
            _graph.Link(source, PressureGraph.SourceOutlet, first, 0);
            _graph.Link(first, 1, direct, PressureGraph.HoseStart);
            _graph.Link(first, 2, second, 0);
            _graph.Link(second, 1, nested, PressureGraph.HoseStart);
            _graph.Link(second, 2, other, PressureGraph.HoseStart);

            PressureSolver.Solve(_graph, _result);

            Assert.AreEqual(50f, _result.TipPressure(direct), Tolerance);
            Assert.AreEqual(25f, _result.TipPressure(nested), Tolerance);
        }

        [Test]
        public void Pinch_ZeroesPressureBeyondIt()
        {
            var hose = ConnectedHose(10f, 2f);
            _graph.Pinch(hose, 0.5f);

            PressureSolver.Solve(_graph, _result);

            Assert.AreEqual(95f, _result.Sample(hose, 0.25f), Tolerance);
            Assert.AreEqual(0f, _result.Sample(hose, 0.75f), Tolerance);
            Assert.AreEqual(0f, _result.TipPressure(hose), Tolerance);
        }

        [Test]
        public void Pinch_OnReversedHose_CountsFromTheInletEnd()
        {
            var source = _graph.AddSource(100f);
            var hose = _graph.AddHose(10f, 2f);
            _graph.Link(source, PressureGraph.SourceOutlet, hose, PressureGraph.HoseEnd);
            _graph.Pinch(hose, 0.25f);

            PressureSolver.Solve(_graph, _result);

            Assert.AreEqual(98f, _result.Sample(hose, 0.9f), Tolerance);
            Assert.AreEqual(0f, _result.Sample(hose, 0.1f), Tolerance);
        }

        [Test]
        public void Pinch_UpstreamOfSplitter_ZeroesEveryBranch()
        {
            var source = _graph.AddSource(100f);
            var feed = _graph.AddHose(10f, 0f);
            var splitter = _graph.AddSplitter();
            var left = _graph.AddHose(10f, 0f);
            var right = _graph.AddHose(10f, 0f);
            _graph.Link(source, PressureGraph.SourceOutlet, feed, PressureGraph.HoseStart);
            _graph.Link(feed, PressureGraph.HoseEnd, splitter, 0);
            _graph.Link(splitter, 1, left, PressureGraph.HoseStart);
            _graph.Link(splitter, 2, right, PressureGraph.HoseStart);
            _graph.Pinch(feed, 0.5f);

            PressureSolver.Solve(_graph, _result);

            Assert.AreEqual(0f, _result.TipPressure(left), Tolerance);
            Assert.AreEqual(0f, _result.TipPressure(right), Tolerance);
        }

        [Test]
        public void Leak_SubtractsPressureBeyondIt()
        {
            var hose = ConnectedHose(10f, 2f, 100f);
            _graph.Leak(hose, 0.5f, 30f);

            PressureSolver.Solve(_graph, _result);

            Assert.AreEqual(95f, _result.Sample(hose, 0.25f), Tolerance);
            Assert.AreEqual(55f, _result.Sample(hose, 0.75f), Tolerance);
            Assert.AreEqual(50f, _result.TipPressure(hose), Tolerance);
        }

        [Test]
        public void Thumb_BoostsTipOnly()
        {
            var hose = ConnectedHose(10f, 2f);
            _graph.Thumb(hose, 0.5f);

            PressureSolver.Solve(_graph, _result);

            Assert.AreEqual(120f, _result.TipPressure(hose), Tolerance);
            Assert.AreEqual(80f, _result.Sample(hose, 1f), Tolerance);
        }

        [Test]
        public void PinchRunEdges_OnReversedHose_CutAtTheEdgeNearestTheInlet()
        {
            var source = _graph.AddSource(100f);
            var hose = _graph.AddHose(10f, 0f);
            _graph.Link(source, PressureGraph.SourceOutlet, hose, PressureGraph.HoseEnd);
            _graph.Pinch(hose, 0.25f);
            _graph.Pinch(hose, 0.5f);

            PressureSolver.Solve(_graph, _result);

            Assert.AreEqual(100f, _result.Sample(hose, 0.6f), Tolerance);
            Assert.AreEqual(0f, _result.Sample(hose, 0.45f), Tolerance);
        }

        [Test]
        public void PinchSurge_RaisesPressureBeforeIt_AndKeepsZeroBeyondIt()
        {
            var hose = ConnectedHose(10f, 2f);
            _graph.Pinch(hose, 0.5f, 0.4f);

            PressureSolver.Solve(_graph, _result);

            Assert.AreEqual(95f * 1.4f, _result.Sample(hose, 0.25f), Tolerance);
            Assert.AreEqual(0f, _result.Sample(hose, 0.75f), Tolerance);
        }

        [Test]
        public void PinchSurge_OnReversedHose_RaisesTheInletSide()
        {
            var source = _graph.AddSource(100f);
            var hose = _graph.AddHose(10f, 2f);
            _graph.Link(source, PressureGraph.SourceOutlet, hose, PressureGraph.HoseEnd);
            _graph.Pinch(hose, 0.25f, 0.5f);

            PressureSolver.Solve(_graph, _result);

            Assert.AreEqual(98f * 1.5f, _result.Sample(hose, 0.9f), Tolerance);
            Assert.AreEqual(0f, _result.Sample(hose, 0.1f), Tolerance);
        }

        [Test]
        public void PinchSurge_LeavesOtherHosesUnchanged()
        {
            var pinched = ConnectedHose(10f, 0f);
            var free = ConnectedHose(10f, 0f);
            _graph.Pinch(pinched, 0.5f, 0.4f);

            PressureSolver.Solve(_graph, _result);

            Assert.AreEqual(100f, _result.Sample(free, 0.25f), Tolerance);
        }

        private int ConnectedHose(float length, float lossPerMeter, float sourcePressure = 100f)
        {
            var source = _graph.AddSource(sourcePressure);
            var hose = _graph.AddHose(length, lossPerMeter);
            _graph.Link(source, PressureGraph.SourceOutlet, hose, PressureGraph.HoseStart);
            return hose;
        }
    }
}
