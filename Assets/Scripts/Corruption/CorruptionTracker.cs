using System;
using UnityEngine;

namespace DontPushTheButton.Corruption
{
    /// <summary>
    /// 腐败值追踪（M2.5 最小版；M2.6 加 HUD 腐败条 + 数据 SO + 永不恢复约束强化）。
    /// 本关永不恢复（只增），满格(100)触发事件。GDD 4.4。
    /// </summary>
    public class CorruptionTracker : MonoBehaviour
    {
        [Tooltip("满格阈值（GDD 4.4）")]
        [SerializeField] private float _maxValue = 100f;
        [Tooltip("每次按超载键的腐败增量（GDD [PLACEHOLDER]，M2.5 写死，M2.6/M3 调参走 SO）")]
        [SerializeField] private float _incrementPerPress = 12f;

        public float Value { get; private set; }
        public float MaxValue => _maxValue;
        public float Normalized => _maxValue > 0f ? Value / _maxValue : 0f;
        public bool IsFull => Value >= _maxValue;

        /// <summary>满格事件（M2.8 接状态切换：满格→重置同关，腐败清零）。</summary>
        public event Action OnCorruptionFull;

        /// <summary>按超载键加腐败（GDD 4.3/4.4：超载键按即腐败，无论绑能力/移动）。</summary>
        public void AddOverloadPress()
        {
            if (IsFull) return;
            Value = Mathf.Min(_maxValue, Value + _incrementPerPress);
            if (IsFull) OnCorruptionFull?.Invoke();
        }

        /// <summary>重置（M2.8 满格重置同关时调）。</summary>
        public void ResetValue() => Value = 0f;
    }
}
