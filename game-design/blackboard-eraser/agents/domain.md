# 领域文档

领域语言来源：

```text
game-design/blackboard-eraser/GAME_CONTEXT.md
```

ADR 目录：

```text
game-design/blackboard-eraser/adr/
```

规则：

- `game-design/blackboard-eraser/GAME_CONTEXT.md` 只记录体感擦黑板的游戏语言和概念。
- 新增小游戏时创建自己的 `game-design/<game-slug>/GAME_CONTEXT.md` 和 `game-design/<game-slug>/agents/`，不要混用另一个小游戏的术语表或 Agent 工作约定。
- 代码实现细节写入任务或技术说明，不写入 `GAME_CONTEXT.md`。
- 难以回退且有真实取舍的设计决策才创建 ADR。

当前核心术语：

- 值日生玩家：现实中站在大屏前挥手控制板擦的玩家。
- 目标内容：本轮老师口令、字幕和高亮明确要求擦掉的内容。
- 干扰项：同一块黑板上不应被擦除的非目标内容。
- 左板擦/右板擦：双人协作时两名玩家分别控制的板擦。
- 协作擦除任务：双人按左右分工或共同擦除同一大目标的任务。
- 协作连击：双人连续擦对且没有擦错时累积的全队连击。
