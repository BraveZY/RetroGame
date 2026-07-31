# 领域文档

领域语言来源：

```text
game-design/motion-dodgeball/GAME_CONTEXT.md
```

ADR 目录：

```text
game-design/motion-dodgeball/adr/
```

规则：

- `game-design/motion-dodgeball/GAME_CONTEXT.md` 只记录体感丢沙包的游戏语言和概念。
- 新增小游戏时创建自己的 `game-design/<game-slug>/GAME_CONTEXT.md`，不要混用另一个小游戏的术语表。
- 代码实现细节写入任务或技术说明，不写入 `GAME_CONTEXT.md`。
- 难以回退且有真实取舍的设计决策才创建 ADR。

当前核心术语：

- 躲避者 C：玩家控制的中间角色。
- 投手 A/B：位于场地上、下两侧的 AI 角色，可持有、投掷和接住沙包。
- 持球投手：当前持有沙包并准备向躲避者 C 投掷的 A 或 B。
- 接球投手：沙包未击中 C 后主动接球的对侧投手。
- 沙包：A/B 之间传接并投向 C 的主要危险物，MVP 优先按单颗沙包循环处理。
- 躲避区：躲避者 C 可移动的中间区域。
- 模拟体感输入：真实摄像头方案确认前，用于验证玩法闭环的开发期输入源。
