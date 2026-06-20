using System.Collections.Generic;
using DontPushTheButton.Abilities;

namespace DontPushTheButton.Binding
{
    /// <summary>
    /// 配置校验器（纯函数）。GDD §A / 4.5 / toConfirm §A 规则落地。
    ///
    /// Error（致命，配置无效）：
    ///   - AbilityDoubleBound 同一能力绑多键（§A「一能双绑禁止」，重绑需先解绑）
    ///   - MoveDirectionDoubleBound 同一移动方向绑多键（WASD 各占独立槽）
    ///   - SlotOverflow 绑定项数 > 槽位数（4.5 槽位限制）
    ///   - AbilityUnavailable 绑了本关不可用能力（4.6 关卡可用能力）
    ///
    /// Warning（放行但有代价）：
    ///   - MoveDirectionUnbound 某移动方向未绑定（§A「放行但瘫痪」，不拦截）
    ///   - MoveOnOverload 移动方向绑超载键（§A「纯腐败无强化补偿」）
    /// </summary>
    public static class LoadoutValidator
    {
        public static ValidationResult Validate(LoadoutConfig config)
        {
            var result = new ValidationResult();
            if (config == null) { result.Add(ValidationLevel.Error, "NullConfig", "配置为空"); return result; }

            var slots = config.Slots;
            if (slots == null) { result.Add(ValidationLevel.Error, "NullSlots", "槽位列表为空"); return result; }

            // 1. 槽位超限：绑定项数 > 槽位数
            int boundCount = 0;
            foreach (var s in slots) if (s.Binding.HasValue) boundCount++;
            if (boundCount > config.SlotCount)
                result.Add(ValidationLevel.Error, "SlotOverflow",
                    $"绑定项 {boundCount} > 槽位数 {config.SlotCount}");

            // 2. 一能双绑 / 移动方向双绑 / 不可用能力
            var abilityFirstKey = new Dictionary<AbilityKind, string>();
            var dirFirstKey = new Dictionary<MoveDirection, string>();
            foreach (var s in slots)
            {
                if (!s.Binding.HasValue) continue;
                var item = s.Binding.Value;

                if (item.Type == BindingItemType.Ability)
                {
                    if (config.AvailableAbilities == null || !config.AvailableAbilities.Contains(item.Ability))
                        result.Add(ValidationLevel.Error, "AbilityUnavailable",
                            $"能力 {item.Ability} 不在本关可用集合");

                    if (abilityFirstKey.TryGetValue(item.Ability, out var firstKey))
                        result.Add(ValidationLevel.Error, "AbilityDoubleBound",
                            $"能力 {item.Ability} 已绑 {firstKey}，又绑 {s.KeyName}（每能一键）");
                    else
                        abilityFirstKey[item.Ability] = s.KeyName;
                }
                else // MoveDirection
                {
                    if (dirFirstKey.TryGetValue(item.Direction, out var firstKey))
                        result.Add(ValidationLevel.Error, "MoveDirectionDoubleBound",
                            $"移动方向 {item.Direction} 已绑 {firstKey}，又绑 {s.KeyName}（每方向一键）");
                    else
                        dirFirstKey[item.Direction] = s.KeyName;
                }
            }

            // 3. 未绑移动方向（Warning，放行但瘫痪）—— §A「未绑定移动放行但瘫痪」
            var boundDirs = new HashSet<MoveDirection>();
            foreach (var s in slots)
                if (s.Binding.HasValue && s.Binding.Value.Type == BindingItemType.MoveDirection)
                    boundDirs.Add(s.Binding.Value.Direction);
            foreach (MoveDirection d in System.Enum.GetValues(typeof(MoveDirection)))
                if (!boundDirs.Contains(d))
                    result.Add(ValidationLevel.Warning, "MoveDirectionUnbound",
                        $"移动方向 {d} 未绑定（放行但该方向瘫痪）");

            // 4. 移动方向绑超载键（Warning，纯腐败无强化）—— §A「移动绑超载=纯腐败无强化」
            foreach (var s in slots)
                if (s.IsOverload && s.Binding.HasValue && s.Binding.Value.Type == BindingItemType.MoveDirection)
                    result.Add(ValidationLevel.Warning, "MoveOnOverload",
                        $"移动方向 {s.Binding.Value.Direction} 绑在超载键 {s.KeyName}（按即腐败、无强化补偿）");

            return result;
        }
    }
}
