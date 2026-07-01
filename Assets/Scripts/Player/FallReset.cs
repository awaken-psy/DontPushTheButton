using UnityEngine;

namespace DontPushTheButton.Player
{
    /// <summary>
    /// 掉沟重置（M2.7 简单版）。角色 y 低于阈值 → 回起点（避免试玩掉沟无限下落）。
    /// **M2.8 接正式失败重置**（腐败清零、回配置阶段等），本脚本届时替换/收窄。
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class FallReset : MonoBehaviour
    {
        [Tooltip("掉落 y 阈值（低于此值视为掉沟）")]
        [SerializeField] private float _fallThreshold = -5f;
        [Tooltip("重置位置（保持 0,0,0 则用 Awake 时初始位置）")]
        [SerializeField] private Vector3 _respawnPos = new Vector3(0f, 1f, 0f);
        [Tooltip("重置朝向（欧拉角）")]
        [SerializeField] private Vector3 _respawnEuler = new Vector3(0f, 90f, 0f);

        private CharacterController _cc;

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
        }

        private void Update()
        {
            if (transform.position.y < _fallThreshold)
            {
                _cc.enabled = false;
                transform.position = _respawnPos;
                transform.eulerAngles = _respawnEuler;
                _cc.enabled = true;
                Debug.Log("[FallReset] 掉沟，回起点（M2.7 简单重置；M2.8 接正式失败重置）");
            }
        }

        /// <summary>M4.2：检查点更新出生点（CheckPoint 碰触发调，幂等）。</summary>
        public void SetRespawn(Vector3 pos, Vector3 euler)
        {
            _respawnPos = pos;
            _respawnEuler = euler;
        }
    }
}
