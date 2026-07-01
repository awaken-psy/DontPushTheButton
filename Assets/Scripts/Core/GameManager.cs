using UnityEngine;
using UnityEngine.InputSystem;
using DontPushTheButton.Player;
using DontPushTheButton.Corruption;
using DontPushTheButton.Scoring;
using DontPushTheButton.UI;

namespace DontPushTheButton.Core
{
    /// <summary>
    /// 游戏全局管理器单例 + 关卡流程状态机（M2.8 收口，M3.7 加评分/结算）。
    /// 状态：Loadout(配置) ↔ Playing(关卡) → Won(过关结算) / Failed(腐败满格) / Paused(Esc)。
    /// M3.7：EnterPlaying 记软计时起点；EnterWon 算 4 星 + 存 PlayerPrefs + 弹 ScorePopup，点继续才回 Loadout。
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState CurrentState { get; private set; } = GameState.Boot;
        /// <summary>本关开始时间（M3.7 软计时，TimerHUD 读）。EnterPlaying 时记录。</summary>
        public float PlayStartTime => _playStartTime;

        [Header("模块引用")]
        [SerializeField] private GameObject _loadoutCanvas;        // 配置 UI（LoadoutUIController 根）
        [SerializeField] private GameObject _corruptionHUD;       // 腐败 HUD（TimerHUD 挂其下）
        [SerializeField] private PlayerAbilityController _player;
        [SerializeField] private CorruptionTracker _corruption;
        [SerializeField] private LevelExit _levelExit;
        [SerializeField] private LoadoutUIController _loadoutUI;
        [Tooltip("过关结算弹窗（M3.7）")]
        [SerializeField] private ScorePopup _scorePopup;

        private float _playStartTime; // M3.7 软计时：进 Playing 记录
        private bool _returnToLoadoutAfterScore = true; // M4.2：结算继续→true=回Loadout(通关)/false=回Playing(检查点)

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

        private void OnDisable()
        {
            if (_corruption != null) _corruption.OnCorruptionFull -= EnterFailed;
            if (_levelExit != null) _levelExit.OnReached.RemoveListener(EnterWon);
        }

        private void Start()
        {
            // 运行时确保引用（防 SerializeField 断，如 builder 重建 LoadoutCanvas 使旧引用失效）
            if (_loadoutUI == null) _loadoutUI = FindObjectOfType<LoadoutUIController>();
            if (_loadoutCanvas == null && _loadoutUI != null) _loadoutCanvas = _loadoutUI.gameObject;
            if (_corruptionHUD == null)
            {
                var hud = FindObjectOfType<CorruptionHUD>();
                if (hud != null) _corruptionHUD = hud.gameObject;
            }
            if (_player == null) _player = FindObjectOfType<PlayerAbilityController>();
            if (_corruption == null) _corruption = FindObjectOfType<CorruptionTracker>();
            if (_levelExit == null) _levelExit = FindObjectOfType<LevelExit>();
            if (_scorePopup == null) _scorePopup = FindObjectOfType<ScorePopup>();
            // 订阅事件（引用就绪后）
            if (_corruption != null) _corruption.OnCorruptionFull += EnterFailed;
            if (_levelExit != null) _levelExit.OnReached.AddListener(EnterWon);
            // M3.11.5：教学关（场景无 LoadoutUI）跳过配置直接 Playing；PlayerAbilityController.Start 用 _levelConfig 自建默认 WASD Loadout
            if (_loadoutUI == null) EnterPlaying();
            else EnterLoadout(); // 初始进配置阶段
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
            SetUI(loadout: false, hud: true, popup: false);
            if (_player != null) _player.enabled = true;
            _playStartTime = Time.time; // M3.7 软计时起点
            CurrentState = GameState.Playing;
            Debug.Log("[GameManager] → Playing");
        }

