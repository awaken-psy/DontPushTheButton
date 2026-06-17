---
description: 里程碑工作流的通用规则，所有里程碑共享
---
# 里程碑通用规则

- 每完成一项任务，立即更新对应的 `docs/milestones/M{N}.md`，不要等批量更新。
- 每项任务完成后必须自测，并在里程碑文件中记录测试方式和结果，**不允许无测试记录的 `[x]`**。
- 遵守 `CLAUDE.md` 中定义的跨里程碑约束（URP / 新版 Input System / 数值集中配置 / 已接入 MCP for Unity 等）。
- 当里程碑全部完成时，自动更新 `CLAUDE.md` 路线图并告知用户。
- **不要跳过当前里程碑去提前做后续里程碑的任务**（M2 完成前不碰「后续待定」的评分/多关卡）。
- 如果发现任务清单遗漏了必要步骤，先添加再执行，不要默默跳过。
- 标记 `[!]` 的任务需要用户确认后才能继续推进。
- 测试遗留问题必须标注，不允许默默跳过失败的测试。

## 本项目补充
- **已接入 MCP for Unity**：Unity 编辑器打开 + MCP 面板 session active 时，Claude 经 `UnityMCP`（local scope 不进 git；配置见 `docs/config/MCP.md`）可直接操作 Unity——`read_console` 查报错、`find_gameobjects`/`manage_scene` 查场景、`run_tests` 跑测试、`manage_packages` 装包查包，作为 `MCP 验证` 手段。与无头 `-batchmode` 编译验证互补：MCP 需 Unity 开着，batchmode 可无人值守。测试方式优先级见 `.claude/commands/next.md`。
- **GDD 是设计单一事实来源**：机制/数值调整先改 `docs/design/GDD_键位重构_v0.3.md`，再落到代码与 ScriptableObject。
- **git 协作**：2-3 人，`feature/xxx` 分支 + PR；`main` 保持可玩。Claude 的 git 执行准则见 `.claude/rules/git-rules.md`，人类协作规范见 `docs/config/GIT.md`。
