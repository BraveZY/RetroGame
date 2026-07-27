# CinematicCameraPro 使用说明

> 通用相机过场动画系统

本文档用于指导 `CinematicCameraPro` 的实际使用，重点覆盖：

- 单机位路径镜头如何创建和编辑
- 多相机切换如何配置和预览
- 当前版本支持什么、不支持什么

---

## 1. 当前能力概览

当前版本已经支持：

- 单相机多 Shot 播放
- 路径类型切换：`Linear / Bezier / CatmullRom`
- LookAt 目标
- Scene 视图路径点和 Bezier 手柄编辑
- 选中路径点后在 Inspector 精确输入位置、FOV 和 Bezier 手柄
- 选中路径点后预览该点对应的相机画面
- 编辑器内 `Play / Pause / Stop / Rewind`
- 控制器事件
- 内置模板
- 多相机切换 V1：`CinematicSequence + CameraTrackClip`
- 多相机 `Cut` 硬切
- 多相机片段预览
- 多相机片段内嵌路径镜头 `embeddedShot`

当前不支持：

- 录制生成镜头
- FOV 动画
- 完整时间轴窗口

---

## 2. 主要组件

### 2.1 单机位控制

- `CinematicCamera`
- `CinematicShot`
- `CinematicPath`
- `PathPoint`

职责：

- 负责一台相机如何沿路径运动
- 负责单机位的镜头段落播放和预览

### 2.2 多机位控制

- `CinematicSequence`
- `CameraTrackClip`

职责：

- 负责什么时候切到哪台相机
- 负责多机位片段排序、时间推进和相机切换

---

## 3. 快速创建

### 3.1 创建单机位镜头

Unity 菜单：

```text
GameObject/Cinematic Camera/Cinematic Camera
```

### 3.2 创建多相机序列

Unity 菜单：

```text
GameObject/Cinematic Camera/Cinematic Sequence
```

---

## 4. 单机位镜头使用

### 4.1 最短步骤

1. 创建 `Cinematic Camera`
2. 点击 `+ Add Shot`
3. 选择 `Path Type`
4. 调整 `Duration`
5. 在 Scene 里拖动路径点
6. 选中路径点后，在 Inspector 的 `Selected Anchor` 区域输入精确位置或点击 `Preview Anchor`
7. 如果是 `Bezier`，继续拖 Bezier 手柄或在 Inspector 输入手柄数值
8. 配置 `LookAt`
9. 点击 `Play` 预览

### 4.2 Shot 的作用

一个 `Shot` 表示一段单独的镜头运动。

例如：

- Shot 1：开场建立镜头
- Shot 2：角色推进镜头
- Shot 3：结尾收束镜头

### 4.3 路径类型怎么选

- `Linear`
  适合简单直线切换
- `Bezier`
  适合需要手柄精修的运镜
- `CatmullRom`
  适合快速搭建平滑轨迹

### 4.4 常用按钮

- `Play`
  播放当前相机序列
- `Stop`
  停止当前序列
- `Rewind`
  跳回起点
- `+ Add Shot`
  新增镜头段落
- `+ Add Shot from Template`
  从模板生成镜头
- `+ Add Anchor`
  新增路径点
- `Preview Anchor`
  预览当前选中路径点对应的相机画面
- `Snap To Camera`
  把当前选中路径点移动到相机当前位置
- `Auto Calculate Tangents`
  自动计算 Bezier 手柄

### 4.5 路径点精确编辑

点击 `Point` 或 Scene 视图里的黄色路径点后，Inspector 会显示 `Selected Anchor`：

- `Position`
  精确输入路径点世界坐标
- `FOV`
  设置该点的相机视场角
- `Tangent In / Tangent Out`
  `Bezier` 路径下精确输入手柄偏移
- `Preview Anchor`
  把相机预览到当前路径点画面
- `Snap To Camera`
  把路径点吸附到当前相机位置

Scene 视图中的路径点除了主点击热区外，还有 `A0 / A1` 这类屏幕按钮，黄色连线会从真实路径点指向按钮。路径点与相机图标重合时，优先点击屏幕按钮选中路径点。选中后，真实路径点旁边会出现一个偏移移动轴；拖这个偏移移动轴会移动真实路径点，避免直接拖到预览相机自己的坐标轴。`Scene Path Editing` 开启时会优先进入路径点编辑，避免相机图标抢选；需要重新选择普通场景物体时，先关闭 `Scene Path Editing`。

### 4.6 推荐工作流

推荐先粗后细：

1. 先定 Shot 数量
2. 再定每段时长
3. 再定路径类型
4. 再拖路径点
5. 最后调 Bezier 手柄和 LookAt

---

## 5. 多相机切换使用

