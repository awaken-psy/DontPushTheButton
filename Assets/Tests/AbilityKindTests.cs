using System.Collections.Generic;
using NUnit.Framework;
using DontPushTheButton.Abilities;

namespace DontPushTheButton.Tests
{
    /// <summary>
    /// AbilityKind 枚举完整性单测（M3.1-3.5）：确认 6 能力（含 M3 新增 Pull/Dash，Pickup 搬运型 + Push 光束型）都注册。
    /// </summary>
    [TestFixture]
    public class AbilityKindTests
    {
        [Test] public void AbilityKind_Contains_AllSix()
        {
            var names = new HashSet<string>(System.Enum.GetNames(typeof(AbilityKind)));
            Assert.IsTrue(names.Contains("Move"));
            Assert.IsTrue(names.Contains("Jump"));
            Assert.IsTrue(names.Contains("Pickup"), "缺 Pickup（M3.1 拾取搬运）");
            Assert.IsTrue(names.Contains("Pull"), "缺 Pull（M3.2）");
            Assert.IsTrue(names.Contains("Dash"), "缺 Dash（M3.4）");
            Assert.IsTrue(names.Contains("Push"), "缺 Push（M3.3 光束推）");
            Assert.AreEqual(6, names.Count, "应正好 6 个能力（Move/Jump/Pickup/Pull/Dash/Push）");
        }
    }
}
