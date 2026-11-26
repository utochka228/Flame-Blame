using FlameBlame.Firefighter.Components;
using FlameBlame.Firefighter.Config;
using UnityEngine;

namespace FlameBlame.Firefighter.Presentation
{
    [RequireComponent(typeof(Animator))]
    public class FirefighterAnimation : MonoBehaviour
    {
        private const float SpeedDampSeconds = 0.1f;

        private static readonly int SpeedFloat = Animator.StringToHash("Speed");
        private static readonly int IsGroundedBool = Animator.StringToHash("IsGrounded");
        private static readonly int JumpTrigger = Animator.StringToHash("Jump");

        [SerializeField] private FirefighterBody _body;
        [SerializeField] private FirefighterConfigSO _config;

        private Animator _animator;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        private void OnEnable()
        {
            _body.Jumped += OnJumped;
        }

        private void OnDisable()
        {
            _body.Jumped -= OnJumped;
        }

        private void Update()
        {
            var velocity = _body.Velocity;
            var planarSpeed = new Vector2(velocity.x, velocity.z).magnitude;
            _animator.SetFloat(SpeedFloat, planarSpeed / _config.Movement.MoveSpeed, SpeedDampSeconds, Time.deltaTime);
            _animator.SetBool(IsGroundedBool, _body.IsGrounded || _body.IsMotionOverridden);
        }

        private void OnJumped()
        {
            _animator.SetTrigger(JumpTrigger);
        }
    }
}
