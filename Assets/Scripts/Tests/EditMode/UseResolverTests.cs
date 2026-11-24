using FlameBlame.Firefighter.Simulation;
using NUnit.Framework;

namespace FlameBlame.Tests.EditMode
{
    public class UseResolverTests
    {
        [Test]
        public void Resolve_NothingInReach_IsNone()
        {
            Assert.AreEqual(UseAction.None, UseResolver.Resolve(new UseContext(false, false, false, false)));
        }

        [TestCase(true, false, false, false, UseAction.Connect)]
        [TestCase(false, true, false, false, UseAction.Tape)]
        [TestCase(false, false, true, false, UseAction.Disconnect)]
        [TestCase(false, false, false, true, UseAction.ToggleValve)]
        public void Resolve_SingleOption_PicksIt(bool connect, bool burst, bool joint, bool valve, UseAction expected)
        {
            Assert.AreEqual(expected, UseResolver.Resolve(new UseContext(connect, burst, joint, valve)));
        }

        [Test]
        public void Resolve_PrefersConnectThenTapeThenDisconnectThenValve()
        {
            Assert.AreEqual(UseAction.Connect, UseResolver.Resolve(new UseContext(true, true, true, true)));
            Assert.AreEqual(UseAction.Tape, UseResolver.Resolve(new UseContext(false, true, true, true)));
            Assert.AreEqual(UseAction.Disconnect, UseResolver.Resolve(new UseContext(false, false, true, true)));
        }
    }
}
