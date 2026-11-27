using System.Collections.Generic;
using UnityEngine;

namespace FlameBlame.Water.Simulation
{
    public class PuddleSet
    {
        private readonly PuddleSettings _settings;
        private readonly List<Puddle> _puddles = new List<Puddle>();

        public PuddleSet(PuddleSettings settings)
        {
            _settings = settings;
        }

        public IReadOnlyList<Puddle> Puddles => _puddles;

        public void Add(Vector3 point, float volume)
        {
            if (volume <= 0f) return;

            var nearest = FindNearest(point, out var distance);
            if (ShouldMerge(nearest, distance))
                _puddles[nearest] = CreatePuddle(_puddles[nearest].Position, _puddles[nearest].Volume + volume);
            else
                _puddles.Add(CreatePuddle(point, volume));
        }

        public void Step(float deltaTime)
        {
            var evaporated = _settings.EvaporationPerSecond * deltaTime;
            for (var index = _puddles.Count - 1; index >= 0; index--)
            {
                var volume = _puddles[index].Volume - evaporated;
                if (volume <= 0f)
                    _puddles.RemoveAt(index);
                else
                    _puddles[index] = CreatePuddle(_puddles[index].Position, volume, _puddles[index].Opacity);
            }
        }

        public void Fade(int index, float amount)
        {
            var opacity = _puddles[index].Opacity - amount;
            if (opacity <= 0f)
                _puddles.RemoveAt(index);
            else
                _puddles[index] = CreatePuddle(_puddles[index].Position, _puddles[index].Volume, opacity);
        }

        public float TractionAt(Vector3 point)
        {
            foreach (var puddle in _puddles)
            {
                if (!puddle.IsFading && FlatDistance(puddle.Position, point) <= puddle.Radius)
                    return _settings.MinTraction;
            }

            return 1f;
        }

        private bool ShouldMerge(int nearest, float distance)
        {
            if (nearest < 0) return false;

            var isClose = distance <= Mathf.Max(_puddles[nearest].Radius, _settings.MergeRadius);
            return isClose || _puddles.Count >= _settings.Capacity;
        }

        private int FindNearest(Vector3 point, out float nearestDistance)
        {
            var nearest = -1;
            nearestDistance = float.MaxValue;
            for (var index = 0; index < _puddles.Count; index++)
            {
                if (_puddles[index].IsFading) continue;

                var distance = FlatDistance(_puddles[index].Position, point);
                if (distance >= nearestDistance) continue;

                nearest = index;
                nearestDistance = distance;
            }

            return nearest;
        }

        private Puddle CreatePuddle(Vector3 position, float volume, float opacity = 1f) =>
            new Puddle(position, volume, _settings.RadiusPerSqrtVolume * Mathf.Sqrt(volume) * opacity, opacity);

        private static float FlatDistance(Vector3 first, Vector3 second) =>
            Vector2.Distance(new Vector2(first.x, first.z), new Vector2(second.x, second.z));
    }
}
