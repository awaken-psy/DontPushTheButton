using UnityEngine;

namespace DontPushTheButton.Config
{
    /// <summary>
    /// 腐败系统调参数据（M2.6）。承载 GDD 4.4 [PLACEHOLDER] 集中配置。
    /// </summary>
    [CreateAssetMenu(fileName = "CorruptionTuning", menuName = "DPTB/Corruption Tuning")]
    public class CorruptionTuning : ScriptableObject
    {
        [Tooltip("满格阈值（GDD 4.4，满格→关卡重置）")]
        [SerializeField] private float _maxValue = 100f;
        [Tooltip("每次按超载键的腐败增量（GDD [PLACEHOLDER]，M3 调参）")]
        [SerializeField] private float _incrementPerPress = 12f;

        public float MaxValue => _maxValue;
        public float IncrementPerPress => _incrementPerPress;
    }
}
