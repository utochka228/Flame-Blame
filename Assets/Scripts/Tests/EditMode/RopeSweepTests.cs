using System.Collections.Generic;
using FlameBlame.Hose.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace FlameBlame.Tests.EditMode
{
    public class RopeSweepTests
    {
        private const float WallX = 1f;
        private const float Radius = 0.05f;
        private const float FixedStep = 1f / 60f;

        private class ThinWallCollider : IRopeCollider
        {
            public void Prepare(IReadOnlyList<Vector3> positions, float radius)
            {
            }

            public Vector3 Sweep(Vector3 from, Vector3 to, float radius)
            {
                var limit = WallX - radius;
                if (from.x <= limit && to.x > limit)
                    to.x = limit;

                return to;
            }

            public Vector3 Resolve(Vector3 position, Vector3 lastFreePosition, float radius)
            {
                if (Mathf.Abs(position.x - WallX) < radius)
                    position.x = WallX + Mathf.Sign(position.x - WallX) * radius;

                return position;
            }
        }

        [Test]
        public void Step_FastParticles_DoNotTunnelThroughAThinWall()
        {
            var settings = new RopeSettings { ParticleCount = 6, Substeps = 1, MaxSpeed = 500f, Gravity = Vector3.zero, ParticleRadius = Radius };
            var rope = new XpbdRope(settings, Vector3.zero, Vector3.up, new ThinWallCollider()) { IsPinned = false };
            for (var index = 0; index < settings.ParticleCount; index++)
                rope.AddVelocity(index, new Vector3(100f, 0f, 0f));

            rope.Step(FixedStep);

            foreach (var position in rope.Positions)
                Assert.LessOrEqual(position.x, WallX, "A particle crossed the wall");
        }
    }
}
