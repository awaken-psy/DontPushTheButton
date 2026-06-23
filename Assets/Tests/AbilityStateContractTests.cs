using NUnit.Framework;
using UnityEngine;
using DontPushTheButton.Abilities;
using DontPushTheButton.Player;

namespace DontPushTheButton.Tests
{
    /// <summary>
    /// 能力状态接口契约单测（M3.10）：这些状态供「代码控制 transform 的程序动画」读取（队友写）。
    /// - JumpAbility.IsFlying / PickupAbility.IsCarrying 默认值正确；
    /// - PlayerAbilityController.NotifyCast → OnInstantCast 路由正确（Dash/Pull/Push 触发瞬间事件源）。
    /// 保接口契约不破——队友动画代码依赖这些信号。接口清单见 docs/design/动画系统.md。
    /// </summary>
    [TestFixture]
    public class AbilityStateContractTests
    {
        private GameObject _go;

        [SetUp] public void SetUp() { _go = new GameObject("TestAnimatorHost"); }
        [TearDown] public void TearDown() { Object.DestroyImmediate(_go); }

        [Test] public void JumpAbility_IsFlying_DefaultFalse()
        {
            var jump = _go.AddComponent<JumpAbility>();
            Assert.IsFalse(jump.IsFlying); // 默认 Ground 态
        }

        [Test] public void PickupAbility_IsCarrying_DefaultFalse()
        {
            var pickup = _go.AddComponent<PickupAbility>();
            Assert.IsFalse(pickup.IsCarrying); // 默认 Idle 态
        }

        [Test] public void PlayerAbilityController_NotifyCast_RaisesOnInstantCast()
        {
            var ctrl = _go.AddComponent<PlayerAbilityController>();
            AbilityKind received = (AbilityKind)(-1);
            ctrl.OnInstantCast += k => received = k;

            ctrl.NotifyCast(AbilityKind.Dash);
            Assert.AreEqual(AbilityKind.Dash, received, "Dash 应路由");

            ctrl.NotifyCast(AbilityKind.Pull);
            Assert.AreEqual(AbilityKind.Pull, received, "Pull 应路由");

            ctrl.NotifyCast(AbilityKind.Push);
            Assert.AreEqual(AbilityKind.Push, received, "Push 应路由");
        }

        [Test] public void PlayerAbilityController_NotifyCast_NoSubscriber_DoesNotThrow()
        {
            var ctrl = _go.AddComponent<PlayerAbilityController>();
            // 无订阅者时 invoke null delegate 不应崩（?. 保护）
            Assert.DoesNotThrow(() => ctrl.NotifyCast(AbilityKind.Dash));
        }
    }
}
