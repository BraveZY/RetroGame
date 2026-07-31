# 1-3 人完整单局系统设计

## 设计范围

| 项目 | 内容 |
|---|---|
| Epic | `game-design/motion-dodgeball/tasks/multiplayer-complete-session/00-epic.md` |
| 设计来源 | 当前 GDD、`GAME_CONTEXT.md`、多人功能增量简报与现有单人控制器。 |
| 本次要实现的玩家结果 | 1P-C 固定躲避，A/B 按 1-3 人模式由 AI 或玩家承担；先以共享键盘完成 100 秒内的投掷、躲避、接球、胜负、结算与重开。 |
| 不在本设计内 | 真实摄像头 SDK 选型/绑定和设备验收、联网同步、局内换位、C 反杀、复杂骨骼、资产终稿。 |

## 复杂度判断

- 模式：验证优先的标准设计。
- 原因：多人规则、输入 owner、场景运行态和 HUD 共享同一局状态；真实广角摄像头是独立的未验证外部依赖。
- 本轮最大实现模块数：5；不引入 Singleton、事件总线、Service Locator 或游戏包级 `GameManager`。

## 压缩后的实现形态

| 实现模块 | 包含职责 | 本轮不再拆分的原因 |
|---|---|---|
| `DodgeballSession` | 模式、阵容、Ready/Countdown/Playing/Result、100 秒、3 命、统计、阵营胜负、评级。 | 这些状态必须在同一纯规则对象内原子推进，方便 EditMode 测试。 |
| `PlayerMotionInput` | 1P/2P/3P 横向位置与出手请求；当前键盘 adapter、未来摄像头 adapter。 | 输入源是唯一真实替换 seam，按玩家拆类会制造不必要的 3 套状态。 |
| `DodgeballMatchController` | 场景生命周期、角色 Transform、持球 owner、AI/人类出手门控、单颗沙包状态机、接球与重置。 | 现有控制器的沙包循环可连续演进；拆为多个 MonoBehaviour 会让持球权重复 owner。 |
| `DodgeballHudPresenter` | 模式、分区/输入状态、时间、生命、统计、预警、阵营结果、重开提示。 | 当前 TextMesh 场景规模小，保持单一表现 adapter 即可。 |
| `DodgeballSessionConfig` | 回合、投掷、lane、挥臂去抖和评级的可调数值。 | 多人/真机调参需要独立于规则与场景序列化；不承载运行时状态。 |

## 系统框架树

```text
MotionDodgeball Scene
├── DodgeballMatchController (MonoBehaviour)
│   ├── DodgeballSession (plain C# runtime state)
│   ├── PlayerMotionInput (keyboard adapter now; camera adapter later)
│   └── one runtime sandbag + A/B/C transforms + warning visual
├── DodgeballHudPresenter (MonoBehaviour)
│   └── existing TextMesh / placeholder mode and input-state fields
└── DodgeballSessionConfig (ScriptableObject asset; read-only at runtime)

Tests
├── DodgeballSessionTests (EditMode)
├── PlayerMotionInput tests/doubles (EditMode)
└── scene flow / sandbag owner checks (PlayMode + recording)
```

## 模块边界

| 模块 | 职责 | 最小入口 | 状态 owner | Unity 接线 |
|---|---|---|---|---|
| `DodgeballSession` | 创建阵容；开始/推进/重置单局；记录命中和躲避；给出阵营胜负、评级和 ResultReason。 | `Start()`, `Tick(delta)`, `RecordHit()`, `RecordDodge()`, `Reset()`；构造时传入模式和 `DodgeballSessionConfig`。 | 模式、角色 owner、回合阶段、剩余时间、生命、统计、结果。 | 无 Unity 依赖；由 MatchController 驱动。 |
| `PlayerMotionInput` | 返回每个 PlayerSlot 的横向值与一次性 `throwRequested`；当前读取固定键盘映射。 | `Read(PlayerSlot)`；camera adapter 后续实现同一数据形状。 | 仅输入采样/去抖缓存，不拥有游戏规则或角色位置。 | MonoBehaviour/Inspector；当前对接键盘，后续对接摄像头。 |
| `DodgeballMatchController` | 根据 Session 和 Input 更新 C/A/B Transform；判断当前持球者 owner；人类挥臂或 AI 计时后释放同一颗沙包；推进飞行、命中、接球、重置。 | Unity `Start/Update`；读取 Session phase 和 Input snapshot。 | 沙包状态、持球方、预警计时、接球目标、运行时对象缓存。 | 绑定 C/A/B、沙包根节点、预警物体、Config 与 HUD Presenter。 |
| `DodgeballHudPresenter` | 将 Session、Input、Match 的只读快照转为可读 HUD；不反向修改规则。 | `Present(session, inputStates, matchState)`。 | 无业务状态；可缓存文本引用。 | 绑定现有 TextMesh；缺失的新字段先使用 Placeholder 文本。 |
| `DodgeballSessionConfig` | 保存 100 秒、3 秒、3 命、投掷间隔、速度、lane 边界、挥臂冷却、评级阈值。 | Inspector 只读配置。 | 无运行时可变状态。 | 一个场景引用；不做全局 Resources 加载。 |

## 关键规则与调用关系

```text
mode selection
  -> create Session(mode, config)
  -> keyboard adapter filters slots / supplies snapshots
  -> MatchController asks Session for owner of A/B
  -> human holder: active mode + lane aim + throwRequested -> release
     AI holder: warning timer -> release
  -> same sandbag flies -> Hit or Catch -> Session records result -> holder switches
  -> Session ends at 100 s or third hit -> Match stops field state -> HUD presents result
```

