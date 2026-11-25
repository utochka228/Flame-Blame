using FlameBlame.Hose.Simulation;
using UnityEngine;

namespace FlameBlame.Hose.Components
{
    public class HeatField : MonoBehaviour
    {
        private readonly HeatSources _sources = new HeatSources();

        public void Register(IHeatSource source) => _sources.Register(source);

        public void Unregister(IHeatSource source) => _sources.Unregister(source);

        public float Sample(Vector3 point) => _sources.Sample(point);
    }
}
