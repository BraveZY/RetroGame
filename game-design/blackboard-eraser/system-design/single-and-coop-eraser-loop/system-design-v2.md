# 体感擦黑板 1-2 人 60 秒玩法闭环系统设计 v2

## 设计范围

本设计覆盖 `blackboard-eraser / single-and-coop-eraser-loop`。实现顺序先跑通单人 60 秒闭环，再扩展双人协作，最后做安卓大屏和摄像头验证。

玩家第一眼应该看到的是一块可互动的怀旧绿色黑板：老师口令指出目标，目标框高亮，板擦跟随玩家挥手擦过目标，目标内容逐渐消失并冒出粉笔灰，HUD 显示剩余时间、分数和连击。

效果图参考：

```text
unity-skill-lab/game-design/blackboard-eraser/img/ig_0404550b4f3cd27b016a4b193b22648191bc59d9610a234daf.png
```

## 任务依据

- `game-design/blackboard-eraser/GAME_CONTEXT.md`
- `game-design/blackboard-eraser/gdd/2026-07-03-blackboard-eraser-mode-gdd.md`
- `game-design/blackboard-eraser/tasks/single-and-coop-eraser-loop/00-epic.md`
- `game-design/blackboard-eraser/tasks/single-and-coop-eraser-loop/01-single-player-ready-and-eraser-movement.md`
- `game-design/blackboard-eraser/tasks/single-and-coop-eraser-loop/02-single-player-target-and-erase-judgement.md`
- `game-design/blackboard-eraser/tasks/single-and-coop-eraser-loop/03-single-player-timer-score-and-result.md`
- `game-design/blackboard-eraser/tasks/single-and-coop-eraser-loop/04-two-player-calibration-and-dual-erasers.md`
- `game-design/blackboard-eraser/tasks/single-and-coop-eraser-loop/05-two-player-coop-erase-tasks.md`
- `game-design/blackboard-eraser/tasks/single-and-coop-eraser-loop/06-two-player-coop-scoring-and-result.md`
- `game-design/blackboard-eraser/tasks/single-and-coop-eraser-loop/07-android-camera-and-large-screen-validation.md`
- `game-unity-system-design/references/UNITY-ENGINEERING-STANDARD.md`
- `game-unity-system-design/references/VERIFICATION-STANDARD.md`

## 系统框架文档

```text
game-design/blackboard-eraser/system-design/single-and-coop-eraser-loop/system-design-v2.md
```

## 复杂度判断

- 模式：轻量模式 + 验证先行模式。
- 理由：主玩法是单场景 60 秒小游戏，核心闭环是“看提示 -> 移动板擦 -> 擦目标 -> 反馈 -> 结算”。真实摄像头、安卓大屏、双人身份识别属于外部依赖不确定项，单独验证，不阻塞 01-06 的模拟输入闭环。
- 本次最多建议模块数：4 个。

## MVP 压缩实现形态

| 实现模块 | 包含职责 | 暂不继续拆的原因 |
|---|---|---|
| `BlackboardEraserGame` | 准备、倒计时、Playing、Result、60 秒计时、当前任务、分数、连击、结算。 | 规则量不大，状态、计分、结算先合并，避免过早拆 `ScoreCalculator`、`ComboRule` 等小类。 |
| `EraserInputMapping` | 单人模拟输入、双人左右输入、输入丢失、坐标映射、黑板边界限制。 | 摄像头以后只替换输入来源，映射规则可保持。 |
| `EraseTaskJudge` | 三类任务、目标/干扰项、目标高亮、擦除进度、擦错、共同擦除、贡献。 | 任务和判定强绑定，首版合并更利于跑通玩家闭环。 |
| `BlackboardEraserController` | MonoBehaviour 场景入口、板擦 Transform、黑板内容显示、HUD、粉笔灰、音效、重开。 | Unity 接线集中，避免一开始拆太多 View / Manager。 |

## 系统框架树

