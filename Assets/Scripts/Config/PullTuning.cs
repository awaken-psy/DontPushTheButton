using UnityEngine;

namespace DontPushTheButton.Config
{
    /// <summary>
    /// 拉动能力调参数据（M3.2 磁力枪·牵引 + 超载抓钩，承载 GDD [PLACEHOLDER] 集中配置）。
    /// </summary>
    [CreateAssetMenu(fileName = "PullTuning", menuName = "DPTB/Pull Tuning")]
    public class PullTuning : ScriptableObject
    {
        [Header("普通拉动（把目标吸向自己）")]
        [Tooltip("抓取检测距离 (m)：前方此距离内最近的可拉物体被吸向自己")]
        [SerializeField] private float _grabRange = 2f;
        [Tooltip("目标吸到玩家前方的距离 (m)")]
        [SerializeField] private float _pullToDistance = 1.5f;

        [Header("超载抓钩（把自己吸向目标 · v1.0）")]
        [Tooltip("抓钩射线距离 (m)：超载时前方此距离内命中的可拉物体作为抓钩落点")]
        [SerializeField] private float _hookRange = 8f;
        [Tooltip("玩家落到目标前方的距离 (m)")]
        [SerializeField] private float _hookLandDistance = 1.5f;

        public float GrabRange => _grabRange;
        public float PullToDistance => _pullToDistance;
        public float HookRange => _hookRange;
        public float HookLandDistance => _hookLandDistance;
    }
}
