using FlameBlame.Level.Components;
using UnityEngine;
using UnityEngine.UI;

namespace FlameBlame.UI
{
    public class GoalView : MonoBehaviour
    {
        private const string AllOutText = "All fires out!";

        [SerializeField] private LevelGoal _goal;
        [SerializeField] private Text _label;

        private void OnEnable()
        {
            _goal.Changed += Refresh;
        }

        private void Start()
        {
            Refresh();
        }

        private void OnDisable()
        {
            _goal.Changed -= Refresh;
        }

        private void Refresh()
        {
            _label.text = _goal.IsComplete ? AllOutText : $"Fires left: {_goal.FiresLeft}";
        }
    }
}
