# MotionX Retro Pack 协作约定

本文件是本仓库中 Agent 的入口。以当前工作区中的代码、Unity 设置和对应小游戏文档为事实来源；产品资料是设计意图，不能替代已验证的运行时行为、平台能力或发布状态。

## 项目概览

- Unity `2022.3.62f3`（revision `96770f904ca7`），URP `14.0.12`。
- 这是一个包含多个复古体感小游戏的主题包，不要将不同小游戏的玩法、资源和验收混在同一项改动中。
- 当前已有可运行代码的小游戏是 `MotionDodgeball`，其代码位于 `Assets/Games/MotionDodgeball/`；目录下包含 `Runtime`、`Editor`、`Scenes`、`Prefabs`、`Art`、`Audio` 和 EditMode 测试。
- 旧 `com.njljh.unitymcp` 已于 2026-07-30 移除；后续 UniMcp 的集成方式待确认。在新插件完成安装与连接验证前，不依赖旧 UnityMCP 服务、设置或客户端配置。

## 先读什么

按任务范围选择最小必要集合，避免全量扫描：

1. 始终先读本文件，并检查 `git status --short`；保留与当前任务无关的未提交改动。
2. 产品层：`game-design/motionx-retro-pack/agents/game-project.md`、`game-design/motionx-retro-pack/agents/domain.md`，以及 `Retro_Games_plans/docs/项目管理计划/MotionX-Retro-Pack-产品定义.md`。
3. 单游戏功能：优先读 `game-design/<game-slug>/GAME_CONTEXT.md`、`briefs/`、`gdd/`、`system-design/`、`tasks/` 中与需求直接相关的文件。当前 `motion-dodgeball`、`chalk-dodge`、`blackboard-eraser` 均有独立约定。
4. 工程实现：先读目标目录相邻的 C#、`.asmdef`、Scene/Prefab 和对应测试，再修改。
5. 平台、资源或任务工作流：读对应小游戏的 `game-design/<game-slug>/agents/`；产品级参考见 `game-design/motionx-retro-pack/agents/references/`。

若文档与当前代码、`ProjectSettings/`、`Packages/manifest.json` 冲突，以后者为当前工程事实，并在交付中说明差异；不要暗中把旧文档当作实现依据。

## 代码与 Unity 资源规则

- 保持改动限于指定小游戏与任务范围。新小游戏使用独立的 `game-design/<game-slug>/` 文档上下文；先确认目录和命名，再创建生产目录或装配定义。
- 运行时脚本放在对应小游戏的 `Scripts/Runtime/`，Editor 工具放在 `Editor/`，测试放在 `Scripts/Tests/EditMode/` 或已确认的 PlayMode 路径；维护 `.asmdef` 引用边界。
- 凡涉及 C# 的查询、评审、新增、修改、删除或重构，**必须先使用** [`csharp-doc-expert`](/Users/dukechen/.codex/skills/csharp-doc-expert/SKILL.md)，并以其为注释语义标准；注释的检查、补齐、改写或清理必须与代码改动一并交付，不得留作后续项。
- 改动 `.unity`、Prefab、`.asset`、`ProjectSettings/`、`Packages/` 或导入设置前，先检查引用与 Git diff；完成后回读并在可用 Unity 环境中验证。不要批量重序列化无关资源。
- Unity 资源必须连同 `.meta` 文件处理；不要手工删除或重新生成已有 `.meta`，不要修改 `Library/`、`Temp/`、`Logs/` 等生成目录。
- 保持复古亲子体感的产品语言；禁止把真实老歌、影视、街机/游戏 IP、可识别品牌直接作为生产内容。视觉/音频意图以 `Retro_Games_plans/docs/` 的对应设计资料为准。

## 文档、任务与验收

- 本地 Markdown 是已确认的任务载体，既有工程任务在 `agent/task/`，小游戏实施任务在 `game-design/<game-slug>/tasks/`。只在用户委托范围内创建或更新任务，并如实回填证据。
- 玩法、范围或跨小游戏的持久决策，应先更新对应 brief/GDD/system-design，或在用户确认后建立 ADR；不要只改代码或在聊天中留下规则。
- 不要把 Mock、编译通过、dry-run、Unity Console 无错误或 Editor 可见状态描述为后端、真机、签名、商店或正式发布已就绪。未执行的验证标记为“待确认”。
- 中文文档与 C# 注释默认使用简体中文。C# 类使用职责清晰的 XML `summary`，仅为核心/复杂/易歧义方法写单行 XML；关键字段可用极简行尾注释，长且职责分段明确的文件使用中文横线分区。不得为简单生命周期、转发或 setter 堆砌注释；发现过时或孤儿注释必须同步修正或删除。

## 验证与交付

- 代码变更后，优先运行与改动相邻的 EditMode/PlayMode 测试，并检查编译和 Unity Console；无法运行时说明具体阻塞（Unity license、平台模块、MCP 连接、设备或签名）。
- Android/iOS 的 `PlayerSettings`、IL2CPP、API level 与 Build Profile 以当前 `ProjectSettings/ProjectSettings.asset` 和实际 Unity 回读为准。真实 Android 构建、iOS Xcode 导出、签名和真机体验必须单独验证。
- 不执行未获委托的 Git 提交、推送、分支切换、包升级、场景重建或平台设置改写。
- 最终交付简洁说明：修改文件、验证结果、未验证的风险/待确认项；给出相关文件的绝对路径。

## Agent skills

按任务使用仓库的游戏技能库；常用入口为：

- 需求与范围：`game-task-triage`、`game-brief`、`game-feature-slicer`
- Unity 设计与实现：`game-unity-system-design`、`game-unity-implementation`
- 质量与发布：`game-qa-debug`、`game-performance-build`、`game-release-liveops`
- 风格一致性：`motionx-retro-pack-style-dna`
- C# 代码与注释：`csharp-doc-expert`（强制；规则见上）

执行任何技能前，先读取其 `SKILL.md`，并遵守其中的适用范围和验证要求。
