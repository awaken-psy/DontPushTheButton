using NUnit.Framework;
using UnityEngine;
using DontPushTheButton.Corruption;

namespace DontPushTheButton.Tests
{
    /// <summary>
    /// 腐败视效单测（M3.8）：ComputeLevel 层级边界（纯函数）+ 默认系数（L0 都 1）。
    /// </summary>
    [TestFixture]
    public class CorruptionEffectsTests
    {
        [Test] public void ComputeLevel_Boundaries()
        {
            Assert.AreEqual(0, CorruptionEffects.ComputeLevel(0f), "0 → L0");
            Assert.AreEqual(0, CorruptionEffects.ComputeLevel(0.24f), "<0.25 → L0");
            Assert.AreEqual(1, CorruptionEffects.ComputeLevel(0.25f), "0.25 → L1");
            Assert.AreEqual(1, CorruptionEffects.ComputeLevel(0.49f), "<0.5 → L1");
            Assert.AreEqual(2, CorruptionEffects.ComputeLevel(0.5f), "0.5 → L2");
            Assert.AreEqual(2, CorruptionEffects.ComputeLevel(0.74f), "<0.75 → L2");
            Assert.AreEqual(3, CorruptionEffects.ComputeLevel(0.75f), "0.75 → L3");
            Assert.AreEqual(3, CorruptionEffects.ComputeLevel(0.94f), "<0.95 → L3");
            Assert.AreEqual(4, CorruptionEffects.ComputeLevel(0.95f), "0.95 → L4");
            Assert.AreEqual(4, CorruptionEffects.ComputeLevel(1.0f), "1.0 → L4");
        }

        [Test] public void Multipliers_DefaultLevelZero_AllOnes()
        {
            // 默认 _level=0（无 tracker → Refresh return）→ 系数都 1（无降速/延长）
            var go = new GameObject("fx");
            var fx = go.AddComponent<CorruptionEffects>();
            Assert.AreEqual(1f, fx.MoveSpeedMultiplier, "L0 移速 1");
            Assert.AreEqual(1f, fx.CooldownMultiplier, "L0 冷却 1");
            Object.DestroyImmediate(go);
        }
    }
}
