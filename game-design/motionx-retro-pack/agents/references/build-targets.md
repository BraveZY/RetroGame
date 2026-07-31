# 构建与验证目标

## 已确认的平台事实

- 当前 Active Build Target：`StandaloneOSX`（来自现有手机平台设置任务）。
- 已配置/计划的移动端目标：Android 与 iOS；Android/iOS application identifier 均为 `com.kinhank.motionxretro`。
- Android 已记录的开发基线：IL2CPP、Minimum API 29、Target API 34、Target Architectures `3`；`android-dev` Profile 指向 `Builds/Android/MotionX_Retro_Pack.apk`。
- iOS 已记录的开发基线：IL2CPP、Device SDK；`ios-xcode-dev` Profile 指向 `Builds/iOS/MotionX_Retro_Pack`。
- 当前 Build Settings 启用场景：`Assets/Scenes/SampleScene.unity`。

## 当前可用验证证据

按当前任务记录，已完成或可复核的路径：

1. UnityMCP/UnityCaptain 回读 Player Settings、Build Profile、目标状态与启用场景。
2. 脚本编译状态：现有任务记录为无错误。
3. Unity Console Error：现有任务记录为 0 entries。
4. Android build request dry-run：现有任务记录为通过，但提示需要切换目标平台。

尚未在仓库中确认专用的 EditMode/PlayMode 测试目录、测试 assembly、CI 配置、batchmode 命令或自定义构建脚本。不得把常见 Unity 命令写为本项目已经可用的命令。

## 构建与设备验证边界

- Android 真实构建需要先切换到 Android target，可能触发导入；当前尚未作为完成证据。
- iOS Xcode 导出及后续签名仍待验证。
- Android 正式 AAB/keystore、iOS Apple Team/Provisioning Profile、横竖屏、目标设备范围、图形 API、纹理压缩、帧率和真机性能均待产品/发布负责人确认。
- Unity Hub license、对应 Platform Build Support、设备和签名材料可能成为外部阻塞；验证失败时应交由 `game-qa-debug` 按编译、设置、依赖、签名、平台模块和设备记录分类。

## UniMcp 集成状态

- 旧 `com.njljh.unitymcp`、项目设置和客户端连接配置已于 2026-07-30 移除。
- 后续将以新的方式集成 UniMcp；在新插件完成安装、编译和实际连接验证前，本项目没有可用的 UniMcp 验证入口。
