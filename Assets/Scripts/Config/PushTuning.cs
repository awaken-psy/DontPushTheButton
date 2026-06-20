using UnityEngine;

namespace DontPushTheButton.Config
{
    /// <summary>
    /// 推动能力调参数据（M2.1 最简推箱，承载 GDD [PLACEHOLDER] 集中配置）。
    /// M3.1 升级为磁力枪搬运型推动时会扩展（抓取距离/多目标等）。
    /// </summary>
    [CreateAssetMenu(fileName = "PushTuning", menuName = "DPTB/Push Tuning")]
    public class PushTuning : ScriptableObject
    {
        [Header("推动（最简版：瞬时射线推，非物理 H2）")]
        [Tooltip("前方射线作用距离 (m)：玩家按推动键时，此距离内命中的可推物体才被推动")]
        [SerializeField] private float _range = 2f;
        [Tooltip("单次推动位移 (m)：命中后物体沿玩家朝向位移的距离")]
        [SerializeField] private float _pushDistance = 1.5f;
        [Tooltip("推动冷却 (s)：两次推动间最短间隔；<=0 = 无冷却。本期留 0（M3 调参）")]
        [SerializeField] private float _cooldown = 0f;

        public float Range => _range;
        public float PushDistance => _pushDistance;
        public float Cooldown => _cooldown;
    }
}
