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
- **已接入 MCP for Unity**：Unity 编辑器交互式打开 + MCP 面板 session active 时，Claude 经 `UnityMCP`（`http://127.0.0.1:9090/mcp`，local scope 不进 git）可直接操作 Unity（`read_console`/`find_gameobjects`/`manage_scene`/`manage_packages`/`run_tests`），作为 `MCP 验证` 手段。无头编译验证仍用 `-batchmode`（见 `.claude/commands/next.md`）。两者互补：MCP 需 Unity 开着，batchmode 可无人值守。
- **GDD 是设计单一事实来源**：机制/数值调整先改 `docs/GDD_键位重构_v0.3.md`，再落到代码与 ScriptableObject。
- **git 协作**：2-3 人，`feature/xxx` 分支 + PR；`main` 保持可玩；提交信息用中文。
