# PoseAPI 与 MotionSport 对齐升级

## 当前状态

| 项 | 结论 |
|----|------|
| 目标与当前判断 | 将 RetroGame 的 `Assets/Scripts/PoseAPI` 升级为 MotionSport 当前模块化 PoseAPI；补齐 macOS 本地 YOLO bundle。 |
| 状态 | ✅ 完成 |
| 卡点/待确认 | 未进入 PlayMode/摄像头实测；当前 UnityMCP 连接归属 B，未用于 A 的运行态验证。 |
| 下一步 | 如需行为闭环，在 A 的目标场景使用其自身 UnityMCP 或 Editor 进入 PlayMode。 |

## Task Contract

| 目标结果 | Invariants | 允许/禁止范围 | 验收 | 回退 |
|----------|------------|---------------|------|------|
| PoseAPI 的源代码、测试、编辑器工具和 macOS 本地 YOLO 能力与 MotionSport 一致 | 不覆盖无关改动；保留同名既有脚本的 Unity GUID，避免 A 场景丢失组件绑定 | 仅 `Assets/Scripts/PoseAPI`、`Assets/Plugins/macOS/MacYoloPose.bundle` 及其 `.meta` | B/A 文件清单和内容对比；依赖、YAML 引用、Unity 编译检查 | 以本次 Git diff 反向还原所改路径 |

## 执行与验证

| 阶段 | 工作项 | 完成标准 | 状态 | 证据/缺口/下一步 |
|------|--------|----------|------|------------------|
| 事实与边界 | 核对 A/B 模块及 native 依赖 | 明确差异和已有依赖 | ✅ 完成 | A 的 GameCore、Windows/Android native DLL 与 B SHA-256 一致；仅缺 macOS bundle。 |
| 实施或取证 | 同步 PoseAPI、macOS bundle 与 SkeletonCenter 消费方 | 新 API 文件齐全且 GUID 保留 | ✅ 完成 | `PoseAPI`、MacYolo bundle/构建源与 `SkeletonCenter` 已逐文件对比，和 B 完全一致（包含 `.meta`）。 |
| 定向验证 | 静态合约及 YAML 引用检查 | 核心目录等价、场景无缺失脚本 | ✅ 完成 | 迁移的 6 个 script GUID 均与 B 一致；外部 C# 无旧 PoseAPI 符号残留。 |
| 回归与一致性 | Unity Editor 编译 | 无新增编译错误 | ✅ 完成 | A 的已打开 Unity 2022.3.62f3 于 11:56:35 产出 Core、GameCore、MacYolo、Editor、Samples 和 PlayModeTests 程序集；日志未见本轮错误。 |
| 同步与收尾 | 复核 diff 与任务记录 | 变更范围可回退 | ✅ 完成 | `git diff --check` 通过；macOS bundle `codesign --verify --deep --strict` 通过。 |