### 人数模式 owner 表

| 模式 | C | A | B |
|---|---|---|---|
| 单人 | 1P | AI | AI |
| 双人 | 1P | 2P | AI |
| 三人 | 1P | 2P | 3P |

### 键盘模式过滤

| 角色 | 当前键盘模式下的行为 | 禁止行为 |
|---|---|---|
| C | 只读取 1P 的 `A/D`，对局和沙包继续。 | 读取 2P/3P 横移键。 |
| 人类 A/B | 只在角色参与当前模式且持球时读取自己的瞄准/出手键。 | 未参与模式时出手、AI 接管。 |
| AI A/B | 按规则出手/接球。 | 读取人类按键。 |

## 关键 seam / adapter

| 边界 | 目的 | 替换与验证方式 |
|---|---|---|
| `PlayerMotionInput` snapshot | 隔离当前共享键盘与未选定摄像头 SDK。 | EditMode 用可控 snapshot；PlayMode 用固定键盘；后续真机用摄像头 adapter 与录屏。 |
| `DodgeballSessionConfig` | 将 100 秒、挥臂冷却、lane/命中/速度、评级参数从规则中移出。 | EditMode 传固定测试配置；Inspector 调参后用 PlayMode/设备记录。 |
| `DodgeballSession` | 让阵营胜负、计时、生命和统计不依赖 Scene。 | EditMode 测试模式 owner、100 秒结束、三击结束、重置和评级。 |
| HUD snapshot | 防止 TextMesh 文案成为规则 state。 | PlayMode/截图验证；替换正式 UI 时不改 Session。 |

## Scene、Prefab 与配置计划

| 类型 | 本轮计划 | 说明 |
|---|---|---|
| Scene | 继续使用 `Assets/Games/MotionDodgeball/Scenes/MotionDodgeball.unity`。 | 不新建按人数拆分的场景；模式只切换 owner 与 HUD。 |
| Runtime component | 将当前 `SinglePlayerReadyFlowController` 演进或替换为 `DodgeballMatchController`。 | 不能保留两个组件同时推进沙包或回合。 |
| Plain C# | 以 `DodgeballSession` 替代单人命名的规则 owner；可在迁移完成前保留旧类作兼容测试基线。 | 迁移后不应让两个规则对象同时拥有阶段/生命。 |
| Config asset | 新建一个 `DodgeballSessionConfig` ScriptableObject 供该场景引用。 | 仅保存可调参数，不保存 session 或玩家状态。 |
| HUD | 复用 TextMesh，增加模式/owner/输入状态/阵营结果文本占位。 | 正式美术与音效后续独立替换。 |
| Sandbag | 复用现有单颗运行时沙包策略。 | 禁止人类投手各自 Instantiate 沙包。 |

## 测试与验证计划

| 范围 | 入口 | 通过条件 |
|---|---|---|
| 规则层 | EditMode `DodgeballSessionTests` | 三种模式 owner 正确；100 秒、三击、结果、评级与 Reset 正确。 |
| 输入 seam | EditMode 测试 double | 固定按键的横向/出手请求在 C、人类投手与 AI owner 上的门控正确。 |
| 场景循环 | PlayMode/录屏 | 三种模式不生成多颗沙包；人类/AI 出手和接球循环正确。 |
| UI/HUD | Unity 场景截图/录屏 | 模式、键盘提示、时间、生命、预警、胜负和称号在 16:9 下可读。 |
| 设备 | 后续广角摄像头 + 安卓大屏录屏 | 横向分区、挥臂与“不暂停、不替换”规则真实成立；未验证前不得勾选 05。 |

## 任务覆盖矩阵

| 子任务 | 覆盖模块 | 实现顺序 | 特殊风险 |
|---|---|---|---|
| 01 模式/入口 | Session、Config、HUD | 1 | 60 秒旧默认值与单人入口兼容。 |
| 02 键盘/输入 | PlayerMotionInput、MatchController、HUD | 2 | 先完成共享键盘；摄像头 SDK 后续验证。 |
| 03 投手循环 | MatchController、Session、Input、Config | 3 | 单颗沙包与人类挥臂去抖。 |
| 04 结算/重开 | Session、MatchController、HUD | 4 | 不能让 Result 后场上状态继续推进。 |
| 05 设备验证 | PlayerMotionInput camera adapter、HUD | 5 | 需要实际摄像头、权限、光照与安卓大屏。 |

## 进入实现的边界

可以直接实施 01-04 的 Editor/共享键盘路径，前提是：

1. 同一时刻只有 `DodgeballSession` 拥有回合/生命/统计/结果，只有 `DodgeballMatchController` 拥有沙包运行态。
2. 后续真实摄像头只经 `PlayerMotionInput` 输入 snapshot 进入；未选定 SDK 不得阻塞当前键盘规则层与场景路径。
3. 旧 `motion-dodgeball-mvp` 任务包不被删除，单人 AI 传接和重开必须作为回归基线。

## 当前不建议

- 不新建全局 `GameManager`、Singleton、事件总线或 Service Locator。
- 不为 1P/2P/3P 各创建独立输入/投手/规则类。
- 不把摄像头 SDK 的人体数据直接传入沙包或回合规则。
- 不在真实设备验证前，把挥臂阈值、100 秒难度曲线或评级阈值标记为最终平衡值。
