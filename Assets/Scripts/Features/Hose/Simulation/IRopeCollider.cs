using System.Collections.Generic;
using UnityEngine;

namespace FlameBlame.Hose.Simulation
{
    public interface IRopeCollider
    {
        void Prepare(IReadOnlyList<Vector3> positions, float radius);

        Vector3 Sweep(Vector3 from, Vector3 to, float radius);

        Vector3 Resolve(Vector3 position, Vector3 lastFreePosition, float radius);
    }
}
