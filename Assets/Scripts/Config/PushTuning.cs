using UnityEngine;

namespace DontPushTheButton.Config
{
    /// <summary>
    /// 推动能力调参数据（M3.1 磁力枪·抓取搬运型；承载 GDD [PLACEHOLDER] 集中配置）。
    /// 搬运字段（grabRange/carryDistance/carrySpacing/maxCarryCount）= M3.1；
    /// range/pushDistance/cooldown 预留给 3.3 牵引光束型，本期不用。
    /// </summary>
    [CreateAssetMenu(fileName = "PushTuning", menuName = "DPTB/Push Tuning")]
    public class PushTuning : ScriptableObject
    {
        [Header("搬运型（M3.1 磁力枪抓取 · 当前推动形态）")]
        [Tooltip("抓取检测距离 (m)：按抓取键时，玩家此距离内的可推物体可被抓取吸附")]
        [SerializeField] private float _grabRange = 2f;
        [Tooltip("持有距离 (m)：持有物体吸附在玩家前方的距离")]
        [SerializeField] private float _carryDistance = 1.5f;
        [Tooltip("多物体搬运时横向间距 (m)")]
        [SerializeField] private float _carrySpacing = 1f;
        [Tooltip("超载最多同时搬运数量")]
        [SerializeField] private int _maxCarryCount = 3;

        [Header("光束型预留（3.3 牵引光束用，本期不用）")]
        [Tooltip("光束作用距离 (m) —— 3.3 光束型推动用")]
        [SerializeField] private float _range = 2f;
        [Tooltip("光束单次推动位移 (m) —— 3.3 光束型用")]
        [SerializeField] private float _pushDistance = 1.5f;
        [Tooltip("光束冷却 (s) —— 3.3 光束型用")]
        [SerializeField] private float _cooldown = 0f;

        public float GrabRange => _grabRange;
        public float CarryDistance => _carryDistance;
        public float CarrySpacing => _carrySpacing;
        public int MaxCarryCount => _maxCarryCount;
        public float Range => _range;
        public float PushDistance => _pushDistance;
        public float Cooldown => _cooldown;
    }
}
