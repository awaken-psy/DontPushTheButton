using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DontPushTheButton.Binding;

namespace DontPushTheButton.UI
{
    /// <summary>
    /// 能力/方向图标（M2.4 拖拽源）。IBeginDragHandler/IDragHandler/IEndDragHandler。
    /// 拖拽中 CanvasGroup.blocksRaycasts=false 让 Drop 射线穿透到 KeySlotUI；
    /// 拖完复位（图标可重复拖）。GDD 4.1「鼠标拖拽能力图标到按键」。
    /// </summary>
    public class AbilityIcon : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private Text _label;
        [SerializeField] private CanvasGroup _canvasGroup;

        public BindingItem Item { get; private set; }

        private Vector3 _originLocalPos;
        private Transform _originParent;

        public void Setup(BindingItem item, string displayLabel)
        {
            Item = item;
            if (_label != null) _label.text = displayLabel;
        }

        private void Start()
        {
            _originLocalPos = transform.localPosition;
            _originParent = transform.parent;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_canvasGroup != null) _canvasGroup.blocksRaycasts = false;
            transform.SetParent(transform.root, true); // 移到顶层避免被容器裁剪
        }

        public void OnDrag(PointerEventData eventData)
        {
            transform.position = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_canvasGroup != null) _canvasGroup.blocksRaycasts = true;
            if (_originParent != null) transform.SetParent(_originParent, false);
            transform.localPosition = _originLocalPos;
        }
    }
}
