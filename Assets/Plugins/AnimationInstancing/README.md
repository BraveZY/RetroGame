# Animation Instancing for Unity URP

**Animation Instancing** 是一个高性能的大规模人群渲染解决方案，专为 Unity URP (Universal Render Pipeline) 设计。

它通过 **Vertex Animation Texture (VAT)** 技术，将骨骼动画的计算开销从 CPU 转移到 GPU，从而实现使用 GPU Instancing 渲染数千个带有独立动画的动态角色。

## ✨ 核心特性

- **高性能渲染**：支持 10,000+ 动态角色同屏，DrawCall 极低。
- **零 CPU 开销**：运行时无需计算骨骼矩阵，完全由 GPU 顶点着色器处理。
- **极致易用**：
  - 🎨 **一键烘焙**：自动从 Animator 提取动画并生成纹理。
  - ⚡️ **智能配置**：一键自动关联数据、创建材质并转换组件，零手动操作。
  - 🚀 **异步烘焙**：多线程分帧处理，海量动画烘焙不卡顿。
  - 🔄 **平滑过渡**：支持 Cross-fade 动画混合，动作切换更自然。
  - 🌑 **Unlit 支持**：内置 Unlit Shader，支持风格化渲染和 CharacterUnlit 自动适配。
  - ⏱️ **自定义采样**：支持 1~120 FPS 灵活采样，平衡流畅度与显存。
  - 🛡️ **智能验证**：自动检测 Prefab 有效性，防止错误操作。
  - 🔄 **一键转换**：自动将 SkinnedMeshRenderer 转换为高性能 Instanced Mesh。
  - 📱 **移动端优化**：支持 RGBAHalf 精度，减少 50% 显存占用。
- **🎨 颜色定制**：
  - **RGB 遮罩**：使用纹理的 R、G、B 通道分别控制角色不同区域的颜色。
  - **全局配置**：通过 ScriptableObject 集中管理颜色方案，支持多角色自动映射。
  - **随机化**：支持为每个通道配置独立的随机颜色池。
- **URP 原生支持**：基于 URP Shader Library 编写，完美兼容 SRP Batcher 和 GPU Instancing。

## 🚀 快速开始

1. **打开烘焙工具**：菜单栏 `Window > Animation Instancing > Baker`。
2. **烘焙动画**：
   - 拖入你的角色 Prefab。
   - 点击 **"Auto Find Clips"** 自动获取动画片段。
   - 点击 **"Bake Animation"** 生成 `.asset` 数据文件。
3. **设置场景角色**：
   - 在场景中选中角色。
   - 添加 `InstancedAnimator` 组件。
   - 点击 Inspector 上的 **"Convert from SkinnedMeshRenderer"** 按钮。
   - 将烘焙生成的 `_AnimData` 文件拖入 **Data Asset** 属性。
4. **运行**：点击 Play，享受海量人群渲染！

## 📦 目录结构

```text
Assets/Plugins/AnimationInstancing/
├── Editor/          # 编辑器工具 (Baker, Inspector)
├── Runtime/         # 运行时组件 (InstancedAnimator, DataAsset)
├── Shaders/         # URP Shader (GPUSkinning)
├── README.md        # 项目简介
└── Documentation.md # 详细使用文档
```

## ⚠️ 注意事项

- **骨骼限制**：单个模型的骨骼数量建议控制在 **256** 以内（受限于纹理宽度）。
- **材质设置**：确保材质勾选了 **Enable GPU Instancing**。
- **Shader**：目前仅内置了 URP Lit Shader 的修改版，如需自定义 Shader 请参考 `GPUSkinning.shader` 中的 `vert` 函数。
