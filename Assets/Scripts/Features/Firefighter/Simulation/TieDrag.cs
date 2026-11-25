using UnityEngine;

namespace FlameBlame.Firefighter.Simulation
{
    public class TieDrag
    {
        private const float PullSmoothingSeconds = 0.15f;

        private readonly TieDragSettings _settings;
        private Vector3 _pull;

        public TieDrag(TieDragSettings settings)
        {
            _settings = settings;
        }

        public Vector3 Step(Vector3 tension, bool isPulled, Vector3 velocity, float deltaTime)
        {
            _pull = Vector3.Lerp(_pull, tension, deltaTime / PullSmoothingSeconds);
            
            var horizontalPull = new Vector3(_pull.x, 0f, _pull.z);
            var excess = Mathf.Max(0f, horizontalPull.magnitude - _settings.MinTension);
            var speed = isPulled ? Mathf.Min(excess * _settings.SpeedMultiplier, _settings.MaxSpeed) : 0f;
            
            var horizontal = Vector3.MoveTowards(new Vector3(velocity.x, 0f, velocity.z),
                horizontalPull.normalized * speed,
                _settings.Acceleration * deltaTime);
            
            return new Vector3(horizontal.x, velocity.y, horizontal.z);
        }
    }
}
