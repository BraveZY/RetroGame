# 游戏项目约定

## 引擎

Unity 2022.3.62f3。

## 项目类型

同一 Unity 体感大屏产品下的独立小游戏：躲粉笔头。

## 当前目标

验证单人 60 秒体感反应躲避闭环：

```text
玩家站入识别区 -> 老师背对黑板 -> 同学恶作剧丢来粉笔头/橡皮 -> 玩家左右闪或下蹲躲粉笔头、接橡皮 -> 老师回头时站定装认真 -> 统计躲避/接住/乱动/连击 -> 结算
```

当前设计来源：

```text
game-design/chalk-dodge/briefs/2026-07-03-chalk-dodge-brief.md
```

## Agent 读取顺序

1. `README.md`
2. `game-design/chalk-dodge/agents/*.md`
3. `game-design/chalk-dodge/GAME_CONTEXT.md`
4. `game-design/chalk-dodge/gdd/*.md`
5. `game-design/chalk-dodge/tasks/**/00-epic.md`
6. `game-design/chalk-dodge/tasks/**/*.md`
7. `Assets/Scripts/Runtime/*.cs`
8. `Assets/Scripts/Tests/EditMode/*.cs`

## 设计文档目录

当前小游戏 Agent 工作约定：

```text
game-design/chalk-dodge/agents/
```

当前小游戏目录：

```text
game-design/chalk-dodge/
```

同一 Unity 工程新增小游戏时，在 `game-design/` 下创建新的游戏名目录，例如 `game-design/<game-slug>/`，并在该目录下创建自己的 `agents/`、简报、GDD、任务和领域文档，不把多个小游戏的 Agent 工作约定或设计文档混放在同一层。

项目简报：

```text
game-design/chalk-dodge/briefs/
```

GDD 和功能设计：

```text
game-design/chalk-dodge/gdd/
```

可验证功能任务：

```text
game-design/chalk-dodge/tasks/<feature-slug>/
```
