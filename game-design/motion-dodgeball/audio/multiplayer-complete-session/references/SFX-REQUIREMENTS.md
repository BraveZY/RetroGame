# 体感丢沙包｜原创音效需求

| 字段 | 内容 |
|---|---|
| 游戏名称 | 体感丢沙包 |
| 所属主题馆 | 时光体感馆 / 童年操场 |
| 单局范围 | 1–3 人完整单局；躲避者 C、投手 A/B |

## 范围与边界

- 覆盖：主题馆 Loading → 模式选择 → 入戏开场 / 站位识别 → 倒计时 / 对局 → 结算。
- 目标：让家庭玩家在约 3 米电视观看距离下听清“正在进入、轮到谁、危险来了、是否命中、这局结束”，但不依赖声音完成核心操作。
- 记忆锚点：**原创下课铃**（入场）与**原创短口哨**（倒计时、投掷预警）。
- 禁止：真实校园铃声采样、现成音乐旋律、影视/街机/游戏 IP 音效、枪械/硬球音色、羞辱性失败音、金币/连击/抽奖语义。
- `P0` 是首局必须有的反馈；`P1` 是增强可读性；`P2` 可在首版后补齐。所有资源与 Unity 事件入口均为**待实现建议**，不是现有代码能力声明。

## 音效清单

