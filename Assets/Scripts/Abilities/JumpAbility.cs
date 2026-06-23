using UnityEngine;
using DontPushTheButton.Config;

namespace DontPushTheButton.Abilities
{
    /// <summary>
    /// 跳跃能力（M3.5 持续型，飞行状态机 + 缓落）。
    /// 普通跳：接地按 → √(2gh) 起跳 → 正常重力落。
    /// 超载（StateSwitch）：起跳(×倍率) → Flying(飞行期持续上升) → SlowFall(缓落，重力×FallGravityScale) → Ground。
    /// 飞行/缓落需每帧 → Trigger=Continuous（M2.5 原 Instant）。JumpTuning SO 调参（H3：M2.5 写死/M3.5 SO+缓落）。
    /// </summary>
    public class JumpAbility : AbilityBase
    {
        public override AbilityKind Kind => AbilityKind.Jump;
        public override AbilityTrigger Trigger => AbilityTrigger.Continuous;
        public override OverloadParadigm Overload => OverloadParadigm.StateSwitch;

        [SerializeField] private JumpTuning _tuning;

        private enum FlyState { Ground, Ascend, Flying, SlowFall }
        private FlyState _state = FlyState.Ground;
        private float _flyEndTime = -1f;

        /// <summary>超载飞行中（M3.10 动画驱动：Flying 态）。</summary>
        public bool IsFlying => _state == FlyState.Flying;
        /// <summary>缓落态（超载飞行结束后下落），PlayerAbilityController.ApplyGravity 据此减重力。</summary>
        public bool IsSlowFalling => _state == FlyState.SlowFall;
        public float FallGravityScale => _tuning != null ? _tuning.FallGravityScale : 1f;

        public override void TickContinuous(IAbilityContext ctx)
        {
            if (_tuning == null) return;

            // 起跳（按键 + 接地）
            if (ctx.WasPressedThisFrame(Kind) && ctx.IsGrounded)
            {
                float g = Mathf.Abs(ctx.Tuning.Gravity);
                float v = Mathf.Sqrt(2f * g * ctx.Tuning.JumpHeight);
                if (ctx.IsOverloadTrigger(Kind))
                {
                    v *= _tuning.OverloadJumpMultiplier;
                    _flyEndTime = Time.time + _tuning.OverloadFlyDuration;
                    _state = FlyState.Flying;
                    ctx.ChargeOverload(Kind); // 超载起跳扣一次腐败
                }
                else
                {
                    _state = FlyState.Ascend;
                }
                ctx.SetVerticalVelocity(v);
            }

            // 状态更新（每帧）
            switch (_state)
            {
                case FlyState.Flying:
                    if (Time.time < _flyEndTime) ctx.SetVerticalVelocity(_tuning.OverloadFlyVelocity);
                    else _state = FlyState.SlowFall; // 飞行结束 → 缓落
                    break;
                case FlyState.Ascend:
                    if (ctx.IsGrounded) _state = FlyState.Ground; // 普通跳落地
                    break;
                case FlyState.SlowFall:
                    if (ctx.IsGrounded) _state = FlyState.Ground; // 缓落落地
                    break;
            }
        }
    }
}
