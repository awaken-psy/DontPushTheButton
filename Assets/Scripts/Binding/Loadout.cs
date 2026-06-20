using System.Collections.Generic;
using DontPushTheButton.Abilities;

namespace DontPushTheButton.Binding
{
    /// <summary>
    /// 按键槽位：一个物理按键 + 超载身份标记（关卡指定）+ 绑定项（可空=空闲槽）。GDD §A / 4.3 / 4.5。
    /// </summary>
    public class KeySlot
    {
        /// <summary>逻辑键名（如 "W"/"Space"/"E"），与 InputSystem binding 对应。</summary>
        public string KeyName;
        /// <summary>该按键是否为本关的超载身份（按键身份标记，非独立槽；GDD §A / 4.3）。</summary>
        public bool IsOverload;
        /// <summary>绑定的项；null = 空闲槽（4.5 允许空闲，给评分奖励）。</summary>
        public BindingItem? Binding;

        public KeySlot(string keyName, bool isOverload = false, BindingItem? binding = null)
        {
            KeyName = keyName; IsOverload = isOverload; Binding = binding;
        }
    }

    /// <summary>
    /// 校验问题级别。Error = 致命（配置无效）；Warning = 放行但有代价（GDD §A）。
    /// </summary>
    public enum ValidationLevel { Error, Warning }

    /// <summary>
    /// 单条校验问题。
    /// </summary>
    public readonly struct ValidationIssue
    {
        public readonly ValidationLevel Level;
        public readonly string Code;    // 机器可读码，如 "AbilityDoubleBound"
        public readonly string Message; // 人类可读
        public ValidationIssue(ValidationLevel level, string code, string message)
        { Level = level; Code = code; Message = message; }
        public override string ToString() => $"[{Level}] {Code}: {Message}";
    }

    /// <summary>
    /// 校验结果：错误（致命）+ 警告（放行）。IsValid = 无 Error。
    /// </summary>
    public class ValidationResult
    {
        public readonly List<ValidationIssue> Issues = new List<ValidationIssue>();

        public bool IsValid
        {
            get { for (int i = 0; i < Issues.Count; i++) if (Issues[i].Level == ValidationLevel.Error) return false; return true; }
        }

        public bool HasWarnings
        {
            get { for (int i = 0; i < Issues.Count; i++) if (Issues[i].Level == ValidationLevel.Warning) return true; return false; }
        }

        public void Add(ValidationLevel level, string code, string message)
            => Issues.Add(new ValidationIssue(level, code, message));
    }

    /// <summary>
    /// 玩家配置（一关的绑键结果）。槽位数 + 槽位列表 + 本关可用能力（M2.3 LevelConfigSO 提供）。
    /// </summary>
    public class LoadoutConfig
    {
        /// <summary>本关槽位数（默认 6，关 5 可 7；GDD 4.5）。</summary>
        public int SlotCount = 6;
        /// <summary>槽位列表（每个对应一个物理按键）。</summary>
        public List<KeySlot> Slots = new List<KeySlot>();
        /// <summary>本关可用能力集合（M2.3 LevelConfig 传入；绑定项的能力必须在此集合内）。</summary>
        public HashSet<AbilityKind> AvailableAbilities = new HashSet<AbilityKind>();
    }
}
