using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEditor;
using DontPushTheButton.UI;
using DontPushTheButton.Config;

namespace DontPushTheButton.Editor
{
    /// <summary>
    /// 构建配置阶段 UI（M2.4，M2.9 重写修 builder 系统性 bug）。
    /// 菜单 DPTB/Build Loadout UI (M2.4)。删旧 LoadoutCanvas → 重建（正确 size/color/active）。
    /// </summary>
    public static class LoadoutUIBuilder
    {
        private const string PrefabPath = "Assets/Prefabs/UI/LoadoutCanvas.prefab";

        [MenuItem("DPTB/Build Loadout UI (M2.4)")]
        public static void Build()
        {
            // 删旧场景实例（避免重复）
            var old = GameObject.Find("LoadoutCanvas");
            if (old != null) Object.DestroyImmediate(old);

            var root = new GameObject("LoadoutCanvas");
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            root.AddComponent<GraphicRaycaster>();
            var ctrl = root.AddComponent<LoadoutUIController>();

            // 背景（全屏半透明黑，白字可见）
            var bg = UICreate.Img("Background", root.transform, new Color(0, 0, 0, 0.85f));
            UIStretch(bg);

            // 标题（顶部）
            var title = UICreate.Text("Title", root.transform, "配置阶段 — 拖拽能力图标到按键（再次点击解绑）", 30, Color.white);
            UIPlace(title, new Vector2(0.5f, 1f), new Vector2(0, -50), new Vector2(1000, 50));

            // 槽位容器（中上）
            var slotCont = UICreate.Empty("SlotContainer", root.transform);
            var slg = slotCont.AddComponent<HorizontalLayoutGroup>();
            slg.spacing = 16f; slg.padding = new RectOffset(0, 0, 0, 0);
            slg.childAlignment = TextAnchor.UpperCenter;
            slg.childControlWidth = slg.childControlHeight = slg.childScaleWidth = slg.childScaleHeight = false;
            UIPlace(slotCont, new Vector2(0.5f, 0.72f), Vector2.zero, new Vector2(1100, 160));

            // 图标容器（中下）
            var iconCont = UICreate.Empty("IconContainer", root.transform);
            var ilg = iconCont.AddComponent<HorizontalLayoutGroup>();
            ilg.spacing = 16f; ilg.padding = new RectOffset(0, 0, 0, 0);
            ilg.childAlignment = TextAnchor.UpperCenter;
            ilg.childControlWidth = ilg.childControlHeight = ilg.childScaleWidth = ilg.childScaleHeight = false;
            UIPlace(iconCont, new Vector2(0.5f, 0.45f), Vector2.zero, new Vector2(1100, 140));

            // 校验文本
            var valText = UICreate.Text("ValidationText", root.transform, "", 26, Color.yellow);
            UIPlace(valText, new Vector2(0.5f, 0.25f), Vector2.zero, new Vector2(1000, 80));

            // 开始按钮
            var btn = UICreate.Img("StartButton", root.transform, new Color(0.2f, 0.7f, 0.3f));
            var button = btn.AddComponent<Button>();
            var btnLabel = UICreate.Text("Label", btn.transform, "开始关卡", 28, Color.white);
            UIStretch(btnLabel);
            UIPlace(btn, new Vector2(0.5f, 0.12f), Vector2.zero, new Vector2(260, 70));

            // 模板（隐藏；LoadoutUIController.BuildSlots/BuildIcons 会 SetActive(true)）
            var slotTemplate = CreateSlotTemplate(root.transform);
            slotTemplate.SetActive(false);
            var iconTemplate = CreateIconTemplate(root.transform);
            iconTemplate.SetActive(false);

            // 赋 controller private 引用
            var so = new SerializedObject(ctrl);
            so.FindProperty("_levelConfig").objectReferenceValue = AssetDatabase.LoadAssetAtPath<LevelConfigSO>("Assets/Config/Level2.asset");
            so.FindProperty("_slotContainer").objectReferenceValue = slotCont.transform;
            so.FindProperty("_iconContainer").objectReferenceValue = iconCont.transform;
            so.FindProperty("_slotPrefab").objectReferenceValue = slotTemplate.GetComponent<KeySlotUI>();
            so.FindProperty("_iconPrefab").objectReferenceValue = iconTemplate.GetComponent<AbilityIcon>();
            so.FindProperty("_validationText").objectReferenceValue = valText.GetComponent<Text>();
            so.FindProperty("_startButton").objectReferenceValue = button;
            so.ApplyModifiedPropertiesWithoutUndo();

            Directory.CreateDirectory("Assets/Prefabs/UI");
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            EnsureEventSystem();
            // 场景实例化（Start 自动 Build）
            PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath));
            Object.DestroyImmediate(root);

