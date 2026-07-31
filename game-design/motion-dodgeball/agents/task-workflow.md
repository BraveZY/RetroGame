# 任务流程

当前样例使用本地 Markdown 任务。

任务目录：

```text
game-design/motion-dodgeball/tasks/<feature-slug>/
```

多小游戏规则：每个小游戏使用自己的 `game-design/<game-slug>/agents/` 和 `game-design/<game-slug>/tasks/`。新增小游戏时先创建对应游戏名目录，不把 Agent 工作约定或任务混放在 `game-design/agents/`、`game-design/tasks/`。

每个功能拆分应包含一个总任务和多个子任务：

```text
game-design/motion-dodgeball/tasks/<feature-slug>/
  00-epic.md
  01-<slice>.md
  02-<slice>.md
```

`00-epic.md` 只管理目标、范围、总体验收和子任务索引。每个子任务必须引用父级总任务。

每个任务必须包含：

- 父级总任务引用
- 玩家可见结果
- 需要实现的功能
- 涉及系统
- 涉及资源
- 验收标准
- 验收证据
- 建议验证方式
- 依赖关系
- 风险

功能拆分应优先采用垂直切片，不按“先写所有代码、再做所有资源”横向拆分。

## 验收同步

- `- [ ]` 表示未验证或未完成。
- `- [x]` 表示已按任务写明的方式验证通过。
- 每个已勾选项必须能追溯到 UnityCaptain 证据、编译/测试结果、截图/录屏、设备记录或人工流程。
- 实现任务时，Agent 应在验证后自动同步复选框；没有证据的验收项保持未勾选。
- 父级 `00-epic.md` 只在子任务证据充分时更新状态或总体验收项。

## 数值与调参记录

涉及速度、间隔、生命、命中区域、难度曲线、奖励或经济参数时，必须在 `game-design/motion-dodgeball/balance/<feature-slug>/` 创建或更新调参记录。

调参记录至少包含：

- 当前参数值和来源。
- 建议范围和调整理由。
- 风险和验证方式。
- 哪些结论已经验证，哪些仍是待验证假设。

未经过 UnityCaptain、录屏、PlayMode、设备或人工流程验证的参数，不得写成最终值，也不得用来勾选任务验收项。

## 系统框架记录

涉及玩法规则、输入、UI、场景、Prefab、资源、存档、设备或测试 seam 的任务，在进入实现前应在 `game-design/motion-dodgeball/system-design/<feature-slug>/` 创建或更新系统框架设计文档。

系统框架记录至少包含：

- 模块边界和职责。
- 普通 C#、MonoBehaviour adapter、Scene、Prefab、ScriptableObject 或配置表的分工。
- 状态归属、调用入口和 Unity 接线。
- 验证方式和进入实现的边界。

没有系统框架记录时，不应直接把复杂任务压进 `game-unity-implementation`。
