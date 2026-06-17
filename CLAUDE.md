# Cyber Sousa · 键位重构 (Key Remap)

金海豚游戏开发大赛参赛作品。核心机制：把"键位绑定"做成解谜动词——在战前把能力拖到按键上（含魔键），魔键强化能力但累积腐败。详见 `GDD_键位重构_v0.3.md`（**设计单一事实来源**）。

## 技术栈与约定
- **引擎**: Unity 2022.3 LTS + URP（Lowpoly 3D，腐败系统的后处理视效靠 URP Volume）
- **输入**: 新版 Input System（M1 固定 Action Map，M2 起支持运行时动态重绑）
- **视角/角色**: 默认第三人称跟随相机 + CharacterController（解谜物理交互更可控；可调）
- **配置数据**: GDD 中所有 `[PLACEHOLDER]` 调优变量（腐败增量、强化倍率、冷却、槽位数等）一律走 ScriptableObject 集中配置
- **平台**: PC 键鼠

## git 协作（2-3 人小队）
- `main` 始终保持可玩；新功能开 `feature/xxx` 分支，PR review 后合并
- 提交信息用中文，一句话说清改动
- Unity Editor 设置：`Version Control = Visible Meta Files`，`Asset Serialization = Force Text`
- `.gitignore` 用 Unity 官方模板（Library/ Temp/ Obj/ Build/ Logs/ 等不入库）

## 里程碑路线图

### M1: 工程骨架与核心移动（当前）
目标：Unity URP 工程 + git 协作就绪，角色能在白盒测试房间移动/跳跃，产出第一个可跑 build。
详细任务：milestones/M1.md

### M2: 最小可玩 Demo（核心循环验证）
目标：打通「配置键位 → 进入关卡 → 普通/魔键能力过关 → 腐败累积与满格重置」核心循环，用 1 个示范房间（对标 GDD「魔键初现」）验证乐趣。
前置：M1 完成
详细任务：milestones/M2.md

### 后续（待定）
评分系统、更多能力（时间减缓/旋转）、多关卡、音效打磨、打包发布——待 M2 验证核心乐趣后按敏捷再规划。

### 跨里程碑约束
- 所有新系统先「能跑」（构建验证 + 编辑器试玩通过）才可标记完成，不允许无测试记录的 `[x]`
- 数值全部走 ScriptableObject 集中配置，便于 GDD 调优
- GDD 改动需同步回顾本文件的技术栈/约束是否要调整
- 本项目无 Unity 运行时 MCP（现有 MCP 为 Godot 专用），验证手段为 Unity 命令行 `-batchmode` 编译 + 人工编辑器试玩
