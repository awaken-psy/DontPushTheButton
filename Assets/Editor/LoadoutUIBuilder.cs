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
    /// 构建配置阶段 UI（M2.4）。菜单 DPTB/Build Loadout UI (M2.4)。
    /// 创建 LoadoutCanvas（Canvas + 槽位/图标容器 + 开始按钮 + 隐藏模板），
    /// 用 SerializedObject 赋 LoadoutUIController 的 private 引用 + Level2，存 prefab 并在当前场景实例化。
    /// </summary>
    public static class LoadoutUIBuilder
    {
        private const string PrefabPath = "Assets/Prefabs/UI/LoadoutCanvas.prefab";

        [MenuItem("DPTB/Build Loadout UI (M2.4)")]
        public static void Build()
        {
            // ---- Canvas 根 ----
            var root = new GameObject("LoadoutCanvas");
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            root.AddComponent<GraphicRaycaster>();
            var ctrl = root.AddComponent<LoadoutUIController>();

            // 背景
            var bg = UICreate.Img("Background", root.transform, new Color(0, 0, 0, 0.85f));
            UIStretch(bg);

            // 标题
            var title = UICreate.Text("Title", root.transform, "配置阶段 — 拖拽能力图标到按键（再次点击解绑）", 30);
            UISetAnchored(title, new Vector2(0.5f, 1f), new Vector2(0, -30));

            // 槽位容器
            var slotCont = UICreate.Empty("SlotContainer", root.transform);
            var slg = slotCont.AddComponent<HorizontalLayoutGroup>();
            slg.spacing = 16f; slg.padding = new RectOffset(40, 40, 0, 0);
            slg.childAlignment = TextAnchor.UpperCenter;
            slg.childControlWidth = slg.childControlHeight = false;
            slotCont.AddComponent<ContentSizeFitter>();
            UISetAnchored(slotCont, new Vector2(0.5f, 0.7f), new Vector2(0, 0));

            // 图标容器
            var iconCont = UICreate.Empty("IconContainer", root.transform);
            var ilg = iconCont.AddComponent<HorizontalLayoutGroup>();
            ilg.spacing = 16f; ilg.padding = new RectOffset(40, 40, 0, 0);
            ilg.childAlignment = TextAnchor.UpperCenter;
            iconCont.AddComponent<ContentSizeFitter>();
            UISetAnchored(iconCont, new Vector2(0.5f, 0.45f), new Vector2(0, 0));

            // 校验文本
            var valText = UICreate.Text("ValidationText", root.transform, "", 26);
            UISetAnchored(valText, new Vector2(0.5f, 0.25f), new Vector2(0, 0));

            // 开始按钮
            var btn = UICreate.Img("StartButton", root.transform, new Color(0.2f, 0.7f, 0.3f));
            var button = btn.AddComponent<Button>();
            var btnLabel = UICreate.Text("Label", btn.transform, "开始关卡", 28);
            UIStretch(btnLabel);
            UISetAnchored(btn, new Vector2(0.5f, 0.12f), new Vector2(240, 70));

            // 模板（隐藏）
            var slotTemplate = CreateSlotTemplate(root.transform);
            slotTemplate.SetActive(false);
            var iconTemplate = CreateIconTemplate(root.transform);
            iconTemplate.SetActive(false);

            // 赋 controller private 引用（SerializedObject）
            var so = new SerializedObject(ctrl);
            so.FindProperty("_levelConfig").objectReferenceValue = AssetDatabase.LoadAssetAtPath<LevelConfigSO>("Assets/Config/Level2.asset");
            so.FindProperty("_slotContainer").objectReferenceValue = slotCont.transform;
            so.FindProperty("_iconContainer").objectReferenceValue = iconCont.transform;
            so.FindProperty("_slotPrefab").objectReferenceValue = slotTemplate.GetComponent<KeySlotUI>();
            so.FindProperty("_iconPrefab").objectReferenceValue = iconTemplate.GetComponent<AbilityIcon>();
            so.FindProperty("_validationText").objectReferenceValue = valText.GetComponent<Text>();
            so.FindProperty("_startButton").objectReferenceValue = button;
            so.ApplyModifiedPropertiesWithoutUndo();

            // 存 prefab
            Directory.CreateDirectory("Assets/Prefabs/UI");
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);

            // EventSystem（新 Input System 用 InputSystemUIInputModule）
            EnsureEventSystem();

            // 场景实例化
            PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath));
            Object.DestroyImmediate(root); // 删除临时根（prefab 已存）

            Debug.Log($"[LoadoutUIBuilder] LoadoutCanvas prefab 存到 {PrefabPath}，已实例化到当前场景。");
        }

        private static GameObject CreateSlotTemplate(Transform parent)
        {
            // 槽位：白底（超载由 KeySlotUI 染红）+ 键名 + 绑定标签 + 超载红框
            var go = UICreate.Img("SlotTemplate", parent, Color.white);
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(140, 140);
            var key = UICreate.Text("KeyLabel", go.transform, "W", 30);
            UISetAnchored(key, new Vector2(0.5f, 0.85f), Vector2.zero);
            var bind = UICreate.Text("BindingLabel", go.transform, "", 20);
            UISetAnchored(bind, new Vector2(0.5f, 0.3f), Vector2.zero);
            var highlight = UICreate.Img("OverloadHighlight", go.transform, new Color(1f, 0.2f, 0.2f, 0.4f));
            UIStretch(highlight);
            highlight.SetActive(false);
            var slotUI = go.AddComponent<KeySlotUI>();
            // 赋 KeySlotUI private 引用
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
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(120, 120);
            var label = UICreate.Text("Label", go.transform, "", 22);
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
                var img = go.AddComponent<Image>();
                img.color = c;
                return go;
            }
            public static GameObject Text(string name, Transform parent, string text, int size)
            {
                var go = Empty(name, parent);
                var t = go.AddComponent<Text>();
                t.text = text; t.fontSize = size; t.alignment = TextAnchor.MiddleCenter; t.color = Color.white;
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

        private static void UISetAnchored(GameObject go, Vector2 anchor, Vector2 size)
        {
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = anchor;
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = size;
        }
    }
}
