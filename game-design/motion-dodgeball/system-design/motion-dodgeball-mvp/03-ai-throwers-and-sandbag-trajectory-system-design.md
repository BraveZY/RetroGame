# AI 投手传接与沙包轨迹系统框架设计

## 关联任务

| 项目 | 内容 |
|---|---|
| 父级任务 | `game-design/motion-dodgeball/tasks/motion-dodgeball-mvp/00-epic.md` |
| 子任务 | `game-design/motion-dodgeball/tasks/motion-dodgeball-mvp/03-ai-throwers-and-sandbag-trajectory.md` |
| 设计状态 | 已按“C 暂不接沙包，A/B 需要接沙包”的规则更新；不代表验收已完成。 |

## 设计范围

覆盖 A/B 投手角色、单颗沙包持球权、投掷预警、飞行、C 命中/未命中后的对侧接球、持球权切换、接球失败兜底和轨迹可读性。

本设计不覆盖 C 接沙包/反杀、双人模式、真实复杂物理、最终数值平衡、正式美术动画和设备摄像头输入。命中扣生命与躲避统计仍由 04 继续承接，但 03 实现可以保留当前可运行统计以保证样例闭环。

## 系统框架树

```text
Motion Dodgeball MVP
├── Round Flow / 回合流程
│   ├── SinglePlayerReadyFlow（已有，普通 C#；Ready/Countdown/Playing/Result、时间、生命、统计）
│   └── SinglePlayerReadyFlowController（已有，MonoBehaviour adapter；连接场景、HUD、输入、投手和沙包运行态）
│
├── Thrower Role / 投手角色
│   ├── ThrowerId（建议，普通 C# enum；A/B）
│   ├── ThrowerRuntime（建议或 Controller 内部结构；Transform、初始位置、初始缩放、当前角色状态）
│   ├── Thrower_A_Placeholder（已有，Scene Transform；上侧投手）
│   └── Thrower_B_Placeholder（已有，Scene Transform；下侧投手）
│
├── Possession / 持球权
│   ├── currentHolder（建议，运行态字段；当前持球投手 A/B）
│   ├── activeThrower（从 currentHolder 派生；负责预警和出手）
│   └── receiver（currentHolder 的对侧；沙包未命中后负责接球）
│
├── Sandbag State / 单沙包状态机
│   ├── Held（沙包贴在当前持球投手附近，等待预警）
│   ├── Warning（持球投手前摇，预警线指向 C）
│   ├── FlyingToDodger（沙包从持球投手飞向 C）
│   ├── Catching（未击中 C 后，沙包飞向或停在对侧接球点，接球投手主动接球）
│   ├── Caught（接球成功，currentHolder 切换到接球投手）
│   ├── Hit（命中 C，记录命中后重置到对侧或当前规则指定持球者）
│   └── Reset（越界或异常时把沙包放回预期持球投手，避免卡死）
│
├── Warning Visual / 投掷预警
│   ├── Throw_Warning runtime cube（已有运行时占位对象）
│   └── UpdateWarning（已有入口；从持球投手到 C 的线段）
│
├── Balance Config / 参数配置
│   ├── throwIntervalSeconds、warningSeconds、sandbagSpeed（已有 SerializeField）
│   ├── throwerAimSpeed、throwerLaneHalfWidth、throwerWindupOffset（已有 SerializeField）
│   ├── catchDistance、catchBoundaryY（已有 SerializeField；接球距离和接球边界）
│   ├── flightTimeoutSeconds、catchTimeoutSeconds（已有 SerializeField；飞行/接球异常兜底）
│   └── `game-design/motion-dodgeball/balance/.../03-ai-throwers-and-sandbag-trajectory-tuning.md`
│
└── Verification / 验证入口
    ├── 代码检查：确认只有一个 runtime sandbag 对象循环复用
    ├── UnityCaptain/录屏：观察 A/B 持球、出手、未命中后接球、持球权切换
    └── 人工流程：连续 3 局检查不会卡住
```

## 模块边界

