namespace FlameBlame.Firefighter.Simulation
{
    public struct UseContext
    {
        public bool CanConnectHeldEnd { get; private set; }
        public bool BurstInReach { get; private set; }
        public bool JointInReach { get; private set; }
        public bool ValveInReach { get; private set; }
        public bool CanTieHeld { get; private set; }
        public bool TiedInReach { get; private set; }
        public bool RopeInReach { get; private set; }
        public bool IsClimbing { get; private set; }
        public bool DoorInReach { get; private set; }
        public bool CanLatchHeld { get; private set; }
        public bool LatchedInReach { get; private set; }

        public UseContext(bool canConnectHeldEnd, bool burstInReach, bool jointInReach, bool valveInReach)
        {
            CanConnectHeldEnd = canConnectHeldEnd;
            BurstInReach = burstInReach;
            JointInReach = jointInReach;
            ValveInReach = valveInReach;
            CanTieHeld = false;
            TiedInReach = false;
            RopeInReach = false;
            IsClimbing = false;
            DoorInReach = false;
            CanLatchHeld = false;
            LatchedInReach = false;
        }

        public UseContext WithTie(bool canTieHeld, bool tiedInReach)
        {
            var copy = this;
            copy.CanTieHeld = canTieHeld;
            copy.TiedInReach = tiedInReach;
            return copy;
        }

        public UseContext WithSocket(bool canLatchHeld, bool latchedInReach)
        {
            var copy = this;
            copy.CanLatchHeld = canLatchHeld;
            copy.LatchedInReach = latchedInReach;
            return copy;
        }

        public UseContext WithDoor(bool doorInReach)
        {
            var copy = this;
            copy.DoorInReach = doorInReach;
            return copy;
        }

        public UseContext WithClimb(bool ropeInReach, bool isClimbing)
        {
            var copy = this;
            copy.RopeInReach = ropeInReach;
            copy.IsClimbing = isClimbing;
            return copy;
        }
    }
}
