using System;
using DontPushTheButton.Abilities;

namespace DontPushTheButton.Binding
{
    /// <summary>
    /// 移动方向（WASD 4 方向，各占 1 独立槽）。GDD §A / 4.5。
    /// </summary>
    public enum MoveDirection { Up, Down, Left, Right }

    /// <summary>
    /// 绑定项类型：移动方向 | 能力。
    /// </summary>
    public enum BindingItemType { MoveDirection, Ability }

    /// <summary>
    /// 绑定项：能被绑到按键槽位上的东西（移动方向 或 能力）。GDD §A。
    /// 纯值类型（无 Unity 依赖），便于单测。
    /// </summary>
    public readonly struct BindingItem : IEquatable<BindingItem>
    {
        public readonly BindingItemType Type;
        public readonly MoveDirection Direction; // Type == MoveDirection 时有效
        public readonly AbilityKind Ability;     // Type == Ability 时有效

        private BindingItem(BindingItemType type, MoveDirection dir, AbilityKind ability)
        {
            Type = type; Direction = dir; Ability = ability;
        }

        public static BindingItem Move(MoveDirection dir) => new BindingItem(BindingItemType.MoveDirection, dir, default);
        public static BindingItem Of(AbilityKind ability) => new BindingItem(BindingItemType.Ability, default, ability);

        public bool Equals(BindingItem other) =>
            Type == other.Type
            && (Type == BindingItemType.MoveDirection ? Direction == other.Direction : Ability == other.Ability);

        public override bool Equals(object obj) => obj is BindingItem b && Equals(b);
        public override int GetHashCode() =>
            Type == BindingItemType.MoveDirection ? (int)Type * 17 + (int)Direction : (int)Type * 17 + (int)Ability;
        public static bool operator ==(BindingItem a, BindingItem b) => a.Equals(b);
        public static bool operator !=(BindingItem a, BindingItem b) => !a.Equals(b);

        public override string ToString() =>
            Type == BindingItemType.MoveDirection ? $"Move:{Direction}" : $"Ability:{Ability}";
    }
}