### 5.1 核心概念

- `CinematicCamera`
  解决“这台相机怎么动”
- `CinematicSequence`
  解决“什么时候切哪台相机”
- `CameraTrackClip`
  表示时间轴上的一个镜头片段

### 5.2 最简单的多相机用法

适合只做硬切：

1. 在场景中准备多台相机
2. 创建 `Cinematic Sequence`
3. 选中一台相机
4. 在 `Cinematic Sequence` Inspector 点击 `+ Add Clip From Selected Camera`
5. 重复把所有要用的相机加入 `clips`
6. 点击 `Auto Arrange Clips`
7. 配置 `defaultCamera`
8. 点击 `Play` 预览

### 5.3 带路径镜头的多相机用法

适合某个片段切进来后，这台相机还要自己运动：

1. 先给某台相机挂 `CinematicCamera`
2. 配好它自己的 Shot 和路径
3. 把这台相机加入 `CinematicSequence`
4. 勾选 `Use Embedded Shot`
5. 点击 `Use First Shot From Source Camera`
6. 在 `Embedded Shot` 区域继续调整路径点、手柄和 LookAt
7. 点击 `Preview This Clip`

内嵌路径点同样支持选中后在 Inspector 精确输入 `Position / FOV / Tangent`，也可以点击 `Preview Point` 预览该路径点对应的源相机画面。

### 5.4 `CameraTrackClip` 关键字段

| 字段 | 说明 | 建议 |
|:-----|:-----|:-----|
| `sourceCamera` | 当前片段输出的相机 | 必填 |
| `startTime` | 片段开始时间 | 优先交给 `Auto Arrange Clips` |
| `duration` | 片段时长 | 先粗配，再微调 |
| `defaultCamera` | 停止后恢复的相机 | 建议配置 |
| `useEmbeddedShot` | 是否启用内嵌路径镜头 | 有运动需求时开启 |
| `embeddedShot` | 片段内的路径镜头 | 用于推进、环绕、特写 |

### 5.5 多相机常用按钮

- `+ Add Clip From Selected Camera`
  从当前选中相机智能添加片段
- `Auto Arrange Clips`
  自动重排片段时间
- `▶`
  预览某个片段
- `Preview This Clip`
  预览当前选中片段
- `Use First Shot From Source Camera`
  复制源相机上的第一个 Shot 到内嵌镜头

### 5.6 推荐工作流

推荐按这个顺序做：

1. 先定相机顺序
2. 再排片段时长
3. 再决定哪些片段需要 `embeddedShot`
4. 再去细调路径点
5. 最后再绑事件和业务逻辑

### 5.7 篮球项目示例

```text
Clip 1: WideCam      2.0s   建立镜头
Clip 2: MainCam      3.0s   主镜头推进
Clip 3: CloseUpCam   1.5s   球员特写
Clip 4: MainCam      2.0s   回到主镜头
```

如果 `MainCam` 上挂了 `CinematicCamera`：

- `Clip 2` 开启 `Use Embedded Shot`
- 用路径镜头完成推进或环绕
- 其它片段只做静态切换

---

## 6. 事件使用

### 6.1 `CinematicCamera` 事件

- `onPlay`
- `onPause`
- `onStop`
- `onComplete`
- `onShotStart`

### 6.2 `CinematicSequence` 事件

- `onPlay`
- `onPause`
- `onStop`
- `onComplete`
- `onCameraSwitch`

推荐用途：

- 播放 UI 动画
- 切换 HUD
- 触发音效
- 通知业务状态切换

---

## 7. 当前实现边界

使用时请按下面理解：

- 多相机当前是 V1
- 实际稳定支持的是 `Cut`
- 没有完整时间轴窗口
- 没有录制功能
- 没有 FOV 动画
- `embeddedShot` 已可编辑，但仍属于轻量工作流，不是完整时间轴编辑器

---

## 8. 推荐实践

- 一个片段只表达一个镜头意图
- 主流程优先用少量机位，先把节奏做顺
- 先让 `Cut` 稳定，再考虑后续切换特效
- 只有需要运动的片段才开启 `embeddedShot`
- 停止多相机序列时尽量恢复 `defaultCamera`
- 先做可用版本，再做精修版本

---

## 9. 对应脚本

- `Runtime/Core/CinematicCamera.cs`
- `Runtime/Core/CinematicShot.cs`
- `Runtime/Core/CinematicPath.cs`
- `Runtime/Core/PathPoint.cs`
- `Runtime/Core/CinematicSequence.cs`
- `Runtime/Core/CameraTrackClip.cs`
- `Editor/CinematicCameraEditor.cs`
- `Editor/CinematicSequenceEditor.cs`
- `Editor/CinematicMenuItems.cs`
