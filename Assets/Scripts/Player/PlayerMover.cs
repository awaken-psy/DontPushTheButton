using UnityEngine;
using DontPushTheButton.Config;
using DontPushTheButton.Input;

namespace DontPushTheButton.Player
{
    /// <summary>
    /// 角色移动控制器（M1 硬编码直连）。
    /// 读取 PlayerControls 的 Move/Jump，用 CharacterController 驱动移动/跳跃/重力；
    /// 水平移动方向相对相机水平面（俯视 2.5D）。M2.1 会重构包装为「能力」。
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerControls))]
    public class PlayerMover : MonoBehaviour
    {
        [Header("引用")]
        [SerializeField] private MovementTuning _tuning;
        [Tooltip("水平移动方向所相对的相机；留空则用 Camera.main")]
        [SerializeField] private Camera _relativeCamera;

        private CharacterController _controller;
        private PlayerControls _controls;
        private float _verticalVelocity; // Y 轴速度（跳跃/重力累积）

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _controls = GetComponent<PlayerControls>();
            if (_tuning == null)
                Debug.LogError("[PlayerMover] MovementTuning 未在 Inspector 赋值", this);
            if (_relativeCamera == null)
                _relativeCamera = Camera.main;
        }

        private void Update()
        {
            if (_tuning == null) return;
            ApplyHorizontalMove();
            ApplyVerticalMove();
        }

        private void ApplyHorizontalMove()
        {
            Vector2 stick = _controls.MoveAction != null
                ? _controls.MoveAction.ReadValue<Vector2>()
                : Vector2.zero;

            // 相对相机水平面方向（压平 Y）
            Vector3 fwd = _relativeCamera ? _relativeCamera.transform.forward : Vector3.forward;
            Vector3 right = _relativeCamera ? _relativeCamera.transform.right : Vector3.right;
            fwd.y = 0f; fwd.Normalize();
            right.y = 0f; right.Normalize();

            Vector3 move = fwd * stick.y + right * stick.x;
            if (move.sqrMagnitude > 1f) move.Normalize();

            _controller.Move(move * (_tuning.MoveSpeed * Time.deltaTime));

            // 面向移动方向
            if (move.sqrMagnitude > 0.01f)
            {
                Quaternion target = Quaternion.LookRotation(move, Vector3.up);
                transform.rotation = _tuning.TurnSpeed <= 0f
                    ? target
                    : Quaternion.RotateTowards(transform.rotation, target, _tuning.TurnSpeed * Time.deltaTime);
            }
        }

        private void ApplyVerticalMove()
        {
            bool grounded = _controller.isGrounded;

            // 贴地：避免下落速度无限累积
            if (grounded && _verticalVelocity < 0f)
                _verticalVelocity = -2f;

            bool jumpPressed = _controls.JumpAction != null
                && _controls.JumpAction.WasPressedThisFrame();
            if (grounded && jumpPressed)
            {
                // v = √(2·g·h)，g 取绝对值
                float g = Mathf.Abs(_tuning.Gravity);
                _verticalVelocity = Mathf.Sqrt(2f * g * _tuning.JumpHeight);
            }

            _verticalVelocity += _tuning.Gravity * Time.deltaTime;
            _controller.Move(Vector3.up * (_verticalVelocity * Time.deltaTime));
        }
    }
}
