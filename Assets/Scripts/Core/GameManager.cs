using UnityEngine;
using UnityEngine.SceneManagement;

namespace DontPushTheButton.Core
{
    /// <summary>
    /// 游戏全局管理器单例（M1.6 最小框架）。
    /// 仅做：单例守护 + DontDestroyOnLoad 跨场景持久化 + 持有当前 GameState + 场景加载接口壳。
    /// **不实现状态切换逻辑**（M2.8 填充：转换合法性校验、各状态进入/退出副作用）。
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        /// <summary>当前游戏状态（初始 Boot）。切换逻辑 M2.8 实现。</summary>
        public GameState CurrentState { get; private set; } = GameState.Boot;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"[GameManager] 已存在单例，销毁重复实例 '{name}'。", this);
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>场景加载接口（M1 壳，M2 关卡流使用）。</summary>
        public void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

        // TODO(M2.8): 实现 SetState(GameState) 切换逻辑——
        // 转换合法性校验（如 Loadout↔Playing、Playing→LevelComplete/Failed）、
        // 各状态进入/退出副作用（腐败满格 Failed→Loadout、过关结算 UI 等）。
    }
}
