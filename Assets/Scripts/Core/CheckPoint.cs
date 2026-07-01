using UnityEngine;
using UnityEngine.InputSystem;
using DontPushTheButton.Player;

namespace DontPushTheButton.Core
{
    /// <summary>
    /// 检查点（M4.2）：关卡连续架构的分段点（靠 CheckPoints 区分关卡，非多场景）。
    /// - 碰触发器 → 更新出生点（FallReset.SetRespawn）+ 首次触发弹段结算（GameManager.CheckpointReached）
    /// - 范围内按 E → 打开 LoadoutUI 重新绑键（GameManager.EnterLoadoutForRebind，保留腐败）
    /// - 靠近 _interactRange → 激活 _hintUI 提示
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class CheckPoint : MonoBehaviour
    {
        [Header("出生点")]
        [Tooltip("出生点（默认 = 自身位置）")]
        [SerializeField] private Transform _respawnPoint;

        [Header("交互")]
        [Tooltip("交互键（默认 E）")]
        [SerializeField] private Key _interactKey = Key.E;
        [Tooltip("可交互范围（m，玩家在此距离内可按 E 重绑）")]
        [SerializeField] private float _interactRange = 3f;
        [Tooltip("靠近时的提示 UI（可选，默认隐藏）")]
        [SerializeField] private GameObject _hintUI;

        [Header("自动绑（留空运行时找）")]
        [SerializeField] private FallReset _fallReset;
        [SerializeField] private GameManager _gameManager;

        private bool _reached = false; // 首次触发结算标记
        private bool _inRange = false;
        private Transform _player;

        private void Awake()
        {
            var col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
            if (_respawnPoint == null) _respawnPoint = transform;
            if (_hintUI != null) _hintUI.SetActive(false);
        }

        private void Start()
        {
            if (_fallReset == null) _fallReset = FindObjectOfType<FallReset>();
            if (_gameManager == null) _gameManager = FindObjectOfType<GameManager>();
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) _player = p.transform;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            // 更新出生点（每次碰都更新，重复碰=同点，幂等）
            if (_fallReset != null)
                _fallReset.SetRespawn(_respawnPoint.position, _respawnPoint.eulerAngles);
            // 首次碰 → 段结算（关卡连续：该段评分 → 继续下一关）
            if (!_reached)
            {
                _reached = true;
                if (_gameManager != null) _gameManager.CheckpointReached();
            }
        }

        private void Update()
        {
            if (_player == null) return;
            float dist = Vector3.Distance(_player.position, transform.position);
            bool now = dist <= _interactRange;
            if (now != _inRange)
            {
                _inRange = now;
                if (_hintUI != null) _hintUI.SetActive(now);
            }
            if (_inRange && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                if (_gameManager != null) _gameManager.EnterLoadoutForRebind();
            }
        }
    }
}