| ID | 阶段 / 触发 | 建议资源名 | 设计方向 | 时长 | 优先级 | 建议 Unity 事件入口 | 验收要点 |
|---|---|---|---|---:|---|---|---|
| SFX-001 | 进入丢沙包 Loading | `sfx_dodgeball_loading_enter` | 柔和气流 + 轻粉笔落点，建立操场过渡感 | 0.6s | P1 | `DodgeballLoadingPanel.OnEnter` | 不像通用转圈提示；不压游戏名。 |
| SFX-002 | Loading 粉笔线 / 沙包标记推进 | `sfx_dodgeball_loading_progress` | 极轻粉笔滑行 + 布袋软摩擦，可拆为循环短段 | 0.35s | P2 | `DodgeballLoadingPanel.OnProgressTick` | 循环 8 次不疲劳；低存在感。 |
| SFX-003 | Loading 完成，进入模式选择 | `sfx_dodgeball_loading_ready` | 短木琴上行 + 轻口哨尾音 | 0.55s | P1 | `DodgeballLoadingPanel.OnReady` | 与模式页首帧衔接，无爆音。 |
| SFX-004 | 模式页首次打开，默认聚焦 1 人 | `sfx_dodgeball_mode_enter` | 原创下课铃变体 + 一次短木琴落点 | 0.9s | P1 | `ModeSelectPanel.OnShown` | 2–3 米可听清；不模仿真实校铃。 |
| SFX-005 | 切换 1 / 2 / 3 人模式 | `sfx_dodgeball_mode_focus` | 短口哨点音，末尾轻微上扬 | 0.18s | P1 | `ModeSelectPanel.OnModeFocused` | 180–220ms 卡片动效内完成；连续切换不刺耳。 |
| SFX-006 | 确认“排队上场” | `sfx_dodgeball_mode_confirm` | 短口哨 + 两拍轻拍手 | 0.45s | P0 | `ModeSelectPanel.OnConfirm` | 只表达确认，不误听成开局胜利。 |
| SFX-007 | 入戏开场，下课铃与操场渐显 | `sfx_dodgeball_intro_bell` | 原创非写实铃片 + 远处空气感 | 1.2s | P0 | `DodgeballIntroPanel.OnBell` | 建立入场记忆锚点；禁止真实校园铃录音。 |
| SFX-008 | 沙包滚至白线旁 | `sfx_dodgeball_sandbag_roll` | 布袋落地 + 细砂摩擦 + 轻停止 | 0.7s | P1 | `DodgeballIntroPanel.OnSandbagArrive` | 与视觉落点误差不超过 80ms。 |
| SFX-009 | 单名真人玩家被看到 | `sfx_dodgeball_slot_seen` | 轻亮音 + 极短勾选落点 | 0.28s | P1 | `DodgeballReadyPanel.OnSlotSeen` | 三人连续就绪不叠音失真。 |
| SFX-010 | 提示“左右挪一步 / 回白线” | `sfx_dodgeball_slot_adjust` | 低强调双音提示，无失败惩罚感 | 0.32s | P1 | `DodgeballReadyPanel.OnAdjustPrompt` | 2 秒冷却；不能是警报或技术播报。 |
| SFX-011 | 全员就绪 | `sfx_dodgeball_ready_all` | 三段递进短音 + 口哨尾音 | 0.65s | P0 | `DodgeballReadyPanel.OnAllReady` | 结束后 0.3–0.6 秒进入倒计时；不能像结算。 |
| SFX-012 | 倒计时 3 / 2 / 1 | `sfx_dodgeball_countdown_tick` | 三次由低到高的木质 / 口哨短音 | 0.22s / 次 | P0 | `GameSessionManager.OnCountdownTick` | 与数字同步，节奏稳定，不用长哨声。 |
| SFX-013 | 倒计时“开始” | `sfx_dodgeball_match_start` | 短拍手 + 明亮口哨起音 | 0.38s | P0 | `GameSessionManager.OnMatchStart` | 清楚区别于“全员就绪”。 |
| SFX-014 | A / B 取得沙包 | `sfx_dodgeball_ball_possession` | 布料收拢 + 小木质点音 | 0.18s | P1 | `Match.OnBallPossessionChanged` | 有方位感，但不干扰预警。 |
| SFX-015 | 投掷预警线出现 | `sfx_dodgeball_throw_warning` | 原创短口哨 + 柔和上行 whoosh | 0.35s | P0 | `Match.OnThrowWarning` | A/B 对应方向声像；0.8 秒冷却；非纯颜色提示。 |
| SFX-016 | 沙包出手 / 飞行 | `sfx_dodgeball_throw_release` | 布袋挥出 whoosh，短且有方向 | 0.25s | P0 | `Match.OnThrowReleased` | 与出手帧误差不超过 50ms；不使用硬球或枪械音。 |
| SFX-017 | 对侧投手接回沙包 | `sfx_dodgeball_ball_catch` | 布料闷接 + 轻手掌拍合 | 0.22s | P1 | `Match.OnBallCaught` | 能区别于命中；不暗示接杀/反杀。 |
| SFX-018 | 擦身 / 成功躲开 | `sfx_dodgeball_near_miss` | 气流掠过 + 轻快木琴点 | 0.3s | P1 | `Match.OnNearMiss` | 围绕 C；0.8 秒冷却；无得分或连击语义。 |
| SFX-019 | C 被击中，生命减少 | `sfx_dodgeball_hit` | 布袋闷撞 + 低沉短音 + 生命碎裂细响 | 0.42s | P0 | `Match.OnDodgerHit` | 明确受击但不惊吓儿童；禁用疼痛叫声。 |
| SFX-020 | 第三次受击，投手阵营胜 | `sfx_dodgeball_throwers_win` | 短降调 + 柔和结束落点 | 0.85s | P0 | `GameSessionManager.OnThrowersWin` | 与失败结算匹配，不嘲讽。 |
| SFX-021 | 100 秒结束，躲避者胜 | `sfx_dodgeball_dodger_win` | 短拍手、木琴上行、原创轻口哨 | 0.95s | P0 | `GameSessionManager.OnDodgerWin` | 可辨别胜利，但避免熟悉胜利旋律。 |
| SFX-022 | 结算卡 / 称号出现 | `sfx_dodgeball_result_reveal` | 粉笔星点 + 柔和落点 | 0.45s | P1 | `ResultPanel.OnReveal` | 低于胜负音；不做奖状、金币爆裂感。 |
| SFX-023 | 结算页焦点与确认（再来一局 / 换队） | `sfx_dodgeball_result_action` | 焦点为轻口哨点音；确认加短木琴落点 | 0.25s | P1 | `ResultPanel.OnAction` | `R` / `Esc` 连续输入不叠音。 |

## 音效资产命名与交付

> 下列为**建议落盘命名**。推荐使用 `.wav` 交付母文件；最终目录、采样率、位深与 Unity Audio Import Settings 仍待工程确认，不代表现有工程已存在这些资产。

