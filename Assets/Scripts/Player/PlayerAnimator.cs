using System.Collections.Generic;
using UnityEngine;
using DontPushTheButton.Abilities;
using DontPushTheButton.Corruption;

namespace DontPushTheButton.Player
{
    /// <summary>
    /// 玩家动画驱动（M3.10）：把能力/运动状态桥接到 Animator 参数。
    /// 双方契约（参数名/类型/来源）见 docs/design/动画系统.md。
    /// - LateUpdate 读 PlayerAbilityController/JumpAbility/PickupAbility/CorruptionTracker 的公开状态 → SetFloat/SetBool；
    /// - 瞬时能力（Dash/Pull/Push）经 PlayerAbilityController.OnInstantCast → SetTrigger。
    /// Animator 必须关 Apply Root Motion（朝向已由 MoveAbility 代码强制驱动）。
    /// 容错：Animator/各能力引用缺失，或参数未在 Controller 中定义时静默跳过（队友可增量建 Controller，不刷 warning）。
    /// 无可调数值参数——所有阈值（速度切跑、过渡时长）由 Animator Controller 的 blend tree / transition 处理。
    /// </summary>
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(PlayerAbilityController))]
    public class PlayerAnimator : MonoBehaviour
    {
        // ---- Animator 参数契约（队友照此建 Controller；改名双方同步）----
        private const string Speed = "Speed";
        private const string MoveX = "MoveX";
        private const string MoveY = "MoveY";
        private const string VerticalVelocity = "VerticalVelocity";
        private const string IsGrounded = "IsGrounded";
        private const string IsFlying = "IsFlying";
        private const string IsSlowFall = "IsSlowFall";
        private const string IsCarrying = "IsCarrying";
        private const string Corruption = "Corruption";
        // Trigger（瞬时）按 AbilityKind 路由，名字与枚举同名
        private const string TrigDash = "Dash";
        private const string TrigPull = "Pull";
        private const string TrigPush = "Push";

        private Animator _animator;
        private PlayerAbilityController _controller;
        private JumpAbility _jump;
        private PickupAbility _pickup;
        private CorruptionTracker _corruption;
        private HashSet<string> _params;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _controller = GetComponent<PlayerAbilityController>();
            _jump = GetComponent<JumpAbility>();
            _pickup = GetComponent<PickupAbility>();
            _corruption = GetComponent<CorruptionTracker>();
            CacheParameters();
        }

        /// <summary>缓存 Animator 已定义的参数名，未定义的 setter 静默跳过（队友增量建 Controller 不报 warning）。</summary>
        private void CacheParameters()
        {
            _params = new HashSet<string>();
            if (_animator == null) return;
            foreach (var p in _animator.parameters) _params.Add(p.name);
        }

        private void OnEnable()
        {
            if (_controller != null) _controller.OnInstantCast += OnInstantCast;
        }

        private void OnDisable()
        {
            if (_controller != null) _controller.OnInstantCast -= OnInstantCast;
        }

        private void LateUpdate()
        {
            if (_animator == null || _controller == null) return;

            Vector2 move = _controller.MoveInput;
            SetFloat(Speed, move.magnitude);
            SetFloat(MoveX, move.x);
            SetFloat(MoveY, move.y);
            SetFloat(VerticalVelocity, _controller.VerticalVelocity);
            SetBool(IsGrounded, _controller.IsGrounded);
            if (_jump != null)
            {
                SetBool(IsFlying, _jump.IsFlying);
                SetBool(IsSlowFall, _jump.IsSlowFalling);
            }
            if (_pickup != null) SetBool(IsCarrying, _pickup.IsCarrying);
            if (_corruption != null) SetFloat(Corruption, _corruption.Normalized);
        }

        /// <summary>瞬时能力确认执行 → 路由到对应 Trigger（M3.10）。</summary>
        private void OnInstantCast(AbilityKind kind)
        {
            switch (kind)
            {
                case AbilityKind.Dash: SetTrigger(TrigDash); break;
                case AbilityKind.Pull: SetTrigger(TrigPull); break;
                case AbilityKind.Push: SetTrigger(TrigPush); break;
            }
        }

        // ---- 容错 setter：参数未在 Controller 定义时静默跳过 ----
        private void SetFloat(string name, float v) { if (Has(name)) _animator.SetFloat(name, v); }
        private void SetBool(string name, bool v) { if (Has(name)) _animator.SetBool(name, v); }
        private void SetTrigger(string name) { if (Has(name)) _animator.SetTrigger(name); }
        private bool Has(string name) => _params != null && _params.Contains(name);
    }
}
