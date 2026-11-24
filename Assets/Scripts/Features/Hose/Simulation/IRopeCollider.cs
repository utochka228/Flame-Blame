using System.Collections.Generic;
using UnityEngine;

namespace FlameBlame.Hose.Simulation
{
    public interface IRopeCollider
    {
        void Prepare(IReadOnlyList<Vector3> positions, float radius);

        Vector3 Resolve(Vector3 position, float radius);
    }
}
