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
            var count = _network.HoseOf(particle.Rope).Rope.Positions.Count;
            return (socket.Accepts & FreeParts(particle.Rope) & SocketCatch.PartOf(particle.Index, count)) != 0;
        }

        public void Latch(HoseSocket socket, RopeParticle particle)
        {
            Disarm(particle.Rope);
            socket.Latch(_network.World, particle, _config.Grip.Compliance, _config.Tie.CreateDragSettings());
            if (socket.Splitter)
                PlugSplitterPort(socket, particle);
        }

        public void Free(HoseSocket socket)
        {
            socket.Release(_network.World);
            if (socket.Splitter)
                _network.Unplug(socket.Splitter, socket.SplitterPort);
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
                if (index == SocketCatch.NoParticle) continue;

                Latch(socket, new RopeParticle(rope, index));
                return;
            }
        }

        private void PlugSplitterPort(HoseSocket socket, RopeParticle particle)
        {
            var hose = _network.HoseOf(particle.Rope);
            var isStart = particle.Index == 0;
            var kind = isStart ? RopeEndKind.Start : RopeEndKind.Tail;
            if (!_network.TryPlug(socket.Splitter, socket.SplitterPort, new HoseEndRef(hose, kind))) return;

            var inner = new RopeParticle(particle.Rope, isStart ? 1 : particle.Index - 1);
            socket.Aim(_network.World, inner, hose.Config.Rope.Spacing, _config.Grip.AimCompliance);
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
