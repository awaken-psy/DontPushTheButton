using UnityEngine;

namespace DontPushTheButton.Abilities
{
    /// <summary>
    /// 移动能力（持续型）。读 ctx.MoveInput → 产出位移 + 转向。
    /// 超载强化（StatStack）：moveSpeed 倍增（GDD §A 移动加速，高频→腐败快，极限策略）。
    /// </summary>
    public class MoveAbility : AbilityBase
    {
        public override AbilityKind Kind => AbilityKind.Move;
        public override AbilityTrigger Trigger => AbilityTrigger.Continuous;
        public override OverloadParadigm Overload => OverloadParadigm.StatStack;

        [Tooltip("输入死区")]
        [SerializeField] private float _moveInputThreshold = 0.01f;
        [Tooltip("超载移动速度倍率（H3 写死，M3.5 走 SO）")]
        [SerializeField] private float _overloadSpeedMultiplier = 1.8f;

        public override void TickContinuous(IAbilityContext ctx)
        {
            Vector2 stick = ctx.MoveInput;
            Camera cam = ctx.RelativeCamera;
            Vector3 fwd = cam ? cam.transform.forward : Vector3.forward;
            Vector3 right = cam ? cam.transform.right : Vector3.right;
            fwd.y = 0f; fwd.Normalize();
            right.y = 0f; right.Normalize();

            Vector3 moveDir = fwd * stick.y + right * stick.x;
            if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();

            float speed = ctx.Tuning.MoveSpeed;
            if (ctx.IsOverloadTrigger(AbilityKind.Move))
            {
                speed *= _overloadSpeedMultiplier;
                ctx.ChargeOverload(AbilityKind.Move); // 启动超载加速扣一次（按下帧消费，持续按住不重复扣）
            }
            speed *= ctx.CorruptionSpeedMultiplier; // M3.8：腐败层级2+ 降速
            ctx.AddHorizontal(moveDir * (speed * Time.deltaTime));

            if (moveDir.sqrMagnitude > _moveInputThreshold)
            {
                var target = Quaternion.LookRotation(moveDir, Vector3.up);
                float turn = ctx.Tuning.TurnSpeed;
                ctx.Body.rotation = turn <= 0f ? target
                    : Quaternion.RotateTowards(ctx.Body.rotation, target, turn * Time.deltaTime);
            }
        }
    }
}
