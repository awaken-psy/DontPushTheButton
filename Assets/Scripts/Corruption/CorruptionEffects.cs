using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DontPushTheButton.Corruption
{
    /// <summary>
    /// 腐败视效 + 机制层（M3.8，GDD 4.4）。挂 Player，订阅 CorruptionTracker.OnChanged。
    /// 按 Normalized 算层级（L0-L4）→ 调 URP Volume（泛红/glitch）+ 抖相机 + 暴露移速/冷却系数给能力读。
    /// L5（满格）由 CorruptionTracker.OnCorruptionFull → GameManager.EnterFailed 处理，本组件不管。
    /// </summary>
    public class CorruptionEffects : MonoBehaviour
    {
        [Header("引用")]
        [SerializeField] private CorruptionTracker _tracker;
        [Tooltip("腐败视效 Volume（全局，含 Vignette + ColorAdjustments override）")]
        [SerializeField] private Volume _volume;
        [Tooltip("抖动用 CinemachineImpulseSource（留空则不抖；用 MonoBehaviour 避免硬依赖 CM 类型）")]
        [SerializeField] private MonoBehaviour _impulseSource;

        [Header("层级系数（占位，playtest 调）")]
        [Tooltip("层级2+ 移速倍率（<1 = 降速）")]
        [SerializeField] private float _level2SpeedMult = 0.85f;
        [Tooltip("层级3+ 冷却倍率（>1 = 延长）")]
        [SerializeField] private float _level3CooldownMult = 1.3f;
        [Tooltip("层级4 满泛红强度")]
        [SerializeField] private float _vignetteMaxIntensity = 0.6f;

        private Vignette _vignette;
        private ColorAdjustments _colorAdj;
        private int _level = 0;

        /// <summary>当前腐败层级（0–4）。</summary>
        public int Level => _level;
        /// <summary>移速倍率（L2+ 返回 _level2SpeedMult；否则 1）。MoveAbility 读。</summary>
        public float MoveSpeedMultiplier => _level >= 2 ? _level2SpeedMult : 1f;
        /// <summary>冷却倍率（L3+ 返回 _level3CooldownMult；否则 1）。Dash/Push 读。</summary>
        public float CooldownMultiplier => _level >= 3 ? _level3CooldownMult : 1f;

        private void OnEnable()
        {
            CacheVolumeOverrides();
            if (_tracker != null) _tracker.OnChanged += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            if (_tracker != null) _tracker.OnChanged -= Refresh;
        }

        /// <summary>按 Normalized 算层级 + 应用视效。</summary>
        private void Refresh()
        {
            if (_tracker == null) return;
            _level = ComputeLevel(_tracker.Normalized);
            ApplyVisuals();
        }

        /// <summary>纯函数层级算法（供单测）：Normalized → L0–L4。</summary>
        public static int ComputeLevel(float normalized)
        {
            if (normalized < 0.25f) return 0;
            if (normalized < 0.5f) return 1;
            if (normalized < 0.75f) return 2;
            if (normalized < 0.95f) return 3;
            return 4; // 0.95–1.0；1.0 满格由 EnterFailed 处理
        }

        private void ApplyVisuals()
        {
            float t = Mathf.Clamp01(_level / 4f); // L0:0 → L4:1

            // Volume 权重
            if (_volume != null) _volume.weight = t;

            // Vignette（红，强度随层级）
            if (_vignette != null)
            {
                _vignette.intensity.Override(t * _vignetteMaxIntensity);
                _vignette.color.Override(new Color(0.8f, 0.1f, 0.1f));
            }

            // ColorAdjustments（L3+ glitch 感：色相偏移 + 降饱和）
            if (_colorAdj != null)
            {
                float glitch = Mathf.Max(0, _level - 2) / 2f; // L3:0.5, L4:1
                _colorAdj.hueShift.Override(-30f * glitch);
                _colorAdj.saturation.Override(-30f * glitch);
            }

            // 抖动（L3+，反射调 CinemachineImpulseSource.GenerateImpulseWithForce）
            if (_level >= 3 && _impulseSource != null)
            {
                float force = _level >= 4 ? 1f : 0.5f;
                var mi = _impulseSource.GetType().GetMethod("GenerateImpulseWithForce");
                if (mi != null) mi.Invoke(_impulseSource, new object[] { force });
            }
        }

        private void CacheVolumeOverrides()
        {
            if (_volume == null || _volume.profile == null) return;
            _volume.profile.TryGet(out _vignette);
            _volume.profile.TryGet(out _colorAdj);
        }
    }
}