            Debug.Log($"[LoadoutUIBuilder] 重建 LoadoutCanvas（M2.9 修 builder bug）→ {PrefabPath}");
        }

        private static GameObject CreateSlotTemplate(Transform parent)
        {
            var go = UICreate.Img("SlotTemplate", parent, Color.white);
            ((RectTransform)go.transform).sizeDelta = new Vector2(140, 140);
            var key = UICreate.Text("KeyLabel", go.transform, "W", 30, Color.black);
            UIPlace(key, new Vector2(0.5f, 0.8f), Vector2.zero, new Vector2(140, 35));
            var bind = UICreate.Text("BindingLabel", go.transform, "", 20, Color.black);
            UIPlace(bind, new Vector2(0.5f, 0.3f), Vector2.zero, new Vector2(140, 40));
            var highlight = UICreate.Img("OverloadHighlight", go.transform, new Color(1f, 0.2f, 0.2f, 0.4f));
            UIStretch(highlight);
            highlight.SetActive(false);
            var slotUI = go.AddComponent<KeySlotUI>();
            var so = new SerializedObject(slotUI);
            so.FindProperty("_keyLabel").objectReferenceValue = key.GetComponent<Text>();
            so.FindProperty("_bg").objectReferenceValue = go.GetComponent<Image>();
            so.FindProperty("_bindingLabel").objectReferenceValue = bind.GetComponent<Text>();
            so.FindProperty("_overloadHighlight").objectReferenceValue = highlight;
            so.ApplyModifiedPropertiesWithoutUndo();
            return go;
        }

        private static GameObject CreateIconTemplate(Transform parent)
        {
            var go = UICreate.Img("IconTemplate", parent, new Color(0.3f, 0.4f, 0.6f));
            ((RectTransform)go.transform).sizeDelta = new Vector2(120, 120);
            var label = UICreate.Text("Label", go.transform, "", 22, Color.white);
            UIStretch(label);
            var cg = go.AddComponent<CanvasGroup>();
            var icon = go.AddComponent<AbilityIcon>();
            var so = new SerializedObject(icon);
            so.FindProperty("_label").objectReferenceValue = label.GetComponent<Text>();
            so.FindProperty("_canvasGroup").objectReferenceValue = cg;
            so.ApplyModifiedPropertiesWithoutUndo();
            return go;
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() != null) return;
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<InputSystemUIInputModule>();
        }

        // ---- uGUI 辅助 ----
        private static class UICreate
        {
            public static GameObject Empty(string name, Transform parent)
            {
                var go = new GameObject(name, typeof(RectTransform));
                go.transform.SetParent(parent, false);
                return go;
            }
            public static GameObject Img(string name, Transform parent, Color c)
            {
                var go = Empty(name, parent);
                go.AddComponent<Image>().color = c;
                return go;
            }
            public static GameObject Text(string name, Transform parent, string text, int size, Color color)
            {
                var go = Empty(name, parent);
                var t = go.AddComponent<Text>();
                t.text = text; t.fontSize = size; t.alignment = TextAnchor.MiddleCenter; t.color = color;
                t.raycastTarget = false;
                return go;
            }
        }

        private static void UIStretch(GameObject go)
        {
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        // anchor + anchoredPosition(pos) + sizeDelta(size) —— M2.9 修：pos 与 size 分离
        private static void UIPlace(GameObject go, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = anchor;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
        }
    }
}
