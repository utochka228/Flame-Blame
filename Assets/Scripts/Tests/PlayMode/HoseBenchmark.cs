using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using FlameBlame.Hose.Components;
using FlameBlame.Hose.Simulation;
using FlameBlame.Interactables.Components;
using NUnit.Framework;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace FlameBlame.Tests.PlayMode
{
    [Explicit("Benchmark, run on demand")]
    [Category("Benchmark")]
    public class HoseBenchmark
    {
        private const string ScenePath = "Assets/Scenes/Level_BurningBuilding.unity";
        private const float SettleSeconds = 3f;
        private const int MeasuredFrames = 600;
        private const int DraggedHose = 1;
        private const float GripCompliance = 1e-7f;
        private const float DragLift = 1f;
        private const float DragSweep = 1.5f;
        private const float DragPeriodSeconds = 4f;
        private const float DoorTogglePeriodSeconds = 2.5f;

        private static readonly string[] StageMarkers =
        {
            "XpbdRope.Integrate", "XpbdRope.SolveBends", "XpbdRope.SolveDistances", "XpbdRope.CollideParticles",
            "XpbdRope.CollideSegments", "XpbdRope.EndStep"
        };

        private HoseNetwork _network;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
#if UNITY_EDITOR
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
#endif
            _network = UnityEngine.Object.FindFirstObjectByType<HoseNetwork>();
            yield return new WaitForSeconds(SettleSeconds);
        }

        [UnityTest]
        public IEnumerator Idle()
        {
            yield return Measure("Idle", _ => { });
        }

        [UnityTest]
        public IEnumerator Active()
        {
            var world = _network.World;
            var hose = _network.HoseOf(DraggedHose);
            var particle = new RopeParticle(DraggedHose, hose.Rope.Positions.Count / 2);
            var rest = world.ParticlePosition(particle);
            var along = hose.Rope.Positions[particle.Index + 1] - hose.Rope.Positions[particle.Index - 1];
            var sideways = Vector3.Cross(Vector3.up, along).normalized;
            var grip = world.AddGrip(particle, GripCompliance);
            var doors = UnityEngine.Object.FindObjectsByType<Door>(FindObjectsSortMode.None);
            var nextDoorToggle = DoorTogglePeriodSeconds;

            yield return Measure("Active", elapsed =>
            {
                var phase = 2f * Mathf.PI * elapsed / DragPeriodSeconds;
                var lift = DragLift * 0.5f * (1f - Mathf.Cos(phase));
                world.SetGripTarget(grip, rest + Vector3.up * lift + sideways * (DragSweep * Mathf.Sin(phase)));
                if (elapsed < nextDoorToggle) return;

                nextDoorToggle += DoorTogglePeriodSeconds;
                foreach (var door in doors)
                    door.Toggle();
            });

            world.Release(grip);
        }

        private static IEnumerator Measure(string scenario, Action<float> perFrame)
        {
            var step = ProfilerRecorder.StartNew(ProfilerCategory.Scripts, "RopeWorld.Step", MeasuredFrames);
            var frame = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", MeasuredFrames);
            var stages = StartAll(StageMarkers);
            var start = Time.time;
            for (var index = 0; index < MeasuredFrames; index++)
            {
                perFrame(Time.time - start);
                yield return null;
            }

            var report = Report(scenario, step, frame, stages);
            step.Dispose();
            frame.Dispose();
            DisposeAll(stages);
            Debug.Log(report);
            Assert.Pass(report);
        }

        private static string Report(string scenario, ProfilerRecorder step, ProfilerRecorder frame, List<ProfilerRecorder> stages)
        {
            var stepNs = new List<double>();
            long steps = 0;
            double totalStepNs = 0;
            for (var index = 0; index < step.Count; index++)
            {
                var sample = step.GetSample(index);
                steps += sample.Count;
                totalStepNs += sample.Value;
                stepNs.Add(sample.Value);
            }

            stepNs.Sort();
            var frameMs = Sum(frame) / Math.Max(1, frame.Count) / 1e6;
            var report = new StringBuilder();
            report.AppendLine($"[HoseBenchmark] {scenario}: {step.Count} frames, {steps} steps ({(double)steps / step.Count:F2}/frame), frame {frameMs:F2} ms");
            report.AppendLine($"  RopeWorld.Step   {totalStepNs / Math.Max(1, steps) / 1e6:F3} ms/step   p95 frame {stepNs[(int)(stepNs.Count * 0.95f)] / 1e6:F3} ms");
            for (var index = 0; index < stages.Count; index++)
                report.AppendLine($"  {StageMarkers[index],-26} {Sum(stages[index]) / Math.Max(1, steps) / 1e6:F3} ms/step");

            return report.ToString();
        }

        private static List<ProfilerRecorder> StartAll(string[] names)
        {
            var recorders = new List<ProfilerRecorder>();
            foreach (var name in names)
                recorders.Add(ProfilerRecorder.StartNew(ProfilerCategory.Scripts, name, MeasuredFrames));

            return recorders;
        }

        private static void DisposeAll(List<ProfilerRecorder> recorders)
        {
            foreach (var recorder in recorders)
                recorder.Dispose();
        }

        private static double Sum(ProfilerRecorder recorder)
        {
            double sum = 0;
            for (var index = 0; index < recorder.Count; index++)
                sum += recorder.GetSample(index).Value;

            return sum;
        }
    }
}
