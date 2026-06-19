using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using DontPushTheButton.Abilities;
using DontPushTheButton.Config;
using DontPushTheButton.Input;

namespace DontPushTheButton.Player
{
    /// <summary>
    /// 能力驱动的角色物理权威（M2.1 重构，替代 M1 的 PlayerMover）。
    /// 职责：收集同对象上所有 Ability 的运动意图 → 应用重力 → 合并水平+垂直成
    /// **单次 CharacterController.Move**（保证 isGrounded 时机正确，规避 M1.4 双 Move 坑）。
    /// 能力只产生意图（水平位移 / 起跳速度 / 推箱作用），物理 Move 只在此处发生。
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerControls))]
    public class PlayerAbilityController : MonoBehaviour, IAbilityContext
    {
        [Header("引用")]
        [Tooltip("角色运动调参（移动/跳跃/重力/转向）")]
        [SerializeField] private MovementTuning _tuning;
        [Tooltip("水平移动方向所相对的相机；留空则用 Camera.main")]
        [SerializeField] private Camera _relativeCamera;

        private CharacterController _controller;
        private PlayerControls _controls;
        private readonly List<AbilityBase> _abilities = new List<AbilityBase>();

        private float _verticalVelocity;      // Y 轴速度（跳跃/重力累积）
        private Vector3 _horizontalThisFrame; // 本帧各能力累加的水平位移意图

        // ---- 暴露给能力的 API ----
        public bool IsGrounded => _controller.isGrounded;
        public CharacterController Controller => _controller;
        public MovementTuning Tuning => _tuning;
        public Transform Body => transform;
        public Camera RelativeCamera => _relativeCamera;

        /// <summary>
        /// 能力→输入动作的集中映射。
        /// M2.5 运行时动态重绑时，把这里改成查绑定表（LevelConfig → 槽位 → 按键）。
        /// </summary>
        public InputAction GetAction(AbilityKind kind) => kind switch
        {
            AbilityKind.Move => _controls.MoveAction,
            AbilityKind.Jump => _controls.JumpAction,
            AbilityKind.Push => _controls.PushAction,
            _ => null,
        };

        /// <summary>能力提交本帧水平位移（世界空间，已含 deltaTime）。</summary>
        public void AddHorizontal(Vector3 worldDisp) => _horizontalThisFrame += worldDisp;

        /// <summary>能力设定垂直速度（JumpAbility 起跳用）。</summary>
        public void SetVerticalVelocity(float v) => _verticalVelocity = v;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _controls = GetComponent<PlayerControls>();
            GetComponents(_abilities); // 收集同对象上所有能力组件
            if (_tuning == null) Debug.LogError("[PlayerAbilityController] MovementTuning 未在 Inspector 赋值", this);
            if (_relativeCamera == null) _relativeCamera = Camera.main;
        }

        private void Update()
        {
            if (_tuning == null) return;

            // 1) 清空本帧水平意图，让持续型能力重新填
            _horizontalThisFrame = Vector3.zero;

            // 2) 持续型能力 tick（MoveAbility：读 WASD → 写水平位移 + 转向）
            foreach (var a in _abilities)
                if (a.Trigger == AbilityTrigger.Continuous)
                    a.TickContinuous(this);

            // 3) 垂直：读「上一帧 Move 之后」的 isGrounded（必须在本次 Move 之前读，
            //    拿上一帧合并 Move 后的值）。贴地钳制避免下落速度无限累积。
            bool grounded = _controller.isGrounded;
            if (grounded && _verticalVelocity < 0f)
                _verticalVelocity = _tuning.GroundStickVelocity;

            // 4) 瞬时型能力 tick（JumpAbility：检查接地+起跳键 → 设垂直速度；
            //    PushAbility：检查推动键 → 前方射线推箱）
            foreach (var a in _abilities)
                if (a.Trigger == AbilityTrigger.Instant)
                    a.TickInstant(this);

            // 5) 重力 + 合并单次 Move（水平 + 垂直一起）——保证下一帧 isGrounded 正确反映接地。
            _verticalVelocity += _tuning.Gravity * Time.deltaTime;
            Vector3 vertical = Vector3.up * (_verticalVelocity * Time.deltaTime);
            _controller.Move(_horizontalThisFrame + vertical);
        }
    }
}
