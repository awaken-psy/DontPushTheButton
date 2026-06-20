using System.Collections.Generic;
using NUnit.Framework;
using DontPushTheButton.Abilities;
using DontPushTheButton.Binding;

namespace DontPushTheButton.Tests
{
    /// <summary>
    /// LoadoutValidator 校验规则单测（M2.2）。覆盖 GDD §A / 4.5 全部规则。
    /// </summary>
    [TestFixture]
    public class LoadoutValidatorTests
    {
        // 合法基准：6 槽，WASD 移动 + 跳跃(Space 超载) + 推动(E)
        private static LoadoutConfig ValidConfig() => new LoadoutConfig
        {
            SlotCount = 6,
            AvailableAbilities = new HashSet<AbilityKind> { AbilityKind.Jump, AbilityKind.Push },
            Slots = new List<KeySlot>
            {
                new KeySlot("W", false, BindingItem.Move(MoveDirection.Up)),
                new KeySlot("A", false, BindingItem.Move(MoveDirection.Left)),
                new KeySlot("S", false, BindingItem.Move(MoveDirection.Down)),
                new KeySlot("D", false, BindingItem.Move(MoveDirection.Right)),
                new KeySlot("Space", true, BindingItem.Of(AbilityKind.Jump)), // 超载跳跃（正常，有强化补偿）
                new KeySlot("E", false, BindingItem.Of(AbilityKind.Push)),
            }
        };

        private static bool HasIssue(ValidationResult r, ValidationLevel level, string code)
        {
            foreach (var i in r.Issues) if (i.Level == level && i.Code == code) return true;
            return false;
        }

        [Test] public void ValidConfig_Passes_NoWarnings()
        {
            var r = LoadoutValidator.Validate(ValidConfig());
            Assert.IsTrue(r.IsValid, "合法配置应 IsValid");
            Assert.IsFalse(r.HasWarnings, "合法配置（移动全绑、无移动绑超载）应无 Warning");
        }

        [Test] public void AbilityDoubleBound_IsError()
        {
            var c = ValidConfig();
            c.Slots[5].Binding = BindingItem.Of(AbilityKind.Jump); // E 也绑 Jump（Space 已绑）
            var r = LoadoutValidator.Validate(c);
            Assert.IsFalse(r.IsValid);
            Assert.IsTrue(HasIssue(r, ValidationLevel.Error, "AbilityDoubleBound"));
        }

        [Test] public void MoveDirectionDoubleBound_IsError()
        {
            var c = ValidConfig();
            c.Slots[5].Binding = BindingItem.Move(MoveDirection.Up); // E 也绑 Up（W 已绑）
            var r = LoadoutValidator.Validate(c);
            Assert.IsFalse(r.IsValid);
            Assert.IsTrue(HasIssue(r, ValidationLevel.Error, "MoveDirectionDoubleBound"));
        }

        [Test] public void SlotOverflow_IsError()
        {
            var c = ValidConfig();
            c.SlotCount = 3; // 6 绑定项 > 3 槽
            var r = LoadoutValidator.Validate(c);
            Assert.IsFalse(r.IsValid);
            Assert.IsTrue(HasIssue(r, ValidationLevel.Error, "SlotOverflow"));
        }

        [Test] public void UnavailableAbility_IsError()
        {
            var c = ValidConfig();
            c.AvailableAbilities = new HashSet<AbilityKind> { AbilityKind.Jump }; // 本关只允许 Jump
            var r = LoadoutValidator.Validate(c);
            Assert.IsFalse(r.IsValid);
            Assert.IsTrue(HasIssue(r, ValidationLevel.Error, "AbilityUnavailable"));
        }

        [Test] public void UnboundMoveDirection_IsWarning_ButStillValid()
        {
            var c = ValidConfig();
            c.Slots[2].Binding = null; // 解绑 S（Down 未绑）→ 空闲槽
            var r = LoadoutValidator.Validate(c);
            Assert.IsTrue(r.IsValid, "未绑移动放行，应仍 IsValid（不拦截）");
            Assert.IsTrue(HasIssue(r, ValidationLevel.Warning, "MoveDirectionUnbound"));
        }

        [Test] public void MoveOnOverload_IsWarning_ButStillValid()
        {
            var c = ValidConfig();
            c.Slots[0].IsOverload = true; // W（Up）标为超载
            var r = LoadoutValidator.Validate(c);
            Assert.IsTrue(r.IsValid, "移动绑超载放行");
            Assert.IsTrue(HasIssue(r, ValidationLevel.Warning, "MoveOnOverload"));
        }

        [Test] public void EmptySlot_NoIssue()
        {
            var c = ValidConfig();
            c.Slots[5].Binding = null; // E 空闲（Push 不绑）
            var r = LoadoutValidator.Validate(c);
            Assert.IsTrue(r.IsValid, "空闲槽不应报错");
            Assert.IsFalse(HasIssue(r, ValidationLevel.Error, "SlotOverflow"));
        }
    }
}