        /// <summary>过关（LevelExit 触发，M3.7）：算评分 + 存星级 + 弹结算窗，点继续才回配置。</summary>
        public void EnterWon()
        {
            _returnToLoadoutAfterScore = true; // 通关结算→继续回配置
            CurrentState = GameState.LevelComplete;
            EvaluateAndShowScore();
        }

        /// <summary>M4.2：检查点段结算——弹 ScorePopup（该段评分），继续回 Playing（关卡连续，不回配置）。</summary>
        public void CheckpointReached()
        {
            _returnToLoadoutAfterScore = false; // 检查点结算→继续回 Playing
            CurrentState = GameState.LevelComplete;
            EvaluateAndShowScore();
        }

        /// <summary>M4.2：检查点按 E 重绑——进配置阶段（保留腐败），重绑后「开始关卡」回 Playing。</summary>
        public void EnterLoadoutForRebind() => EnterLoadout();

        /// <summary>M3.7：算 4 星 + 持久化 + 弹结算窗。</summary>
        private void EvaluateAndShowScore()
        {
            float elapsed = Time.time - _playStartTime;
            int overloadCount = _corruption != null ? _corruption.OverloadCount : 0;
            int freeSlots = 0;
            if (_loadoutUI != null && _loadoutUI.Config != null)
                foreach (var s in _loadoutUI.Config.Slots)
                    if (!s.Binding.HasValue) freeSlots++;
            // 阈值（默认占位，LevelConfig 覆盖）
            float timeSec = 30f; int overloadMax = 3; int freeMin = 1; string levelId = "Level";
            if (_loadoutUI != null && _loadoutUI.LevelConfig != null)
            {
                var lc = _loadoutUI.LevelConfig;
                timeSec = lc.ScoreTimeSec; overloadMax = lc.ScoreOverloadCount; freeMin = lc.ScoreFreeSlots;
                levelId = lc.LevelId;
            }
            var r = ScoreCalculator.Compute(elapsed, overloadCount, freeSlots, timeSec, overloadMax, freeMin);

            // 持久化（重玩取最高）
            string key = $"dptb_stars_{levelId}";
            int prev = PlayerPrefs.GetInt(key, 0);
            if (r.Total > prev) { PlayerPrefs.SetInt(key, r.Total); PlayerPrefs.Save(); }

            Debug.Log($"[GameManager] 过关！{r.Total}/4（通关/时间/节制/精简 = {r.Pass}/{r.Time}/{r.Moderation}/{r.Lean}）用时 {elapsed:F1}s 超载 {overloadCount} 空槽 {freeSlots}");
            if (_scorePopup != null) _scorePopup.Show(r, elapsed, overloadCount, freeSlots);
            SetUI(loadout: false, hud: false, popup: true);
        }

        /// <summary>结算窗「继续」按钮回调 → 通关回配置 / 检查点段结算回 Playing（M4.2）。</summary>
        public void OnScorePopupContinue()
        {
            if (_scorePopup != null) _scorePopup.Hide();
            if (_returnToLoadoutAfterScore)
            {
                EnterLoadout(); // 通关结算→回配置
            }
            else
            {
                // 检查点段结算→回 Playing 继续玩（关卡连续）
                SetUI(loadout: false, hud: true, popup: false);
                if (_player != null) _player.enabled = true;
                CurrentState = GameState.Playing;
                _returnToLoadoutAfterScore = true; // 复位
            }
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
            SetUI(loadout: true, hud: false, popup: false);
            if (_player != null) _player.enabled = false; // 配置时禁用 Player
            CurrentState = GameState.Loadout;
        }

        private void SetUI(bool loadout, bool hud, bool popup)
        {
            if (_loadoutCanvas != null) _loadoutCanvas.SetActive(loadout);
            if (_corruptionHUD != null) _corruptionHUD.SetActive(hud);
            if (!popup && _scorePopup != null && _scorePopup.gameObject.activeSelf) _scorePopup.Hide();
        }

        /// <summary>场景加载接口（M1 壳，保留）。</summary>
        public void LoadScene(string sceneName)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
    }
}
