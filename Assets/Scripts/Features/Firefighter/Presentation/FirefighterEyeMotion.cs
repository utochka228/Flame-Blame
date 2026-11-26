using FlameBlame.Firefighter.Components;
using FlameBlame.Firefighter.Config;
using FlameBlame.Firefighter.Simulation;
using UnityEngine;

namespace FlameBlame.Firefighter.Presentation
{
    [RequireComponent(typeof(Camera))]
    public class FirefighterEyeMotion : MonoBehaviour
    {
        private const float LateralShakeRate = 1.7f;
        private const float VerticalShakeRate = 2.3f;
        private const float RollShakeRate = 1.1f;

        [SerializeField] private FirefighterBody _body;
        [SerializeField] private FirefighterConfigSO _config;

        private Camera _camera;
        private readonly CameraSpring _landingDip = new();
        private readonly CameraTrauma _trauma = new();
        private Vector3 _restPosition;
        private float _bobPhase;
        private float _bobWeight;
        private float _tiltDegrees;
        private float _baseFieldOfView;
        private float _extraFieldOfView;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            _baseFieldOfView = _camera.fieldOfView;
            _restPosition = transform.localPosition;
        }

        private void OnEnable()
        {
            _body.Landed += OnLanded;
        }

        private void OnDisable()
        {
            _body.Landed -= OnLanded;
        }

        private void LateUpdate()
        {
            var eyeMotion = _config.EyeMotion;
            var deltaTime = Time.deltaTime;
            var velocity = _body.Velocity;
            var horizontalSpeed = new Vector2(velocity.x, velocity.z).magnitude;
            var motionWeight = eyeMotion.IsCameraMotionReduced ? 0f : 1f;

            _landingDip.Step(deltaTime, eyeMotion.SpringAngularFrequency, eyeMotion.SpringDampingRatio);
            _trauma.Step(Time.unscaledDeltaTime, eyeMotion.TraumaDecay, eyeMotion.ShakeFrequency);
            var bob = StepHeadBob(eyeMotion, horizontalSpeed, motionWeight, deltaTime);
            StepStrafeTilt(eyeMotion, velocity, motionWeight, deltaTime);
            StepFieldOfView(eyeMotion, horizontalSpeed, motionWeight, deltaTime);

            transform.localPosition = _restPosition + bob + ShakeOffset(eyeMotion) + Vector3.up * _landingDip.Offset;
            transform.localRotation = _body.EyeLookRotation * Quaternion.Euler(0f, 0f, _tiltDegrees + ShakeRollDegrees(eyeMotion));
        }

        private Vector3 StepHeadBob(FirefighterEyeMotionConfig eyeMotion, float horizontalSpeed, float motionWeight, float deltaTime)
        {
            var isWalking = _body.IsGrounded;
            if (isWalking)
                _bobPhase += horizontalSpeed * deltaTime / eyeMotion.BobStepLength * Mathf.PI;

            var bobTarget = isWalking ? Mathf.Clamp01(horizontalSpeed / _config.Movement.MoveSpeed) * motionWeight : 0f;
            _bobWeight = Mathf.MoveTowards(_bobWeight, bobTarget, eyeMotion.BobBlendSpeed * deltaTime);

            var lateral = Mathf.Sin(_bobPhase) * eyeMotion.BobLateralAmplitude;
            var vertical = Mathf.Sin(2f * _bobPhase) * eyeMotion.BobVerticalAmplitude;
            return new Vector3(lateral, vertical, 0f) * _bobWeight;
        }

        private void StepStrafeTilt(FirefighterEyeMotionConfig eyeMotion, Vector3 velocity, float motionWeight, float deltaTime)
        {
            var strafe = Mathf.Clamp(Vector3.Dot(velocity, _body.transform.right) / _config.Movement.MoveSpeed, -1f, 1f);
            var tiltTarget = -strafe * eyeMotion.StrafeTiltDegrees * motionWeight;
            _tiltDegrees = Smooth(_tiltDegrees, tiltTarget, eyeMotion.TiltBlendSpeed, deltaTime);
        }

        private void StepFieldOfView(FirefighterEyeMotionConfig eyeMotion, float horizontalSpeed, float motionWeight, float deltaTime)
        {
            var fovTarget = Mathf.Clamp01(horizontalSpeed / eyeMotion.FovFullSpeed) * eyeMotion.FovExtraDegrees * motionWeight;
            _extraFieldOfView = Smooth(_extraFieldOfView, fovTarget, eyeMotion.FovBlendSpeed, deltaTime);
            _camera.fieldOfView = _baseFieldOfView + _extraFieldOfView;
        }

        private Vector3 ShakeOffset(FirefighterEyeMotionConfig eyeMotion) =>
            new Vector3(_trauma.Wave(LateralShakeRate), _trauma.Wave(VerticalShakeRate), 0f) * (eyeMotion.ShakeMaxOffset * eyeMotion.ShakeStrengthFraction);

        private float ShakeRollDegrees(FirefighterEyeMotionConfig eyeMotion) =>
            _trauma.Wave(RollShakeRate) * eyeMotion.ShakeMaxRollDegrees * eyeMotion.ShakeStrengthFraction;

        private void OnLanded(float fallSpeed)
        {
            var eyeMotion = _config.EyeMotion;
            if (fallSpeed > eyeMotion.LandingMinFallSpeed)
                _landingDip.Kick(-Mathf.Min(fallSpeed * eyeMotion.LandingKickPerFallSpeed, eyeMotion.LandingMaxKick));
            if (fallSpeed > eyeMotion.LandingShakeMinFallSpeed)
                _trauma.Add((fallSpeed - eyeMotion.LandingShakeMinFallSpeed) * eyeMotion.LandingTraumaPerFallSpeed);
        }

        private static float Smooth(float current, float target, float speed, float deltaTime) =>
            Mathf.Lerp(current, target, 1f - Mathf.Exp(-speed * deltaTime));
    }
}
