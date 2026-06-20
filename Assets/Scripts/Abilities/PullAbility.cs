using UnityEngine;
using DontPushTheButton.Config;

namespace DontPushTheButton.Abilities
{
    /// <summary>
    /// 拉动能力（M3.2 磁力枪·牵引，瞬时型）。
    /// 普通：前方 grabRange 内最近 Pushable → 吸向自己（位移到玩家前方 pullToDistance）。
    /// 超载（v1.0 抓钩）：前方射线命中 Pushable → 玩家瞬时位移到目标前方 hookLandDistance。
    /// 复用 Pushable tag。超载位移用 CharacterController disable/set/enable 避免 teleport 抖动。
    /// </summary>
    public class PullAbility : AbilityBase
    {
        public override AbilityKind Kind => AbilityKind.Pull;
        public override AbilityTrigger Trigger => AbilityTrigger.Instant;
        public override OverloadParadigm Overload => OverloadParadigm.StateSwitch;

        [SerializeField] private PullTuning _pullTuning;
        [Tooltip("检测/射线发射的世界 Y 高度（对准物体中心）")]
        [SerializeField] private float _grabHeight = 0.5f;

        public override void TickInstant(IAbilityContext ctx)
        {
            if (_pullTuning == null) return;
            if (!ctx.WasPressedThisFrame(Kind)) return;

            Vector3 origin = ctx.Body.position; origin.y = _grabHeight;
            Vector3 fwd = ctx.Body.forward; fwd.y = 0f; fwd.Normalize();

            if (ctx.IsOverloadTrigger(Kind))
            {
                // 超载：抓钩——射线命中 Pushable（RaycastAll 跳过玩家自身 collider）→ 玩家位移到目标前方
                foreach (var hit in Physics.RaycastAll(origin, fwd, _pullTuning.HookRange))
                {
                    if (!hit.collider.CompareTag("Pushable")) continue;
                    Vector3 land = hit.point - fwd * _pullTuning.HookLandDistance;
                    land.y = ctx.Body.position.y; // 保持玩家 Y（俯视 2.5D 不改高度）
                    Teleport(ctx, land);
                    ctx.ChargeOverload(Kind); // 超载抓钩生效，扣一次腐败
                    break;
                }
            }
            else
            {
                // 普通：前方 grabRange 内最近 Pushable → 吸向自己
                Collider nearest = null;
                float minSqr = float.MaxValue;
                foreach (var c in Physics.OverlapSphere(origin, _pullTuning.GrabRange))
                {
                    if (!c.CompareTag("Pushable")) continue;
                    float d = (c.transform.position - origin).sqrMagnitude;
                    if (d < minSqr) { minSqr = d; nearest = c; }
                }
                if (nearest != null)
                {
                    Vector3 tgt = ctx.Body.position + fwd * _pullTuning.PullToDistance;
                    tgt.y = nearest.transform.position.y; // 保持物体 Y
                    nearest.transform.position = tgt;
                }
            }
        }

        /// <summary>瞬时位移玩家（CharacterController disable/set/enable 避免 teleport 抖动）。</summary>
        private static void Teleport(IAbilityContext ctx, Vector3 pos)
        {
            var cc = ctx.Controller;
            if (cc != null) cc.enabled = false;
            ctx.Body.position = pos;
            if (cc != null) cc.enabled = true;
        }
    }
}
