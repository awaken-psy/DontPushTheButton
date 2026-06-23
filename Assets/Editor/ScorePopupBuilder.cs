using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using DontPushTheButton.Core;
using DontPushTheButton.UI;

namespace DontPushTheButton.Editor
{
    /// <summary>
    /// 评分结算 UI builder（M3.7）：菜单「DPTB/Build Score UI」一次性建：
    /// - ScorePopupCanvas（Canvas + Panel + 4 星 + 3 数据 Text + Total + 继续按钮 + ScorePopup 脚本 + 连引用，默认隐藏）
    /// - TimerHUD（挂 CorruptionHUDCanvas 下，Text + TimerHUD 脚本）
    /// - 连 GameManager._scorePopup
    /// 布局简陋（功能优先，美化留 playtest 后）。可重复执行（会重建）。
    /// </summary>
    public static class ScorePopupBuilder
    {
        [MenuItem("DPTB/Build Score UI (M3.7)")]
        public static void Build()
        {
            // 清理旧实例（可重复执行）
            var old = GameObject.Find("ScorePopupCanvas");
            if (old != null) Object.DestroyImmediate(old);

            // ---- ScorePopupCanvas ----
            var popupGO = new GameObject("ScorePopupCanvas");
            var canvas = popupGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            popupGO.AddComponent<CanvasScaler>();
            popupGO.AddComponent<GraphicRaycaster>();

            // Panel 背景（居中半透明）
            var panel = CreateImage("Panel", popupGO.transform, new Color(0, 0, 0, 0.85f));
            SetStretch(panel.rectTransform, new Vector2(0.3f, 0.25f), new Vector2(0.7f, 0.75f));

            // Title
            var title = CreateText("Title", "过关！", popupGO.transform, 32, Color.white);
            SetStretch(title.rectTransform, new Vector2(0, 0.85f), new Vector2(1, 1));

            // 4 星（暗星背景固定显示 + 亮星默认隐藏，Show 时 SetActive(earned)）
            var stars = new GameObject[4];
            string[] labels = { "通关", "时间", "节制", "精简" };
            for (int i = 0; i < 4; i++)
            {
                var dark = CreateText($"StarDark{i}", "★", popupGO.transform, 40, new Color(0.3f, 0.3f, 0.3f));
                SetAnchor(dark.rectTransform, new Vector2(0.2f + i * 0.2f, 0.62f));
                var bright = CreateText($"Star{i}", "★", popupGO.transform, 40, Color.yellow);
                SetAnchor(bright.rectTransform, new Vector2(0.2f + i * 0.2f, 0.62f));
                bright.gameObject.SetActive(false);
                stars[i] = bright.gameObject;
            }

            // 数据 Text
            var timeText = CreateText("TimeText", "用时 --", popupGO.transform, 24, Color.white);
            SetStretch(timeText.rectTransform, new Vector2(0, 0.42f), new Vector2(1, 0.55f));
            var overloadText = CreateText("OverloadText", "超载 --", popupGO.transform, 24, Color.white);
            SetStretch(overloadText.rectTransform, new Vector2(0, 0.32f), new Vector2(1, 0.42f));
            var freeSlotsText = CreateText("FreeSlotsText", "空槽 --", popupGO.transform, 24, Color.white);
            SetStretch(freeSlotsText.rectTransform, new Vector2(0, 0.22f), new Vector2(1, 0.32f));
            var totalText = CreateText("TotalText", "0 / 4", popupGO.transform, 28, Color.yellow);
            SetStretch(totalText.rectTransform, new Vector2(0, 0.10f), new Vector2(1, 0.22f));

            // 继续按钮
            var btnGO = new GameObject("ContinueButton");
            btnGO.transform.SetParent(popupGO.transform, false);
            var btnImg = btnGO.AddComponent<Image>();
            btnImg.color = new Color(0.2f, 0.6f, 0.2f);
            SetStretch(btnImg.rectTransform, new Vector2(0.35f, 0.02f), new Vector2(0.65f, 0.10f));
            var btn = btnGO.AddComponent<Button>();
            var btnLabel = CreateText("Label", "继续", btnGO.transform, 22, Color.white);
            SetStretch(btnLabel.rectTransform, Vector2.zero, Vector2.one);

            // ScorePopup 脚本 + 连引用
            var popup = popupGO.AddComponent<ScorePopup>();
            var so = new SerializedObject(popup);
            var starsProp = so.FindProperty("_stars");
            starsProp.arraySize = 4;
            for (int i = 0; i < 4; i++) starsProp.GetArrayElementAtIndex(i).objectReferenceValue = stars[i];
            so.FindProperty("_timeText").objectReferenceValue = timeText;
            so.FindProperty("_overloadText").objectReferenceValue = overloadText;
            so.FindProperty("_freeSlotsText").objectReferenceValue = freeSlotsText;
            so.FindProperty("_totalText").objectReferenceValue = totalText;
            so.FindProperty("_continueButton").objectReferenceValue = btn;
            so.ApplyModifiedProperties();

            popupGO.SetActive(false); // 默认隐藏（Show 时激活）

            // ---- TimerHUD（挂 CorruptionHUDCanvas 下）----
            // GameObject.Find 不找 inactive（配置阶段 HUD 隐藏），遍历根物体找
            GameObject hudGO = null;
            foreach (var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
                if (root.name == "CorruptionHUDCanvas") { hudGO = root; break; }
            if (hudGO != null)
            {
                var oldTimer = hudGO.transform.Find("TimerText");
                if (oldTimer != null) Object.DestroyImmediate(oldTimer.gameObject);
                var timerText = CreateText("TimerText", "00:00", hudGO.transform, 28, Color.white);
                SetStretch(timerText.rectTransform, new Vector2(0f, 0.9f), new Vector2(0.35f, 1f));
                var oldComp = hudGO.GetComponent<TimerHUD>();
                if (oldComp == null)
                {
                    var timer = hudGO.AddComponent<TimerHUD>();
                    var tso = new SerializedObject(timer);
                    tso.FindProperty("_timeText").objectReferenceValue = timerText;
                    tso.ApplyModifiedProperties();
                }
            }
            else Debug.LogWarning("[ScorePopupBuilder] CorruptionHUDCanvas 未找到，TimerHUD 未建");

            // ---- GameManager._scorePopup 连 ----
            var gm = Object.FindObjectOfType<GameManager>();
            if (gm != null)
            {
                var gso = new SerializedObject(gm);
                gso.FindProperty("_scorePopup").objectReferenceValue = popup;
                gso.ApplyModifiedProperties();
            }
            else Debug.LogWarning("[ScorePopupBuilder] GameManager 未找到，_scorePopup 未连");

            Undo.RegisterCreatedObjectUndo(popupGO, "Build Score UI");
            EditorUtility.SetDirty(popupGO);
            Debug.Log("[ScorePopupBuilder] Score UI 已建（ScorePopupCanvas + TimerHUD + 连 GameManager._scorePopup）");
        }

        private static Text CreateText(string name, string content, Transform parent, int size, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var t = go.AddComponent<Text>();
            t.text = content;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = size;
            t.color = color;
            t.alignment = TextAnchor.MiddleCenter;
            t.raycastTarget = false;
            return t;
        }

        private static Image CreateImage(string name, Transform parent, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = color;
            return img;
        }

        private static void SetStretch(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax)
        {
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static void SetAnchor(RectTransform rt, Vector2 center)
        {
            rt.anchorMin = center;
            rt.anchorMax = center;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(60, 60);
        }
    }
}
