using UnityEngine;
using UnityEngine.InputSystem;
using DontPushTheButton.Config;

namespace DontPushTheButton.Abilities
{
    /// <summary>
    /// 能力种类（M2.1：Move/Jump/Push；M3 将补 Pull/Dash）。
    /// </summary>
    public enum AbilityKind { Move, Jump, Push }

    /// <summary>
    /// 能力触发类型（GDD 4.2：移动是唯一持续型能力）。
    /// Continuous = 每帧读输入；Instant = 按键即触发。
    /// </summary>
    public enum AbilityTrigger { Continuous, Instant }

    /// <summary>
    /// 超载强化范式（R5 接口预留）：M2.5 实现强化时，能力按自身范式响应 SetOverloadActive。
    /// - StatStack：数值叠加（移动加速、推动推更远）
    /// - StateSwitch：状态切换（跳跃 → 短时飞行）
    /// - MultiTarget：多目标（搬运型同时搬多个）
    /// </summary>
    public enum OverloadParadigm { None, StatStack, StateSwitch, MultiTarget }

    /// <summary>
    /// 能力运行时上下文：能力通过它与物理权威/输入/调参交互，
    /// 避免依赖具体控制器类（单向依赖：controller → abilities）。
    /// 由 PlayerAbilityController 实现。
    /// </summary>
    public interface IAbilityContext
    {
        /// <summary>角色是否接地（上一帧合并 Move 后的值）。</summary>
        bool IsGrounded { get; }
        /// <summary>角色运动调参（移动/跳跃/重力/转向）。</summary>
        MovementTuning Tuning { get; }
        /// <summary>角色 Transform（能力读朝向/位移、写转向）。</summary>
        Transform Body { get; }
        /// <summary>水平移动方向所相对的相机。</summary>
        Camera RelativeCamera { get; }

        /// <summary>能力→输入动作的集中映射（M2.5 改为查绑定表）。</summary>
        InputAction GetAction(AbilityKind kind);
        /// <summary>提交本帧水平位移意图（世界空间，已含 deltaTime）。</summary>
        void AddHorizontal(Vector3 worldDisp);
        /// <summary>设定垂直速度（起跳用）。</summary>
        void SetVerticalVelocity(float v);
    }

    /// <summary>
    /// 能力契约（M2.2 绑定系统按此接口装配能力；M2.5 运行时重绑后可替换能力实例）。
    /// </summary>
    public interface IAbility
    {
        AbilityKind Kind { get; }
        AbilityTrigger Trigger { get; }
        /// <summary>超载强化开关（R5 预留，本期空实现，M2.5 填）。</summary>
        void SetOverloadActive(bool active);
    }

    /// <summary>
    /// 能力抽象基类（MonoBehaviour 组件，挂在玩家对象上）。
    /// 持续型覆写 TickContinuous，瞬时型覆写 TickInstant；
    /// 由 PlayerAbilityController 每帧按 Trigger 分流调用。
    /// </summary>
    public abstract class AbilityBase : MonoBehaviour, IAbility
    {
        public abstract AbilityKind Kind { get; }
        public abstract AbilityTrigger Trigger { get; }

        /// <summary>本能力采用的超载范式（R5 预留；本期默认 None，M2.5 各能力覆写）。</summary>
        public virtual OverloadParadigm Overload => OverloadParadigm.None;

        /// <summary>持续型：每帧调用（读连续输入、产出位移/转向意图）。</summary>
        public virtual void TickContinuous(IAbilityContext ctx) { }

        /// <summary>瞬时型：每帧调用，能力内部检测按键按下再执行效果。</summary>
        public virtual void TickInstant(IAbilityContext ctx) { }

        /// <summary>R5 预留：超载强化开关，本期空实现，M2.5 填。</summary>
        public virtual void SetOverloadActive(bool active) { }
    }
}
