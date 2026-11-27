using FlameBlame.Audio.Components;
using FlameBlame.Audio.Config;
using FlameBlame.Core.Events;
using FlameBlame.Fire.Events;
using FlameBlame.Firefighter.Components;
using FlameBlame.Firefighter.Events;
using FlameBlame.Firefighter.Simulation;
using FlameBlame.Hose.Events;
using FlameBlame.Level.Components;
using FlameBlame.Level.Events;
using UnityEngine;
using UnityEngine.UIElements;

namespace FlameBlame.UI.Hud
{
    [RequireComponent(typeof(UIDocument))]
    public class HudPresenter : MonoBehaviour
    {
        [SerializeField] private LevelGoal _goal;
        [SerializeField] private FirefighterSwap _swap;
        [SerializeField] private AudioManager _audio;
        [SerializeField] private SoundCueSO _swapWarning;
        [SerializeField] [Min(0f)] private float _swapWarningLeadSeconds = 2f;

        private ObjectiveView _objective;
        private InteractionPromptView _prompt;
        private ReticleView _reticle;
        private EventFeedView _feed;
        private SwapChipView _swapChip;
        private LevelCompleteScreen _endScreen;
        private SwapOverlayView _swapOverlay;
        private ControlsView _controls;
        private bool _wasSwapWarning;

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            _objective = new ObjectiveView(root);
            _prompt = new InteractionPromptView(root);
            _reticle = new ReticleView(root);
            _feed = new EventFeedView(root);
            _swapChip = new SwapChipView(root);
            _endScreen = new LevelCompleteScreen(root);
            _swapOverlay = new SwapOverlayView(root);
            _controls = new ControlsView(root);

            _goal.Changed += OnGoalChanged;
            _swap.Swapped += OnSwapped;
            EventBus.Subscribe<InteractionTargetChanged>(OnInteractionTargetChanged);
            EventBus.Subscribe<InteractionUsed>(OnInteractionUsed);
            EventBus.Subscribe<FireExtinguished>(OnFireExtinguished);
            EventBus.Subscribe<HoseBurst>(OnHoseBurst);
            EventBus.Subscribe<HoseSocketLatched>(OnSocketLatched);
            EventBus.Subscribe<HoseSocketReleased>(OnSocketReleased);
            EventBus.Subscribe<LevelCompleted>(OnLevelCompleted);
        }

        private void Start()
        {
            _objective.Show(_goal.FiresLeft, _goal.IsComplete);
            _swapChip.ShowActive(_swap.IsFirstActive, false);
        }

        private void OnDisable()
        {
            _goal.Changed -= OnGoalChanged;
            _swap.Swapped -= OnSwapped;
            EventBus.Unsubscribe<InteractionTargetChanged>(OnInteractionTargetChanged);
            EventBus.Unsubscribe<InteractionUsed>(OnInteractionUsed);
            EventBus.Unsubscribe<FireExtinguished>(OnFireExtinguished);
            EventBus.Unsubscribe<HoseBurst>(OnHoseBurst);
            EventBus.Unsubscribe<HoseSocketLatched>(OnSocketLatched);
            EventBus.Unsubscribe<HoseSocketReleased>(OnSocketReleased);
            EventBus.Unsubscribe<LevelCompleted>(OnLevelCompleted);

            _objective.Dispose();
            _prompt.Dispose();
            _reticle.Dispose();
            _feed.Dispose();
            _swapChip.Dispose();
            _endScreen.Dispose();
            _controls.Dispose();
        }

        private void LateUpdate()
        {
            var isSwapWarning = _swap.SecondsLeft <= _swapWarningLeadSeconds;
            if (isSwapWarning && !_wasSwapWarning)
                _audio.Play(_swapWarning);
            _wasSwapWarning = isSwapWarning;
            _swapChip.ShowWarning(_swap.WarningFraction);
            _swapOverlay.Show(_swap.WarningFraction, _swap.VisorFraction);
            _controls.Show(_swap.Commands);
        }

        private void OnInteractionTargetChanged(InteractionTargetChanged changed)
        {
            _prompt.Show(changed.Action);
            _reticle.SetActive(changed.Action != UseAction.None);
        }

        private void OnInteractionUsed(InteractionUsed used)
        {
            _prompt.Press();
            _reticle.Press();
        }

        private void OnSwapped()
        {
            _swapChip.ShowActive(_swap.IsFirstActive, true);
        }

        private void OnGoalChanged()
        {
            _objective.Show(_goal.FiresLeft, _goal.IsComplete);
        }

        private void OnFireExtinguished(FireExtinguished extinguished)
        {
            _feed.Push("Fire out!", FeedTone.Good);
        }

        private void OnHoseBurst(HoseBurst burst)
        {
            _feed.Push("Hose burst!", FeedTone.Danger);
        }

        private void OnSocketLatched(HoseSocketLatched latched)
        {
            _feed.Push("Hose latched", FeedTone.Neutral);
        }

        private void OnSocketReleased(HoseSocketReleased released)
        {
            _feed.Push("Hose released", FeedTone.Neutral);
        }

        private void OnLevelCompleted(LevelCompleted completed)
        {
            _endScreen.Show(completed);
        }
    }
}
