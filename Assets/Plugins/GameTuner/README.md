# GameTuner

运行时参数调试插件。同一 WiFi 下通过手机或 PC 浏览器实时调整游戏参数，支持 Unity Editor 和 Android APK 两种环境。

> 更新时间：2026-04-02

---

## 快速接入

### 1. 在场景中挂载入口组件

在任意 GameObject 上添加 **`TunerInit`** 组件（推荐新建空 GameObject 命名为 `GameTuner`）。

进入 Play 模式后，Console 会输出调参地址：

```
[GameTuner] 调参地址: http://192.168.x.x:8089
```

### 2. 为具体游戏模块创建桥接层

在游戏模块目录下创建桥接脚本（参考 `BasketballTunerBridge.cs`），在 `OnEnable` 中注册参数，在 `Update` 中轮询回写，在 `OnDisable` 中注销。

```csharp
void OnEnable()
{
    ParameterHub.Register(new ParameterEntry
    {
        id           = "my_param",
        category     = "模块名称",
        name         = "参数显示名",
        description  = "参数说明",
        minValue     = 0f,
        maxValue     = 1f,
        defaultValue = 0.5f,
        currentValue = 0.5f,
        step         = 0.05f
    });
}

void Update()
{
    float v = ParameterHub.GetValue("my_param");
    // 将 v 回写到对应配置字段
}

void OnDisable()
{
    ParameterHub.Unregister("my_param");
}
```

### 3. 打开浏览器调参

*   **Editor**：直接在本机浏览器输入 `http://localhost:8089`
*   **Android APK**：查看 Console 日志中的调参地址，如 `http://192.168.1.100:8089`

---

## 文件结构

```
Assets/Plugins/GameTuner/
└── Runtime/
    ├── GameTuner.asmdef        # 独立程序集，autoReferenced=true
    ├── ParameterEntry.cs       # 参数数据模型
    ├── ParameterHub.cs         # 线程安全参数注册中心（静态类）
    ├── GameTunerServer.cs      # 独立 HTTP 服务器（端口 8089）+ 内嵌调参页面
    └── TunerInit.cs            # 场景入口 MonoBehaviour
```

---

## 环境说明

| 环境 | 访问地址 | 说明 |
|------|------|------|
| Unity Editor | `http://localhost:8089` | 直接在本机浏览器访问 |
| Android APK | `http://[设备IP]:8089` | 需与设备在同一 WiFi |

仅在 `UNITY_EDITOR` 或 `DEVELOPMENT_BUILD` 下启动 HTTP 服务，正式包体积不受影响。

> Console 启动日志示例：`[GameTuner] 调参地址: http://192.168.1.100:8089`

---

## 已接入参数（篮球模块）

| 参数 ID | 显示名 | 范围 | 对应字段 |
|------|------|------|------|
| `basketball_player_recovery_speed` | 回防速度倍率 | 0.5 ~ 2.5 | `BasketballPlayerCharacterConfig.defenseRecoveryTowardSpeedScale` |
| `basketball_player_lateral_speed` | 横移速度 | 1 ~ 8 | `Role.arcMoveSpeed` |

> 桥接脚本：`Assets/CoreGameScript/Basketball_Script/Runtime/BasketballTunerBridge.cs`

---

## 扩展新参数

1. 在对应游戏模块的桥接脚本中调用 `ParameterHub.Register()`
2. 参数 ID 统一采用 `{game}_{module}_{name}` 格式，如 `basketball_ai_skill_level`
3. 新增参数无需修改插件本体，对现有参数无影响

---

## 注意事项

- 插件代码包含 `#if UNITY_EDITOR || DEVELOPMENT_BUILD` 保护，不会出现在正式包中
- HTTP 服务端口默认 8089，与项目现有 HttpServer（8088）互不干扰
- `ParameterHub` 是线程安全的静态类，HTTP 线程和主线程均可安全调用
- 确保 Android 设备 WiFi 权限已在 `AndroidManifest.xml` 中开启
