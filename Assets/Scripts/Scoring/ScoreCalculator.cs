namespace DontPushTheButton.Scoring
{
    /// <summary>
    /// 评分结果（M3.7，GDD 4.7）：4 星 = 通关 + 时间 + 节制 + 精简。纯值类型。
    /// </summary>
    public readonly struct ScoreResult
    {
        /// <summary>通关星（过关即得，恒 true）。</summary>
        public readonly bool Pass;
        /// <summary>时间星：用时 ≤ 阈值。</summary>
        public readonly bool Time;
        /// <summary>节制星：超载次数 ≤ 阈值。</summary>
        public readonly bool Moderation;
        /// <summary>精简星：空闲槽位 ≥ 阈值。</summary>
        public readonly bool Lean;
        /// <summary>得星总数（0–4）。</summary>
        public readonly int Total;

        public ScoreResult(bool pass, bool time, bool moderation, bool lean, int total)
        {
            Pass = pass; Time = time; Moderation = moderation; Lean = lean; Total = total;
        }
    }

    /// <summary>
    /// 评分计算（M3.7）：纯静态，零 MonoBehaviour 依赖，便于单测。
    /// 4 项判定：通关(过关即得) / 时间(elapsed≤timeSec) / 节制(overload≤overloadMax) / 精简(freeSlots≥freeMin)。
    /// </summary>
    public static class ScoreCalculator
    {
        public static ScoreResult Compute(float elapsed, int overloadCount, int freeSlots,
                                          float timeSec, int overloadMax, int freeMin)
        {
            bool pass = true; // 通关星：过关即得
            bool time = elapsed <= timeSec;
            bool moderation = overloadCount <= overloadMax;
            bool lean = freeSlots >= freeMin;
            int total = (pass ? 1 : 0) + (time ? 1 : 0) + (moderation ? 1 : 0) + (lean ? 1 : 0);
            return new ScoreResult(pass, time, moderation, lean, total);
        }
    }
}
