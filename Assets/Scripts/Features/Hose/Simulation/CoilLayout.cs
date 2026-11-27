using UnityEngine;

namespace FlameBlame.Hose.Simulation
{
    public readonly struct CoilShape
    {
        public float InnerRadius { get; }
        public float Pitch { get; }
        public float Spacing { get; }

        public CoilShape(float innerRadius, float pitch, float spacing)
        {
            InnerRadius = innerRadius;
            Pitch = pitch;
            Spacing = spacing;
        }
    }

    public static class CoilLayout
    {
        public static void Fill(Vector3[] positions, Vector3 center, Vector3 heading, CoilShape shape)
        {
            var radiusPerRadian = shape.Pitch / (2f * Mathf.PI);
            var startAngle = Mathf.Atan2(heading.z, heading.x);
            var angle = 0f;
            for (var index = 0; index < positions.Length; index++)
            {
                var radius = shape.InnerRadius + radiusPerRadian * angle;
                positions[index] = center + new Vector3(Mathf.Cos(startAngle + angle), 0f, Mathf.Sin(startAngle + angle)) * radius;
                angle += StepAngle(radius, radiusPerRadian, shape.Spacing);
            }
        }

        public static float OuterRadius(int particleCount, CoilShape shape)
        {
            var length = (particleCount - 1) * shape.Spacing;
            return Mathf.Sqrt(shape.InnerRadius * shape.InnerRadius + length * shape.Pitch / Mathf.PI);
        }

        private static float StepAngle(float radius, float radiusPerRadian, float spacing)
        {
            const int Refinements = 3;
            var step = 2f * Mathf.Asin(Mathf.Min(1f, spacing / (2f * radius)));
            for (var refinement = 0; refinement < Refinements; refinement++)
            {
                var growth = radiusPerRadian * step;
                var sine = Mathf.Sqrt(Mathf.Max(0f, spacing * spacing - growth * growth) / (4f * radius * (radius + growth)));
                step = 2f * Mathf.Asin(Mathf.Min(1f, sine));
            }

            return step;
        }
    }
}
