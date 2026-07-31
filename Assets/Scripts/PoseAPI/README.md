# PoseAPI

PoseAPI 为 MotionSport 提供统一的 20 点人体姿态输入。Android Player 与 Windows Editor PlayMode 使用 GameCore SDK；macOS Editor/Player 使用本机 Core ML YOLO。`PoseFrame20` 是当前唯一公开姿态数据契约。

## 当前安装边界

当前版本仍是 `Assets` 内嵌插件，不是 UPM package。支持两种安装 profile：

| Profile | 文件范围 | 用途 |
| --- | --- | --- |
| Core-only | `Assets/Scripts/PoseAPI`，排除 `Sources/GameCore` 与 `Sources/MacYolo` | 自定义 `IPoseDataSource`、Prefab、Inspector、Samples 和 fake 生命周期测试 |
| Full | 完整 `Assets/Scripts/PoseAPI`，再安装下表外部依赖 | MotionSport 的 GameCore SDK 与 macOS Local YOLO |

Core assembly 只通过 `PoseDataSourceRegistry` 查找已安装 source，不直接引用 GameCore 或 MacYolo。Full profile 的可选程序集边界为：

| Assembly | 职责 | 外部依赖 |
| --- | --- | --- |
| `PoseAPI` | 20 点契约、生命周期、组件与可视化 | 无 |
| `PoseAPI.GameCore` | SDK source 与 `PoseDataConverter` 兼容层 | `GameCore_Runtime.dll` |
| `PoseAPI.MacYolo` | macOS 本地 YOLO source 与 native bridge | `GameCore_Runtime.dll`、`MacYoloPose.bundle` |

Full profile 还必须具备：

| 依赖 | 权威路径 | 影响 |
| --- | --- | --- |
| GameCore Runtime | `Assets/ADD/Scripts/Runtime/GameCore_Runtime.dll` | `PoseAPI.GameCore`、`PoseAPI.MacYolo` 编译及 Mac 相机 owner |
| Windows pose native | `Assets/Plugins/x64/Detect/detect_pose.dll` | Windows Editor 的 GameCore 姿态检测 |
| Windows pose transform | `Assets/Plugins/x64/Detect/transformpose.dll` | Windows Editor 的 GameCore 20 点转换 |
| Mac 原生插件 | `Assets/Plugins/macOS/MacYoloPose.bundle` | macOS Core ML session |
| Mac 模型 manifest | `Assets/Plugins/macOS/MacYoloPose.bundle/Contents/Resources/ModelManifest.json` | 模型输入输出契约 |
| GameCore 初始化入口 | `Assets/ADD/Scripts/Project/AddInit.cs` | 创建并启动 `GameCore.Camera` |

缺失依赖时，选中 `PoseDataManager`，统一 Inspector 的 **Dependencies** 区域会显示缺失路径、影响和恢复方式。诊断不会下载 DLL、替换 bundle、创建第二路摄像头或偷偷切换 source。

Windows 两枚 DLL 必须仅启用 Windows Editor x86_64 与 Win64；不要启用 Any、Linux 或 macOS。Android 的 GameCore pose/camera native 当前按 ARM64 安装，正式 APK 仍需确认构建 ABI 与目标设备一致。

### 升级与卸载

- 从旧目录升级时，以当前 `Assets/Scripts/PoseAPI` 为一个整体替换；不要同时保留旧 `Network`、`Utils/PoseDataConverter.cs` 或 `Core/CoordinateRenderer.cs`。
- 下列移动均保留原 `.meta` GUID，既有场景和 Prefab 不需要重新绑定：

