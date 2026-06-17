# Git 协作规范

> DontPushTheButton · 2-3 人小队
> 最后更新：2026-06-17

`main` 永远可玩、可编译；新功能开 feature 分支，PR review 后合并。

---

## 分支

| 分支 | 用途 | 规则 |
|---|---|---|
| `main` | 可玩主线 | 已保护，永远可编译可玩，只通过 PR 合入，**不直接推** |
| `feature/<简述>` | 每个功能/任务一分支 |  |
| `fix/<简述>` | bug 修复 | |

---

## 提交规范

- 提交信息用**中文**，一句话说清改动；大改动可加正文分行。
- **小步提交**：一个逻辑改动一个 commit。
- `.meta` 文件必须一起提交（Unity 靠 meta 关联资源）。
- 永远不要提交 `Library/`、`Temp/`、`Obj/`、`Build/`、`Logs/`（`.gitignore` 已排除）。

---

## PR

- 标题同提交信息风格；正文写清改了什么、如何测试。
- 合并前自测：Unity 能编译 + 相关功能 Play 通过 + 。
- 用 **Squash merge** 保持 main 历史干净。

---

## 分支保护

`main` 已启用分支保护（2026-06-17 配置，仓库 public 后免费可用）。实际生效规则（经 GitHub API 验证）：

| 规则 | 设置 | 说明 |
|------|------|------|
| 合并前必须开 PR | ✅ | 禁止直接 push `main` |
| 最少批准数 | **1** | PR 至少一人 approve 才能合并 |
| 新提交撤销旧批准 | ✅ | 推新代码后旧 approval 失效，强制 reviewer 重看 |
| 讨论须全部解决 | ✅ | PR 评论全部标记「已解决」才能合 |
| 线性历史 | ✅ | 配合 Squash merge，main 无 merge commit |
| 禁止 force push | ✅ | `main` 历史不可改写 |
| 禁止删分支 | ✅ | `main` 不可删 |
| enforce_admins | ✅ | 管理员也受规则约束，无人可绕过 |
| status check（CI）| ⏸️ 未启用 | 暂无 GitHub Actions；建好 `-batchmode` 编译 CI 后再加为必需 |

> ⚠️ **enforce_admins 已开**：包括仓库 owner 在内，所有人都**不能直接 push `main`，必须走 PR**；紧急修复也一样。
> ⚠️ GitHub 不允许 PR 作者 approve 自己的 PR——**最少批准数 1 意味着每条 PR 需要另一位队友批准**。若你是唯一活跃的 git 用户，PR 会卡住，届时把「最少批准数」改 0（仍保留 PR + 其余保护）。


