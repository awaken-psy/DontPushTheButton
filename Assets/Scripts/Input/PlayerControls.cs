using UnityEngine;
using UnityEngine.InputSystem;

namespace DontPushTheButton.Input
{
    /// <summary>
    /// 玩家输入封装（M1 固定 Action Map）。
    /// 把 PlayerControls.inputactions 资产里的 Player/Move、Look、Jump 三个 Action
    /// 暴露为强类型属性，供角色控制器读取。
    /// M2 会把这套结构重构为「能力(Ability)」并支持运行时动态重绑。
    /// </summary>
    [RequireComponent(typeof(CharacterController))] // 预留：1.4 角色控制器挂同一对象
    public class PlayerControls : MonoBehaviour
    {
        [SerializeField] private InputActionAsset _actions;

        public InputAction MoveAction { get; private set; }
        public InputAction LookAction { get; private set; }
        public InputAction JumpAction { get; private set; }

        private void Awake()
        {
            if (_actions == null)
            {
                Debug.LogError("[PlayerControls] InputActionAsset 未在 Inspector 赋值", this);
                return;
            }

            MoveAction = _actions.FindAction("Move");
            LookAction = _actions.FindAction("Look");
            JumpAction = _actions.FindAction("Jump");
        }

        private void OnEnable()
        {
            MoveAction?.Enable();
            LookAction?.Enable();
            JumpAction?.Enable();
        }

        private void OnDisable()
        {
            MoveAction?.Disable();
            LookAction?.Disable();
            JumpAction?.Disable();
        }
    }
}
