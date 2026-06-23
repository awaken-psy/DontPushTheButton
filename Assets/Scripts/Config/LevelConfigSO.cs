using System;
using System.Collections.Generic;
using UnityEngine;
using DontPushTheButton.Abilities;

namespace DontPushTheButton.Config
{
    /// <summary>
    /// 单个可用按键的定义（含超载身份标记）。GDD §A / O2：超载用集合标记，非物理位置字段。
    /// </summary>
    [Serializable]
    public class KeySlotDef
    {
        [Tooltip("逻辑键名，与 InputSystem binding 对应，如 W/Space/E")]
        public string KeyName = "";
        [Tooltip("该键是否为本关超载身份（按即腐败；只有绑能力才有强化补偿）")]
        public bool IsOverload;
    }

    /// <summary>
    /// 关卡配置（每关一个 .asset，设计师 Inspector 编辑）。GDD 4.6 / §A。
    /// 定义本关的配置约束：槽位数、可用按键（含超载身份）、可用能力。
    /// 玩家在配置阶段(2.4)据此约束生成 LoadoutConfig(M2.2 玩家绑定结果)。
    /// </summary>
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "DPTB/Level Config")]
    public class LevelConfigSO : ScriptableObject
    {
        [Header("关卡元数据")]
        [Tooltip("关卡唯一标识，如 Level2")]
        [SerializeField] private string _levelId = "";
        [Tooltip("显示名，如 裂口")]
        [SerializeField] private string _displayName = "";
        [Tooltip("关卡场景名（M2.7 填，本期可留空）")]
        [SerializeField] private string _sceneName = "";

        [Header("配置约束")]
        [Tooltip("本关按键槽位数（默认 6，关 5 可 7；GDD 4.5）")]
        [SerializeField] private int _slotCount = 6;

        [Tooltip("本关可用按键 + 超载身份标记（超载键 = IsOverload=true；O2 超载用集合标记，非物理位置）")]
        [SerializeField] private List<KeySlotDef> _availableKeys = new List<KeySlotDef>();

        [Tooltip("本关可绑定的能力（不含移动；移动方向 Up/Down/Left/Right 默认每关可用——GDD §A 移动=4槽是绑定项，非单键能力）")]
        [SerializeField] private List<AbilityKind> _availableAbilities = new List<AbilityKind>();

        [Header("评分阈值（M3.7；R6 每关独立，占位值 playtest 调）")]
        [Tooltip("时间星阈值（秒）：用时 ≤ 此值得时间星")]
        [SerializeField] private float _scoreTimeSec = 30f;
        [Tooltip("节制星阈值：超载次数 ≤ 此值得节制星")]
        [SerializeField] private int _scoreOverloadCount = 3;
        [Tooltip("精简星阈值：空闲槽位 ≥ 此值得精简星")]
        [SerializeField] private int _scoreFreeSlots = 1;

        // ---- 只读访问（运行时/M2.4 配置 UI 读取）----
        public string LevelId => _levelId;
        public string DisplayName => _displayName;
        public string SceneName => _sceneName;
        public int SlotCount => _slotCount;
        public IReadOnlyList<KeySlotDef> AvailableKeys => _availableKeys;
        public IReadOnlyList<AbilityKind> AvailableAbilities => _availableAbilities;
        public float ScoreTimeSec => _scoreTimeSec;
        public int ScoreOverloadCount => _scoreOverloadCount;
        public int ScoreFreeSlots => _scoreFreeSlots;
    }
}
