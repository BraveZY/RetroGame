# AI 投手传接与沙包轨迹调参记录

## 关联任务

| 项目 | 内容 |
|---|---|
| 父级任务 | `game-design/motion-dodgeball/tasks/motion-dodgeball-mvp/00-epic.md` |
| 子任务 | `game-design/motion-dodgeball/tasks/motion-dodgeball-mvp/03-ai-throwers-and-sandbag-trajectory.md` |
| 状态 | 当前值已从代码和场景提取；玩法体验仍待 UnityCaptain、录屏或人工流程验证。 |

## 调优目标

| 目标 | 本轮含义 |
|---|---|
| 公平性 | 玩家在预警后能看懂投掷方向，并有反应窗口。 |
| 可读性 | 沙包从持球投手到中间躲避区、再到对侧接球投手的轨迹在 16:9 大屏视图下清楚可见。 |
| 节奏 | A/B 围绕同一颗沙包交替持球、投掷和接球，60 秒内节奏逐步增加但不连续压制。 |
| 新手容错 | 开局阶段不应让玩家因看不懂轨迹或投手不动而快速失去生命。 |

## 当前参数表

| 参数 | 当前值 | 来源 | 建议范围 | 调整理由 | 风险 | 验证方式 |
|---|---:|---|---:|---|---|---|
| `playerSpeed` | `3.4` | `SinglePlayerReadyFlowController.cs` / `SkillLab.unity` | `3.0-4.2` | 影响躲避者能否在预警窗口内移动到安全区域。 | 过低会显得不公平，过高会削弱投掷压力。 | PlayMode 或录屏观察玩家横向/纵向移动覆盖。 |
| `dodgeZoneHalfExtents` | `2.05, 1.05` | `SinglePlayerReadyFlowController.cs` | 待测 | 决定中间躲避区范围和玩家可移动空间。 | 区域过小会放大误判，过大会让沙包压力不足。 | 截图检查边界可读性，录屏确认玩家不会越界。 |
| `throwIntervalSeconds` | `1.8s` | `SinglePlayerReadyFlowController.cs` / `SkillLab.unity` | `1.8-3.0s` | 控制投掷密度，当前偏向较快节奏。 | 开局如果过快，玩家会觉得没有准备时间。 | 连续 3 局统计每局投掷次数和空窗时间。 |
| `warningSeconds` | `0.45s` | `SinglePlayerReadyFlowController.cs` / `SkillLab.unity` | `0.35-0.8s` | 给玩家识别投掷方向和移动的窗口。 | 过短会不公平，过长会没有紧张感。 | 录屏测量预警出现到沙包出手的时间。 |
| `sandbagSpeed` | `4.4` | `SinglePlayerReadyFlowController.cs` / `SkillLab.unity` | `3.8-6.0` | 控制沙包穿越中间区域的速度。 | 过快会让轨迹不可读，过慢会缺少压力。 | 录屏测量沙包穿越躲避区耗时。 |
| `throwerWindupOffset` | `0.26` | `SinglePlayerReadyFlowController.cs` | `0.2-0.45` | 投掷前投手前摇位移，帮助玩家读懂出手。 | 过小看不出动作，过大可能遮挡 lane。 | UnityCaptain 截图或录屏确认前摇可见。 |
| `throwerAimSpeed` | `3.2` | `SinglePlayerReadyFlowController.cs` | `2.0-5.0` | 持球投手朝 C 当前 X 位置瞄准；接球投手朝接球点移动。 | 过低像没动，过高像硬锁玩家或接球瞬移。 | 录屏观察持球瞄准和对侧接球是否自然。 |
| `throwerLaneHalfWidth` | `3.55` | `SinglePlayerReadyFlowController.cs` | 待测 | 限制投手在各自 lane 内的横向移动范围。 | 过窄瞄准不明显，过宽可能跑出视觉 lane。 | 截图检查投手是否始终留在 lane 内。 |
| `catchDistance` | `0.32` | `SinglePlayerReadyFlowController.cs` | `0.25-0.45` | 判定接球投手和同一颗沙包都已到达接球点。 | 过大可能提前接住，过小可能让可达的接球超时重置。 | 录屏确认接球动作与持球权切换一致。 |
| `catchBoundaryY` | `2.95` | `SinglePlayerReadyFlowController.cs` | `2.8-3.1` | 沙包越过中区后进入对侧接球状态。 | 边界太近可能提前计躲避，太远会拖慢传接节奏。 | 录屏确认沙包越过中区后才开始接球。 |
| `flightTimeoutSeconds` | `3.0s` | `SinglePlayerReadyFlowController.cs` | `2.6-4.0s` | 限制异常飞行的最长时间，防止沙包永久停留在飞行态。 | 过短会打断正常远距离投掷，过长会延后异常恢复。 | 人工将沙包引导到非正常路径，确认 3 秒内回到持球态。 |
| `catchTimeoutSeconds` | `2.6s` | `SinglePlayerReadyFlowController.cs` | `2.3-3.2s` | 限制接球投手无法到达接球点时的等待时间。 | 过短会让远端接球频繁重置，过长会出现明显卡顿。 | 人工阻断接球投手或改变接球位置，确认超时后重置。 |
| 命中距离 | `0.48` | `SinglePlayerReadyFlowController.cs` | `0.35-0.55` | 沙包与躲避者距离小于该值时命中。 | 过大会产生擦边误判，过小会让命中不稳定。 | PlayMode/录屏对照沙包和躲避者重叠关系。 |
| 首次出手延迟 | `0.45s` | `StartRound` 后设置 `throwTimer = warningSeconds` | `0.8-1.5s` | 当前按下开始后几乎立刻预警并出手。 | 新手可能没有足够时间理解场面。 | 人工流程观察从按 Space 到第一颗沙包的可接受性。 |
| 难度曲线 | `max(0.8, 1.8 - elapsed * 0.012)` | `SinglePlayerReadyFlowController.cs` | 待测 | 60 秒内逐步缩短投掷间隔。 | 后半局可能节奏过密，尤其与 3 条生命叠加时。 | 连续 3 局记录失败时间分布和投掷次数。 |
| 回合时长 | `60s` | `SinglePlayerReadyFlow.cs` | 已由任务固定 | MVP 任务要求 60 秒闭环。 | 暂不作为本轮调参变量。 | 验证倒计时和结算流程。 |
| 起始生命 | `3` | `SinglePlayerReadyFlow.cs` | `3-5` | 影响失败容错和完整局体验。 | 生命太少会让调投掷参数时误判难度。 | 统计 60 秒内平均命中次数和失败时间。 |
| 评级规则 | `A: dodge>=12 && lives>=2; B: dodge>=6 && lives>=1; else C` | `SinglePlayerReadyFlow.cs` | 待测 | 当前是占位评级，依赖投掷密度和命中率。 | 投掷次数未稳定前，评级不可作为最终平衡。 | 先验证 03/04/05 后再调评级阈值。 |

