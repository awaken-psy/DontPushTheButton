using UnityEngine;
using UnityEngine.InputSystem;
using DontPushTheButton.Config;

namespace DontPushTheButton.Abilities
{
    /// <summary>
    /// 能力种类（Move/Jump/Pickup/Pull/Dash/Push（M3.3 推动光束型，与 Pickup 搬运型二选一））。
    /// </summary>
    public enum AbilityKind { Move, Jump, Pickup, Pull, Dash, Push }

    /// <summary>
    /// 能力触发类型（GDD 4.2：移动是唯一持续型能力）。
    /// </summary>
    public enum AbilityTrigger { Continuous, Instant }

    /// <summary>
    /// 超载强化范式（R5）：M2.5 能力按自身范式响应超载触发。
    /// </summary>
    public enum OverloadParadigm { None, StatStack, StateSwitch, MultiTarget }

    /// <summary>
    /// 能力运行时上下文（M2.5：从「暴露 InputAction」改为「暴露输入状态」）。
    /// controller 轮询 LoadoutConfig 填充 MoveInput/WasPressedThisFrame/IsOverloadTrigger，
    /// 能力据此执行 + 强化，不直接碰 InputSystem。由 PlayerAbilityController 实现。
    /// </summary>
    public interface IAbilityContext
    {
        bool IsGrounded { get; }
        CharacterController Controller { get; }
        MovementTuning Tuning { get; }
        Transform Body { get; }
        Camera RelativeCamera { get; }

        /// <summary>本帧移动输入（归一化方向，-1..1；轮询移动槽 isPressed 累积）。</summary>
        Vector2 MoveInput { get; }
        /// <summary>该能力本帧是否被触发（绑定的键按下）。</summary>
        bool WasPressedThisFrame(AbilityKind kind);
        /// <summary>该能力本次触发是否超载强化（绑在超载键上）。</summary>
        bool IsOverloadTrigger(AbilityKind kind);
        /// <summary>能力确认执行超载强化时调用，扣一次腐败（每按下只扣一次；未生效不扣）。返回是否本次扣费。</summary>
        bool ChargeOverload(AbilityKind kind);

        void AddHorizontal(Vector3 worldDisp);
        void SetVerticalVelocity(float v);

        /// <summary>瞬时能力确认执行后回报（M3.10 动画驱动：统一瞬时事件出口，由 PlayerAbilityController raise OnInstantCast）。</summary>
        void NotifyCast(AbilityKind kind);

        /// <summary>腐败移速倍率（M3.8：层级2+ &lt;1 降速；否则 1）。转发 CorruptionEffects。</summary>
        float CorruptionSpeedMultiplier { get; }
        /// <summary>腐败冷却倍率（M3.8：层级3+ &gt;1 延长；否则 1）。转发 CorruptionEffects。</summary>
        float CorruptionCooldownMultiplier { get; }
    }

    /// <summary>
    /// 能力契约（M2.2 绑定系统/M2.5 运行时按此装配）。
    /// </summary>
    public interface IAbility
    {
        AbilityKind Kind { get; }
        AbilityTrigger Trigger { get; }
    }

    /// <summary>
    /// 能力抽象基类（MonoBehaviour 组件）。持续型覆写 TickContinuous，瞬时型覆写 TickInstant。
    /// </summary>
    public abstract class AbilityBase : MonoBehaviour, IAbility
    {
        public abstract AbilityKind Kind { get; }
        public abstract AbilityTrigger Trigger { get; }
        public virtual OverloadParadigm Overload => OverloadParadigm.None;
        public virtual void TickContinuous(IAbilityContext ctx) { }
        public virtual void TickInstant(IAbilityContext ctx) { }
    }
}
