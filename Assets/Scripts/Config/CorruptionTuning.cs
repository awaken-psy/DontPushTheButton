using UnityEngine;

namespace DontPushTheButton.Config
{
    /// <summary>
    /// 腐败系统调参数据（M2.6 + M4.4 扩展）。承载 GDD 4.4 [PLACEHOLDER] 集中配置。
    /// M4.4：层级阈值/机制惩罚/视效/抖动参数全 SO 化（原写死在 CorruptionEffects/场景）。
    /// </summary>
    [CreateAssetMenu(fileName = "CorruptionTuning", menuName = "DPTB/Corruption Tuning")]
    public class CorruptionTuning : ScriptableObject
    {
        [Header("基础（M2.6）")]
        [Tooltip("满格阈值（满格→关卡重置）")]
        [SerializeField] private float _maxValue = 100f;
        [Tooltip("每次超载执行的腐败增量")]
        [SerializeField] private float _incrementPerPress = 12f;

        [Header("层级阈值（Normalized，GDD 4.4）")]
        [Tooltip("L1 起始（轻微泛红）")]
        [SerializeField] private float _level1Threshold = 0.25f;
        [Tooltip("L2 起始（移速降）")]
        [SerializeField] private float _level2Threshold = 0.5f;
        [Tooltip("L3 起始（glitch + 冷却延长）")]
        [SerializeField] private float _level3Threshold = 0.75f;
        [Tooltip("L4 起始（满泛红 + 强抖）")]
        [SerializeField] private float _level4Threshold = 0.95f;

        [Header("机制惩罚")]
        [Tooltip("L2+ 移速倍率（<1=降速）")]
        [SerializeField] private float _level2SpeedMult = 0.85f;
        [Tooltip("L3+ 冷却倍率（>1=延长）")]
        [SerializeField] private float _level3CooldownMult = 1.3f;

        [Header("视效")]
        [Tooltip("L4 满泛红强度")]
        [SerializeField] private float _vignetteMaxIntensity = 0.6f;
        [Tooltip("Vignette 颜色")]
        [SerializeField] private Color _vignetteColor = new Color(0.8f, 0.1f, 0.1f);
        [Tooltip("L1 差异化：轻微 postExposure（L1 信号，0=L1 无感）")]
        [SerializeField] private float _level1PostExposure = 0.15f;
        [Tooltip("L3+ glitch 色相偏移")]
        [SerializeField] private float _colorAdjHueShift = -30f;
        [Tooltip("L3+ glitch 降饱和")]
        [SerializeField] private float _colorAdjSaturation = -30f;

        [Header("抖动（L3+，需绑 CinemachineImpulseSource）")]
        [Tooltip("L3 抖动力度")]
        [SerializeField] private float _level3ImpulseForce = 0.5f;
        [Tooltip("L4 抖动力度")]
        [SerializeField] private float _level4ImpulseForce = 1.0f;

        public float MaxValue => _maxValue;
        public float IncrementPerPress => _incrementPerPress;
        public float Level1Threshold => _level1Threshold;
        public float Level2Threshold => _level2Threshold;
        public float Level3Threshold => _level3Threshold;
        public float Level4Threshold => _level4Threshold;
        public float Level2SpeedMult => _level2SpeedMult;
        public float Level3CooldownMult => _level3CooldownMult;
        public float VignetteMaxIntensity => _vignetteMaxIntensity;
        public Color VignetteColor => _vignetteColor;
        public float Level1PostExposure => _level1PostExposure;
        public float ColorAdjHueShift => _colorAdjHueShift;
        public float ColorAdjSaturation => _colorAdjSaturation;
        public float Level3ImpulseForce => _level3ImpulseForce;
        public float Level4ImpulseForce => _level4ImpulseForce;
    }
}
