using UnityEngine;
using DontPushTheButton.Config;

namespace DontPushTheButton.Abilities
{
    /// <summary>
    /// 推动光束型（M3.3，瞬时型）。牵引光束远距推物。
    /// 普通：前方射线命中 Pushable → 位移 beamPushDistance；超载：推距 ×倍率。
    /// RaycastAll 跳过玩家自身 collider。与 PickupAbility（搬运型 M3.1）二选一（关卡固定，LevelConfigSO offer）。
    /// </summary>
    public class PushAbility : AbilityBase
    {
        public override AbilityKind Kind => AbilityKind.Push;
        public override AbilityTrigger Trigger => AbilityTrigger.Instant;
        public override OverloadParadigm Overload => OverloadParadigm.StatStack;

        [SerializeField] private PushTuning _pushTuning;
        [Tooltip("射线发射 Y 高度")]
        [SerializeField] private float _beamHeight = 0.5f;
        [Tooltip("超载推距倍率")]
        [SerializeField] private float _overloadBeamMultiplier = 2f;

        private float _lastBeamTime = -999f;

        public override void TickInstant(IAbilityContext ctx)
        {
            if (_pushTuning == null) return;
            if (!ctx.WasPressedThisFrame(Kind)) return;
            if (Time.time - _lastBeamTime < _pushTuning.Cooldown) return;

            Vector3 origin = ctx.Body.position; origin.y = _beamHeight;
            Vector3 dir = ctx.Body.forward; dir.y = 0f; dir.Normalize();

            foreach (var hit in Physics.RaycastAll(origin, dir, _pushTuning.BeamRange))
            {
                if (!hit.collider.CompareTag("Pushable")) continue; // 跳过玩家自身（Player tag）
                float dist = _pushTuning.BeamPushDistance;
                if (ctx.IsOverloadTrigger(Kind))
                {
                    dist *= _overloadBeamMultiplier;
                    ctx.ChargeOverload(Kind);
                }
                hit.transform.position += dir * dist;
                _lastBeamTime = Time.time;
                ctx.NotifyCast(Kind); // M3.10 动画驱动：确认命中并推动 → 触发 Push 动画
                break;
            }
        }
    }
}
