using UnityEngine;
using UnityEngine.InputSystem;
using DontPushTheButton.Player;
using DontPushTheButton.Corruption;
using DontPushTheButton.UI;

namespace DontPushTheButton.Core
{
    /// <summary>
    /// 游戏全局管理器单例 + 关卡流程状态机（M2.8 收口，串联 M2.4-M2.7）。
    /// 状态：Loadout(配置) ↔ Playing(关卡) → Won(过关) / Failed(腐败满格) / Paused(Esc 返回配置)。
    /// 配置保留：LoadoutConfig 跨状态不清空。
    /// 腐败清零策略（GDD 4.4 / H4）：满格重置=清零+保留配置；主动返回(Esc)=保留腐败（惩罚放弃）。
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState CurrentState { get; private set; } = GameState.Boot;

        [Header("模块引用")]
        [SerializeField] private GameObject _loadoutCanvas;        // 配置 UI（LoadoutUIController 根）
        [SerializeField] private GameObject _corruptionHUD;       // 腐败 HUD
        [SerializeField] private PlayerAbilityController _player;
        [SerializeField] private CorruptionTracker _corruption;
        [SerializeField] private LevelExit _levelExit;
        [SerializeField] private LoadoutUIController _loadoutUI;

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

        private void OnEnable()
        {
            if (_corruption != null) _corruption.OnCorruptionFull += EnterFailed;
            if (_levelExit != null) _levelExit.OnReached.AddListener(EnterWon);
        }

        private void OnDisable()
        {
            if (_corruption != null) _corruption.OnCorruptionFull -= EnterFailed;
            if (_levelExit != null) _levelExit.OnReached.RemoveListener(EnterWon);
        }

        private void Start()
        {
            EnterLoadout(); // 初始进配置阶段
        }

        private void Update()
        {
            // Esc 暂停返回配置（死局逃生，保留腐败）
            if (CurrentState == GameState.Playing
                && Keyboard.current != null
                && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                EnterLoadoutFromPause();
            }
        }

        /// <summary>LoadoutUIController「开始关卡」调：校验通过 → 进关卡。</summary>
        public void EnterPlaying()
        {
            if (_loadoutUI != null && _player != null)
                _player.SetLoadout(_loadoutUI.Config); // 传玩家配置（保留）
            SetUI(loadout: false, hud: true);
            if (_player != null) _player.enabled = true;
            CurrentState = GameState.Playing;
            Debug.Log("[GameManager] → Playing");
        }

        /// <summary>过关（LevelExit 触发）。M2 单关：回配置重玩（结算/星级归 M3）。</summary>
        public void EnterWon()
        {
            CurrentState = GameState.LevelComplete;
            Debug.Log("[GameManager] 过关！→ 回配置重玩（结算/评分归 M3）");
            EnterLoadout();
        }

        /// <summary>腐败满格（CorruptionTracker.OnCorruptionFull）：清零 + 保留配置 → 回配置。</summary>
        private void EnterFailed()
        {
            if (_corruption != null) _corruption.ResetValue(); // H4：满格重置=腐败清零
            Debug.Log("[GameManager] 腐败满格 → 重置（保留配置 + 腐败清零，鼓励重试）");
            EnterLoadout(); // LoadoutConfig 不动（保留）
        }

        /// <summary>Esc 返回配置（死局逃生）：保留腐败（惩罚放弃）。</summary>
        private void EnterLoadoutFromPause()
        {
            Debug.Log("[GameManager] Esc 返回配置（保留腐败，死局逃生 §J1）");
            EnterLoadout(); // 不清零
        }

        private void EnterLoadout()
        {
            SetUI(loadout: true, hud: false);
            if (_player != null) _player.enabled = false; // 配置时禁用 Player
            CurrentState = GameState.Loadout;
        }

        private void SetUI(bool loadout, bool hud)
        {
            if (_loadoutCanvas != null) _loadoutCanvas.SetActive(loadout);
            if (_corruptionHUD != null) _corruptionHUD.SetActive(hud);
        }

        /// <summary>场景加载接口（M1 壳，保留）。</summary>
        public void LoadScene(string sceneName)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
    }
}
