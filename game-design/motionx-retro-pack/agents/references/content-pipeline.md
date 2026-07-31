# 内容与资源管线

## 已确认的 Unity 内容结构

| 类型 | 当前路径 | 现状 |
| --- | --- | --- |
| 场景 | `Assets/Scenes/` | 已发现 `SampleScene.unity`。 |
| 渲染设置 | `Assets/Settings/`、`Assets/UniversalRenderPipelineGlobalSettings.asset` | URP 模板/配置资产。 |
| 教程内容 | `Assets/TutorialInfo/` | Unity 模板教程资源和脚本。 |

当前未发现独立的 `Assets/Art/`、`Assets/Audio/`、`Assets/Animations/`、`Assets/Prefabs/`、`Assets/Resources/`、`Assets/StreamingAssets/`、`ScriptableObject` 或 Addressables 内容目录。它们不能被假定为既定生产路径。

## 产品资料与生产边界

- 视觉、音频与交互方向的资料位于 `Retro_Games_plans/docs/`，包括音频方案、交互原型、风格概念和小游戏 GDD；它们是设计意图来源，不等同于已导入 Unity 的生产资源。
- 当前视觉基调以产品文档为准：明亮童年生活 2D 手绘插画、轻 3D/2.5D 空间感、现代清晰 UI；禁止直接使用真实老歌、影视、街机/游戏 IP 或可识别品牌。
- 目前未发现统一的占位、原型、成品资源命名、导入设置、Atlas/Addressables 分组规则。新增规则、目录或资源包前须确认，不能将猜测写成项目规范。

## 修改规则与风险

- 本次只记录约定，不得移动/重命名现有资源，也不得将模板资源误作生产资源。
- Scene 与 Prefab（如后续引入）属于协作热点：只在明确任务范围内改动，改前检查引用，改后在 Unity Editor 或可用测试路径中验证。
- 可以创建占位资源、是否允许 Agent 修改 Scene/Prefab、以及生产资源目录的 owner 均为待确认项；在确认前，任务应优先采用文档、可逆配置或用户提供的资源。
- 移动端纹理压缩、URP 质量档、Render Scale、MSAA、阴影和后处理尚未完成真机验证，不应沿用桌面模板假设。
