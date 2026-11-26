using FlameBlame.Firefighter.Simulation;
using NUnit.Framework;

namespace FlameBlame.Tests.EditMode
{
    public class JumpGateTests
    {
        private const float BufferSeconds = 0.12f;
        private const float CoyoteSeconds = 0.1f;
        private const float LockoutSeconds = 0.15f;
        private const float Tick = 0.02f;

        private JumpGate _gate;

        [SetUp]
        public void SetUp()
        {
            _gate = new JumpGate(BufferSeconds, CoyoteSeconds, LockoutSeconds);
        }

        [Test]
        public void Press_OnGround_JumpsOnNextStep()
        {
            _gate.Press();

            Assert.That(_gate.Step(true, Tick), Is.True);
        }

        [Test]
        public void Press_InAir_WithoutGround_NeverJumps()
        {
            _gate.Press();

            Assert.That(_gate.Step(false, Tick), Is.False);
        }

        [Test]
        public void Press_JustBeforeLanding_IsBuffered()
        {
            _gate.Press();
            _gate.Step(false, Tick);
            _gate.Step(false, Tick);

            Assert.That(_gate.Step(true, Tick), Is.True);
        }

        [Test]
        public void Press_LongBeforeLanding_ExpiresFromBuffer()
        {
            _gate.Press();
            for (var step = 0; step < 10; step++)
                _gate.Step(false, Tick);

            Assert.That(_gate.Step(true, Tick), Is.False);
        }

        [Test]
        public void Press_JustAfterLeavingLedge_UsesCoyoteTime()
        {
            _gate.Step(true, Tick);
            _gate.Step(false, Tick);
            _gate.Press();

            Assert.That(_gate.Step(false, Tick), Is.True);
        }

        [Test]
        public void Press_LongAfterLeavingLedge_DoesNotJump()
        {
            _gate.Step(true, Tick);
            for (var step = 0; step < 10; step++)
                _gate.Step(false, Tick);
            _gate.Press();

            Assert.That(_gate.Step(false, Tick), Is.False);
        }

        [Test]
        public void Jump_ConsumesPress_NoSecondJump()
        {
            _gate.Press();
            _gate.Step(true, Tick);

            Assert.That(_gate.Step(true, Tick), Is.False);
        }

        [Test]
        public void AfterJump_GroundIgnoredDuringLockout()
        {
            _gate.Press();
            _gate.Step(true, Tick);
            _gate.Step(true, Tick);

            Assert.That(_gate.IsGrounded, Is.False);
        }

        [Test]
        public void AfterJump_PressDuringLockout_DoesNotDoubleJump()
        {
            _gate.Press();
            _gate.Step(true, Tick);
            _gate.Press();

            Assert.That(_gate.Step(true, Tick), Is.False);
        }

        [Test]
        public void Cancel_DropsBufferedPressAndCoyoteTime()
        {
            _gate.Step(true, Tick);
            _gate.Press();
            _gate.Cancel();

            Assert.That(_gate.Step(false, Tick), Is.False);
            Assert.That(_gate.IsGrounded, Is.False);
        }

        [Test]
        public void AfterLockout_GroundCountsAgain()
        {
            _gate.Press();
            _gate.Step(true, Tick);
            for (var step = 0; step < 10; step++)
                _gate.Step(true, Tick);

            Assert.That(_gate.IsGrounded, Is.True);
        }
    }
}
