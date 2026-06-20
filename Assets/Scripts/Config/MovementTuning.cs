using UnityEngine;

namespace DontPushTheButton.Config
{
    /// <summary>
    /// 角色移动调参数据（M1 占位值，承载 GDD [PLACEHOLDER] 集中配置）。
    /// 满足「数值全部走 ScriptableObject 集中配置」跨里程碑约束；
    /// M2.1 Ability 重构时会随能力系统扩展（冷却/强化倍率等）。
    /// </summary>
    [CreateAssetMenu(fileName = "MovementTuning", menuName = "DPTB/Movement Tuning")]
    public class MovementTuning : ScriptableObject
    {
        [Header("移动")]
        [Tooltip("水平移动速度 (m/s)")]
        [SerializeField] private float _moveSpeed = 5f;

        [Header("跳跃与重力")]
        [Tooltip("跳跃高度 (m)，用于反推初始起跳速度")]
        [SerializeField] private float _jumpHeight = 1.5f;
        [Tooltip("重力加速度 (m/s²)，向下为负")]
        [SerializeField] private float _gravity = -20f;
        [Tooltip("贴地钳制速度 (m/s)：grounded 时把下落速度钳到此值，避免无限累积；负值向下")]
        [SerializeField] private float _groundStickVelocity = -2f;

        [Header("转向")]
        [Tooltip("面向移动方向的旋转速度 (°/s)，<=0 = 立即朝向")]
        [SerializeField] private float _turnSpeed = 720f;

        public float MoveSpeed => _moveSpeed;
        public float JumpHeight => _jumpHeight;
        public float Gravity => _gravity;
        public float GroundStickVelocity => _groundStickVelocity;
        public float TurnSpeed => _turnSpeed;
    }
}
