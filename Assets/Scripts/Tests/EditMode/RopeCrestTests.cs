using FlameBlame.Firefighter.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace FlameBlame.Tests.EditMode
{
    public class RopeCrestTests
    {
        private static Vector3[] RopeOverEdge()
        {
            return new[]
            {
                new Vector3(0f, 0f, 0f), new Vector3(0f, 1f, 0f), new Vector3(0f, 2f, 0f), new Vector3(0f, 3f, 0f),
                new Vector3(1f, 2.9f, 0f), new Vector3(2f, 1.9f, 0f), new Vector3(2f, 0.9f, 0f)
            };
        }

        [Test]
        public void Find_FromTheClimbSide_ReturnsTheEdge()
        {
            Assert.AreEqual(3, RopeCrest.Find(RopeOverEdge(), 1));
        }

        [Test]
        public void Find_FromTheFarSide_ReturnsTheEdgeWithinTheRiseTolerance()
        {
            Assert.That(RopeCrest.Find(RopeOverEdge(), 6), Is.InRange(3, 4));
        }

        [Test]
        public void Find_AtThePeak_ReturnsThePeak()
        {
            Assert.AreEqual(3, RopeCrest.Find(RopeOverEdge(), 3));
        }
    }
}
