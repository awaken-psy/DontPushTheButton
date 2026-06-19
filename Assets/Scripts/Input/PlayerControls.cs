using UnityEngine;
using UnityEngine.InputSystem;

namespace DontPushTheButton.Input
{
    /// <summary>
    /// 玩家输入封装（M1 固定 Action Map，M2.1 加 Push）。
    /// 把 PlayerControls.inputactions 资产里的 Player/Move、Look、Jump、Push 四个 Action
    /// 暴露为强类型属性，供 PlayerAbilityController / 各能力读取。
    /// M2.5 运行时动态重绑时，action 来源将从固定映射改为绑定表。
    /// </summary>
    [RequireComponent(typeof(CharacterController))] // 角色控制器挂同一对象
    public class PlayerControls : MonoBehaviour
    {
        [SerializeField] private InputActionAsset _actions;

        public InputAction MoveAction { get; private set; }
        public InputAction LookAction { get; private set; }
        public InputAction JumpAction { get; private set; }
        public InputAction PushAction { get; private set; }

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
            PushAction = _actions.FindAction("Push");
        }

        private void OnEnable()
        {
            MoveAction?.Enable();
            LookAction?.Enable();
            JumpAction?.Enable();
            PushAction?.Enable();
        }

        private void OnDisable()
        {
            MoveAction?.Disable();
            LookAction?.Disable();
            JumpAction?.Disable();
            PushAction?.Disable();
        }
    }
}
