using UnityEngine;

namespace DontPushTheButton.Config
{
    /// <summary>
    /// 冲刺能力调参数据（M3.4 dash + 超载 dash×2，承载 GDD [PLACEHOLDER] 集中配置）。
    /// </summary>
    [CreateAssetMenu(fileName = "DashTuning", menuName = "DPTB/Dash Tuning")]
    public class DashTuning : ScriptableObject
    {
        [Tooltip("单次冲刺位移距离 (m)；超载 dash×2 = 此距离×2")]
        [SerializeField] private float _dashDistance = 5f;
        [Tooltip("冲刺冷却 (s)；防连按滥用，<=0 = 无")]
        [SerializeField] private float _cooldown = 0.5f;

        public float DashDistance => _dashDistance;
        public float Cooldown => _cooldown;
    }
}
