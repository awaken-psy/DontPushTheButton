---
description: 本项目 Claude 的 git 执行准则（分支/提交时机/提交信息风格/Unity 纪律/PR）
---
# Git 执行准则（Claude）

> 约束 **Claude** 在本项目的 git 操作动作。人类协作规范（分支模型 / PR 礼仪 / 分支保护规则）的单一事实来源是 `docs/config/GIT.md`，本规则不重复，只补充 Claude 执行时的行为约束。

## 1. 分支
- 在 `feature/<简述>` 或 `fix/<简述>` 分支上工作。
- **绝不直接 commit / push `main`**：`main` 已启用分支保护（enforce_admins），连 owner 都推不上去。主动开分支，不要试错。

## 2. 提交时机
- 只在用户**明确要求**时执行 `git add/commit/push`（承接全局准则），不擅自提交。
- 「做完」「推进任务」**不等于**「提交」——提交需显式指示。

## 3. 提交信息
- **中文**，一句话说清改动。
- 格式 `<type>(<scope>): <一句话>`，遵循现有 git log 风格：
  - type ∈ `feat` / `fix` / `docs` / `refactor` / `chore` / `test`
  - scope 用里程碑编号，如 `M1.3`、`M2.1`；不归属某任务时可省 scope。
  - 例：`feat(M1.3): 接入 Input System + PlayerControls Action Map`、`docs(M1): 补记 1.3 重启验证`
- 大改动可在标题后空行加正文分行说明。

## 4. 小步提交
- 一个逻辑改动一个 commit。
- **文档改动与代码改动分开提交**（文档重组 ≠ 功能代码）。

## 5. Unity 纪律
- `.meta` 文件必须和对应资产 / 脚本**一起提交**（Unity 靠 meta 关联资源，漏提交会断引用）。
- commit 前 `git status` 核对，**绝不提交**：`Library/`、`Temp/`、`Obj/`、`Build/`、`Logs/`、`UserSettings/`、`*.sln`、`*.csproj`（`.gitignore` 已排除，仍需核对未误加）。

## 6. PR（合并 main 的唯一途径）
- 合并 `main` 必须**开 PR**（分支保护强制）。
- PR 标题同提交信息风格；**body 用中文**写清：改了什么、如何测试、关联里程碑任务。
- GitHub 不允许作者 approve 自己的 PR——**PR 需另一位队友批准**才能合；若团队仅一人活跃 git，告知用户把「最少批准数」改 0。

## 参考
- 人类协作规范（SoT）：`docs/config/GIT.md`
- 里程碑流程：`.claude/rules/milestone-rules.md`、`docs/milestones/M{N}.md`
