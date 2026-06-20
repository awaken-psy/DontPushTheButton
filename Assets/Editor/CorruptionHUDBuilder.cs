using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using DontPushTheButton.UI;
using DontPushTheButton.Corruption;

namespace DontPushTheButton.Editor
{
    /// <summary>
    /// 构建腐败 HUD（M2.6）。菜单 DPTB/Build Corruption HUD (M2.6)。
    /// 创建 CorruptionHUDCanvas（腐败条 + 超载计数），自动绑定场景里的 CorruptionTracker。
    /// </summary>
    public static class CorruptionHUDBuilder
    {
        [MenuItem("DPTB/Build Corruption HUD (M2.6)")]
        public static void Build()
        {
            var tracker = Object.FindObjectOfType<CorruptionTracker>();
            if (tracker == null) { Debug.LogError("[CorruptionHUDBuilder] 场景无 CorruptionTracker"); return; }

            var canvas = new GameObject("CorruptionHUDCanvas", typeof(RectTransform));
            var c = canvas.AddComponent<Canvas>(); c.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.AddComponent<CanvasScaler>();
            canvas.AddComponent<GraphicRaycaster>();

            // 腐败条背景（左上）
            var barBg = new GameObject("BarBg", typeof(RectTransform));
            barBg.transform.SetParent(canvas.transform, false);
            var bgImg = barBg.AddComponent<Image>(); bgImg.color = new Color(0, 0, 0, 0.6f);
            var bgRT = (RectTransform)barBg.transform;
            bgRT.anchorMin = bgRT.anchorMax = new Vector2(0, 1f);
            bgRT.anchoredPosition = new Vector2(180, -30);
            bgRT.sizeDelta = new Vector2(340, 30);

            // 腐败条填充（绿→红， CorruptionHUD 运行时按 Gradient/Lerp 染色）
            var barFill = new GameObject("BarFill", typeof(RectTransform));
            barFill.transform.SetParent(barBg.transform, false);
            var fillImg = barFill.AddComponent<Image>(); fillImg.color = Color.green;
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillAmount = 0f;
            var fRT = (RectTransform)barFill.transform;
            fRT.anchorMin = Vector2.zero; fRT.anchorMax = Vector2.one;
            fRT.offsetMin = fRT.offsetMax = Vector2.zero;

            // 超载计数文本
            var countGo = new GameObject("Count", typeof(RectTransform));
            countGo.transform.SetParent(canvas.transform, false);
            var countText = countGo.AddComponent<Text>();
            countText.text = "超载次数：0 / 腐败 0%"; countText.fontSize = 22; countText.color = Color.white;
            var countRT = (RectTransform)countGo.transform;
            countRT.anchorMin = countRT.anchorMax = new Vector2(0, 1f);
            countRT.anchoredPosition = new Vector2(180, -65);
            countRT.sizeDelta = new Vector2(340, 30);

            // CorruptionHUD 组件 + 赋引用
            var hud = canvas.AddComponent<CorruptionHUD>();
            var so = new SerializedObject(hud);
            so.FindProperty("_tracker").objectReferenceValue = tracker;
            so.FindProperty("_barFill").objectReferenceValue = fillImg;
            so.FindProperty("_countText").objectReferenceValue = countText;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(canvas);
            Debug.Log($"[CorruptionHUDBuilder] CorruptionHUDCanvas 已创建，tracker={tracker.gameObject.name}");
        }
    }
}
