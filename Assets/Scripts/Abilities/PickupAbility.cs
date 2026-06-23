using System;
using System.Collections.Generic;
using UnityEngine;
using DontPushTheButton.Config;

namespace DontPushTheButton.Abilities
{
    /// <summary>
    /// 拾取能力（M3.1 磁力枪·抓取搬运型，持续型）。
    /// 切换式：按拾取键 → Idle 抓取前方 grabRange 内 Pushable（普通1个 / 超载范围内上限 maxCarryCount）→ Carrying；
    /// 持有期间物体每帧吸附玩家前方 carryDistance（多个横向分布）；再按 → 释放（原地放下）→ Idle。
    /// 非物理（直接 setPosition，与 M2.1 H2 一致）。超载搬多个 = 抓取成功时 ChargeOverload 扣一次。
    /// </summary>
    public class PickupAbility : AbilityBase
    {
        public override AbilityKind Kind => AbilityKind.Pickup;
        public override AbilityTrigger Trigger => AbilityTrigger.Continuous;
        public override OverloadParadigm Overload => OverloadParadigm.MultiTarget;

        [SerializeField] private PickupTuning _pickupTuning;
        [Tooltip("抓取/持有时物体的世界 Y 高度（对准物体中心）")]
        [SerializeField] private float _grabHeight = 0.5f;
        [Tooltip("持有物体相对玩家中心的 Y 偏移（玩家起跳时物体跟着起跳；-0.5≈物体在脚部地面高度）")]
        [SerializeField] private float _carryYOffset = -0.5f;

        private enum CarryState { Idle, Carrying }
        private CarryState _state = CarryState.Idle;
        private readonly List<Transform> _carried = new List<Transform>();
        private readonly List<Collider> _hitBuffer = new List<Collider>();

        /// <summary>是否正在搬运物体（M3.10 动画驱动：Carrying 态）。</summary>
        public bool IsCarrying => _state == CarryState.Carrying;

        /// <summary>搬运状态变化事件（M3.10 动画驱动）：true=捡起瞬间，false=放下瞬间。供动画切捡起/放下姿态。</summary>
        public event Action<bool> OnCarryChanged;

        public override void TickContinuous(IAbilityContext ctx)
        {
            if (_pickupTuning == null) return;

            if (ctx.WasPressedThisFrame(Kind))
            {
                if (_state == CarryState.Idle) TryGrab(ctx);
                else Release();
            }

            if (_state == CarryState.Carrying) UpdateCarryPositions(ctx);
        }

        /// <summary>抓取前方 grabRange 内 Pushable：普通 1 个（最近），超载范围内上限 maxCarryCount。</summary>
        private void TryGrab(IAbilityContext ctx)
        {
            Vector3 center = ctx.Body.position;
            center.y = _grabHeight;
            // 收集范围内 Pushable
            _hitBuffer.Clear();
            foreach (var hit in Physics.OverlapSphere(center, _pickupTuning.GrabRange))
                if (hit.CompareTag("Pushable")) _hitBuffer.Add(hit);
            if (_hitBuffer.Count == 0) return;

            // 按到玩家距离排序（最近优先）
            _hitBuffer.Sort((a, b) =>
                (a.transform.position - center).sqrMagnitude
                .CompareTo((b.transform.position - center).sqrMagnitude));

            bool overload = ctx.IsOverloadTrigger(Kind);
            int count = overload
                ? Mathf.Min(_pickupTuning.MaxCarryCount, _hitBuffer.Count)
                : 1;

            _carried.Clear();
            for (int i = 0; i < count; i++) _carried.Add(_hitBuffer[i].transform);
            _state = CarryState.Carrying;
            OnCarryChanged?.Invoke(true); // M3.10 动画驱动：捡起瞬间

            if (overload) ctx.ChargeOverload(Kind); // 超载搬多个，抓取成功扣一次腐败
        }

        /// <summary>释放：持有物体停在当前跟随位置（放下）。</summary>
        private void Release()
        {
            _carried.Clear();
            _state = CarryState.Idle;
            OnCarryChanged?.Invoke(false); // M3.10 动画驱动：放下瞬间
        }

        /// <summary>持有物体每帧吸附玩家前方 carryDistance，多个横向均匀分布。</summary>
        private void UpdateCarryPositions(IAbilityContext ctx)
        {
            Vector3 fwd = ctx.Body.forward; fwd.y = 0f; fwd.Normalize();
            Vector3 right = ctx.Body.right; right.y = 0f; right.Normalize();
            Vector3 basePos = ctx.Body.position + fwd * _pickupTuning.CarryDistance;

            int n = _carried.Count;
            for (int i = 0; i < n; i++)
            {
                float offset = (i - (n - 1) * 0.5f) * _pickupTuning.CarrySpacing; // 居中横向分布
                Vector3 p = basePos + right * offset;
                p.y = ctx.Body.position.y + _carryYOffset; // 跟随玩家 Y（起跳时物体跟着起跳）
                _carried[i].position = p;
            }
        }
    }
}
