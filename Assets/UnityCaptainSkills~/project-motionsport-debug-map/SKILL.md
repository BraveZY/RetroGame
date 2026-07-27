---
name: project-motionsport-debug-map
description: MotionSport 项目调试地图。从项目 Git 历史中的 MotionSport 调试参考恢复，补充启动、运动模块、资源加载与 HybridCLR 热更新排查入口；只做项目上下文路由，具体编译、日志、PlayMode 和修复动作仍交给 Unity Captain 内置 skill。
---

# MotionSport Debug Map

## Use

- 处理 MotionSport 编译、Console、PlayMode、资源加载、HybridCLR 或运动模块异常时，先用本地图确认项目责任区域。
- 本技能只提供项目路径和排查顺序，不注册工具、不替代 `unity-debug`、`unity-runtime-evidence`、`unity-asset`、`unity-refactor` 等内置技能。
- 若问题已经能归入 Entry/Input/Decision/State/Execution/Presentation/Data/Resource/Build 层级，同时读取 `project-motionsport-layer-routing`。

## Project Identity

| Item | Value |
|---|---|
| Product | `MotionSport_URP(Unity 2021.3.43f1)` |
| Unity version | read `ProjectSettings/ProjectVersion.txt` |
| Render pipeline | URP |
| Hot update | HybridCLR |

## First Routing

| Symptom | First area | Next skill |
|---|---|---|
| compile error / Console exception | first project file in log | `unity-debug` |
| PlayMode behavior mismatch | runtime object, state, task, animator evidence | `unity-runtime-evidence` |
| missing scene/prefab reference | serialized field, prefab, scene object | `unity-asset` / `unity-component` |
| script fix or refactor | owner module below | `unity-refactor` |
| Android or HybridCLR build issue | HOT/AOT, build command, generated output | `unity-android-build` |
| acceptance proof | matching validation template | `project-motionsport-validation-contract` |

## Main Debug Areas

### Entry And Project Startup

- `Assets/ADD/Scripts/Project/AddInit.cs`
- 当前场景入口 Manager
- 单例初始化顺序和场景启动对象

### Tennis

- `Assets/CoreGameScript/Tennis_Script/`
- `Assets/CoreGameAssets/Tennis_Assets/`
- 优先链路：规则/AI -> 状态 -> Role/Task -> Ball -> Animator/UI

### Basketball

- `Assets/CoreGameScript/Basketball_Script/`
- `Assets/CoreGameAssets/Basketball_Assets/`
- `Assets/ADD/Resources/Basketball/`
- 优先链路：模块入口 -> 角色控制 -> 移动/投篮/得分 -> UI/Animator

### FootBall

- `Assets/CoreGameScript/FootBall_Script/`
- `Assets/CoreGameAssets/FootBall_Assets/`
- 优先链路：球员/球 -> 规则 -> 状态 -> 执行 -> 表现

### Bowing / Bowling

- `Assets/CoreGameScript/Bowing_Script/`
- `Assets/CoreGameAssets/Bowing_Assets/`
- `Assets/Resources/BowlingRole/`
- 注意项目目录拼写是 `Bowing_Script`，不要误建 `Bowling_Script`。

### Pose And Input

- `Assets/Scripts/PoseAPI/`
- 先确认输入桥、传感器数据和动作识别，再查 AI、状态机或 Animator。

### Resource Loading

- `Assets/AssetBundlesLoadTools/ResourceManager.cs`
- `Assets/AssetBundlesLoadTools/DownloadManager.cs`
- 先核对资源路径、Bundle key、加载时序和生命周期，再改业务逻辑。

### HybridCLR And Hot Update

- `Assets/CoreGameScript/AssemblyUpdateScriptsHOT/`
- `Assets/CoreGameScript/AssemblyUpdateScriptsAOT/`
- `Assets/AssemblyHotList.txt`
- 先确认程序集归属、AOT/HOT 边界和构建生成物，再决定是否改代码。

### Android Build

- `Assets/Editor/BuildAndroidCommand.cs`
- 先看构建日志里第一个项目相关错误，再查 PlayerSettings、HybridCLR 和资源构建链。

## Triage Rules

- 编译错误：从日志里第一个指向项目代码的文件开始查，不先改第三方或生成代码。
- 空引用或丢失引用：先看相关模块的场景对象、Prefab 绑定和序列化字段。
- 资源或 Bundle 异常：先核对路径规范、Bundle key 和加载时序，再改业务逻辑。
- 热更新异常：先确认程序集归属和 HOT/AOT 边界，再决定改代码或构建配置。
- 动画/表现异常：先证明状态和执行层确实给出了正确输入，再查 Animator/VFX/UI。
- 证据不足时停止猜测式修复，输出已确认日志、可能责任层和下一步需要的 Unity 侧证据。

## Output Contract

使用本技能后的最终汇报至少包含：

```text
项目定位：
- <命中的 MotionSport 模块或入口>

关键证据：
- <日志 / 场景对象 / Prefab / 字段 / 运行态状态 / 构建输出>

责任层判断：
- <Entry/Input/Decision/State/Execution/Presentation/Data/Resource/Build>

下一步：
- <交给哪个 Unity Captain skill 或需要人工补哪项证据>
```
