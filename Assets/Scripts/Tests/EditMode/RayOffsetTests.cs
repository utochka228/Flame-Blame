using FlameBlame.Core.Extensions;
using NUnit.Framework;
using UnityEngine;

namespace FlameBlame.Tests.EditMode
{
    public class RayOffsetTests
    {
        [Test]
        public void OffsetFromRay_PointOnTheRay_IsZero()
        {
            Assert.AreEqual(0f, new Vector3(0f, 0f, 2f).OffsetFromRay(Vector3.zero, Vector3.forward, 5f), 1e-5f);
        }

        [Test]
        public void OffsetFromRay_PointBesideTheRay_IsThePerpendicularDistance()
        {
            Assert.AreEqual(1.5f, new Vector3(1.5f, 0f, 2f).OffsetFromRay(Vector3.zero, Vector3.forward, 5f), 1e-5f);
        }

        [Test]
        public void OffsetFromRay_BehindOrBeyondReach_IsOutOfRange()
        {
            Assert.AreEqual(float.MaxValue, new Vector3(0f, 0f, -1f).OffsetFromRay(Vector3.zero, Vector3.forward, 5f));
            Assert.AreEqual(float.MaxValue, new Vector3(0f, 0f, 6f).OffsetFromRay(Vector3.zero, Vector3.forward, 5f));
        }
    }
}
