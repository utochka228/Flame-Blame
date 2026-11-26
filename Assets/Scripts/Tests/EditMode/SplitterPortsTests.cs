using FlameBlame.Hose.Simulation;
using NUnit.Framework;

namespace FlameBlame.Tests.EditMode
{
    public class SplitterPortsTests
    {
        private const int PortCount = 3;

        private static readonly RopeEnd MainTail = new RopeEnd(0, RopeEndKind.Tail);
        private static readonly RopeEnd BranchStart = new RopeEnd(1, RopeEndKind.Start);

        private SplitterPorts _ports;

        [SetUp]
        public void SetUp()
        {
            _ports = new SplitterPorts(PortCount);
        }

        [Test]
        public void NewSplitter_HasNoPluggedPorts()
        {
            Assert.That(_ports.PluggedCount, Is.EqualTo(0));
            Assert.That(_ports.IsPlugged(0), Is.False);
        }

        [Test]
        public void Plug_FreePort_StoresTheEnd()
        {
            Assert.That(_ports.Plug(1, MainTail), Is.True);
            Assert.That(_ports.IsPlugged(1), Is.True);
            Assert.That(_ports.EndAt(1).Rope, Is.EqualTo(MainTail.Rope));
            Assert.That(_ports.EndAt(1).Kind, Is.EqualTo(MainTail.Kind));
            Assert.That(_ports.PluggedCount, Is.EqualTo(1));
        }

        [Test]
        public void Plug_OccupiedPort_IsRefused()
        {
            _ports.Plug(0, MainTail);

            Assert.That(_ports.Plug(0, BranchStart), Is.False);
            Assert.That(_ports.EndAt(0).Rope, Is.EqualTo(MainTail.Rope));
        }

        [Test]
        public void Plug_SameEndIntoSecondPort_IsRefused()
        {
            _ports.Plug(0, MainTail);

            Assert.That(_ports.Plug(1, MainTail), Is.False);
            Assert.That(_ports.PluggedCount, Is.EqualTo(1));
        }

        [Test]
        public void Plug_OtherEndOfSameHose_IsAccepted()
        {
            _ports.Plug(0, MainTail);

            Assert.That(_ports.Plug(1, new RopeEnd(0, RopeEndKind.Start)), Is.True);
        }

        [Test]
        public void Unplug_FreesThePortForReuse()
        {
            _ports.Plug(0, MainTail);
            _ports.Unplug(0);

            Assert.That(_ports.Contains(MainTail), Is.False);
            Assert.That(_ports.Plug(0, BranchStart), Is.True);
        }

        [Test]
        public void Contains_FindsOnlyPluggedEnds()
        {
            _ports.Plug(2, BranchStart);

            Assert.That(_ports.Contains(BranchStart), Is.True);
            Assert.That(_ports.Contains(MainTail), Is.False);
        }
    }
}
