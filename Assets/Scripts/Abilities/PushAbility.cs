using UnityEngine;
using DontPushTheButton.Config;

namespace DontPushTheButton.Abilities
{
    /// <summary>
    /// 推动能力（瞬时型）。读推动触发 → 前方射线命中 Pushable → 非物理位移。
    /// 超载强化（StatStack）：推距倍增（GDD §A 推动超载=推更远）。
    /// </summary>
    public class PushAbility : AbilityBase
    {
        public override AbilityKind Kind => AbilityKind.Push;
        public override AbilityTrigger Trigger => AbilityTrigger.Instant;
        public override OverloadParadigm Overload => OverloadParadigm.StatStack;

        [SerializeField] private PushTuning _pushTuning;
        [Tooltip("射线发射的世界 Y 高度（对准物体中心）")]
        [SerializeField] private float _rayHeight = 0.5f;
        [Tooltip("超载推动距离倍率")]
        [SerializeField] private float _overloadPushMultiplier = 2f;

        private float _lastPushTime = -999f;

        public override void TickInstant(IAbilityContext ctx)
        {
            if (_pushTuning == null) return;
            if (!ctx.WasPressedThisFrame(Kind)) return;
            if (Time.time - _lastPushTime < _pushTuning.Cooldown) return;

            Vector3 origin = ctx.Body.position;
            origin.y = _rayHeight;
            Vector3 dir = ctx.Body.forward;
            if (!Physics.Raycast(origin, dir, out RaycastHit hit, _pushTuning.Range)) return;
            if (!hit.collider.CompareTag("Pushable")) return;

            float dist = _pushTuning.PushDistance * (ctx.IsOverloadTrigger(Kind) ? _overloadPushMultiplier : 1f);
            hit.transform.position += dir * dist;
            _lastPushTime = Time.time;
        }
    }
}
