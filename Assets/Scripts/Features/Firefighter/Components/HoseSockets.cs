using FlameBlame.Core.Extensions;
using FlameBlame.Firefighter.Config;
using FlameBlame.Firefighter.Simulation;
using FlameBlame.Hose.Components;
using FlameBlame.Hose.Simulation;
using UnityEngine;

namespace FlameBlame.Firefighter.Components
{
    public class HoseSockets : MonoBehaviour
    {
        [SerializeField] private FirefighterConfigSO _config;
        [SerializeField] private HoseNetwork _network;
        [SerializeField] private HoseSocket[] _sockets;

        private readonly CatchWindows _windows = new CatchWindows();

        public bool IsCatchArmed => _windows.OpenRopes.Count > 0;

        private void FixedUpdate()
        {
            _windows.Step(Time.fixedDeltaTime);
            CatchThrownHoses();
            foreach (var socket in _sockets)
            {
                if (socket.IsOccupied)
                    socket.Step(_network.World, _config.Throw.LatchBlendSeconds, Time.fixedDeltaTime);
            }
        }

        public void ArmCatch(int rope) => _windows.Arm(rope, _config.Throw.CatchWindowSeconds);

        public void Disarm(int rope) => _windows.Close(rope);

        public bool TryFindInReach(Transform eye, out HoseSocket found)
        {
            found = null;
            var bestOffset = _config.Interaction.AimRadiusMeters;
            foreach (var socket in _sockets)
            {
                var offset = socket.Position.OffsetFromRay(eye.position, eye.forward, _config.Interaction.ReachMeters);
                if (offset >= bestOffset) continue;

                bestOffset = offset;
                found = socket;
            }

            return found;
        }

        public bool Accepts(HoseSocket socket, RopeParticle particle)
        {
            var hose = _network.HoseOf(particle.Rope);
            var count = hose.Rope.Positions.Count;
            if (socket.FeedsSource && IsNozzleParticle(hose, particle.Index)) return false;

            return (socket.Accepts & FreeParts(particle.Rope) & SocketCatch.PartOf(particle.Index, count)) != 0;
        }

        public void Latch(HoseSocket socket, RopeParticle particle)
        {
            Disarm(particle.Rope);
            socket.Latch(_network.World, particle, _config.Grip.Compliance, _config.Tie.CreateDragSettings());
            if (socket.Splitter || socket.FeedsSource)
                PlugEnd(socket, particle);
        }

        public void Free(HoseSocket socket)
        {
            socket.Release(_network.World);
            if (socket.Splitter)
                _network.Unplug(socket.Splitter, socket.SplitterPort);
            if (socket.FeedsSource)
                _network.UnplugSource();
        }

        public void FreeAt(RopeParticle particle, int toleranceParticles)
        {
            foreach (var socket in _sockets)
            {
                if (socket.IsLatchedNear(_network.World, particle, toleranceParticles))
                    Free(socket);
            }
        }

        private void CatchThrownHoses()
        {
            var openRopes = _windows.OpenRopes;
            for (var slot = openRopes.Count - 1; slot >= 0; slot--)
                TryCatch(openRopes[slot]);
        }

        private void TryCatch(int rope)
        {
            var positions = _network.HoseOf(rope).Rope.Positions;
            var freeParts = FreeParts(rope);
            foreach (var socket in _sockets)
            {
                if (socket.IsOccupied) continue;

                var index = SocketCatch.Closest(positions, socket.Accepts & freeParts, socket.Position, socket.CaptureRadiusMeters);
                if (index == SocketCatch.NoParticle || !Accepts(socket, new RopeParticle(rope, index))) continue;

                Latch(socket, new RopeParticle(rope, index));
                return;
            }
        }

        private void PlugEnd(HoseSocket socket, RopeParticle particle)
        {
            var hose = _network.HoseOf(particle.Rope);
            foreach (var end in hose.OpenEnds)
            {
                if (end.ParticleIndex != particle.Index) continue;
                if (!TryPlug(socket, new HoseEndRef(hose, end.Kind))) return;

                socket.Aim(_network.World, new RopeParticle(particle.Rope, end.InnerIndex), hose.Config.Rope.Spacing, _config.Grip.AimCompliance);
                return;
            }
        }

        private bool TryPlug(HoseSocket socket, HoseEndRef end)
        {
            return socket.FeedsSource ? _network.TryPlugSource(end) : _network.TryPlug(socket.Splitter, socket.SplitterPort, end);
        }

        private bool IsNozzleParticle(HoseSegment hose, int index)
        {
            var lastIndex = hose.Rope.Positions.Count - 1;
            if (index != 0 && index != lastIndex) return false;

            return _network.IsNozzleEnd(new HoseEndRef(hose, index == 0 ? RopeEndKind.Start : RopeEndKind.Tail));
        }

        private CatchParts FreeParts(int rope)
        {
            var hose = _network.HoseOf(rope);
            var parts = CatchParts.Middle;
            if (_network.IsEndFree(new HoseEndRef(hose, RopeEndKind.Start)))
                parts |= CatchParts.Start;
            if (_network.IsEndFree(new HoseEndRef(hose, RopeEndKind.Tail)))
                parts |= CatchParts.Tail;
            return parts;
        }
    }
}