| 项目 | 命名规则 / 建议值 |
|---|---|
| 建议目录 | `Assets/Games/MotionDodgeball/Audio/SFX/`（待工程确认） |
| 文件格式 | `sfx_dodgeball_<stage>_<event>[_altNN|_loop].wav` |
| 阶段缩写 | `loading`、`mode`、`intro`、`ready`、`countdown`、`match`、`result` |
| 变体规则 | 同一触发允许 `_alt01`、`_alt02`；循环段使用 `_loop`；不要用 `final`、`new`、`最新版` 等含糊后缀。 |
| Unity Clip 名 | 与文件名一致，去掉 `.wav`；例如 `sfx_dodgeball_match_throw_warning_alt01`。 |

| ID | 建议交付文件名 |
|---|---|
| SFX-001 | `sfx_dodgeball_loading_enter.wav` |
| SFX-002 | `sfx_dodgeball_loading_progress_loop.wav` |
| SFX-003 | `sfx_dodgeball_loading_ready.wav` |
| SFX-004 | `sfx_dodgeball_mode_enter.wav` |
| SFX-005 | `sfx_dodgeball_mode_focus_alt01.wav` |
| SFX-006 | `sfx_dodgeball_mode_confirm.wav` |
| SFX-007 | `sfx_dodgeball_intro_bell.wav` |
| SFX-008 | `sfx_dodgeball_intro_sandbag_roll.wav` |
| SFX-009 | `sfx_dodgeball_ready_slot_seen.wav` |
| SFX-010 | `sfx_dodgeball_ready_slot_adjust.wav` |
| SFX-011 | `sfx_dodgeball_ready_all.wav` |
| SFX-012 | `sfx_dodgeball_countdown_tick.wav` |
| SFX-013 | `sfx_dodgeball_countdown_start.wav` |
| SFX-014 | `sfx_dodgeball_match_ball_possession.wav` |
| SFX-015 | `sfx_dodgeball_match_throw_warning_alt01.wav` |
| SFX-016 | `sfx_dodgeball_match_throw_release_alt01.wav` |
| SFX-017 | `sfx_dodgeball_match_ball_catch.wav` |
| SFX-018 | `sfx_dodgeball_match_near_miss_alt01.wav` |
| SFX-019 | `sfx_dodgeball_match_dodger_hit.wav` |
| SFX-020 | `sfx_dodgeball_result_throwers_win.wav` |
| SFX-021 | `sfx_dodgeball_result_dodger_win.wav` |
| SFX-022 | `sfx_dodgeball_result_reveal.wav` |
| SFX-023 | `sfx_dodgeball_result_action.wav` |

## 制作与混音规则

| 项目 | 要求 | 验收方法 |
|---|---|---|
| 声音层级 | 投掷预警、出手、命中、胜负为 P0；菜单、识别、擦身、结算点缀为 P1；Loading 进度为 P2。 | 三人同屏录屏检查；同一时刻 P0 优先，其他音 duck 或限频。 |
| 方位语义 | `A/B → C` 的预警、出手、接回使用对应声像；菜单与结算为近场中心声像。 | 电视扬声器和单声道都能听出事件，不依赖耳机。 |
| 冷却 | 站位调整、投掷预警、擦身需限频。 | 重复触发不叠音、不刺耳、不遮挡下一次 P0 预警。 |
| 无障碍 | P0 状态同时保留文字、图标或动效，声音只做加强。 | 静音或低音量下，玩家仍能完成完整单局。 |
| 音量 | 菜单与环境提示低于 P0 对局反馈；峰值保留余量，避免电视扬声器破音。 | 最终 LUFS、峰值与 duck 参数待音频实现阶段在目标设备确认。 |
| 交付格式 | 提供工作母带与 Unity 可用导出；文件名须与上表资源名一致。 | 采样率、位深、压缩格式待目标平台和工程 Audio Import Settings 确认。 |

## 当前待确认

| 事项 | 当前口径 | 需要确认 |
|---|---|---|
| Loading 时长 | 以实际准备完成为准，最短展示约 0.6 秒以避免闪屏。 | 场景切换和资源加载 owner。 |
| 声像实现 | 以电视扬声器可读性优先，方向感不能依赖耳机。 | Unity 2D/3D AudioSource 布局与最终电视设备。 |
| 音频参数 | 本文只给方向、层级、时长建议。 | 最终 LUFS、峰值、压缩格式、导入设置与内存预算。 |
