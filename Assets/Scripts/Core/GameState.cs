namespace DontPushTheButton.Core
{
    /// <summary>
    /// 游戏全局状态（M1.6 仅定义枚举，切换逻辑 M2.8 实现）。
    /// 覆盖 GDD 全流程：启动→菜单→引导→配置绑键→关卡→暂停/过关/失败→通关。
    /// </summary>
    public enum GameState
    {
        Boot,            // 启动 / 初始化
        MainMenu,        // 主菜单
        Onboarding,      // 新手引导（关 0，GDD §5）
        Loadout,         // 配置阶段 / 战前绑键（GDD §4.1）
        Playing,         // 关卡进行中
        Paused,          // 暂停（可返回配置或继续）
        LevelComplete,   // 过关结算（GDD §4.7 星级）
        Failed,          // 腐败满格 → 关卡重置回配置（GDD §4.6）
        GameComplete     // 5 关全通
    }
}
