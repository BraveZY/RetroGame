# MotionX Retro Pack 工程约定

## 工程事实

- 项目类型：Unity 复古亲子体感游戏主题包；以多个小游戏接入统一主题体验为产品方向。
- Unity：`2022.3.62f3`（revision `96770f904ca7`）。
- 渲染：URP `14.0.12`。
- 当前 Unity 内容基线：启用场景为 `Assets/Scenes/SampleScene.unity`；当前仅发现教程脚本 `Assets/TutorialInfo/Scripts/`，未发现业务运行时代码目录或业务 `.asmdef`。
- 当前产品目标：面向国内家庭场景的 `MotionX Retro Pack / 复古体感包`，首批样板为操场木头人、电玩城投篮机、家庭电视健美操；每局目标为 1–3 分钟。

## 推荐阅读顺序

1. 本文件与 `domain.md`，了解工程边界和产品语言。
2. `Retro_Games_plans/docs/项目管理计划/MotionX-Retro-Pack-产品定义.md`，确认当前产品范围。
3. 对应小游戏的 GDD（位于 `Retro_Games_plans/docs/设计方案/游戏集合策划案/`）和相关交互/音频文档。
4. `references/task-workflow.md`，确认任务状态和验收证据。
5. `references/content-pipeline.md`、`references/build-targets.md`，再进行资源或工程配置改动。

## 关键目录

| 目的 | 当前路径 | 说明 |
| --- | --- | --- |
| Unity 内容 | `Assets/` | 当前含场景、URP Settings 与 TutorialInfo。 |
| Unity 包 | `Packages/manifest.json` | 包依赖的事实来源。 |
| Unity 项目设置 | `ProjectSettings/` | Player/Build/Graphics 设置；属于协作热点，修改前后需检查 diff。 |
| 产品与设计资料 | `Retro_Games_plans/docs/` | 当前产品定义、GDD、交互原型、音频、项目管理资料的主路径。 |
| 现有工程任务 | `agent/task/` | 当前发现的本地 Markdown 工程任务。 |
| 本产品 Agent 约定 | `game-design/motionx-retro-pack/agents/` | 本目录；不替代或迁移既有资料。 |

## 文档写入原则

- 既有产品文档、小游戏 GDD 和任务继续留在当前路径；不得为初始化而移动或扁平化它们。
- 后续若为 `MotionX Retro Pack` 新建 brief、GDD、任务、ADR、平衡或系统设计文档，先与用户确认是否采用 `game-design/motionx-retro-pack/` 的对应子目录；当前没有足够证据将其声明为已启用的团队路径。
- 单个可独立交付的小游戏应有自己的明确标识，并在任务中链接所属产品资料和小游戏 GDD；不要把不同小游戏的规则、资产和验收混为一份任务。

## 改动边界

- Scene、Prefab、`ProjectSettings/` 和 `Packages/` 都是高影响文件；写入前读当前状态，写入后回读并检查 Git diff。
- 本初始化文档不授权修改场景、Prefab、资源、C# 或平台配置。
