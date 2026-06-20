using UnityEngine;
using UnityEngine.Events;

namespace DontPushTheButton.Core
{
    /// <summary>
    /// 关卡出口触发器（M2.7）。玩家抵达 → 触发 OnReached。
    /// M2.8 接状态切换（OnReached → GameState.LevelComplete / 过关结算）。
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class LevelExit : MonoBehaviour
    {
        [Tooltip("抵达目标 tag（Player）")]
        [SerializeField] private string _targetTag = "Player";
        [Tooltip("抵达事件（M2.8 接过关状态）")]
        public UnityEvent OnReached;

        private void Awake()
        {
            var col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(_targetTag))
            {
                Debug.Log($"[LevelExit] 抵达出口（{name}）——过关（M2.8 接 LevelComplete 状态）");
                OnReached?.Invoke();
            }
        }
    }
}
