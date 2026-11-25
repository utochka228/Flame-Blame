using System;
using UnityEngine;

namespace FlameBlame.Level.Simulation
{
    public class LevelProgress
    {
        private readonly int _totalFires;

        public LevelProgress(int totalFires)
        {
            _totalFires = totalFires;
            FiresLeft = totalFires;
        }

        public int FiresLeft { get; private set; }
        public bool IsComplete { get; private set; }

        public event Action Completed;

        public void Report(int firesOut)
        {
            FiresLeft = Mathf.Max(0, _totalFires - firesOut);
            if (IsComplete || _totalFires == 0 || FiresLeft > 0) return;

            IsComplete = true;
            Completed?.Invoke();
        }
    }
}
