using UnityEngine;

namespace DontPushTheButton.Config
{
    /// <summary>
    /// 拾取能力调参数据（M3.1 磁力枪·抓取搬运型；承载 GDD [PLACEHOLDER] 集中配置）。
    /// </summary>
    [CreateAssetMenu(fileName = "PickupTuning", menuName = "DPTB/Pickup Tuning")]
    public class PickupTuning : ScriptableObject
    {
        [Header("搬运型（M3.1 磁力枪抓取）")]
        [Tooltip("抓取检测距离 (m)：按抓取键时，玩家此距离内的可推物体可被抓取吸附")]
        [SerializeField] private float _grabRange = 2f;
        [Tooltip("持有距离 (m)：持有物体吸附在玩家前方的距离")]
        [SerializeField] private float _carryDistance = 1.5f;
        [Tooltip("多物体搬运时横向间距 (m)")]
        [SerializeField] private float _carrySpacing = 1f;
        [Tooltip("超载最多同时搬运数量")]
        [SerializeField] private int _maxCarryCount = 3;

        public float GrabRange => _grabRange;
        public float CarryDistance => _carryDistance;
        public float CarrySpacing => _carrySpacing;
        public int MaxCarryCount => _maxCarryCount;
    }
}
