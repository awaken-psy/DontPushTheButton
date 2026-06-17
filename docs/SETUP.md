# 项目配置与上手指南 (SETUP)

> DontPushTheButton · 键位重构 (Key Remap) · 金海豚游戏开发大赛
> 最后更新：2026-06-17

本文档面向**所有队友**：从零搭环境、首次新建工程、到 clone 接手开发。读完即可在本地跑起来。

---

## 1. 环境与版本要求

| 项 | 要求 | 说明 |
|---|---|---|
| Unity | **2022.3 LTS**（具体小版本见下「版本锁定」） | 通过 Unity Hub 安装；全队必须同一小版本 |
| 渲染管线 | URP (Universal Render Pipeline) | 工程建立时选 3D (URP) 模板 |
| 输入 | 新版 Input System | 工程建立后在 Player 设置启用 |
| 平台 | PC（Windows / macOS / Linux） | 键鼠 |
| Unity Hub | 最新版 | 管理多版本 Unity 与工程 |
| IDE | VS Code (C# Dev Kit) 或 JetBrains Rider | Rider 对 Unity 支持最好 |
| Git | 任意现代版本 | |
| GitHub CLI (`gh`) | 可选，简化推送/PR | 见第 5 节 |
| 远程仓库 | https://github.com/awaken-psy/DontPushTheButton | private，需邀请协作者 |

### 版本锁定（重要）
- Unity 具体小版本以工程根目录 `ProjectSettings/ProjectVersion.txt` 为准（M1.1 建立工程后生成）。
- **所有队友必须安装与之完全一致的小版本**，否则 Unity 会警告 "different version" 并可能改动 Library。
- 包版本以 `Packages/manifest.json` 为准，**不要在本地手动升级包**；要升包走 PR。
- **当前锁定**：Unity `2022.3.62f3`（revision 96770f904ca7）+ URP `14.0.12`（`com.unity.render-pipelines.universal`）。所有成员必须安装 `2022.3.62f3`。

---

## 2. 版本检查清单

每次拉取 / 开始工作前快速过一遍：

- [ ] `ProjectSettings/ProjectVersion.txt` 的版本 = 本机 Unity 版本（Hub 顶部可见）
- [ ] 拉取后 Unity 没有 "Inconsistent / Different Unity version" 警告
- [ ] `Assets/` 下没有未预期的大量 `.meta` 变动（通常因 Unity 版本不一致）
- [ ] `Library/` 是本机生成的（`.gitignore` 已排除，绝不手动提交）
- [ ] 当前分支不是 `main`（在 feature 分支上工作）

---

## 3. 首次新建项目（已完成；保留步骤供重建/参考）

> ✅ 本仓库的 Unity 工程已于 2026-06-17 用 **Universal 3D (URP)** 模板建好，工程文件在仓库内。队友**无需重建**，直接第 4 节「clone 接手」即可。本节仅留作重建/参考。

### 用 Unity Hub 的 Universal 3D 模板（实际采用的方式）
1. Unity Hub → New Project → 模板选 **Universal 3D**（= 3D URP；**不要**选 "3D (Built-In)"）。
   - 该模板右上角如有下载图标，点选后 Hub 会联网下载模板（约几十秒）。
2. Project name 随意（如 `tmp_project`），Location 选仓库**之外**的临时目录（仓库根目录已非空，Hub 不允许直接在此新建）。
3. Version 选 `2022.3.62f3`，Create project。
4. 首次打开确认 URP 正常（`Assets/Settings/` 下有 `URP-*.asset`），关闭。
5. 把工程内容搬进仓库根目录（保留现有 `CLAUDE.md` / `docs/` / `.claude/` / `.git`）：
   - **搬入**：`Assets/`、`Packages/`、`ProjectSettings/`（含 `ProjectVersion.txt`）。
   - **不要搬**：`Library/`、`Logs/`、`Obj/`、`Temp/`、`UserSettings/`、`*.sln`、`*.csproj`（已在 `.gitignore`）。
6. Unity Hub → Add → 选仓库目录，确认能打开并 Play。
7. git 提交（见第 5 节）。

### 工程建立后的必要配置
- Editor → Project Settings → **Editor**：
  - Version Control → `Visible Meta Files`
  - Asset Serialization → `Force Text`
- **补装 Input System**：Universal 3D 模板默认用旧 Input Manager，本工程要用新版 Input System。
  - Package Manager 安装 **Input System**；安装后 Player → Active Input Handling 选 `Input System Package (New)` 或 `Both`（M1.3 任务）。
- 版本号已回填本文档第 1 节「版本锁定」。

---

## 4. 队友接手：clone & run

```bash
git clone https://github.com/awaken-psy/DontPushTheButton.git
cd DontPushTheButton
```

1. **安装 Unity**：Unity Hub → Installs → Install Editor → 选 `ProjectSettings/ProjectVersion.txt` 写明的 2022.3 LTS 小版本。
2. **打开工程**：Hub → Add → Add project from disk → 选 `DontPushTheButton` 根目录。
3. **首次打开**会自动导入并生成 `Library/`（几分钟），等进度条走完。
4. 确认 Console 无红色报错 → 按 ▶ Play 能跑（M1 完成后即可在白盒房间走/跳）。
5. （可选）配置推送凭证：`gh auth login` 或 `gh auth setup-git`。

> Unity 若提示版本不一致：以 `ProjectVersion.txt` 为准装对应版本，**不要点 "Upgrade"**。

---

## 5. Git 协作约定（2-3 人小队）

### 分支模型
- `main`：永远可玩、可编译。只通过 PR 合入，不直接推。
- `feature/<简述>`：每个功能/任务一分支，如 `feature/m1-character-controller`、`feature/m2-bind-ui`。
- `fix/<简述>`：bug 修复。

### 工作流
```bash
git checkout main && git pull
git checkout -b feature/xxx
# ...开发...
git add -A && git commit -m "<中文，一句话说清改动>"
git push -u origin feature/xxx
# 在 GitHub 开 PR，至少一人 review 后合并
```

### 提交规范
- 提交信息用**中文**，一句话说清改动；大改动可加正文分行。
- **小步提交**：一个逻辑改动一个 commit。
- `.meta` 文件必须一起提交（Unity 靠 meta 关联资源）。
- 永远不要提交 `Library/`、`Temp/`、`Obj/`、`Build/`（`.gitignore` 已排除）。

### PR
- 标题同提交信息风格；正文写清改了什么、如何测试。
- 合并前自测：Unity 能编译 + 相关功能 Play 通过。
- 至少一人 approve 再合并；用 **Squash merge** 保持 main 历史干净。

---

## 6. 项目结构

```
DontPushTheButton/
├── CLAUDE.md              # 项目总纲：技术栈 / 约定 / 里程碑路线图
├── README.md              # （建议补充）项目简介与快速入口
├── docs/
│   ├── SETUP.md           # 本文件
│   ├── GDD_键位重构_v0.3.md  # 游戏设计文档（设计单一事实来源）
│   └── milestones/        # 里程碑任务清单
│       ├── M1.md          # 工程骨架与核心移动
│       └── M2.md          # 最小可玩 Demo
├── Assets/                # Unity 资源（M1.1 后生成）
├── Packages/              # Unity 包依赖
├── ProjectSettings/       # Unity 工程设置（含 ProjectVersion.txt）
└── .claude/               # Claude Code 工作流配置（/next 命令、里程碑规则）
```

### 建议的 Assets 子结构（开发中逐步建立）
```
Assets/
├── Scenes/            # .unity 场景
├── Scripts/           # C# 脚本（按模块分：Player / Ability / UI / System…）
├── Prefabs/           # 预制体
├── ScriptableObjects/ # 数值/配置（腐败增量、强化倍率、关卡配置…）
├── Art/               # 美术资源（模型 / 贴图 / 材质）
├── Audio/             # 音效 / 音乐
└── Settings/          # URP 资产、Input Actions 等
```

---

## 7. 开发约定

- **数值集中配置**：GDD 中所有 `[PLACEHOLDER]` 调优变量（腐败增量、强化倍率、冷却、槽位数等）一律用 **ScriptableObject**，放 `Assets/ScriptableObjects/`，便于策划/测试即调，不要硬编码。
- **GDD 是设计单一事实来源**：机制或数值调整先改 `docs/GDD_键位重构_v0.3.md`，再落到代码与配置。
- **命名**：C# 类用 PascalCase，私有字段 `_camelCase`，文件名与类名一致。
- **输入**：统一走新版 Input System 的 Action Map，不要直接 `Input.GetKey`。
- **任务推进**：用 `/next`（见 `.claude/commands/next.md`）按里程碑推进，每完成一项在对应 `docs/milestones/M{N}.md` 记录测试结果。

---

## 8. 验证方式（无 Unity 运行时 MCP）

本项目无 Unity 运行时 MCP，验证靠：
1. **构建验证**（最低要求）：命令行编译确认无报错。
   ```
   Unity -batchmode -nographics -quit -projectPath <仓库根目录> -logFile -
   ```
2. **手动验证 / 试玩**：Unity Editor 中 Play，检查目标行为；M2 起核心循环需实际试玩确认手感。

每个里程碑任务标记完成前，至少要有「构建验证」通过的记录。

---

## 9. 常见问题 FAQ

**Q: 打开工程提示 Unity 版本不一致？**
A: 看 `ProjectSettings/ProjectVersion.txt`，在 Hub 装完全相同的小版本；不要点 Upgrade。

**Q: merge 冲突出现在 `.meta` / `.unity` 场景文件？**
A: `.meta` 冲突通常取其一并保证 guid 一致；场景冲突最好在 Editor 里解决，或与改动者协调。养成「改场景前先 pull」的习惯。

**Q: 我的 Library / 导入结果和别人不一样？**
A: 正常，Library 是本机生成的，不进 git。删掉 `Library/` 重新打开工程会重新生成。

**Q: 推送被拒（non-fast-forward）？**
A: 先 `git pull --rebase`，解决冲突再推。

**Q: 如何加新队友？**
A: 仓库 Settings → Collaborators → 邀请其 GitHub 账号；对方接受后按第 4 节接手。
