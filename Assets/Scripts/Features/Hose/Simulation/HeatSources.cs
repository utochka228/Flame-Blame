using System.Collections.Generic;
using UnityEngine;

namespace FlameBlame.Hose.Simulation
{
    public class HeatSources
    {
        private readonly List<IHeatSource> _sources = new List<IHeatSource>();

        public void Register(IHeatSource source)
        {
            if (!_sources.Contains(source))
                _sources.Add(source);
        }

        public void Unregister(IHeatSource source)
        {
            _sources.Remove(source);
        }

        public float Sample(Vector3 point)
        {
            var heat = 0f;
            foreach (var source in _sources)
                heat += source.HeatAt(point);

            return Mathf.Clamp01(heat);
        }
    }
}
