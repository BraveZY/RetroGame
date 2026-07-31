# 游戏项目约定

## 引擎

Unity 2022.3.62f3。

## 项目类型

同一 Unity 体感大屏产品下的独立小游戏：体感擦黑板。

## 当前目标

验证 1-2 人 60 秒体感擦黑板闭环：

```text
玩家站入识别区 -> 老师提示目标 -> 玩家挥手控制板擦 -> 擦对指定内容或擦错干扰项 -> 统计正确率/反应速度/连击 -> 结算
```

当前设计来源：

```text
game-design/blackboard-eraser/briefs/2026-07-03-blackboard-eraser-mode-brief.md
game-design/blackboard-eraser/gdd/2026-07-03-blackboard-eraser-mode-gdd.md
```

## Agent 读取顺序

1. `README.md`
2. `game-design/blackboard-eraser/agents/*.md`
3. `game-design/blackboard-eraser/GAME_CONTEXT.md`
4. `game-design/blackboard-eraser/gdd/*.md`
5. `game-design/blackboard-eraser/tasks/**/00-epic.md`
6. `game-design/blackboard-eraser/tasks/**/*.md`
7. `Assets/Scripts/Runtime/*.cs`
8. `Assets/Scripts/Tests/EditMode/*.cs`

## 设计文档目录

当前小游戏 Agent 工作约定：

```text
game-design/blackboard-eraser/agents/
```

当前小游戏目录：

```text
game-design/blackboard-eraser/
```

同一 Unity 工程新增小游戏时，在 `game-design/` 下创建新的游戏名目录，例如 `game-design/<game-slug>/`，并在该目录下创建自己的 `agents/`、简报、GDD、任务和领域文档，不把多个小游戏的 Agent 工作约定或设计文档混放在同一层。

项目简报：

```text
game-design/blackboard-eraser/briefs/
```

GDD 和功能设计：

```text
game-design/blackboard-eraser/gdd/
```

可验证功能任务：

```text
game-design/blackboard-eraser/tasks/<feature-slug>/
```
