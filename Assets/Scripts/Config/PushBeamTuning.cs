using UnityEngine;

namespace DontPushTheButton.Config
{
    /// <summary>
    /// 推动光束型调参数据（M3.3 牵引光束远距推物 + 超载推距倍率）。
    /// 与 PushTuning（搬运型 M3.1）并列；关卡固定二选一（LevelConfigSO offer Push 或 PushBeam）。
    /// </summary>
    [CreateAssetMenu(fileName = "PushBeamTuning", menuName = "DPTB/Push Beam Tuning")]
    public class PushBeamTuning : ScriptableObject
    {
        [Tooltip("光束作用距离 (m)：前方此距离内命中的可推物体被推动")]
        [SerializeField] private float _beamRange = 8f;
        [Tooltip("单次推动位移 (m)；超载 ×_overloadBeamMultiplier")]
        [SerializeField] private float _beamPushDistance = 3f;
        [Tooltip("冷却 (s)；<=0 = 无")]
        [SerializeField] private float _cooldown = 0f;

        public float BeamRange => _beamRange;
        public float BeamPushDistance => _beamPushDistance;
        public float Cooldown => _cooldown;
    }
}