| 模块 | 职责 | 对外入口 | 拥有状态 | Unity 接线 |
|---|---|---|---|---|
| Round Flow | 控制 Ready、Countdown、Playing、Result。 | `StartRound`、`Tick`、`RecordHit`、`RecordDodge` | 回合阶段、时间、生命、统计 | 由 `SinglePlayerReadyFlowController` 调用并驱动 HUD。 |
| Thrower Role | 管理 A/B 初始位置、当前位置、缩放和当前动作表现。 | `GetThrower(ThrowerId)`、`MoveThrowerTowardX`、`ResetThrowerPose` | A/B Transform 的运行时位置和缩放 | 引用 `throwerA`、`throwerB`。 |
| Possession | 决定谁持球、谁接球、下一次从哪里出手。 | `currentHolder`、`GetReceiver()`、`SwitchHolder()` | 持球投手 A/B | 不直接接场景，只通过投手 Transform 和沙包对象表现。 |
| Sandbag State | 推进单颗沙包从持有、预警、飞行、接球到切换持球权。 | `EnsureSandbagObject`、`ReleaseSandbag`、`UpdateSandbag`、`AttachSandbagToHolder` | 沙包对象、状态、方向、接球目标、命中标记 | 运行时创建或复用 `Prototype_Sandbag_Runtime`，挂到 `projectileRoot`。 |
| Warning Visual | 表示持球投手到 C 的出手方向和前摇。 | `ShowWarning`、`UpdateWarning`、`HideWarning` | 预警对象可见状态 | 运行时创建 `Throw_Warning` cube。 |
| Balance Parameters | 保存投掷、接球和可读性参数。 | Inspector 字段；调参记录 | 参数值，不保存业务状态 | 当前继续放在 Controller SerializeField；参数变多后再考虑 ScriptableObject。 |

## 状态机

```text
Held
  -> Warning（throwTimer 到达预警窗口）
  -> FlyingToDodger（warningTimer 结束，持球投手出手）
  -> Hit（沙包碰到 C，记录命中，重置或切换到下一持球者）
  -> Catching（沙包穿过中区且未命中 C，对侧投手接球）
  -> Caught（接球投手接住同一颗沙包）
  -> Held（持球权切换，等待下一轮）
```

关键规则：

- C 暂时只躲避，不接球、不反击。
- A/B 是角色，不是左右或上下两个独立发射器。
- 任意时刻最多一颗 MVP 沙包参与主循环。
- 只有当前持球投手瞄准和前摇；对侧投手在沙包未命中后才进入接球行为。
- 接球成功后才统计躲避成功并切换持球权。
- 接球失败、飞行/接球超时或越界时，沙包必须进入 Reset/Held，不能持续生成新沙包或卡在场外。

## 关键 seam / adapter

| 边界 | 目的 | 替换/验证方式 |
|---|---|---|
| Time step | 让预警、飞行、接球移动和投掷间隔可重复验证。 | 后续可下沉普通 C# 状态机，用固定 delta 做 EditMode 测试。 |
| Dodger position | 让出手目标可接入模拟输入或摄像头输入。 | PlayMode/录屏观察预警线和出手方向是否跟随 C。 |
| Possession state | 防止 A/B 被当成独立发射器。 | 代码检查 `currentHolder`，录屏确认下一轮从接球者出手。 |
| Sandbag object lifecycle | 保证同一颗沙包循环复用。 | 代码检查和 UnityCaptain 层级读回，确认不堆叠多个沙包。 |
| Receiver movement | 让“B 主动接沙包”可观察。 | 录屏确认未命中后对侧投手移动到接球点。 |
| Balance parameters | 避免投掷/接球数值散落。 | 调参记录同步，未验证参数保持待测。 |

## ScriptableObject / Prefab / Scene 计划

第一轮不引入新全局框架。

- Controller 继续持有场景引用和 SerializeField 参数。
- 沙包和预警继续使用运行时占位 cube；正式资源接入时再替换为 Prefab。
- 本轮不新增 ScriptableObject，等 03/04/05 参数稳定后再考虑 `DodgeballThrowConfig`。
- 不新增 Manager、Singleton、事件总线或 Service Locator。

## 测试与验证计划

| 验证项 | 方式 | 对应验收 |
|---|---|---|
| A/B 都能作为持球投手出手 | UnityCaptain/录屏/人工流程观察 A/B 轮流持球。 | 03 A/B 出手验收 |
| 只存在一颗主循环沙包 | 代码检查或 UnityCaptain 层级读回。 | 单颗沙包运行态 |
| 未命中后对侧投手主动接球 | 录屏观察接球投手移动到接球点。 | 对侧接球验收 |
| 接球后下一轮从接球投手出手 | 录屏观察持球权切换。 | 持球权切换验收 |
| 预警清楚可见 | 截图或录屏检查预警线。 | 预警验收 |
| 连续 3 局不卡住 | 人工流程或录屏。 | 稳定性验收 |

## 进入 game-unity-implementation 的边界

可以进入实现的范围：

- 把 `SinglePlayerReadyFlowController` 中旧的 `throwIndex + SpawnSandbag` 模型改为 `currentHolder + single sandbag state` 模型。
- 让当前持球投手瞄准 C，未命中后对侧投手主动接球。
- 保留占位 cube 和现有 HUD，避免场景/Prefab 大改。
- 只在有证据时勾选任务验收；代码实现完成但未录屏的项保持未验收。

不建议现在做：

- C 接沙包、反杀、手势识别或双人模式。
- 真实抛物线/复杂物理、正式角色动画和正式音效。
- 把 03 扩成完整平衡或评级系统。
- 没有运行证据就批量勾选 03/父级验收项。
