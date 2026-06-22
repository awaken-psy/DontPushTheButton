using UnityEngine;
using DontPushTheButton.Config;

namespace DontPushTheButton.Abilities
{
    /// <summary>
    /// 冲刺能力（M3.4，瞬时型）。关 4 引入。
    /// 普通：向移动输入方向（无输入按朝向）瞬时突进 dashDistance（Raycast 防穿墙）。
    /// 超载（J4）：dash×2 = 距离×2，按 1 次触发算 1 次腐败。
    /// Teleport 模式（CharacterController disable/set/enable），与 PullAbility 一致。
    /// </summary>
    public class DashAbility : AbilityBase
    {
        public override AbilityKind Kind => AbilityKind.Dash;
        public override AbilityTrigger Trigger => AbilityTrigger.Instant;
        public override OverloadParadigm Overload => OverloadParadigm.StatStack; // 超载距离×2

        [SerializeField] private DashTuning _dashTuning;
        [Tooltip("Raycast 防穿墙检测的 Y 高度")]
        [SerializeField] private float _dashHeight = 0.5f;

        private float _lastDashTime = -999f;

        public override void TickInstant(IAbilityContext ctx)
        {
            if (_dashTuning == null) return;
            if (!ctx.WasPressedThisFrame(Kind)) return;
            if (Time.time - _lastDashTime < _dashTuning.Cooldown) return;

            // 方向：移动输入（有输入）或朝向（无输入）
            Vector2 moveIn = ctx.MoveInput;
            Vector3 fwd = ctx.Body.forward; fwd.y = 0f; fwd.Normalize();
            Vector3 right = ctx.Body.right; right.y = 0f; right.Normalize();
            Vector3 dir = right * moveIn.x + fwd * moveIn.y;
            if (dir.sqrMagnitude < 0.01f) return; // 只朝当前移动方向，无移动输入不 dash
            dir.Normalize();

            bool overload = ctx.IsOverloadTrigger(Kind);
            float dist = _dashTuning.DashDistance * (overload ? 2f : 1f); // 超载 dash×2

            // Raycast 防穿墙：最近非玩家障碍
            Vector3 origin = ctx.Body.position; origin.y = _dashHeight;
            float travel = dist;
            bool blocked = false;
            foreach (var hit in Physics.RaycastAll(origin, dir, dist))
            {
                if (hit.collider.transform == ctx.Body) continue; // 跳过玩家自身
                if (hit.distance < travel) { travel = hit.distance; blocked = true; }
            }
            if (blocked) travel = Mathf.Max(0f, travel - 0.1f); // 墙前 0.1m 停

            Vector3 target = ctx.Body.position + dir * travel;
            target.y = ctx.Body.position.y; // 保持 Y
            Teleport(ctx, target);
            _lastDashTime = Time.time;
            if (overload) ctx.ChargeOverload(Kind); // 超载 dash×2 算 1 次腐败
            ctx.NotifyCast(Kind); // M3.10 动画驱动：确认执行 → 触发 Dash 动画
        }

        private static void Teleport(IAbilityContext ctx, Vector3 pos)
        {
            var cc = ctx.Controller;
            if (cc != null) cc.enabled = false;
            ctx.Body.position = pos;
            if (cc != null) cc.enabled = true;
        }
    }
}
