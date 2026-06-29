using UnityEngine;
using UnityEngine.UI;
using DontPushTheButton.Core;

namespace DontPushTheButton.UI
{
    /// <summary>
    /// 软计时 HUD（M3.7）：Playing 时显示本关用时（mm:ss）。墙钟（Esc 暂停不停表，软计时不催）。
    /// 读 GameManager.PlayStartTime；GameObject 由 GameManager.SetUI 控制显隐（Playing 时显示）。
    /// </summary>
    public class TimerHUD : MonoBehaviour
    {
        [Tooltip("用时文本")]
        [SerializeField] private Text _timeText;

        private void Update()
        {
            if (_timeText == null) return;
            if (GameManager.Instance == null) return;
            if (GameManager.Instance.CurrentState != GameState.Playing) return;

            float elapsed = Time.time - GameManager.Instance.PlayStartTime;
            int m = Mathf.FloorToInt(elapsed / 60f);
            int s = Mathf.FloorToInt(elapsed % 60f);
            _timeText.text = $"{m:00}:{s:00}";
        }
    }
}
