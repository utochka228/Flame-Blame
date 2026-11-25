using FlameBlame.Interactables.Simulation;
using NUnit.Framework;

namespace FlameBlame.Tests.EditMode
{
    public class DoorModelTests
    {
        private const float OpenAngle = 100f;
        private const float OpenSpeed = 90f;
        private const float CloseSpeed = 400f;

        private DoorModel _door;
        private int _slamCount;

        [SetUp]
        public void SetUp()
        {
            _door = new DoorModel(OpenAngle, OpenSpeed, CloseSpeed);
            _slamCount = 0;
            _door.Slammed += () => _slamCount++;
        }

        private void OpenFully()
        {
            _door.Toggle();
            _door.Step(5f);
        }

        [Test]
        public void NewDoor_IsClosed()
        {
            Assert.That(_door.Angle, Is.EqualTo(0f));
            Assert.That(_door.IsOpen, Is.False);
        }

        [Test]
        public void Toggle_OpensAtTheOpeningSpeed()
        {
            _door.Toggle();
            _door.Step(0.5f);

            Assert.That(_door.IsOpen, Is.True);
            Assert.That(_door.Angle, Is.EqualTo(45f).Within(1e-4f));
        }

        [Test]
        public void Opening_StopsAtTheOpenAngle()
        {
            OpenFully();

            Assert.That(_door.Angle, Is.EqualTo(OpenAngle));
        }

        [Test]
        public void Toggle_Again_ClosesAtTheFasterClosingSpeed()
        {
            OpenFully();

            _door.Toggle();
            _door.Step(0.1f);

            Assert.That(_door.IsOpen, Is.False);
            Assert.That(_door.Angle, Is.EqualTo(OpenAngle - CloseSpeed * 0.1f).Within(1e-4f));
        }

        [Test]
        public void ReachingClosed_WhileClosing_SlamsOnce()
        {
            OpenFully();
            _door.Toggle();

            _door.Step(1f);
            _door.Step(1f);

            Assert.That(_door.Angle, Is.EqualTo(0f));
            Assert.That(_slamCount, Is.EqualTo(1));
        }

        [Test]
        public void Opening_NeverSlams()
        {
            OpenFully();

            Assert.That(_slamCount, Is.EqualTo(0));
        }

        [Test]
        public void ClosedDoor_ToggledShut_DoesNotSlam()
        {
            _door.Toggle();
            _door.Toggle();

            _door.Step(1f);

            Assert.That(_slamCount, Is.EqualTo(0));
        }

        [Test]
        public void ReopeningMidClose_DoesNotSlam()
        {
            OpenFully();
            _door.Toggle();
            _door.Step(0.1f);

            _door.Toggle();
            _door.Step(5f);

            Assert.That(_door.Angle, Is.EqualTo(OpenAngle));
            Assert.That(_slamCount, Is.EqualTo(0));
        }
    }
}
