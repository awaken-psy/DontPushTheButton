using UnityEngine;

namespace DontPushTheButton.Abilities
{
    /// <summary>
    /// 跳跃能力（瞬时型）。读跳跃触发 → 接地时按 v=√(2gh) 起跳。
    /// 超载强化（StateSwitch）：短时飞行——初速倍增 + 飞行期持续上升速度（GDD §A 跳跃超载=短飞；H3 时长写死，M3.5 缓落+SO）。
    /// </summary>
    public class JumpAbility : AbilityBase
    {
        public override AbilityKind Kind => AbilityKind.Jump;
        public override AbilityTrigger Trigger => AbilityTrigger.Instant;
        public override OverloadParadigm Overload => OverloadParadigm.StateSwitch;

        [Tooltip("超载跳跃初速倍率")]
        [SerializeField] private float _overloadJumpMultiplier = 1.4f;
        [Tooltip("超载飞行持续时间（秒）")]
        [SerializeField] private float _overloadFlyDuration = 0.45f;
        [Tooltip("超载飞行期持续上升速度（m/s）")]
        [SerializeField] private float _overloadFlyVelocity = 6f;

        private float _flyEndTime = -1f;

        public override void TickInstant(IAbilityContext ctx)
        {
            if (ctx.WasPressedThisFrame(Kind))
            {
                if (!ctx.IsGrounded) return;
                float g = Mathf.Abs(ctx.Tuning.Gravity);
                float v = Mathf.Sqrt(2f * g * ctx.Tuning.JumpHeight);
                if (ctx.IsOverloadTrigger(Kind))
                {
                    v *= _overloadJumpMultiplier;
                    _flyEndTime = Time.time + _overloadFlyDuration;
                    ctx.ChargeOverload(Kind); // 确认超载起跳（已过 IsGrounded），扣一次腐败
                }
                ctx.SetVerticalVelocity(v);
            }
            // 超载短飞：飞行期持续上升（每帧设正速度，克服重力）
            if (_flyEndTime > Time.time)
                ctx.SetVerticalVelocity(_overloadFlyVelocity);
            else if (_flyEndTime > 0f && Time.time >= _flyEndTime)
                _flyEndTime = -1f;
        }
    }
}
