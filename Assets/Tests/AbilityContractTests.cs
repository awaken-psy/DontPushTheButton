using NUnit.Framework;
using UnityEngine;
using DontPushTheButton.Abilities;

namespace DontPushTheButton.Tests
{
    /// <summary>
    /// 各能力契约单测（M3.1-3.5）：确认 Kind/Trigger/Overload 映射正确。
    /// 回归 M3 改动：Pickup 搬运改 Continuous、Jump 飞行状态机改 Continuous 等。
    /// </summary>
    [TestFixture]
    public class AbilityContractTests
    {
        private GameObject _go;

        [SetUp] public void SetUp() { _go = new GameObject("TestAbilityHost"); }
        [TearDown] public void TearDown() { Object.DestroyImmediate(_go); }

        [Test] public void MoveAbility_Contract()
        {
            var a = _go.AddComponent<MoveAbility>();
            Assert.AreEqual(AbilityKind.Move, a.Kind);
            Assert.AreEqual(AbilityTrigger.Continuous, a.Trigger);
            Assert.AreEqual(OverloadParadigm.StatStack, a.Overload);
        }

        [Test] public void JumpAbility_Contract()
        {
            var a = _go.AddComponent<JumpAbility>();
            Assert.AreEqual(AbilityKind.Jump, a.Kind);
            Assert.AreEqual(AbilityTrigger.Continuous, a.Trigger); // M3.5 改（飞行状态机）
            Assert.AreEqual(OverloadParadigm.StateSwitch, a.Overload);
        }

        [Test] public void PickupAbility_Contract()
        {
            var a = _go.AddComponent<PickupAbility>();
            Assert.AreEqual(AbilityKind.Pickup, a.Kind);
            Assert.AreEqual(AbilityTrigger.Continuous, a.Trigger); // M3.1 磁力枪搬运
            Assert.AreEqual(OverloadParadigm.MultiTarget, a.Overload);
        }

        [Test] public void PullAbility_Contract()
        {
            var a = _go.AddComponent<PullAbility>();
            Assert.AreEqual(AbilityKind.Pull, a.Kind);
            Assert.AreEqual(AbilityTrigger.Instant, a.Trigger);
            Assert.AreEqual(OverloadParadigm.StateSwitch, a.Overload);
        }

        [Test] public void DashAbility_Contract()
        {
            var a = _go.AddComponent<DashAbility>();
            Assert.AreEqual(AbilityKind.Dash, a.Kind);
            Assert.AreEqual(AbilityTrigger.Instant, a.Trigger);
            Assert.AreEqual(OverloadParadigm.StatStack, a.Overload);
        }

        [Test] public void PushAbility_Contract()
        {
            var a = _go.AddComponent<PushAbility>();
            Assert.AreEqual(AbilityKind.Push, a.Kind);
            Assert.AreEqual(AbilityTrigger.Instant, a.Trigger); // 光束型瞬时
            Assert.AreEqual(OverloadParadigm.StatStack, a.Overload);
        }
    }
}
