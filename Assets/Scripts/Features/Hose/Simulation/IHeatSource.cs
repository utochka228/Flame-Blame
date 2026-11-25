using UnityEngine;

namespace FlameBlame.Hose.Simulation
{
    public interface IHeatSource
    {
        float HeatAt(Vector3 point);
    }
}
