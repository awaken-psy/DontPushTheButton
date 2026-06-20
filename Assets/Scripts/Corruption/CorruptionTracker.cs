using System;
using UnityEngine;
using DontPushTheButton.Config;

namespace DontPushTheButton.Corruption
{
    /// <summary>
    /// 腐败值追踪（M2.5 最小版 → M2.6 加数据 SO + 超载计数 + OnChanged 事件）。
    /// 本关永不恢复（只增），满格触发事件。GDD 4.4。
    /// 双超载（关5）同池：所有超载键按都 AddOverloadPress 到同一 Value。§C。
    /// 满格重置/主动返回的状态切换归 M2.8（H4）。
    /// </summary>
    public class CorruptionTracker : MonoBehaviour
    {
        [Tooltip("腐败调参（M2.6 SO）")]
        [SerializeField] private CorruptionTuning _tuning;

        public float Value { get; private set; }
        /// <summary>本关按超载键次数（R4 试玩用）。</summary>
        public int OverloadCount { get; private set; }

        public float MaxValue => _tuning != null ? _tuning.MaxValue : 100f;
        public float Normalized => MaxValue > 0f ? Value / MaxValue : 0f;
        public bool IsFull => Value >= MaxValue;
        public CorruptionTuning Tuning => _tuning;

        /// <summary>满格事件（M2.8 接状态切换：满格→重置同关，腐败清零）。</summary>
        public event Action OnCorruptionFull;
        /// <summary>值变化事件（HUD 监听刷新）。M2.6。</summary>
        public event Action OnChanged;

        /// <summary>按超载键加腐败（GDD 4.3/4.4：超载键按即腐败，无论绑能力/移动）。</summary>
        public void AddOverloadPress()
        {
            if (IsFull || _tuning == null) return;
            Value = Mathf.Min(MaxValue, Value + _tuning.IncrementPerPress);
            OverloadCount++;
            OnChanged?.Invoke();
            if (IsFull) OnCorruptionFull?.Invoke();
        }

        /// <summary>重置（M2.8 满格重置同关时调；主动返回不调——保留腐败）。</summary>
        public void ResetValue()
        {
            Value = 0f;
            OverloadCount = 0;
            OnChanged?.Invoke();
        }
    }
}