```text
BlackboardEraser
├── BlackboardEraserGame
│   ├── Phase: Ready / Countdown / Playing / Result
│   ├── Mode: Single / Coop
│   ├── Timer: countdown / round / task
│   └── Stats: score / combo / mistakes / reaction / rating
├── EraserInputMapping
│   ├── Single simulated input
│   ├── Left / Right simulated input
│   ├── Input lost state
│   └── Board bounds mapping
├── EraseTaskJudge
│   ├── Duty roster / timetable target
│   ├── Red-circled wrong answer target
│   ├── Doodle target
│   ├── Distractor regions
│   └── Erase progress / mistake / contribution
└── BlackboardEraserController
    ├── BlackboardRoot / ContentRoot / HighlightRoot
    ├── EraserRoot
    ├── HudRoot
    └── FeedbackRoot
```

## 模块边界

| 模块 | 职责 | 对外入口 | 拥有状态 | Unity 接线 |
|---|---|---|---|---|
| `BlackboardEraserGame` | 管整局流程、计时、分数、结算。 | `StartSingle()`、`StartCoop()`、`Tick(delta)`、`ApplyTaskResult()`、`Restart()` | 阶段、模式、时间、分数、连击、结算。 | Controller 每帧调用，HUD 读取快照。 |
| `EraserInputMapping` | 把输入转成板擦坐标。 | `ReadInputs()`、`MapToBoard()` | 输入有效性、玩家坐标、板擦坐标。 | Controller 移动板擦对象。 |
| `EraseTaskJudge` | 生成当前目标并判断擦除。 | `CreateNextTask()`、`ApplyErase()`、`CheckTaskTimeout()` | 当前任务、目标进度、擦错、贡献。 | Controller 根据板擦和目标区域传入接触样本。 |
| `BlackboardEraserController` | 连接规则和 Unity 表现。 | `Update()`、UI/按键事件。 | 场景引用、临时反馈动画。 | 挂在 `BlackboardEraserRoot`。 |

## 普通 C# / MonoBehaviour 分工

| 层 | 放什么 | 不放什么 |
|---|---|---|
| 规则层 | `BlackboardEraserGame`、`EraserInputMapping`、`EraseTaskJudge` 的状态推进、判定、计分、坐标映射。 | 不直接操作 Transform、SpriteRenderer、TextMesh、AudioSource。 |
| Unity Adapter | `BlackboardEraserController` 读取输入、持有场景引用、把规则快照同步到表现。 | 不吞掉全部玩法规则，不把计分和擦除判定写死在 `Update()`。 |
| 数据配置 | 首版用 Inspector 字段和内置数组；跑通后再抽 `BlackboardGameConfig`、`BlackboardTaskSet`。 | 不用 ScriptableObject 承载运行态分数、进度、当前任务。 |
| 表现层 | 黑板、内容贴片、目标框、板擦、HUD、粉笔灰、擦对/擦错反馈。 | 不拥有玩法真相，只读规则快照或反馈事件。 |
| 平台层 | 07 的摄像头 adapter、安卓权限、输入延迟、设备性能和可读性记录。 | 不作为 01-06 的前置条件。 |

## 关键 seam / adapter

| 边界 | 目的 | 替换/验证方式 |
|---|---|---|
| 输入 seam | 不让真实摄像头阻塞 01-06。 | 先用鼠标/键盘/调试输入；07 替换摄像头 adapter。 |
| 时间 seam | 让倒计时、60 秒、单题超时可测。 | 普通 C# `Tick(delta)`。 |
| 擦除判定 seam | 不依赖正式遮罩资源。 | 首版用矩形/圆形区域覆盖；正式资源后再换 Mask。 |
| 任务数据 seam | 保证三类目标可控出现。 | 首版内置数组或 Inspector 列表；后续再 ScriptableObject。 |
| UI seam | UI 不拥有玩法真相。 | 只读 `BlackboardEraserGame` 快照。 |

## ScriptableObject / Prefab / Scene 计划

