using NUnit.Framework;
using UnityEngine;
using DontPushTheButton.Abilities;
using DontPushTheButton.Player;

namespace DontPushTheButton.Tests
{
    /// <summary>
    /// 动画驱动接口契约单测（M3.10）：
    /// - 能力暴露的状态属性默认值正确（JumpAbility.IsFlying / PickupAbility.IsCarrying）；
    /// - PlayerAbilityController.NotifyCast → OnInstantCast 路由正确（瞬时能力动画触发的事件源）。
    /// Animator 参数实际 set 需 PlayMode + Controller 资产，属队友交付后验证，此处只验驱动逻辑。
    /// </summary>
    [TestFixture]
    public class PlayerAnimatorContractTests
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
