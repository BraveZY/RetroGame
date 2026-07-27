---
name: project-motionsport-validation-contract
description: MotionSport 项目验证契约：为 UI、动画、AI、状态机、Task/位移、资源、数据、构建等任务定义固定验收口径；只读验收模板，不注册工具、不替代具体验证工具。
---

# MotionSport Validation Contract

用于 MotionSport 项目任务的最终验收口径。它不决定怎么修，也不替代 Unity Captain 内置 Skill；只规定“什么证据能证明这个层级的问题已经被验证”。

## 通用验收输出

最终汇报必须包含：

```text
验证目标：
- <要证明什么现象消失或出现>

验证证据：
- <工具 / 场景对象 / 字段 / 日志 / 截图 / diff / 测试>

通过标准：
- <达到什么状态才算通过>

未覆盖项：
- <未进入 PlayMode / 未截图 / 未跑构建 / 未形成 Review / 需要人工手感确认>
```

禁止只写：
- “已验证”
- “看起来没问题”
- “应该可以”
- “已检查代码”

## 分层验收模板

| 类型 | 必看证据 | 通过标准 | 常见未覆盖 |
|------|----------|----------|------------|
| UI 静态结构 | Canvas/Screen 根、真实节点、RectTransform、Text/Image/Button、绑定字段、默认 active 状态 | 目标节点存在，层级正确，关键字段绑定不为空，文本/显隐/锚点符合预期 | 未进 PlayMode、未截图、多分辨率未测 |
| UI 运行态 | 入口事件、Presenter/View 刷新、数据源、CanvasGroup/active 状态、截图或快照 | 触发后数据写入 UI，显示状态与业务状态一致，没有运行时代码补结构 | 数据时序未覆盖、人工交互未复现 |
| 动画 | Animator 参数、当前状态、Layer/Transition、Clip、连续帧是否被打断 | 目标状态真实切换并保持到预期窗口，Transition 未被每帧重置，参数来源明确 | 未进 PlayMode、只查了 Controller 静态结构 |
| AI / Decision | 决策输入、目标对象、条件分支、随机/概率结果、决策输出 | 输入满足时产生正确目标/意图；输入不满足时能解释提前退出原因 | 未覆盖随机种子、未覆盖边界局面 |
| State | 当前状态、切换条件、切换历史、接管 owner、是否被抢回 | 状态按预期切换，后续帧没有被错误 owner 抢回 | 未连续采样、缺触发步骤 |
| Execution / Task | Task 创建、入队、开始/结束、目标点、速度/方向、执行器字段 | Task 到达执行器，目标点有效，Transform/Rigidbody/控制器在连续帧中按预期变化 | 只看了 Task 创建，没看执行结果 |
| 位移 / 物理 | 目标点、速度、朝向、Rigidbody/Collider/CharacterController、覆盖源 | 位移来源唯一或 owner 明确，连续帧变化符合目标，没有被其它脚本覆盖 | 未采样连续帧、未区分物理/非物理位移 |
| Resource / Prefab | 资源路径、GUID/引用、Prefab 来源、加载日志、Prefab/Scene 字段 | 资源存在且引用正确，运行时加载路径可解释，Prefab/Scene 绑定未断 | 资源索引陈旧、未验证运行时加载 |
| Data / 配置 | SO/配置表/默认值、运行时实际读值、覆盖链 | 运行时读到的值与目标配置一致，旧序列化数据不会覆盖新默认值 | 未检查现有 Prefab/Scene/.asset 旧数据 |
| Build / Platform | 编译状态、构建日志、PlayerSettings、Package、asmdef、HybridCLR/HOT/AOT | 编译通过，构建关键阶段通过，平台边界和热更配置无阻塞 | 未真机构建/安装/运行 |
| Review / 序列化写入 | 写前 snapshot、写后 snapshot、semantic diff、Monitor Review 或替代证据 | diff 范围与目标一致，Review 证据形成；不能完整 diff 时明确 no_full_diff | 缺写前 snapshot、替代证据未人工确认 |
| C# 重构 / 引用影响 | callsite、序列化绑定、Prefab/Scene/Inspector、测试或编译 | 代码引用和 Unity 绑定影响面清楚，编译通过，必要时保留 FormerlySerializedAs | 未跑 PlayMode、资源索引覆盖不足 |

## 验证升级规则

1. 静态结构能证明的问题，不强行进入 PlayMode。
2. 运行时行为、状态抢占、动画打断、位移覆盖必须至少有单帧快照；涉及覆盖必须有连续帧证据。
3. 写 Unity 序列化内容必须有 Review 证据或 `no_full_diff` 替代证据。
4. 改 C# 符号、序列化字段、脚本、Prefab 或共享资源前后，必须说明引用影响面。
5. UI 视觉问题至少要有结构回读；布局或显示问题需要截图或 UI 工具审计。
6. 构建问题不能只用代码检查代替构建/编译日志。

## MotionSport 层级映射

| MotionSport 层 | 默认验证 Skill / 证据 |
|----------------|-----------------------|
| Entry | `unity-debug`、Console、active scene、入口对象、初始化顺序 |
| Input | 输入事件、PoseAPI/输入桥接日志、运行态值 |
| Decision | 决策输入、目标、条件分支、概率/规则输出 |
| State | 状态机当前状态、切换历史、连续帧抢占证据 |
| Execution | Task 队列、目标点、执行器、Transform/Rigidbody 连续帧 |
| Presentation | Animator/UI/VFX/Audio 状态、截图、运行态快照 |
| Data | 配置来源、运行时读值、旧序列化覆盖链 |
| Resource | Prefab/资源路径、引用、加载日志、反向引用 |
| Build | 编译/构建日志、HybridCLR/HOT/AOT/asmdef、真机安装结果 |

## Output

输出：
- 验证目标
- 验证证据
- 通过标准
- 未覆盖项
- 下一步补证动作
