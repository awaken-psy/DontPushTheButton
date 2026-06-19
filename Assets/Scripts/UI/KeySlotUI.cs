using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DontPushTheButton.Binding;

namespace DontPushTheButton.UI
{
    /// <summary>
    /// 按键槽 UI（M2.4）。显示键名 + 绑定项 + 超载高亮；
    /// 接收拖拽放下（IDropHandler）；点击已绑槽解绑（IPointerClickHandler，GDD「再次点击解除绑定」）。
    /// </summary>
    public class KeySlotUI : MonoBehaviour, IDropHandler, IPointerClickHandler
    {
        [SerializeField] private Text _keyLabel;
        [SerializeField] private Image _bg;
        [SerializeField] private Text _bindingLabel;
        [Tooltip("超载键高亮对象（红框）；空则用 _bg 淡红")]
        [SerializeField] private GameObject _overloadHighlight;

        private int _slotIndex = -1;
        private LoadoutUIController _controller;
        private BindingItem? _binding;
        private bool _isOverload;

        private static readonly Color OverloadTint = new Color(1f, 0.6f, 0.6f, 1f);
        private static readonly Color NormalTint = Color.white;

        public void Setup(int index, string keyName, bool isOverload, LoadoutUIController controller)
        {
            _slotIndex = index;
            _controller = controller;
            _isOverload = isOverload;
            if (_keyLabel != null) _keyLabel.text = keyName;
            if (_overloadHighlight != null) _overloadHighlight.SetActive(isOverload);
            if (_bg != null) _bg.color = isOverload ? OverloadTint : NormalTint;
            SetBinding(null);
        }

        public void SetBinding(BindingItem? item)
        {
            _binding = item;
            if (_bindingLabel != null)
                _bindingLabel.text = item.HasValue ? item.Value.ToString() : "";
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (_controller == null || eventData.pointerDrag == null) return;
            var icon = eventData.pointerDrag.GetComponent<AbilityIcon>();
            if (icon != null) _controller.BindToSlot(_slotIndex, icon.Item);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_binding.HasValue && _controller != null)
                _controller.UnbindSlot(_slotIndex);
        }
    }
}
