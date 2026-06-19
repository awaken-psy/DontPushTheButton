using UnityEngine;

namespace DontPushTheButton.Abilities
{
    /// <summary>
    /// 移动能力（持续型，GDD 4.2 唯一持续型能力）。
    /// 读 WASD → 相对相机水平面产出位移 + 面向移动方向转向。
    /// 位移以意图提交给 PlayerAbilityController.AddHorizontal（物理 Move 由 controller 统一执行）。
    /// </summary>
    public class MoveAbility : AbilityBase
    {
        public override AbilityKind Kind => AbilityKind.Move;
        public override AbilityTrigger Trigger => AbilityTrigger.Continuous;

        [Tooltip("输入死区：水平输入向量平方长度低于此值视为静止（不移动/不转向）")]
        [SerializeField] private float _moveInputThreshold = 0.01f;

        public override void TickContinuous(IAbilityContext ctx)
        {
            var action = ctx.GetAction(Kind);
            Vector2 stick = action != null ? action.ReadValue<Vector2>() : Vector2.zero;

            Camera cam = ctx.RelativeCamera;
            Vector3 fwd = cam ? cam.transform.forward : Vector3.forward;
            Vector3 right = cam ? cam.transform.right : Vector3.right;
            fwd.y = 0f; fwd.Normalize();
            right.y = 0f; right.Normalize();

            Vector3 moveDir = fwd * stick.y + right * stick.x;
            if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();

            // 水平位移意图交给 controller（含 deltaTime）
            ctx.AddHorizontal(moveDir * (ctx.Tuning.MoveSpeed * Time.deltaTime));

            // 面向移动方向
            if (moveDir.sqrMagnitude > _moveInputThreshold)
            {
                Quaternion target = Quaternion.LookRotation(moveDir, Vector3.up);
                float turn = ctx.Tuning.TurnSpeed;
                ctx.Body.rotation = turn <= 0f ? target
                    : Quaternion.RotateTowards(ctx.Body.rotation, target, turn * Time.deltaTime);
            }
        }
    }
}