首版不强制 ScriptableObject。可以先用 `BlackboardEraserController` Inspector 配置原型数据。等三类任务跑通后，再抽：

```text
BlackboardGameConfig
BlackboardTaskSet
```

建议 Scene：

```text
BlackboardEraserRoot
├── BlackboardEraserController
├── BlackboardRoot
│   ├── Background
│   ├── ContentRoot
│   └── HighlightRoot
├── EraserRoot
│   ├── Eraser_LeftOrSingle
│   └── Eraser_Right
├── HudRoot
└── FeedbackRoot
```

首版视觉元素：

- 绿色黑板背景。
- 值日表/课程表。
- 红圈错题错字。
- 课间涂鸦。
- 黄色或红色目标高亮框。
- 板擦擦除轨迹。
- 粉笔灰粒子或占位 Sprite。
- HUD：时间、分数、连击、老师口令。

## 测试与验证计划

系统设计阶段不替实现阶段完成验证，只标清后续入口。

| 类型 | 验证入口 |
|---|---|
| 规则层 | EditMode 测 `BlackboardEraserGame` 阶段、计时、结算、重开。 |
| 输入映射 | EditMode 测坐标映射、边界 clamp、输入丢失。 |
| 擦除判定 | EditMode 测目标进度、擦错、共同擦除、贡献。 |
| MonoBehaviour 接线 | PlayMode 或人工流程验证板擦移动、HUD 刷新。 |
| Scene / Prefab / UI | 截图验证目标高亮、老师口令、粉笔灰、结算可读。 |
| 设备输入 / 性能 / 构建 | 07 单独设备记录、Profiler 或构建证据，不阻塞主闭环。 |

## 任务覆盖矩阵

| 子任务 | 覆盖模块 | 实现顺序 | 特殊风险 |
|---|---|---|---|
| 01 单人准备、倒计时、板擦移动 | `BlackboardEraserGame`、`EraserInputMapping`、`BlackboardEraserController` | 1 | 必须先用模拟输入。 |
| 02 单人目标提示和擦除判定 | `EraseTaskJudge`、`BlackboardEraserController` | 2 | 阈值先用原型值。 |
| 03 单人计分、连击、结算 | `BlackboardEraserGame`、`EraseTaskJudge` | 3 | 重开要清理状态。 |
| 04 双人校准和左右板擦 | `EraserInputMapping`、`BlackboardEraserController` | 4 | 只验证双模拟输入。 |
| 05 双人分工和共同擦除 | `EraseTaskJudge`、`BlackboardEraserGame` | 5 | 分工/共同任务共用判定模型。 |
| 06 双人贡献和结算 | `BlackboardEraserGame`、`EraseTaskJudge` | 6 | 贡献算法先简单可见。 |
| 07 安卓摄像头验证 | `EraserInputMapping` 替换 adapter | 7 | 单独验证，不阻塞主玩法。 |

## 进入 game-unity-implementation 的边界

第一轮只实现单人闭环：

```text
准备界面
-> 3 秒倒计时
-> 单人板擦移动
-> 目标高亮和老师口令
-> 擦除进度
-> 擦错反馈
-> 60 秒结算
-> 重开
```

第二轮扩双人：

```text
左右模拟输入
-> 左右板擦颜色/编号
-> 分工任务
-> 共同擦除
-> 双人贡献
-> 协作结算
```

第三轮验证设备：

```text
摄像头 adapter
-> 安卓大屏输入
-> 双人身份区分
-> 2-3 米可读性
-> 延迟/遮挡/光照记录
```

## 不建议现在做

- 不做完整摄像头 SDK 深接入。
- 不做正式资源管线。
- 不做复杂 ScriptableObject 内容系统。
- 不做事件总线、Service Locator、DI。
- 不拆出一堆 `TaskGenerator`、`Judge`、`ScoreCalculator`、`ContributionTracker` 小类。
- 不做联网、排行榜、长期成长。

这版系统设计的落点是：先让效果图里的画面真实跑起来，再谈架构细化。
