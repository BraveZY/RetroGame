# 领域文档

领域语言来源：

```text
game-design/chalk-dodge/GAME_CONTEXT.md
```

ADR 目录：

```text
game-design/chalk-dodge/adr/
```

规则：

- `game-design/chalk-dodge/GAME_CONTEXT.md` 只记录躲粉笔头的游戏语言和概念。
- 新增小游戏时创建自己的 `game-design/<game-slug>/GAME_CONTEXT.md` 和 `game-design/<game-slug>/agents/`，不要混用另一个小游戏的术语表或 Agent 工作约定。
- 代码实现细节写入任务或技术说明，不写入 `GAME_CONTEXT.md`。
- 难以回退且有真实取舍的设计决策才创建 ADR。

当前核心术语：

- 走神学生：玩家扮演的课堂角色。
- 粉笔头：必须躲开的主要危险物。
- 橡皮：可接住的奖励物。
- 左右闪：横向躲避动作。
- 下蹲：纵向躲避动作。
- 反应连击：连续正确反应累积的连击。
