using UnityEngine;

namespace DontPushTheButton.Config
{
    /// <summary>
    /// 跳跃能力调参数据（M3.5 超载飞行状态机 + 缓落）。
    /// 普通跳用 MovementTuning.JumpHeight/Gravity；本 SO 只管超载飞行 + 缓落。
    /// H3：M2.5 瞬时写死 / M3.5 状态机 + SO。
    /// </summary>
    [CreateAssetMenu(fileName = "JumpTuning", menuName = "DPTB/Jump Tuning")]
    public class JumpTuning : ScriptableObject
    {
        [Header("超载飞行")]
        [Tooltip("超载跳跃初速倍率")]
        [SerializeField] private float _overloadJumpMultiplier = 1.4f;
        [Tooltip("超载飞行持续时间 (s)")]
        [SerializeField] private float _overloadFlyDuration = 0.45f;
        [Tooltip("超载飞行期持续上升速度 (m/s)")]
        [SerializeField] private float _overloadFlyVelocity = 6f;

        [Header("缓落（超载飞行结束后）")]
        [Tooltip("缓落态重力倍率（<1=缓落）；超载飞行结束后的下落重力 × 此值")]
        [SerializeField] private float _fallGravityScale = 0.5f;

        public float OverloadJumpMultiplier => _overloadJumpMultiplier;
        public float OverloadFlyDuration => _overloadFlyDuration;
        public float OverloadFlyVelocity => _overloadFlyVelocity;
        public float FallGravityScale => _fallGravityScale;
    }
}
