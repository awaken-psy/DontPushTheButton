# MCP for Unity 配置

> DontPushTheButton · Unity 运行时验证手段
> 最后更新：2026-06-17

本项目用 **MCP for Unity**（[`CoplayDev/unity-mcp`](https://github.com/CoplayDev/unity-mcp)，MIT 开源）作为 Unity 运行时 MCP：Claude（或其它 MCP 客户端）经 HTTP 直接操作 Unity 编辑器——读 Console、查场景、跑测试、改组件——作为「MCP 验证」手段，与无头 `-batchmode` 编译验证**互补**。

---

## 架构

Unity 端装 `MCPForUnity` 包 → 包内起 HTTP server；Claude 端配 HTTP MCP client 连它。

```
Unity 编辑器  ── (MCPForUnity 包起 HTTP server :9090) ──  Claude Code (mcp__UnityMCP__*)
```

---

## 1. Unity 端：装包

`MCPForUnity` 已通过 **git URL** 装入本工程（`Packages/manifest.json`）。

---

## 2. Claude 端：添加 MCP server（local scope，项目私有，不进 git）

```bash
claude mcp add --transport http UnityMCP http://127.0.0.1:9090/mcp -s local
```

- 改完**重启 Claude Code** 才加载 `mcp__UnityMCP__*` 工具。

---

## 3. 验证连通

Window - MCP for Unity - Toggle MCP Window 配置如下：

[mcp配置](../images/image1.png)

---

## 4. 常用工具速查

**只读优先**：

| 工具 | 用途 |
|---|---|
| `read_console` | 读 Unity Console（报错/警告/日志）|
| `find_gameobjects` | 按名/标签/组件/路径搜场景对象 |
| `manage_scene` (get_active / get_hierarchy) | 查当前场景与层级 |
| resource `mcpforunity://instances` / `project/info` / `editor/state` | 编辑器状态 |
| `run_tests` | 跑 Unity 测试 |
| `manage_packages` | 装包/查包 |

---

## 与 `-batchmode` 的分工

| 手段 | 需 Unity 打开 | 场景 |
|---|---|---|
| **MCP 验证** | ✅ 需要（端口 9090）| 读运行时状态、查场景、跑测试、改资产 |
| **batchmode 构建验证** | ❌ 无人值守 | CI / 自动编译验证：`Unity -batchmode -nographics -quit` |

两者互补：MCP 需 Unity 开着，batchmode 可无人值守。
