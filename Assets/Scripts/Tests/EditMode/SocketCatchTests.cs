using FlameBlame.Firefighter.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace FlameBlame.Tests.EditMode
{
    public class SocketCatchTests
    {
        private static readonly Vector3[] Line =
        {
            new Vector3(0f, 0f, 0f),
            new Vector3(1f, 0f, 0f),
            new Vector3(2f, 0f, 0f),
            new Vector3(3f, 0f, 0f)
        };

        [TestCase(0, CatchParts.Start)]
        [TestCase(1, CatchParts.Middle)]
        [TestCase(2, CatchParts.Middle)]
        [TestCase(3, CatchParts.Tail)]
        public void PartOf_ClassifiesEndsAndMiddle(int index, CatchParts expected)
        {
            Assert.AreEqual(expected, SocketCatch.PartOf(index, Line.Length));
        }

        [Test]
        public void Closest_AnyPart_PicksNearestInsideRadius()
        {
            Assert.AreEqual(2, SocketCatch.Closest(Line, CatchParts.All, new Vector3(2.2f, 0f, 0f), 0.5f));
        }

        [Test]
        public void Closest_EndsOnly_RejectsMiddleParticles()
        {
            Assert.AreEqual(SocketCatch.NoParticle, SocketCatch.Closest(Line, CatchParts.Ends, new Vector3(1.5f, 0f, 0f), 0.6f));
            Assert.AreEqual(3, SocketCatch.Closest(Line, CatchParts.Ends, new Vector3(2.6f, 0f, 0f), 0.6f));
        }

        [Test]
        public void Closest_NothingInsideRadius_IsNone()
        {
            Assert.AreEqual(SocketCatch.NoParticle, SocketCatch.Closest(Line, CatchParts.All, new Vector3(1.5f, 2f, 0f), 1f));
        }

        [Test]
        public void CatchWindows_ExpireAfterTheirSeconds()
        {
            var windows = new CatchWindows();
            windows.Arm(4, 1f);

            windows.Step(0.6f);
            CollectionAssert.AreEqual(new[] { 4 }, windows.OpenRopes);

            windows.Step(0.6f);
            CollectionAssert.IsEmpty(windows.OpenRopes);
        }

        [Test]
        public void CatchWindows_RearmRestartsAndCloseRemoves()
        {
            var windows = new CatchWindows();
            windows.Arm(4, 1f);
            windows.Step(0.8f);
            windows.Arm(4, 1f);
            windows.Step(0.8f);
            CollectionAssert.AreEqual(new[] { 4 }, windows.OpenRopes);

            windows.Close(4);
            CollectionAssert.IsEmpty(windows.OpenRopes);
        }
    }
}
