using FlameBlame.Firefighter.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace FlameBlame.Tests.EditMode
{
    public class FirefighterMotionTests
    {
        private const float MoveSpeed = 5f;
        private const float Acceleration = 10f;
        private const float Deceleration = 4f;
        private const float JumpSpeed = 6f;

        private FirefighterMotion _motion;

        [SetUp]
        public void SetUp()
        {
            _motion = new FirefighterMotion(MoveSpeed, Acceleration, Deceleration, JumpSpeed);
        }

        [Test]
        public void Step_MovesRelativeToFacing()
        {
            var velocity = _motion.Step(Vector3.zero, Vector2.up, Quaternion.Euler(0f, 90f, 0f), 10f);

            Assert.That(velocity.x, Is.EqualTo(MoveSpeed).Within(1e-4f));
            Assert.That(velocity.z, Is.EqualTo(0f).Within(1e-4f));
        }

        [Test]
        public void Step_AcceleratesGradually()
        {
            var velocity = _motion.Step(Vector3.zero, Vector2.up, Quaternion.identity, 0.1f);

            Assert.That(velocity.z, Is.EqualTo(Acceleration * 0.1f).Within(1e-4f));
        }

        [Test]
        public void Step_NoInput_Decelerates()
        {
            var velocity = _motion.Step(new Vector3(0f, 0f, MoveSpeed), Vector2.zero, Quaternion.identity, 0.1f);

            Assert.That(velocity.z, Is.EqualTo(MoveSpeed - Deceleration * 0.1f).Within(1e-4f));
        }

        [Test]
        public void Step_KeepsVerticalVelocity()
        {
            var velocity = _motion.Step(new Vector3(0f, -3f, 0f), Vector2.up, Quaternion.identity, 0.1f);

            Assert.That(velocity.y, Is.EqualTo(-3f));
        }

        [Test]
        public void Jump_SetsVerticalSpeed_KeepsHorizontal()
        {
            var velocity = _motion.Jump(new Vector3(2f, -1f, 3f));

            Assert.That(velocity, Is.EqualTo(new Vector3(2f, JumpSpeed, 3f)));
        }
    }
}
