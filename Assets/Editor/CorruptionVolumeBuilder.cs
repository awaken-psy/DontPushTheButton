using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using DontPushTheButton.Corruption;

namespace DontPushTheButton.Editor
{
    /// <summary>
    /// 腐败视效 Volume builder（M3.8）：菜单「DPTB/Build Corruption FX Volume」一次性建：
    /// - VolumeProfile（Assets/Config/CorruptionVolume.asset，含 Vignette 红 + ColorAdjustments override）
    /// - 场景 CorruptionFXVolume（全局 Volume，weight=0，CorruptionEffects 调）
    /// - Player 加 CorruptionEffects + 连 _tracker/_volume（_impulseSource 留空，抖动可选后接）
    /// 可重复执行（清理重建）。
    /// </summary>
    public static class CorruptionVolumeBuilder
    {
        [MenuItem("DPTB/Build Corruption FX Volume (M3.8)")]
        public static void Build()
        {
            // 清旧场景 Volume
            var oldVol = GameObject.Find("CorruptionFXVolume");
            if (oldVol != null) Object.DestroyImmediate(oldVol);

            // ---- VolumeProfile 资产（Vignette + ColorAdjustments）----
            const string profilePath = "Assets/Config/CorruptionVolume.asset";
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, profilePath);
            }
            if (!profile.TryGet<Vignette>(out var vig)) vig = profile.Add<Vignette>(true);
            vig.intensity.Override(0f);
            vig.color.Override(new Color(0.8f, 0.1f, 0.1f));
            if (!profile.TryGet<ColorAdjustments>(out var ca)) ca = profile.Add<ColorAdjustments>(true);
            ca.hueShift.Override(0f);
            ca.saturation.Override(0f);

            // ---- 场景全局 Volume ----
            var volGO = new GameObject("CorruptionFXVolume");
            var vol = volGO.AddComponent<Volume>();
            vol.isGlobal = true;
            vol.weight = 0f; // CorruptionEffects 按层级调
            vol.sharedProfile = profile;

            // ---- Player 加 CorruptionEffects + 连引用 ----
            var player = GameObject.Find("Player");
            if (player != null)
            {
                var fx = player.GetComponent<CorruptionEffects>();
                if (fx == null) fx = player.AddComponent<CorruptionEffects>();
                var tracker = player.GetComponent<CorruptionTracker>();
                var so = new SerializedObject(fx);
                so.FindProperty("_tracker").objectReferenceValue = tracker;
                so.FindProperty("_volume").objectReferenceValue = vol;
                so.ApplyModifiedProperties();
            }
            else Debug.LogWarning("[CorruptionVolumeBuilder] Player 未找到，CorruptionEffects 未连");

            Undo.RegisterCreatedObjectUndo(volGO, "Build Corruption FX Volume");
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            Debug.Log("[CorruptionVolumeBuilder] Corruption FX Volume 已建（VolumeProfile + 场景 Volume + Player.CorruptionEffects 连 _tracker/_volume）");
        }
    }
}
