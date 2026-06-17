# DontPushTheButton · 键位重构 (Key Remap)

> 金海豚游戏开发大赛参赛作品

把「键位绑定」做成解谜动词——在战前把能力拖到按键上（含**魔键**）。普通键正常释放能力；**魔键**强化能力，但每次使用累积**腐败**，腐败满格触发重置。围绕这套机制设计解谜关卡。

---

## 环境与版本要求

| 项 | 要求 | 说明 |
|---|---|---|
| Unity | **2022.3.62f3**（revision `96770f904ca7`）| 必须同一小版本 |
| 渲染管线 | URP 14.0.12（`com.unity.render-pipelines.universal`）| 工程建立时选 Universal 3D 模板 |
| 输入 | 新版 Input System（`com.unity.inputsystem`）| URP 模板默认不含，已补装 |

> ⚠️ **版本锁定**：必须安装 `2022.3.62f3`，否则 Unity 警告版本不一致并改动 `Library`。包版本以 `Packages/manifest.json` 为准。

---

## 如何开始

### 1. clone 仓库

```bash
git clone https://github.com/awaken-psy/DontPushTheButton.git
cd DontPushTheButton
```

### 2. 安装 Unity

Unity Hub → Installs → Install Editor → 选 **2022.3.62f3**。

### 3. 打开工程

Unity Hub → Add → Add project from disk → 选 `DontPushTheButton` 根目录，首次打开会自动导入并生成 `Library/`。

### 4. 验证

- Console 无红色报错 → 按 ▶ Play 能跑。

---

## 项目结构

```
DontPushTheButton/
├── CLAUDE.md                    # 项目总纲：技术栈 / 约定 / 里程碑路线图
├── README.md                    # 本文件
├── docs/
│   ├── design/
│   │   └── GDD_键位重构_v0.3.md   # 游戏设计文档（设计单一事实来源）
│   ├── milestones/             # 里程碑任务清单
│   │   ├── M1.md               # 工程骨架与核心移动（当前）
│   │   └── M2.md               # 最小可玩 Demo
│   └── config/                 # 协作与配置文档
│       ├── SETUP.md            # 版本检查清单 + 首次新建工程(历史/重建参考)
│       ├── GIT.md              # Git 协作规范
│       └── MCP.md              # MCP for Unity 配置
├── Assets/                     # Unity 资源
├── Packages/                   # Unity 包依赖
└── ProjectSettings/            # Unity 工程设置（含 ProjectVersion.txt）
```

---

## 快速入口

- 📖 [设计初稿](docs/design/GDD_键位重构_v0.3.md)
- 🗺 里程碑：
  - [M1 工程骨架与核心移动](docs/milestones/M1.md) 
  - [M2 最小可玩 Demo](docs/milestones/M2.md)
- 🔧 [Git 协作规范](docs/config/GIT.md)
- 🤖 [MCP 配置](docs/config/MCP.md)
