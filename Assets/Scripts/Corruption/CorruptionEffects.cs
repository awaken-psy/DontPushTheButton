using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using DontPushTheButton.Config;

namespace DontPushTheButton.Corruption
{
    /// <summary>
    /// 腐败视效 + 机制层（M3.8，GDD 4.4）。挂 Player，订阅 CorruptionTracker.OnChanged。
    /// M4.4：参数全 SO 化（_tuning），层级阈值/惩罚/视效/抖动从 CorruptionTuning 读（留空用默认）。
    /// L5（满格）由 CorruptionTracker.OnCorruptionFull → GameManager.EnterFailed 处理。
    /// </summary>
    public class CorruptionEffects : MonoBehaviour
    {
        [Header("引用")]
        [SerializeField] private CorruptionTracker _tracker;
        [Tooltip("腐败视效 Volume（含 Vignette + ColorAdjustments override）")]
        [SerializeField] private Volume _volume;
        [Tooltip("抖动用 CinemachineImpulseSource（留空则不抖；M4.4 接）")]
        [SerializeField] private MonoBehaviour _impulseSource;
        [Tooltip("M4.4：参数 SO（层级阈值/惩罚/视效/抖动）。留空用代码默认值。")]
        [SerializeField] private CorruptionTuning _tuning;

        private Vignette _vignette;
        private ColorAdjustments _colorAdj;
        private int _level = 0;

        /// <summary>当前腐败层级（0–4）。</summary>
        public int Level => _level;
        /// <summary>移速倍率（L2+ 返回降速；否则 1）。MoveAbility 读。</summary>
        public float MoveSpeedMultiplier => _level >= 2 ? (_tuning != null ? _tuning.Level2SpeedMult : 0.85f) : 1f;
        /// <summary>冷却倍率（L3+ 返回延长；否则 1）。Dash/Push 读。</summary>
        public float CooldownMultiplier => _level >= 3 ? (_tuning != null ? _tuning.Level3CooldownMult : 1.3f) : 1f;

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

        private void Refresh()
        {
            if (_tracker == null) return;
            _level = (_tuning != null)
                ? ComputeLevel(_tracker.Normalized, _tuning)
                : ComputeLevel(_tracker.Normalized);
            ApplyVisuals();
        }

        /// <summary>纯函数层级算法（默认阈值，供单测 + _tuning 留空时 fallback）。</summary>
        public static int ComputeLevel(float normalized)
        {
            if (normalized < 0.25f) return 0;
            if (normalized < 0.5f) return 1;
            if (normalized < 0.75f) return 2;
            if (normalized < 0.95f) return 3;
            return 4;
        }

        /// <summary>纯函数层级算法（SO 阈值，M4.4）。</summary>
        public static int ComputeLevel(float normalized, CorruptionTuning t)
        {
            if (t == null) return ComputeLevel(normalized);
            if (normalized < t.Level1Threshold) return 0;
            if (normalized < t.Level2Threshold) return 1;
            if (normalized < t.Level3Threshold) return 2;
            if (normalized < t.Level4Threshold) return 3;
            return 4;
        }

        private void ApplyVisuals()
        {
            float t = Mathf.Clamp01(_level / 4f); // L0:0 → L4:1

            float vigMax = _tuning != null ? _tuning.VignetteMaxIntensity : 0.6f;
            Color vigColor = _tuning != null ? _tuning.VignetteColor : new Color(0.8f, 0.1f, 0.1f);
            float l1PostExp = _tuning != null ? _tuning.Level1PostExposure : 0.15f;
            float hueShift = _tuning != null ? _tuning.ColorAdjHueShift : -30f;
            float sat = _tuning != null ? _tuning.ColorAdjSaturation : -30f;
            float impulseL3 = _tuning != null ? _tuning.Level3ImpulseForce : 0.5f;
            float impulseL4 = _tuning != null ? _tuning.Level4ImpulseForce : 1.0f;

            // Volume 权重
            if (_volume != null) _volume.weight = t;

            // Vignette（红，强度随层级）
            if (_vignette != null)
            {
                _vignette.intensity.Override(t * vigMax);
                _vignette.color.Override(vigColor);
            }

            // ColorAdjustments：L1 差异化（postExposure 轻微亮）+ L3+ glitch（hueShift/sat）
            if (_colorAdj != null)
            {
                // L1 信号：轻微 postExposure（M4.4 D，避免 L1 几乎无感）
                float l1Signal = (_level == 1) ? l1PostExp : 0f;
                _colorAdj.postExposure.Override(l1Signal);
                // L3+ glitch
                float glitch = Mathf.Max(0, _level - 2) / 2f; // L3:0.5, L4:1
                _colorAdj.hueShift.Override(hueShift * glitch);
                _colorAdj.saturation.Override(sat * glitch);
            }

            // 抖动（L3+，反射调 CinemachineImpulseSource.GenerateImpulseWithForce）
            if (_level >= 3 && _impulseSource != null)
            {
                float force = _level >= 4 ? impulseL4 : impulseL3;
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
