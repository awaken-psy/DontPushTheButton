using UnityEngine;
using DontPushTheButton.Config;

namespace DontPushTheButton.Abilities
{
    /// <summary>
    /// 推动能力（瞬时型，M2.1 最简版）。
    /// 读推动键 → 前方射线命中「Pushable」物体 → 非物理 transform 直接位移一段（H2「非物理碰撞」）。
    /// M3.1 将升级为磁力枪搬运型推动（替换本最简实现）。
    /// </summary>
    public class PushAbility : AbilityBase
    {
        public override AbilityKind Kind => AbilityKind.Push;
        public override AbilityTrigger Trigger => AbilityTrigger.Instant;

        [Tooltip("推动调参（作用距离/推距/冷却）")]
        [SerializeField] private PushTuning _pushTuning;
        [Tooltip("射线发射的世界 Y 高度 (m)：对准可推物体中心高度（1m 箱子中心约 0.5）")]
        [SerializeField] private float _rayHeight = 0.5f;

        private float _lastPushTime = -999f;

        public override void TickInstant(IAbilityContext ctx)
        {
            if (_pushTuning == null) return;
            var action = ctx.GetAction(Kind);
            if (action == null || !action.WasPressedThisFrame()) return;

            // 冷却（本期 PushTuning 默认 0）
            if (Time.time - _lastPushTime < _pushTuning.Cooldown) return;

            // 射线从玩家 XZ 位置、固定高度（对准物体中心）向前射
            Vector3 origin = ctx.Body.position;
            origin.y = _rayHeight;
            Vector3 dir = ctx.Body.forward;
            if (!Physics.Raycast(origin, dir, out RaycastHit hit, _pushTuning.Range)) return;

            // 命中可推物体：transform 直接位移（非物理引擎，H2）
            if (hit.collider.CompareTag("Pushable"))
            {
                hit.transform.position += dir * _pushTuning.PushDistance;
                _lastPushTime = Time.time;
            }
        }
    }
}
