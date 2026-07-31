# 完全移除 RetroGame 项目 UnityMCP

## 当前状态

| 项 | 结论 |
| --- | --- |
| 目标与当前判断 | 从当前项目移除 UnityMCP 插件本体、项目内 UnityCaptain 设置及项目专用 UnityMCP Skills。 |
| 状态 | ⚠️ 待运行态验证 |
| 授权 | 用户已明确授权“完全移除当前项目中的 unitymcp”。 |
| 已确认边界 | `Assets/Plugins/UnityMCP/`、`Assets/Plugins/UnityMCP.meta`、`Assets/UnityCaptain/`、`Assets/UnityCaptain.meta`、`Assets/UnityCaptainSkills~/`、`Assets/UnityCaptainSkills~.meta`。 |
| 排除范围 | 全局 Codex 配置/Skills、其他工程、游戏业务代码和先前的 `agent/plan/` 文档。 |
| 下一步 | 在 RetroGame Unity Editor 中刷新资产并回读 Console 编译状态。 |

## Task Contract

| 目标结果 | Invariants | 允许/禁止范围 | 验收 | 回退 |
| --- | --- | --- | --- | --- |
| 项目中不再保留 UnityMCP 的本体、启动设置或项目专用 Skills，且现有游戏代码可通过 Unity 编译。 | 保留业务资产、其他插件、全局 AI 工具配置和已有方案文档；不提交、不推送。 | 仅删除已确认 UnityMCP/UnityCaptain 项目资产及其 `.meta`；可移除发现的项目内明确配置引用。禁止删除不属于当前仓库的配置。 | Git 路径与文本残留扫描为零；Unity 刷新/编译无本轮新增错误；最终 diff 仅为任务与 `agent/` 文档。 | 删除内容仍由 Git 可恢复；若 Unity 编译出现对 UnityMCP 的真实依赖，恢复该删除批次并定位 consumer。 |

## 执行与验证

| 阶段 | 工作项 | 完成标准 | 状态 | 证据/缺口/下一步 |
| --- | --- | --- | --- | --- |
| 事实与边界 | 清点插件、设置、Skills、包依赖与项目配置 | 删除边界和外部消费者明确 | ✅ 完成 | 独立复核确认范围外无 GUID、语义、包或项目客户端配置引用。 |
| 实施 | 删除确认路径及 `.meta` | 工作树只出现目标删除 | ✅ 完成 | 已删除 UnityMCP 2,466 个文件、UnityCaptain 设置和 3 个项目专用 Skills；同时移除 `.gitignore` 的专用规则。 |
| 定向验证 | 扫描路径、GUID 和文本残留 | 无项目内 UnityMCP 残留引用 | ✅ 完成 | 删除后扫描仅允许命中本任务记录，不存在项目实现/配置残留。 |
| 回归与一致性 | Unity 刷新、编译和 Console 回读 | 无本轮新增编译错误 | ✅ 完成 | RetroGame 已在 Unity 2022.3.62f3 中完成 ScriptCompilation：`Tundra build success (11.56 seconds)`，并成功 Domain Reload；当前 Console 无 `error CS*` 或 `Scripts have compiler errors`。历史 C# warnings 仍存在；HybridCLR `MonoHook` 初始化异常需独立处理，不归因于 UnityMCP 删除。 |
| 同步与收尾 | 复核 diff、记录实际状态 | 不混入用户改动；真实报告 | ✅ 完成 | 已暂存 2,474 个删除和 `.gitignore` 的 1 个定向修改；未提交、未推送。 |

## 风险与回退

| 风险 | 发现方式 | 处理/回退 |
| --- | --- | --- |
| 游戏代码依赖 UnityMCP 程序集 | 删除后 Unity 编译错误 | 恢复删除批次，定位实际 consumer；不添加兼容桩。 |
| 当前会话/外部客户端失去 Unity MCP 连接 | 删除后端点或工具不可用 | 属于预期结果；不以此阻断游戏项目清理。 |
| 项目专用 Skills 被误当作全局技能保留 | 路径与设置 owner 复核 | 仅删除 `Assets/UnityCaptainSkills~`，不触碰 `~/.codex/skills`。 |