## 验证指标

| 指标 | 记录方式 | 当前状态 |
|---|---|---|
| 60 秒内投掷次数 | 录屏或日志统计每局同一颗沙包的投掷次数。 | 待验证 |
| A/B 持球切换 | 统计 A/B 各自出手次数和接球次数。 | 待验证 |
| 玩家反应窗口 | 预警出现到沙包进入威胁区域的时间。 | 待验证 |
| 沙包穿越中区耗时 | 沙包进入/离开躲避区的时间差。 | 待验证 |
| 投手响应 | 持球投手是否向 C 当前方向瞄准，接球投手是否主动移动到接球点。 | 待验证 |
| 误判/争议次数 | 记录擦边但判命中、重叠却未命中的次数。 | 待验证 |
| 失败时间分布 | 记录生命耗尽发生在 0-20s、20-40s、40-60s 的比例。 | 待验证 |

## 最小调参方案

第一轮先不直接改代码或任务状态，建议按以下顺序取证：

1. 用 UnityCaptain 或录屏连续跑 3 局，记录投掷次数、首次出手时间、投手是否横向移动、沙包穿越耗时。
2. 如果投手移动不明显，优先检查 `throwerAimSpeed` 和投手 lane 位置，不先改沙包速度。
3. 如果玩家反馈“不公平”，优先比较 `warningSeconds`、首次出手延迟和命中距离，而不是只降 `sandbagSpeed`。
4. 如果节奏过密，优先放大 `throwIntervalSeconds` 或降低难度曲线斜率，再观察 3 局。

## 需要运行/设备验证

- UnityCaptain 场景状态截图：确认投手 lane、躲避区、预警线和沙包可见。
- PlayMode 或录屏：确认投手会向 C 当前方向移动/瞄准。
- 连续 3 局人工流程：确认投掷节奏稳定，不会卡住或连续压制。

## 仍然待定

- 当前参数还不是最终平衡值。
- 没有 UnityCaptain/录屏/PlayMode 证据前，不勾选 03 子任务验收项。
- 命中距离、生命、评级规则横跨 04/05 子任务，不能只用 03 的观察结论定稿。
