using System.Collections.Generic;
using FlameBlame.Core.Extensions;
using FlameBlame.Hose.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace FlameBlame.Tests.EditMode
{
    public class TubeMeshBuilderTests
    {
        private const int Samples = 4;
        private const int Sides = 12;
        private const float MiterLimit = 1.5f;
        private const float Radius = 0.05f;
        private const float Spacing = 0.2f;

        private static TubeMeshBuilder CreateBuilder(int particleCount) => new TubeMeshBuilder(particleCount, Samples, Sides, MiterLimit);

        private static Vector3[] StraightLine(int count)
        {
            var points = new Vector3[count];
            for (var index = 0; index < count; index++)
                points[index] = new Vector3(index * Spacing, 0f, 0f);
            return points;
        }

        private static Vector3[] RightAngle()
        {
            var points = new List<Vector3>();
            for (var index = 0; index < 6; index++)
                points.Add(new Vector3(index * Spacing, 0f, 0f));
            for (var index = 1; index < 6; index++)
                points.Add(new Vector3(5 * Spacing, index * Spacing, 0f));
            return points.ToArray();
        }

        private static bool AllFinite(IReadOnlyList<Vector3> vectors)
        {
            foreach (var vector in vectors)
            {
                if (!vector.IsFinite()) return false;
            }

            return true;
        }

        [Test]
        public void Build_ProducesExpectedBufferSizes()
        {
            var builder = CreateBuilder(10);

            builder.Build(StraightLine(10), Radius);

            var ringCount = 9 * Samples + 1;
            Assert.AreEqual(ringCount * Sides, builder.Vertices.Length);
            Assert.AreEqual(builder.Vertices.Length, builder.Normals.Length);
            Assert.AreEqual((ringCount - 1) * Sides * 6 + (Sides - 2) * 3 * 2, builder.Triangles.Length);
        }

        [Test]
        public void Build_TrianglesFaceOutward()
        {
            var builder = CreateBuilder(10);

            builder.Build(StraightLine(10), Radius);

            var sideTriangleLength = (9 * Samples) * Sides * 6;
            for (var index = 0; index < sideTriangleLength; index += 3)
            {
                var first = builder.Vertices[builder.Triangles[index]];
                var second = builder.Vertices[builder.Triangles[index + 1]];
                var third = builder.Vertices[builder.Triangles[index + 2]];
                var faceNormal = Vector3.Cross(second - first, third - first);
                var radial = (first + second + third) / 3f;
                radial.x = 0f;
                Assert.Greater(Vector3.Dot(faceNormal, radial), 0f, "Triangle {0} faces inward", index / 3);
            }
        }

        [Test]
        public void Build_EndCapsFaceAlongTheTube()
        {
            var builder = CreateBuilder(10);

            builder.Build(StraightLine(10), Radius);

            var capStart = (9 * Samples) * Sides * 6;
            var capTriangleLength = (Sides - 2) * 3;
            for (var index = 0; index < capTriangleLength * 2; index += 3)
            {
                var first = builder.Vertices[builder.Triangles[capStart + index]];
                var second = builder.Vertices[builder.Triangles[capStart + index + 1]];
                var third = builder.Vertices[builder.Triangles[capStart + index + 2]];
                var faceNormal = Vector3.Cross(second - first, third - first);
                var expected = index / 3 % 2 == 0 ? -1f : 1f;
                Assert.Greater(faceNormal.x * expected, 0f, "Cap triangle {0} faces the wrong way", index / 3);
            }
        }

        [Test]
        public void Build_NormalsAreUnitLength()
        {
            var builder = CreateBuilder(11);

            builder.Build(RightAngle(), Radius);

            foreach (var normal in builder.Normals)
                Assert.AreEqual(1f, normal.magnitude, 1e-3f);
        }

        [Test]
        public void Build_HairpinStaysFinite()
        {
            var points = new List<Vector3>();
            for (var index = 0; index < 6; index++)
                points.Add(new Vector3(index * Spacing, 0f, 0f));
            for (var index = 4; index >= 0; index--)
                points.Add(new Vector3(index * Spacing, 0.01f, 0f));
            var builder = CreateBuilder(points.Count);

            builder.Build(points, Radius);

            Assert.IsTrue(AllFinite(builder.Vertices));
            Assert.IsTrue(AllFinite(builder.Normals));
        }

        [Test]
        public void Build_CoincidentPointsStayFinite()
        {
            var points = new Vector3[8];
            var builder = CreateBuilder(points.Length);

            builder.Build(points, Radius);

            Assert.IsTrue(AllFinite(builder.Vertices));
            Assert.IsTrue(AllFinite(builder.Normals));
        }

        [Test]
        public void Build_BendKeepsTubeAtLeastAsThickAsRadius()
        {
            var points = RightAngle();
            var builder = CreateBuilder(points.Length);

            builder.Build(points, Radius);

            foreach (var vertex in builder.Vertices)
            {
                var nearest = float.MaxValue;
                foreach (var center in builder.Centers)
                    nearest = Mathf.Min(nearest, Vector3.Distance(vertex, center));
                Assert.GreaterOrEqual(nearest, Radius * 0.9f);
            }
        }

        [Test]
        public void Build_WithRadiusPerParticle_InterpolatesRingRadius()
        {
            var builder = CreateBuilder(10);
            var radii = new float[10];
            for (var index = 0; index < radii.Length; index++)
                radii[index] = 0.1f + 0.02f * index;

            builder.Build(StraightLine(10), radii);

            var lastRing = 9 * Samples;
            var middleRing = 3 * Samples + 2;
            Assert.AreEqual(0.1f, RingRadius(builder, 0), 1e-3f);
            Assert.AreEqual(0.28f, RingRadius(builder, lastRing), 1e-3f);
            Assert.AreEqual(0.17f, RingRadius(builder, middleRing), 1e-3f);
        }

        private static float RingRadius(TubeMeshBuilder builder, int ring)
        {
            return Vector3.Distance(builder.Centers[ring], builder.Vertices[ring * Sides]);
        }

        [Test]
        public void Uvs_RunFromZeroToOneAlongTheTube()
        {
            var builder = CreateBuilder(5);
            var ringCount = (5 - 1) * Samples + 1;

            Assert.That(builder.Uvs.Length, Is.EqualTo(builder.Vertices.Length));
            Assert.That(builder.Uvs[0].x, Is.EqualTo(0f));
            Assert.That(builder.Uvs[(ringCount / 2) * Sides].x, Is.EqualTo(0.5f).Within(1e-5f));
            Assert.That(builder.Uvs[(ringCount - 1) * Sides].x, Is.EqualTo(1f).Within(1e-5f));
        }

        [Test]
        public void Uvs_GoAroundTheRingInV()
        {
            var builder = CreateBuilder(5);

            Assert.That(builder.Uvs[0].y, Is.EqualTo(0f));
            Assert.That(builder.Uvs[Sides / 2].y, Is.EqualTo(0.5f).Within(1e-5f));
            Assert.That(builder.Uvs[Sides].y, Is.EqualTo(0f));
        }
    }
}
