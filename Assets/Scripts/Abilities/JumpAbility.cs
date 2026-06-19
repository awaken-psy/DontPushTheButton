using UnityEngine;

namespace DontPushTheButton.Abilities
{
    /// <summary>
    /// 跳跃能力（瞬时型）。读跳跃键 → 接地时按 v=√(2gh) 设起跳速度。
    /// 起跳速度经 PlayerAbilityController.SetVerticalVelocity 提交，
    /// 重力与合并 Move 由 controller 统一处理（保证 isGrounded 时机正确）。
    /// </summary>
    public class JumpAbility : AbilityBase
    {
        public override AbilityKind Kind => AbilityKind.Jump;
        public override AbilityTrigger Trigger => AbilityTrigger.Instant;

        public override void TickInstant(IAbilityContext ctx)
        {
            var action = ctx.GetAction(Kind);
            if (action == null || !action.WasPressedThisFrame()) return;
            if (!ctx.IsGrounded) return;

            // v = √(2·g·h)，g 取绝对值
            float g = Mathf.Abs(ctx.Tuning.Gravity);
            ctx.SetVerticalVelocity(Mathf.Sqrt(2f * g * ctx.Tuning.JumpHeight));
        }
    }
}
