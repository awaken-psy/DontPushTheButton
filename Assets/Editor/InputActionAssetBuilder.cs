using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;

namespace DontPushTheButton.Editor
{
    /// <summary>
    /// 一次性工具：构建 M1 固定 PlayerControls.inputactions（Move/Look/Jump）。
    /// 菜单 DPTB/Build Input Action Asset 执行。资产生成固化后此脚本可删除。
    /// 用 API 构建（而非手写 JSON），保证 .inputactions 格式与 binding 引用正确。
    /// </summary>
    public static class InputActionAssetBuilder
    {
        [MenuItem("DPTB/Build Input Action Asset")]
        public static void Build()
        {
            var asset = ScriptableObject.CreateInstance<InputActionAsset>();
            asset.name = "PlayerControls";

            var map = new InputActionMap("Player");
            asset.AddActionMap(map);

            // Move: WASD 复合绑定 + 手柄左摇杆
            var move = map.AddAction("Move", InputActionType.Value, expectedControlLayout: "Vector2");
            move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            move.AddBinding("<Gamepad>/leftStick", groups: "Gamepad");

            // Look: 鼠标位移 + 手柄右摇杆
            var look = map.AddAction("Look", InputActionType.Value, expectedControlLayout: "Vector2");
            look.AddBinding("<Mouse>/delta", groups: "KeyboardMouse");
            look.AddBinding("<Gamepad>/rightStick", groups: "Gamepad");

            // Jump: 空格 + 手柄 A 键（buttonSouth）
            var jump = map.AddAction("Jump", InputActionType.Button);
            jump.AddBinding("<Keyboard>/space", groups: "KeyboardMouse");
            jump.AddBinding("<Gamepad>/buttonSouth", groups: "Gamepad");

            // 控制方案（binding group）：便于 M2 运行时按方案切换/重绑
            asset.AddControlScheme("KeyboardMouse").WithRequiredDevice("<Keyboard>").WithRequiredDevice("<Mouse>");
            asset.AddControlScheme("Gamepad").WithRequiredDevice("<Gamepad>");

            if (!AssetDatabase.IsValidFolder("Assets/Input"))
                AssetDatabase.CreateFolder("Assets", "Input");

            // .inputactions 必须是 JSON 格式（由 InputActionImporter 导入），
            // 不能用 AssetDatabase.CreateAsset（会序列化成 YAML 导致 importer 解析失败）。
            // 正确流程：ToJson() → 写文件 → ImportAsset。
            const string path = "Assets/Input/PlayerControls.inputactions";
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path) != null)
                AssetDatabase.DeleteAsset(path);

            System.IO.File.WriteAllText(path, asset.ToJson());
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            AssetDatabase.SaveAssets();

            var loaded = AssetDatabase.LoadAssetAtPath<InputActionAsset>(path);
            Debug.Log($"[InputActionAssetBuilder] created {path} | maps={loaded.actionMaps.Count} actions={loaded.actionMaps[0].actions.Count} schemes={loaded.controlSchemes.Count}");
        }
    }
}
