using System;
using UnityEngine;

namespace FlameBlame.Interactables.Simulation
{
    public class DoorModel
    {
        private float _openAngle;
        private float _openDegreesPerSecond;
        private float _closeDegreesPerSecond;

        public DoorModel(float openAngle, float openDegreesPerSecond, float closeDegreesPerSecond)
        {
            Configure(openAngle, openDegreesPerSecond, closeDegreesPerSecond);
        }

        public float Angle { get; private set; }
        public bool IsOpen { get; private set; }

        public event Action Slammed;

        public void Configure(float openAngle, float openDegreesPerSecond, float closeDegreesPerSecond)
        {
            _openAngle = openAngle;
            _openDegreesPerSecond = openDegreesPerSecond;
            _closeDegreesPerSecond = closeDegreesPerSecond;
        }

        public void Toggle()
        {
            IsOpen = !IsOpen;
        }

        public void Step(float deltaTime)
        {
            if (IsOpen)
            {
                Angle = Mathf.MoveTowards(Angle, _openAngle, _openDegreesPerSecond * deltaTime);
                return;
            }

            var wasOpen = Angle > 0f;
            Angle = Mathf.MoveTowards(Angle, 0f, _closeDegreesPerSecond * deltaTime);
            if (wasOpen && Angle <= 0f)
                Slammed?.Invoke();
        }
    }
}
