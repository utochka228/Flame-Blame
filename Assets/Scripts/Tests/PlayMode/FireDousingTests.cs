using System.Collections;
using System.Reflection;
using FlameBlame.Fire.Config;
using FlameBlame.Hose.Components;
using FlameBlame.Hose.Config;
using FlameBlame.Hose.Simulation;
using FlameBlame.Pressure.Components;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FireComponent = FlameBlame.Fire.Components.Fire;

namespace FlameBlame.Tests.PlayMode
{
    public class FireDousingTests
    {
        private const float SourceFraction = 0.45f;
        private const float HeldHeight = 1f;
        private const float FireDistanceFromTip = 2f;
        private const float SettleSeconds = 2f;
        private const float SprayBudgetSeconds = 8f;

        private GameObject _root;
        private HoseSegment _first;
        private HoseSegment _second;
        private HoseNetwork _network;
        private Nozzle _nozzle;
        private HeatField _heatField;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("FireDousingRoot");
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.SetParent(_root.transform);
            floor.transform.position = new Vector3(0f, -0.5f, 0f);
            floor.transform.localScale = new Vector3(80f, 1f, 80f);

            var config = ScriptableObject.CreateInstance<HoseConfigSO>();
            var source = NewInactive("Source").AddComponent<PressureSource>();
            _first = CreateHose("First", config, Vector3.zero, true);
            _second = CreateHose("Second", config, Vector3.right * config.Length, false);
            _heatField = NewInactive("HeatField").AddComponent<HeatField>();

            var networkObject = NewInactive("Network");
            _network = networkObject.AddComponent<HoseNetwork>();
            SetField(_network, "_source", source);
            SetField(_network, "_sourceHose", _first);
            SetField(_network, "_hoses", new[] { _first, _second });
            SetField(_network, "_joints", new[]
            {
                new HoseJoint(new HoseEndRef(_first, RopeEndKind.Tail), new HoseEndRef(_second, RopeEndKind.Start))
            });

            source.SetPressureFraction(SourceFraction);
            AddNozzle(_second);
            foreach (var child in new[] { source.gameObject, _first.gameObject, _second.gameObject, _heatField.gameObject, networkObject })
                child.SetActive(true);
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(_root);
        }

        [UnityTest]
        public IEnumerator HeldNozzleAimedAtAFire_PutsItOut()
        {
            yield return new WaitForSeconds(SettleSeconds);
            var tip = new Vector3(_nozzle.TipPosition.x, HeldHeight, 0f);
            HoldNozzle(tip, Vector3.right);
            var fire = CreateFire(tip + Vector3.right * FireDistanceFromTip);

            yield return new WaitForSeconds(SprayBudgetSeconds);

            Assert.IsTrue(fire.IsOut, $"fire intensity after spraying: {fire.Intensity}");
        }

        private void HoldNozzle(Vector3 tip, Vector3 direction)
        {
            var tipGrip = _network.World.AddGrip(_nozzle.TipParticle, 1e-7f);
            _network.World.SetGripTarget(tipGrip, tip);
            var aimGrip = _network.World.AddGrip(_nozzle.InnerParticle, 1e-5f);
            _network.World.SetGripTarget(aimGrip, tip - direction * _nozzle.Spacing);
        }

        private FireComponent CreateFire(Vector3 position)
        {
            var fireObject = NewInactive("Fire");
            fireObject.transform.position = position;
            var collider = fireObject.AddComponent<SphereCollider>();
            collider.radius = 0.5f;
            var fire = fireObject.AddComponent<FireComponent>();
            SetField(fire, "_config", ScriptableObject.CreateInstance<FireConfigSO>());
            SetField(fire, "_heatField", _heatField);
            fireObject.SetActive(true);
            return fire;
        }

        private void AddNozzle(HoseSegment hose)
        {
            hose.gameObject.SetActive(false);
            var nozzle = hose.gameObject.AddComponent<Nozzle>();
            var jet = new GameObject("Jet");
            jet.transform.SetParent(hose.transform);
            SetField(nozzle, "_config", ScriptableObject.CreateInstance<NozzleConfigSO>());
            SetField(nozzle, "_hose", hose);
            SetField(nozzle, "_network", _network);
            SetField(nozzle, "_jet", jet.AddComponent<ParticleSystem>());
            hose.gameObject.SetActive(true);
            _nozzle = nozzle;
        }

        private GameObject NewInactive(string objectName)
        {
            var created = new GameObject(objectName);
            created.SetActive(false);
            created.transform.SetParent(_root.transform);
            return created;
        }

        private HoseSegment CreateHose(string objectName, HoseConfigSO config, Vector3 start, bool anchored)
        {
            var hoseObject = NewInactive(objectName);
            var outlet = new GameObject("Outlet").transform;
            outlet.SetParent(hoseObject.transform);
            outlet.position = start + Vector3.up * HeldHeight;
            outlet.rotation = Quaternion.LookRotation(Vector3.right);

            var hose = hoseObject.AddComponent<HoseSegment>();
            SetField(hose, "_config", config);
            SetField(hose, "_outlet", outlet);
            SetField(hose, "_anchoredToOutlet", anchored);
            SetField(hoseObject.AddComponent<HosePinchSensor>(), "_hose", hose);
            SetField(hoseObject.AddComponent<HoseBurstMonitor>(), "_hose", hose);
            return hose;
        }

        private static void SetField(object target, string fieldName, object value)
        {
            target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
        }
    }
}
