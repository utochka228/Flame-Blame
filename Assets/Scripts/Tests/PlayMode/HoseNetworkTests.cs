using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using FlameBlame.Hose.Components;
using FlameBlame.Hose.Config;
using FlameBlame.Hose.Simulation;
using FlameBlame.Pressure.Components;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace FlameBlame.Tests.PlayMode
{
    public class HoseNetworkTests
    {
        private const float SettleSeconds = 2f;
        private const float SourceFraction = 0.4f;
        private const float SourceMaxPressure = 200f;
        private const float Tolerance = 0.02f;
        private const float GapTolerance = 0.15f;

        private GameObject _root;
        private PressureSource _source;
        private HoseSegment _first;
        private HoseSegment _second;
        private HoseNetwork _network;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("HoseNetworkRoot");
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.SetParent(_root.transform);
            floor.transform.position = new Vector3(0f, -0.5f, 0f);
            floor.transform.localScale = new Vector3(60f, 1f, 60f);

            var config = ScriptableObject.CreateInstance<HoseConfigSO>();
            _source = NewInactive("Source").AddComponent<PressureSource>();
            _first = CreateAnchoredHose("First", config, Vector3.zero);
            _second = CreateFreeHose("Second", config, Vector3.right * config.Length);

            var networkObject = NewInactive("Network");
            _network = networkObject.AddComponent<HoseNetwork>();
            SetField(_network, "_source", _source);
            SetField(_network, "_sourceHose", _first);
            SetField(_network, "_hoses", new[] { _first, _second });
            SetField(_network, "_joints", new[]
            {
                new HoseJoint(new HoseEndRef(_first, RopeEndKind.Tail), new HoseEndRef(_second, RopeEndKind.Start))
            });

            _source.SetPressureFraction(SourceFraction);
            foreach (var child in new[] { _source.gameObject, _first.gameObject, _second.gameObject })
                child.SetActive(true);
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(_root);
        }

        [UnityTest]
        public IEnumerator OpenValve_LosesPressureAlongTheChain()
        {
            StartNetwork();

            yield return new WaitForSeconds(SettleSeconds);

            var config = _first.Config;
            var inlet = SourceFraction * SourceMaxPressure;
            var expectedFirstInlet = inlet / config.MaxPressure;
            var expectedSecondTip = Mathf.Max(0f, inlet - 2f * config.Length * config.LossPerMeter - config.OpenEndLeakLoss) / config.MaxPressure;
            Assert.AreEqual(expectedFirstInlet, _first.PressureFractions[0], Tolerance);
            Assert.AreEqual(expectedSecondTip, _second.PressureFractions[_second.PressureFractions.Count - 1], Tolerance);
        }

        [UnityTest]
        public IEnumerator ClosedValve_DepressurisesTheWholeChain()
        {
            _source.IsOpen = false;
            StartNetwork();

            yield return new WaitForSeconds(SettleSeconds);

            Assert.AreEqual(0f, _first.PressureFractions[0], Tolerance);
            Assert.AreEqual(0f, _second.PressureFractions[_second.PressureFractions.Count - 1], Tolerance);
        }

        [UnityTest]
        public IEnumerator CoupledHoses_StayJoined()
        {
            StartNetwork();

            yield return new WaitForSeconds(SettleSeconds);

            var firstTail = _first.Positions[_first.Positions.Count - 1];
            Assert.Less(Vector3.Distance(firstTail, _second.Positions[0]), GapTolerance);
        }

        [UnityTest]
        public IEnumerator NetworkOverThePieceCap_LogsAnErrorAndLeavesTheHosesAlone()
        {
            SetField(_network, "_maxPieces", 2);
            LogAssert.Expect(LogType.Error, new Regex("pieces"));
            StartNetwork();

            yield return null;

            Assert.IsFalse(_network.enabled);
            Assert.IsFalse(_first.IsSteppedByNetwork);
        }

        [UnityTest]
        public IEnumerator NozzleTip_IsNotOfferedAsAFreeEnd()
        {
            AddNozzle(_second);
            StartNetwork();

            yield return null;

            var tipPosition = _network.EndPosition(new HoseEndRef(_second, RopeEndKind.Tail));
            Assert.IsTrue(_network.IsNozzleEnd(new HoseEndRef(_second, RopeEndKind.Tail)));
            Assert.IsFalse(_network.TryFindFreeEnd(tipPosition, 1f, default, out _));
        }

        [UnityTest]
        public IEnumerator PlainFreeEnd_IsOfferedAsAFreeEnd()
        {
            StartNetwork();

            yield return null;

            var tipPosition = _network.EndPosition(new HoseEndRef(_second, RopeEndKind.Tail));
            Assert.IsTrue(_network.TryFindFreeEnd(tipPosition, 1f, default, out var found));
            Assert.AreEqual(_second, found.Hose);
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
        }

        private void StartNetwork()
        {
            _network.gameObject.SetActive(true);
        }

        private GameObject NewInactive(string objectName)
        {
            var created = new GameObject(objectName);
            created.SetActive(false);
            created.transform.SetParent(_root.transform);
            return created;
        }

        private HoseSegment CreateAnchoredHose(string objectName, HoseConfigSO config, Vector3 start) =>
            CreateHose(objectName, config, start, true);

        private HoseSegment CreateFreeHose(string objectName, HoseConfigSO config, Vector3 start) =>
            CreateHose(objectName, config, start, false);

        private HoseSegment CreateHose(string objectName, HoseConfigSO config, Vector3 start, bool anchored)
        {
            var hoseObject = NewInactive(objectName);
            var outlet = new GameObject("Outlet").transform;
            outlet.SetParent(hoseObject.transform);
            outlet.position = start + Vector3.up * 0.8f;
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
