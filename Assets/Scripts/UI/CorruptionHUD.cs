using UnityEngine;
using UnityEngine.UI;
using DontPushTheButton.Corruption;

namespace DontPushTheButton.UI
{
    /// <summary>
    /// 腐败 HUD（M2.6）：腐败条（Image fill，绿→红）+ 超载次数文本。
    /// 监听 CorruptionTracker.OnChanged 刷新。GDD 4.4 腐败可视化 + R4 临时超载计数。
    /// </summary>
    public class CorruptionHUD : MonoBehaviour
    {
        [SerializeField] private CorruptionTracker _tracker;
        [Tooltip("腐败条填充 Image（Image Type = Filled）")]
        [SerializeField] private Image _barFill;
        [Tooltip("超载次数文本")]
        [SerializeField] private Text _countText;
        [Tooltip("腐败条颜色渐变（低=绿，高=红）")]
        [SerializeField] private Gradient _colorGradient;

        private void OnEnable()
        {
            if (_tracker != null) _tracker.OnChanged += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            if (_tracker != null) _tracker.OnChanged -= Refresh;
        }

        private void Refresh()
        {
            if (_tracker == null) return;
            float n = Mathf.Clamp01(_tracker.Normalized);
            if (_barFill != null)
            {
                _barFill.fillAmount = n;
                _barFill.color = (_colorGradient != null && _colorGradient.colorKeys.Length > 0)
                    ? _colorGradient.Evaluate(n)
                    : Color.Lerp(Color.green, Color.red, n); // 配了渐变用渐变，否则兜底绿→红
            }
            if (_countText != null)
                _countText.text = "超载次数：" + _tracker.OverloadCount + " / 腐败 " + Mathf.RoundToInt(_tracker.Value) + "%";
        }
    }
}
