using UnityEngine;
using UnityEngine.UI;
using DontPushTheButton.Core;
using DontPushTheButton.Scoring;

namespace DontPushTheButton.UI
{
    /// <summary>
    /// 过关结算弹窗（M3.7）：显示 4 星（通关/时间/节制/精简）+ 用时 + 超载次数 + 空槽数 + 继续按钮。
    /// GameManager.EnterWon 调 Show；继续按钮 → GameManager.OnScorePopupContinue。
    /// 默认隐藏（GameObject inactive），Show 时激活。星用 GameObject 数组（亮星 SetActive(earned)）。
    /// </summary>
    public class ScorePopup : MonoBehaviour
    {
        [Header("星显示（4 个亮星 GameObject，顺序：通关/时间/节制/精简）")]
        [Tooltip("得星 → SetActive(true)；暗星背景应作为单独固定显示的物体")]
        [SerializeField] private GameObject[] _stars;
        [Header("数据文本")]
        [SerializeField] private Text _timeText;
        [SerializeField] private Text _overloadText;
        [SerializeField] private Text _freeSlotsText;
        [SerializeField] private Text _totalText;
        [Header("继续按钮")]
        [SerializeField] private Button _continueButton;

        private void Awake()
        {
            if (_continueButton != null)
                _continueButton.onClick.AddListener(OnContinue);
        }

        /// <summary>显示结算：填星 + 数据 + 激活。</summary>
        public void Show(ScoreResult r, float elapsed, int overloadCount, int freeSlots)
        {
            gameObject.SetActive(true);
            SetStar(0, r.Pass);
            SetStar(1, r.Time);
            SetStar(2, r.Moderation);
            SetStar(3, r.Lean);
            if (_timeText != null) _timeText.text = FormatTime(elapsed);
            if (_overloadText != null) _overloadText.text = overloadCount.ToString();
            if (_freeSlotsText != null) _freeSlotsText.text = freeSlots.ToString();
            if (_totalText != null) _totalText.text = $"{r.Total} / 4";
        }

        public void Hide() => gameObject.SetActive(false);

        private void SetStar(int i, bool earned)
        {
            if (_stars == null || i >= _stars.Length) return;
            if (_stars[i] != null) _stars[i].SetActive(earned);
        }

        private static string FormatTime(float sec)
        {
            int m = Mathf.FloorToInt(sec / 60f);
            int s = Mathf.FloorToInt(sec % 60f);
            return $"{m:00}:{s:00}";
        }

        private void OnContinue()
        {
            if (GameManager.Instance != null) GameManager.Instance.OnScorePopupContinue();
        }
    }
}
