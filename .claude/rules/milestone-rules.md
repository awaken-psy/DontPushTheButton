---
description: 里程碑工作流通用规则（含提交节奏、测试分流）；何时提交归此文件管
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
- **提交节奏**：每完成一个子任务（`M{N}.x` 一个小节），自测通过后**立即提交一次**（`commit`；是否 `push` 看团队约定 / 用户当次指示），不等批量、不等用户逐次指示。**何时提交归本规则管；如何提交（分支 / 提交信息 / `.meta` / PR）归 `.claude/rules/git-rules.md` 管。**
- **测试方式决定行为**（测试不通过不提交，手动项除外）：
  - 安排为 `手动验证`（手感 / 人工观察类）的任务 → **不阻塞提交**：把「操作 + 期望」补进 `docs/tests/M{N}-手动验证清单.md`，子任务可标记 `[x]` 并提交；手动验证留到该里程碑全部完成后一次性测。
  - 安排为 `MCP 验证` / `集成测试` / `单测` 的任务 → **自己测、自己修**：测试红灯必须修到通过才提交，不允许提交失败测试；修不好则标 `[!]` 停下。
- **微调参数表维护**：每完成一个子任务，若**新增或修改了可微调参数**（SO 字段 / 组件 `[SerializeField]` / 重要的 Unity 内置属性如 CharacterController），必须同步更新 `docs/config/微调参数表.md`——记字段、当前值、位置、调什么、来源 milestone，并在变更记录加一行。参数化规范（什么该抽参数）见 `.claude/rules/code-conventions.md`。

## 本项目补充
- **已接入 MCP for Unity**：Unity 编辑器打开 + MCP 面板 session active 时，Claude 经 `UnityMCP`（local scope 不进 git；配置见 `docs/config/MCP.md`）可直接操作 Unity——`read_console` 查报错、`find_gameobjects`/`manage_scene` 查场景、`run_tests` 跑测试、`manage_packages` 装包查包，作为 `MCP 验证` 手段。与无头 `-batchmode` 编译验证互补：MCP 需 Unity 开着，batchmode 可无人值守。测试方式优先级见 `.claude/commands/next.md`。
- **GDD 是设计单一事实来源**：机制/数值调整先改 `docs/design/GDD_键位重构_v1.0.md`，再落到代码与 ScriptableObject（v0.6 / v0.9 / v0.9a 保留作历史）。
- **git 协作**：2-3 人，`feature/xxx` 分支 + PR；`main` 保持可玩。Claude 的 git 执行准则见 `.claude/rules/git-rules.md`，人类协作规范见 `docs/config/GIT.md`。
