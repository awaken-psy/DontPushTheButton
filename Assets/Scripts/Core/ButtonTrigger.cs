using UnityEngine;
using UnityEngine.Events;

namespace DontPushTheButton.Core
{
    /// <summary>
    /// 按钮触发器（M4.10）：玩家踩 / Pushable 箱压 → OnPressed + 视觉反馈（下沉）。
    /// 参考 CheckPoint / LevelExit 触发器模式。挂 Button 物体，Awake 把 BoxCollider 设 isTrigger。
    /// 触发模式：默认 OnPress（压下 OnPressed，离开 OnReleased）；_toggle=true 时踩一次开/再踩关。
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class ButtonTrigger : MonoBehaviour
    {
        [Tooltip("Toggle 模式：踩一次开(OnPressed)/再踩关(OnReleased)；默认 false=OnPress")]
        [SerializeField] private bool _toggle = false;
        [Tooltip("可触发的 tag（玩家 + 可推箱）")]
        [SerializeField] private string[] _triggerTags = { "Player", "Pushable" };
        [Tooltip("按下时下沉量")]
        [SerializeField] private float _pressDepth = 0.05f;

        [Tooltip("按下时触发（绑机关响应，如 PillarLift.Lift）")]
        public UnityEvent OnPressed = new UnityEvent();
        [Tooltip("松开时触发（OnPress 模式 + 物体离开；机关按需接，如柱保持不复位则不接）")]
        public UnityEvent OnReleased = new UnityEvent();

        private bool _isPressed = false;
        private bool _toggleState = false;
        private Vector3 _originLocalPos;

        private void Awake()
        {
            var col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
            _originLocalPos = transform.localPosition;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!HasTag(other) || _isPressed) return;
            _isPressed = true;
            transform.localPosition = _originLocalPos + new Vector3(0, -_pressDepth, 0);
            if (_toggle)
            {
                _toggleState = !_toggleState;
                if (_toggleState) OnPressed?.Invoke(); else OnReleased?.Invoke();
            }
            else OnPressed?.Invoke();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!HasTag(other)) return;
            _isPressed = false;
            if (!_toggle)
            {
                transform.localPosition = _originLocalPos;
                OnReleased?.Invoke();
            }
        }

        private bool HasTag(Collider other)
        {
            if (_triggerTags == null) return false;
            foreach (var t in _triggerTags)
                if (other.CompareTag(t)) return true;
            return false;
        }
    }
}
