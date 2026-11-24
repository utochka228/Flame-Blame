using System.Collections.Generic;
using UnityEngine;

namespace FlameBlame.Hose.Simulation
{
    public class PlaneRopeCollider : IRopeCollider
    {
        private readonly float _height;

        public PlaneRopeCollider(float height)
        {
            _height = height;
        }

        public void Prepare(IReadOnlyList<Vector3> positions, float radius)
        {
        }

        public Vector3 Resolve(Vector3 position, float radius)
        {
            position.y = Mathf.Max(position.y, _height + radius);
            return position;
        }
    }
}