| 旧路径 | 当前路径 | GUID |
| --- | --- | --- |
| `Core/CoordinateRenderer.cs` | `Renderers/CoordinateRenderer.cs` | `c55ea1c094ac44d0e9a49310117af515` |
| `Network/IPoseDataSource.cs` | `Sources/Abstractions/IPoseDataSource.cs` | `daaff1088f2ee4605a564a8c655d4ddd` |
| `Network/PoseDataSourceConfig.cs` | `Sources/Abstractions/PoseDataSourceConfig.cs` | `ddc566178167c44019f910ab27c442ba` |
| `Network/PoseDataSourceType.cs` | `Sources/Abstractions/PoseDataSourceType.cs` | `395e8528a407945de9a25871916d1416` |
| `Network/PoseDataClientSDK.cs` | `Sources/GameCore/PoseDataClientSDK.cs` | `e5153883153224a2da8f7adbcea2672b` |
| `Utils/PoseDataConverter.cs` | `Sources/GameCore/PoseDataConverter.cs` | `189d0f59f5f794d88b97c211afd2b805` |
| `Network/MacLocalYoloPoseDataSource.cs` | `Sources/MacYolo/MacLocalYoloPoseDataSource.cs` | `cac6f1696952a4a689016e2b1b755236` |
| `Network/MacYoloPoseNative.cs` | `Sources/MacYolo/MacYoloPoseNative.cs` | `ab56d9914845e49da9bc6aa2b3081f5d` |

- 卸载前先从场景/Prefab 移除 PoseAPI 组件，再删除插件目录。`GameCore_Runtime.dll`、`AddInit` 和 Mac bundle 是宿主共享资产，不随 PoseAPI 自动删除。
- Full → Core-only 时只移除 `Sources/GameCore` 与 `Sources/MacYolo`，并确认业务已注册自己的 `IPoseDataSource`；Core Inspector 会继续工作。

## 支持矩阵

| 环境 | 有效 source | 说明 |
| --- | --- | --- |
| Android Player | GameCore SDK | 强制使用 SDK |
| Windows Editor PlayMode | GameCore SDK | 强制使用 SDK |
| macOS Editor / Player | Mac Local YOLO | 复用 `GameCore.Camera.CameraTexture` |
| 其他平台 | Unsupported | 不静默回退 |

Inspector 中的 `Data Source` 是用户选择，`Effective Source` 是当前平台实际采用的类型。平台不匹配会给出提示，不自动覆盖序列化配置。

## 两分钟接入

1. 使用 `GameObject > Pose API > Pose API Manager`，或拖入 `Prefabs/PoseAPIManager.prefab`。
2. 在 `PoseDataManager` 的统一 Inspector 中选择 source、单/双人模式和 source 参数。
3. 新场景建议保持 `Auto Start=false`，确认依赖后进入 Play Mode，使用 **Start**。
4. 业务代码订阅 `OnPoseFrame20Update`：

```csharp
using UnityEngine;

public sealed class PoseConsumer : MonoBehaviour
{
    [SerializeField] private PoseAI.PoseDataManager poseManager;

    private void OnEnable()
    {
        poseManager.OnPoseFrame20Update += HandlePose;
    }

    private void OnDisable()
    {
        poseManager.OnPoseFrame20Update -= HandlePose;
    }

    private void HandlePose(PoseAI.PoseFrame20 frame)
    {
        Debug.Log($"frame={frame.frameId}, players={frame.skeletons.Count}");
    }
}
```

既有四个权威场景已迁移为 `Auto Start=true`，保持原启动时序：

- `Assets/Scenes/pose.unity`
- `Assets/CoreGameAssets/Basketball_Assets/Scenes/Basketball_Main.unity`
- `Assets/CoreGameAssets/Ski_Assets/Scenes/Ski_main.unity`
- `Assets/AssetBundlesLoadTools/Scene/MyDown.unity`

Tennis `Test.unity` 与 `sample.unity` 已确认冗余，不属于迁移或回归范围。

## 生命周期

| API / 配置 | 行为 |
| --- | --- |
| `autoStart=false` | 不创建 source，不占用 SDK、native session 或相机消费循环 |
| `EnsureDataSourceCreated()` | 只创建和配置 source，不启动 |
| `StartReceiving()` | 显式启动；重复调用不重复订阅 |
| `StopReceiving()` | 解绑、停止并销毁 source |
| `Retry()` | 清理错误实例、重置指标并重新启动 |
| `SwitchDataSource(type)` | 按 Stop → Unsubscribe → Destroy → Create → 可选 Start 执行 |

运行状态为 `Idle / Initializing / Running / Stopped / Unsupported / Error`。Inspector 同时显示：

