using System;
using UnityEngine;

namespace FlameBlame.Interactables.Simulation
{
    public class DoorModel
    {
        private readonly float _openAngle;
        private readonly float _openDegreesPerSecond;
        private readonly float _closeDegreesPerSecond;

        public DoorModel(float openAngle, float openDegreesPerSecond, float closeDegreesPerSecond)
        {
            _openAngle = openAngle;
            _openDegreesPerSecond = openDegreesPerSecond;
            _closeDegreesPerSecond = closeDegreesPerSecond;
        }

        public float Angle { get; private set; }
        public bool IsOpen { get; private set; }

        public event Action Slammed;

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
