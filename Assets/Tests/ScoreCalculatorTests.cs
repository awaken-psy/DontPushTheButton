using NUnit.Framework;
using DontPushTheButton.Scoring;

namespace DontPushTheButton.Tests
{
    /// <summary>
    /// 评分计算单测（M3.7）：4 项判定（通关/时间/节制/精简）× 边界。纯函数，无 GameObject。
    /// </summary>
    [TestFixture]
    public class ScoreCalculatorTests
    {
        private const float TimeSec = 30f;
        private const int OverloadMax = 3;
        private const int FreeMin = 1;

        [Test] public void AllCleared_FourStars()
        {
            var r = ScoreCalculator.Compute(10f, 0, 2, TimeSec, OverloadMax, FreeMin);
            Assert.IsTrue(r.Pass && r.Time && r.Moderation && r.Lean);
            Assert.AreEqual(4, r.Total);
        }

        [Test] public void AllMissed_StillOneStar_PassOnly()
        {
            var r = ScoreCalculator.Compute(100f, 10, 0, TimeSec, OverloadMax, FreeMin);
            Assert.IsTrue(r.Pass, "通关星恒 true（过关即得）");
            Assert.IsFalse(r.Time);
            Assert.IsFalse(r.Moderation);
            Assert.IsFalse(r.Lean);
            Assert.AreEqual(1, r.Total);
        }

        [Test] public void Boundary_Time_Exact_Pass()
        {
            var r = ScoreCalculator.Compute(30f, 0, 1, TimeSec, OverloadMax, FreeMin);
            Assert.IsTrue(r.Time, "elapsed == timeSec 应达标（≤）");
        }

        [Test] public void Boundary_Time_Over_Fail()
        {
            var r = ScoreCalculator.Compute(30.01f, 0, 1, TimeSec, OverloadMax, FreeMin);
            Assert.IsFalse(r.Time);
        }

        [Test] public void Boundary_Overload_Exact_Pass()
        {
            var r = ScoreCalculator.Compute(10f, 3, 1, TimeSec, OverloadMax, FreeMin);
            Assert.IsTrue(r.Moderation, "overload == max 应达标（≤）");
        }

        [Test] public void Boundary_FreeSlots_Exact_Pass()
        {
            var r = ScoreCalculator.Compute(10f, 0, 1, TimeSec, OverloadMax, FreeMin);
            Assert.IsTrue(r.Lean, "freeSlots == min 应达标（≥）");
        }

        [Test] public void Pass_Time_Moderation_ThreeStars()
        {
            var r = ScoreCalculator.Compute(20f, 2, 0, TimeSec, OverloadMax, FreeMin);
            Assert.IsTrue(r.Pass && r.Time && r.Moderation);
            Assert.IsFalse(r.Lean);
            Assert.AreEqual(3, r.Total);
        }
    }
}
