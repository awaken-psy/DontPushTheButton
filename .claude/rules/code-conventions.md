---
description: C# 脚本参数化规范——可微调的手感/数值参数必须 SerializeField 或 SO，禁止 magic number 散落
---
# C# 脚本参数化规范

> **目标**：所有「后期可能微调」的重要参数（手感 / 数值 / 阈值）都必须能经 Inspector 或 ScriptableObject 调整，**禁止把可调数值写死成内联字面量**——调手感不应改代码、不应重编译。

## 1. 参数放哪里（按性质分层）

| 参数性质 | 放置方式 | 本项目实例 |
|---------|---------|-----------|
| **跨对象 / 全局数值**（GDD `[PLACEHOLDER]` 调优变量：速度/高度/重力/倍率/冷却…） | **ScriptableObject**（`[SerializeField] private` 字段 + `[Tooltip]` + 只读 `public` 属性） | `MovementTuning`：moveSpeed / jumpHeight / gravity / turnSpeed / groundStickVelocity |
| **组件特有可调参数**（单组件 Inspector 微调） | **`[SerializeField] private`** 字段（MonoBehaviour 上）+ `[Tooltip]` + 默认值 | `LandingMarker._groundY` / `._target`；`PlayerMover._moveInputThreshold` |
| **物理 / 数学常量**（永不调） | 直接写字面量 + 注释说明 | `Mathf.Sqrt(2f * g * h)` 里的 `2f`；向量归一阈值 `1f` |
| **Editor builder / 工具参数**（编辑器一次性构建用） | `const` + 注释；**运行时微调走场景对象**或改 const 重跑菜单 | `CinemachineTopDownBuilder`：Pitch / Yaw / Distance（相机微调 = 调场景 `CM TopDown` 的 Transform.rotation 与 FramingTransposer.m_CameraDistance） |

## 2. 规则

- 写新逻辑前，先识别每个数值「将来会不会调」：
  - **会调** → 归 SO（全局数值）或 `[SerializeField]`（组件特有），**不要内联**。
  - **不会调**（物理公式常量、标准阈值）→ 内联 + 注释。
- SO 字段：`[SerializeField] private` + `[Tooltip("中文说明 + 单位")]` + 合理默认值 + `public float X => _x;` 只读属性。
- 组件 `[SerializeField]`：`private` + `[Tooltip]` + 默认值，命名 `_camelCase`。
- **调手感 = 改 Inspector / SO 值，不是改 `.cs`**（Play 中改 `.asset` 即时生效，重启 Play 更稳）。
- 新增可调参数时同步更新 `docs/tests/M{N}-手动验证清单.md` 顶部「当前实现参数」与附录 A 调参速查。

## 3. 已落实（M1，2026-06-19）

- `MovementTuning` SO：moveSpeed / jumpHeight / gravity / turnSpeed / **groundStickVelocity**（贴地钳制）
- `PlayerMover`：`[SerializeField] _moveInputThreshold`（输入死区）；贴地走 SO
- `LandingMarker`：`[SerializeField] _groundY` / `_target`
- `CinemachineTopDownBuilder`：const + 注释（相机微调走场景 `CM TopDown`）

## 参考
- 数值集中配置的跨里程碑约束见项目 `CLAUDE.md`；GDD `[PLACEHOLDER]` 变量定义见 `docs/design/GDD_键位重构_v1.0.md`。