- `EffectiveSource`
- `LastError`
- `LastFrameTime`
- `FrameCount`
- `DetectedPlayers`

## PoseFrame20 主契约

- 坐标：左上原点，`x/y` 为 `0..1`。
- 玩家：`frame.skeletons` 为当前帧玩家列表。
- 点数：每名玩家固定 20 点。
- `tracked=false` 表示该点不可用。
- `approximate=true` 表示该点由 source 近似补齐；Mac YOLO 的 Hand/Foot 可能属于此类。

| Index | Joint | Index | Joint |
| ---: | --- | ---: | --- |
| 0 | HipCenter | 10 | WristRight |
| 1 | Spine | 11 | HandRight |
| 2 | ShoulderCenter | 12 | HipLeft |
| 3 | Head | 13 | KneeLeft |
| 4 | ShoulderLeft | 14 | AnkleLeft |
| 5 | ElbowLeft | 15 | FootLeft |
| 6 | WristLeft | 16 | HipRight |
| 7 | HandLeft | 17 | KneeRight |
| 8 | ShoulderRight | 18 | AnkleRight |
| 9 | ElbowRight | 19 | FootRight |

旧 `PoseInferenceResult`、`Landmark`、`KeypointIndices` 与 `PoseSmoother` 已移除。`PoseDataConverter` 只负责把 `PoseFrame20` 转成旧 GameCore 容器，位于 `PoseAPI.GameCore` 兼容层；它不是第二套姿态契约。

## Samples

| Sample | 路径 | 用途 |
| --- | --- | --- |
| Minimal | `Samples/Minimal/MinimalPoseAPI.unity` | 订阅 Frame20，每隔固定帧数输出帧号和人数 |
| Skeleton Preview | `Samples/SkeletonPreview/SkeletonPreview.unity` | Canvas 上显示 20 点骨架；属于诊断表现，不是核心依赖 |

两个 Sample 默认都不会启动真实 source。先根据当前平台选择 source，再在 Play Mode Inspector 中按 **Start**。

## 常见问题

### `Unsupported`

当前 Editor 平台与 source 不匹配。Windows Editor 选择 SDK，macOS 选择 Mac Local YOLO；其他平台需要在支持矩阵内验证。

### GameCore 编译或初始化失败

确认 `GameCore_Runtime.dll` 存在，并确保启动链中有 `AddInit`。SDK 会等待 GameCore 初始化，超时后进入 `Error`。

### Windows Editor 找不到 pose native

确认 `detect_pose.dll` 与 `transformpose.dll` 存在，并在 Plugin Import Settings 中启用 Windows Editor x86_64。统一 Inspector 会检查文件与 Editor/Win64 兼容开关，但实际 DLL 依赖和首帧仍需在 Windows Editor 验证。

### Mac bundle / model 缺失

确认 bundle、`ModelManifest.json`、`YoloPose.mlmodelc` 和 `.meta` 同时存在，Plugin Import Settings 已启用 macOS Editor 与 StandaloneOSX。

### Mac 10 秒相机超时

PoseAPI 不创建摄像头。检查 macOS 摄像头权限、GameCore 初始化和 `GameCore.Camera.CameraTexture`；不要新增第二个 camera owner 作为兜底。

### 重复 Manager

同一运行域只允许一个 `PoseDataSourceManager`。依赖诊断会报告数量，但不会自动删除对象；先确认业务 owner 再处理。

## 验证

- `PoseAPI.EditModeTests`：20 点与 assembly 边界契约、创建菜单、依赖诊断、统一 Inspector、Prefab 合约。
- `PoseAPI.PlayModeTests`：autoStart、Start/Stop/Retry/Switch、错误恢复和生产 source 创建不自启。
- Core-only 抽取验证：不带两个可选 Sources 与 GameCore DLL 时，Core、Editor、Samples 独立编译，5 个 fake PlayMode 用例通过。
- Full profile 抽取验证：只带完整 PoseAPI、GameCore DLL 和 Mac bundle 时，5 个 PoseAPI 程序集独立编译。
- fake source 只证明生命周期状态机；真实 Mac 首帧、Windows SDK 和 Android Player 必须分别专项验证。
