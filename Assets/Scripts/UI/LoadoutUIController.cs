using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DontPushTheButton.Abilities;
using DontPushTheButton.Binding;
using DontPushTheButton.Config;

namespace DontPushTheButton.UI
{
    /// <summary>
    /// 配置阶段 UI 主控（M2.4）。读 LevelConfigSO 生成按键槽/能力图标，
    /// 拖拽绑定 → 更新 LoadoutConfig → 校验反馈。「开始关卡」占位（M2.8 接关卡加载）。
    /// GDD 4.1 / §A / §J（J2 超载变更 / J7 WASD 自动填入）。
    /// </summary>
    public class LoadoutUIController : MonoBehaviour
    {
        [Header("数据")]
        [Tooltip("本关关卡配置（M2.3）")]
        [SerializeField] private LevelConfigSO _levelConfig;

        [Header("UI 引用")]
        [SerializeField] private Transform _slotContainer;
        [SerializeField] private Transform _iconContainer;
        [SerializeField] private KeySlotUI _slotPrefab;
        [SerializeField] private AbilityIcon _iconPrefab;
        [SerializeField] private Text _validationText;
        [SerializeField] private Button _startButton;

        [Header("复用（J2/J7，占位）")]
        [Tooltip("上关配置（M2.8 多关传入；留空则用默认 WASD 布局演示 J7 自动填入）")]
        [SerializeField] private LoadoutConfig _carryoverConfig;

        private readonly LoadoutConfig _config = new LoadoutConfig();
        private readonly List<KeySlotUI> _slotUIs = new List<KeySlotUI>();

        public LoadoutConfig Config => _config;
        public LevelConfigSO LevelConfig => _levelConfig;

        private void Start()
        {
            BuildConfig();
            BuildSlots();
            BuildIcons();
            if (_startButton != null) _startButton.onClick.AddListener(OnStartClicked);
            ApplyCarryoverOrDefault();
            RefreshValidation();
        }

        private void BuildConfig()
        {
            if (_levelConfig == null) { Debug.LogError("[LoadoutUI] LevelConfigSO 未赋值", this); return; }
            _config.SlotCount = _levelConfig.SlotCount;
            _config.Slots.Clear();
            foreach (var k in _levelConfig.AvailableKeys)
                _config.Slots.Add(new KeySlot(k.KeyName, k.IsOverload));
            _config.AvailableAbilities.Clear();
            foreach (var a in _levelConfig.AvailableAbilities)
                _config.AvailableAbilities.Add(a);
        }

        private void BuildSlots()
        {
            if (_slotContainer == null || _slotPrefab == null) return;
            for (int i = 0; i < _config.Slots.Count; i++)
            {
                var ui = Instantiate(_slotPrefab, _slotContainer);
                var s = _config.Slots[i];
                ui.Setup(i, s.KeyName, s.IsOverload, this);
                _slotUIs.Add(ui);
            }
        }

        private void BuildIcons()
        {
            if (_iconContainer == null || _iconPrefab == null) return;
            // 移动方向（默认每关可用，GDD §A 移动=4槽是绑定项）
            foreach (MoveDirection d in System.Enum.GetValues(typeof(MoveDirection)))
                InstantiateIcon(BindingItem.Move(d), d.ToString());
            // 能力（跳过 Move —— 移动用方向图标，不作为单键能力）
            foreach (var a in _levelConfig.AvailableAbilities)
                if (a != AbilityKind.Move)
                    InstantiateIcon(BindingItem.Of(a), a.ToString());
        }

        private void InstantiateIcon(BindingItem item, string label)
        {
            var icon = Instantiate(_iconPrefab, _iconContainer);
            icon.Setup(item, label);
        }

        /// <summary>拖拽放下到某槽（KeySlotUI.OnDrop 调）。键维度：后绑覆盖（GDD 4.1 行165）。</summary>
        public void BindToSlot(int slotIndex, BindingItem item)
        {
            if (slotIndex < 0 || slotIndex >= _config.Slots.Count) return;
            _config.Slots[slotIndex].Binding = item;
            if (slotIndex < _slotUIs.Count) _slotUIs[slotIndex].SetBinding(item);
            RefreshValidation();
        }

        /// <summary>点击已绑槽解绑（GDD「再次点击解除绑定」）。</summary>
        public void UnbindSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _config.Slots.Count) return;
            _config.Slots[slotIndex].Binding = null;
            if (slotIndex < _slotUIs.Count) _slotUIs[slotIndex].SetBinding(null);
            RefreshValidation();
        }

        /// <summary>J7 WASD 自动填入（占位）：无 carryover 时填默认 WASD 布局。</summary>
        private void ApplyCarryoverOrDefault()
        {
            if (_carryoverConfig != null) { /* M2.8 多关：复用上关绑定 */ return; }
            TryDefaultBind("W", BindingItem.Move(MoveDirection.Up));
            TryDefaultBind("A", BindingItem.Move(MoveDirection.Left));
            TryDefaultBind("S", BindingItem.Move(MoveDirection.Down));
            TryDefaultBind("D", BindingItem.Move(MoveDirection.Right));
        }

        private void TryDefaultBind(string keyName, BindingItem item)
        {
            for (int i = 0; i < _config.Slots.Count; i++)
                if (_config.Slots[i].KeyName == keyName && !_config.Slots[i].Binding.HasValue)
                { BindToSlot(i, item); break; }
        }

        private void RefreshValidation()
        {
            // 2.4c：Error/Warning 分级反馈（M2.2 校验结果可视化）
            if (_validationText == null) return;
            var r = LoadoutValidator.Validate(_config);
            if (r.IsValid && !r.HasWarnings)
            {
                _validationText.text = "配置有效";
                _validationText.color = Color.green;
                return;
            }
            var sb = new System.Text.StringBuilder();
            foreach (var issue in r.Issues)
                sb.AppendLine("[" + (issue.Level == ValidationLevel.Error ? "错误" : "警告") + "] " + issue.Message);
            _validationText.text = sb.ToString().TrimEnd();
            _validationText.color = r.IsValid ? Color.yellow : Color.red;
        }

        private void OnStartClicked()
        {
            var r = LoadoutValidator.Validate(_config);
            Debug.Log($"[LoadoutUI] 开始关卡（占位，M2.8 接关卡加载）。IsValid={r.IsValid}，HasWarnings={r.HasWarnings}");
        }
    }
}
