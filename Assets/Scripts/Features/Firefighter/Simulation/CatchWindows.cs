using System.Collections.Generic;

namespace FlameBlame.Firefighter.Simulation
{
    public class CatchWindows
    {
        private readonly List<int> _ropes = new List<int>();
        private readonly List<float> _secondsLeft = new List<float>();

        public IReadOnlyList<int> OpenRopes => _ropes;

        public void Arm(int rope, float seconds)
        {
            Close(rope);
            _ropes.Add(rope);
            _secondsLeft.Add(seconds);
        }

        public void Close(int rope)
        {
            var slot = _ropes.IndexOf(rope);
            if (slot < 0) return;

            _ropes.RemoveAt(slot);
            _secondsLeft.RemoveAt(slot);
        }

        public void Step(float deltaTime)
        {
            for (var slot = _ropes.Count - 1; slot >= 0; slot--)
            {
                _secondsLeft[slot] -= deltaTime;
                if (_secondsLeft[slot] > 0f) continue;

                _ropes.RemoveAt(slot);
                _secondsLeft.RemoveAt(slot);
            }
        }
    }
}
