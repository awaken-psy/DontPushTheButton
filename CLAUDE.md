# DontPushTheButton · 键位重构 (Key Remap)

金海豚游戏开发大赛参赛作品。核心机制：把"键位绑定"做成解谜动词——在战前把能力拖到按键上（含超载模块），超载模块强化能力但累积腐败。详见 `docs/design/GDD_键位重构_v1.0.md`（**设计单一事实来源**；v0.6 / v0.9 / v0.9a 保留作历史）。

## 技术栈与约定
- **引擎**: Unity 2022.3 LTS + URP（Lowpoly 3D，腐败系统的后处理视效靠 URP Volume）
- **输入**: 新版 Input System（M1 固定 Action Map，M2 起支持运行时动态重绑）
- **视角/角色**: **俯视 2.5D（已锁定 2026-06-19）** + CharacterController——3D 场景 + 斜俯视固定角度相机。物理交互（推箱/拉物/磁力枪）方向清晰；跳跃纵深靠阴影 + 边缘标记补偿
- **配置数据**: GDD 中所有 `[PLACEHOLDER]` 调优变量（腐败增量、强化倍率、冷却、槽位数等）一律走 ScriptableObject 集中配置
- **平台**: PC 键鼠

## git 协作（2-3 人小队）
- `main` 始终保持可玩；新功能开 `feature/xxx` 分支，PR review 后合并
- 提交信息用中文，一句话说清改动
- Unity Editor 设置：`Version Control = Visible Meta Files`，`Asset Serialization = Force Text`
- `.gitignore` 用 Unity 官方模板（Library/ Temp/ Obj/ Build/ Logs/ 等不入库）

## 里程碑路线图

### M1: 工程骨架与核心移动 ✅
目标：Unity URP 工程 + git 协作就绪，角色能在白盒测试房间移动/跳跃，产出第一个可跑 build。
详细任务：docs/milestones/M1.md

### M2: 核心循环验证（最小可玩核心）✅
目标：打通「配置绑键（含超载）→ 进入关卡 → 普通/超载能力过关 → 腐败累积与满格重置」核心循环，用 **1 个示范关（关 2·裂口：引入超载·双解法）** 验证乐趣。
性质：**核心 Must**（scope 兜底的保底可玩层）
前置：M1 完成
详细任务：docs/milestones/M2.md

### M3: 内容与系统扩展（增量层）✅
目标：补齐全部能力（推动/拉动/冲刺 + 推动二选一 + 跳跃飞行）+ 5 关 + 4 星评分 + 腐败视效。
性质：**增量 Should**（能力优先于视效）
前置：M2 完成
详细任务：docs/milestones/M3.md

### M4: 打磨与深度（打磨层）（当前）
目标：全 5 关 playtest 调参 + 数值精调 + 腐败层级微调 + 3D 配置预览 + 关卡选择界面 + 结局。
性质：**打磨 Could**（时间允许才做；**4.4 playtest 最优先**）
前置：M3 完成 ✅
详细任务：docs/milestones/M4.md

### 跨里程碑约束
- 所有新系统先「能跑」（构建验证 + 编辑器试玩通过）才可标记完成，不允许无测试记录的 `[x]`
- 数值全部走 ScriptableObject 集中配置，便于 GDD 调优
- **scope 兜底（v0.7 决策）**：按优先级增量交付——M2（核心循环）必须独立可玩；M3/M4 按时间推进，超时则外层先砍（视效/机关反转/关卡选择），保核心乐趣验证不落空。详见 `docs/toConfirm.md §J`
- GDD 改动需同步回顾本文件的技术栈/约束是否要调整
- **验证手段**：已接入 **MCP for Unity**（CoplayDev/unity-mcp，开源；区别于商业 Coplay MCP）。Unity 编辑器交互式打开 + MCP 面板 session active 时，Claude 经 `UnityMCP`（`http://127.0.0.1:9090/mcp`，local scope 不进 git）可直接操作 Unity：`read_console`/`find_gameobjects`/`manage_scene`/`manage_packages`/`run_tests` 等。无头编译验证仍用 Unity `-batchmode`。**两者互补**：MCP 需 Unity 开着（端口 9090，因 8080 被 SRS 占），batchmode 可无人值守
