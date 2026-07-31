# 任务流程

当前样例使用本地 Markdown 任务。

任务目录：

```text
game-design/chalk-dodge/tasks/<feature-slug>/
```

多小游戏规则：每个小游戏使用自己的 `game-design/<game-slug>/agents/` 和 `game-design/<game-slug>/tasks/`。新增小游戏时先创建对应游戏名目录，不把 Agent 工作约定或任务混放在 `game-design/agents/`、`game-design/tasks/`。

每个功能拆分应包含一个总任务和多个子任务：

```text
game-design/chalk-dodge/tasks/<feature-slug>/
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

涉及粉笔头速度、生成间隔、危险轨道、躲避判定、橡皮奖励、连击倍率或输入延迟时，必须在 `game-design/chalk-dodge/balance/<feature-slug>/` 创建或更新调参记录。

## 系统框架记录

涉及玩法规则、体感输入、危险物生成、碰撞判定、UI、场景、Prefab、资源、设备或测试 seam 的任务，在进入实现前应在 `game-design/chalk-dodge/system-design/<feature-slug>/` 创建或更新系统框架设计文档。
