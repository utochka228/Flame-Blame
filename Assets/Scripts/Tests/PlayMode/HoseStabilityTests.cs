using System.Collections;
using System.Reflection;
using FlameBlame.Core.Extensions;
using FlameBlame.Hose.Components;
using FlameBlame.Hose.Config;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace FlameBlame.Tests.PlayMode
{
    public class HoseStabilityTests
    {
        private const float DurationSeconds = 30f;
        private const float PressureChangeInterval = 0.5f;
        private const float FloorTolerance = 0.1f;

        private GameObject _root;

        [TearDown]
        public void TearDown()
        {
            if (_root)
                Object.Destroy(_root);
        }

        [UnityTest]
        public IEnumerator HoseWithRandomPressureSweeps_StaysFiniteBoundedAndAboveFloor()
        {
            var config = ScriptableObject.CreateInstance<HoseConfigSO>();
            var hose = BuildScene(config);
            var restLength = (config.ParticleCount - 1) * config.Spacing;
            var maxLength = restLength * (1f + config.MaxStretchFraction) + 0.05f;
            Random.InitState(1234);

            var elapsed = 0f;
            var nextPressureChange = 0f;
            while (elapsed < DurationSeconds)
            {
                if (elapsed >= nextPressureChange)
                {
                    hose.SetPressureFraction(Random.value);
                    nextPressureChange += PressureChangeInterval;
                }

                yield return new WaitForFixedUpdate();
                elapsed += Time.fixedDeltaTime;

                AssertHealthy(hose, maxLength);
            }
        }

        private GameObject CreatePrimitive(PrimitiveType type, Vector3 position, Vector3 scale, bool dynamic)
        {
            var primitive = GameObject.CreatePrimitive(type);
            primitive.transform.SetParent(_root.transform);
            primitive.transform.position = position;
            primitive.transform.localScale = scale;
            if (dynamic)
                primitive.AddComponent<Rigidbody>();
            return primitive;
        }

        private HoseSegment BuildScene(HoseConfigSO config)
        {
            _root = new GameObject("HoseStabilityRoot");
            CreatePrimitive(PrimitiveType.Cube, new Vector3(0f, -0.5f, 0f), new Vector3(40f, 1f, 40f), false);
            var hydrant = CreatePrimitive(PrimitiveType.Cylinder, new Vector3(0f, 0.5f, 0f), new Vector3(0.4f, 0.5f, 0.4f), false);
            var outlet = new GameObject("Outlet").transform;
            outlet.SetParent(hydrant.transform);
            outlet.position = new Vector3(0.3f, 0.8f, 0f);
            outlet.rotation = Quaternion.LookRotation(Vector3.right);

            foreach (var boxPosition in new[]
                     {
                         new Vector3(2f, 0.25f, 0.2f), new Vector3(3.2f, 0.25f, -0.3f), new Vector3(4.5f, 0.25f, 0f),
                         new Vector3(4.5f, 0.75f, 0f), new Vector3(6.5f, 0.25f, 0.3f)
                     })
                CreatePrimitive(PrimitiveType.Cube, boxPosition, Vector3.one * 0.5f, true);

            var hoseObject = new GameObject("Hose");
            hoseObject.SetActive(false);
            hoseObject.transform.SetParent(_root.transform);
            var hose = hoseObject.AddComponent<HoseSegment>();
            SetField(hose, "_config", config);
            SetField(hose, "_outlet", outlet);
            hoseObject.SetActive(true);
            return hose;
        }

        private static void SetField(HoseSegment hose, string fieldName, object value)
        {
            typeof(HoseSegment).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(hose, value);
        }

        private static void AssertHealthy(HoseSegment hose, float maxLength)
        {
            var positions = hose.Positions;
            var length = 0f;
            for (var index = 0; index < positions.Count; index++)
            {
                var position = positions[index];
                Assert.IsTrue(position.IsFinite(), "Non-finite position at particle {0}", index);
                Assert.GreaterOrEqual(position.y, -FloorTolerance, "Particle {0} sank through the floor", index);
                if (index > 0)
                    length += Vector3.Distance(position, positions[index - 1]);
            }

            Assert.LessOrEqual(length, maxLength, "Hose stretched past the recovery limit");
        }
    }
}
