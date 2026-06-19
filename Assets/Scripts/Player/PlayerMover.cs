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

        [Tooltip("输入死区：水平输入向量平方长度低于此值视为静止（不移动/不转向）")]
        [SerializeField] private float _moveInputThreshold = 0.01f;

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

            // 1) 水平输入 → 位移向量（相对相机水平面，压平 Y）
            Vector2 stick = _controls.MoveAction != null
                ? _controls.MoveAction.ReadValue<Vector2>()
                : Vector2.zero;

            Vector3 fwd = _relativeCamera ? _relativeCamera.transform.forward : Vector3.forward;
            Vector3 right = _relativeCamera ? _relativeCamera.transform.right : Vector3.right;
            fwd.y = 0f; fwd.Normalize();
            right.y = 0f; right.Normalize();

            Vector3 moveDir = fwd * stick.y + right * stick.x;
            if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();
            Vector3 horizontal = moveDir * (_tuning.MoveSpeed * Time.deltaTime);

            // 2) 垂直：读「上一帧 Move 之后」的 isGrounded。
            //    关键：CharacterController.isGrounded 只在含向下分量的 Move 后才为 true；
            //    所以必须在本次 Move 之前读（拿上一帧合并 Move 后的值），且水平+垂直合并成单次 Move，
            //    否则纯水平 Move 会把 isGrounded 重置为 false，导致贴地钳制/起跳条件永不满足。
            bool grounded = _controller.isGrounded;
            if (grounded && _verticalVelocity < 0f)
                _verticalVelocity = _tuning.GroundStickVelocity; // 贴地钳制，避免下落速度无限累积

            bool jumpPressed = _controls.JumpAction != null
                && _controls.JumpAction.WasPressedThisFrame();
            if (grounded && jumpPressed)
            {
                // v = √(2·g·h)，g 取绝对值
                float g = Mathf.Abs(_tuning.Gravity);
                _verticalVelocity = Mathf.Sqrt(2f * g * _tuning.JumpHeight);
            }
            _verticalVelocity += _tuning.Gravity * Time.deltaTime;
            Vector3 vertical = Vector3.up * (_verticalVelocity * Time.deltaTime);

            // 3) 合并为单次 Move（水平 + 垂直一起）——保证下一帧 isGrounded 正确反映接地
            _controller.Move(horizontal + vertical);

            // 4) 面向移动方向
            if (moveDir.sqrMagnitude > _moveInputThreshold)
            {
                Quaternion target = Quaternion.LookRotation(moveDir, Vector3.up);
                transform.rotation = _tuning.TurnSpeed <= 0f
                    ? target
                    : Quaternion.RotateTowards(transform.rotation, target, _tuning.TurnSpeed * Time.deltaTime);
            }
        }
    }
}
