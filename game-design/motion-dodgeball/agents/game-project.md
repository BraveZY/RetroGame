# 游戏项目约定

## 引擎

Unity 2022.3.62f3。

## 项目类型

用于打磨技能库的体感丢沙包样例工程。

## 当前目标

验证单人 60 秒体感躲避闭环：

```text
玩家站入识别区 -> 持球投手向 C 投沙包 -> C 躲避或被命中 -> 未命中时对侧投手接沙包 -> 统计生命/躲避/命中 -> 结算
```

当前设计来源：

```text
game-design/motion-dodgeball/briefs/2026-07-03-motion-dodgeball-brief.md
game-design/motion-dodgeball/gdd/2026-07-03-motion-dodgeball-gdd.md
game-design/motion-dodgeball/tasks/motion-dodgeball-mvp/00-epic.md
game-design/motion-dodgeball/tasks/motion-dodgeball-mvp/*.md
```

## Agent 读取顺序

1. `README.md`
2. `game-design/motion-dodgeball/agents/*.md`
3. `game-design/motion-dodgeball/GAME_CONTEXT.md`
4. `game-design/motion-dodgeball/gdd/*.md`
5. `game-design/motion-dodgeball/tasks/**/00-epic.md`
6. `game-design/motion-dodgeball/tasks/**/*.md`
7. `Assets/Scripts/Runtime/*.cs`
8. `Assets/Scripts/Tests/EditMode/*.cs`

## 设计文档目录

当前小游戏 Agent 工作约定：

```text
game-design/motion-dodgeball/agents/
```

当前小游戏目录：

```text
game-design/motion-dodgeball/
```

同一 Unity 工程新增小游戏时，在 `game-design/` 下创建新的游戏名目录，例如 `game-design/<game-slug>/`，并在该目录下创建自己的 `agents/`、简报、GDD、任务和领域文档，不把多个小游戏的 Agent 工作约定或设计文档混放在同一层。

项目简报：

```text
game-design/motion-dodgeball/briefs/
```

GDD 和功能设计：

```text
game-design/motion-dodgeball/gdd/
```

可验证功能任务：

```text
game-design/motion-dodgeball/tasks/<feature-slug>/
```
