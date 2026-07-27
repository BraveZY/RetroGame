# Animation Instancing 使用文档

## 📖 简介
Animation Instancing 插件允许你在 Unity URP 项目中高效渲染大规模人群。通过将骨骼动画烘焙到纹理中，我们绕过了 CPU 的蒙皮计算，利用 GPU Instancing 技术实现极高的渲染性能。

---

## 🛠️ 详细工作流

### 1. 烘焙动画 (Animation Baking)

**工具位置**: `Window > Animation Instancing > Baker`

#### 参数说明
- **Target Prefab**: 需要烘焙的角色预制体。必须包含 `SkinnedMeshRenderer`。
  - *注意：如果拖入不支持的物体，面板会显示红色错误提示。*
- **Output Path**: 烘焙产物的保存路径。
- **Frame Rate**: 采样帧率 (1~120 FPS)。默认为 30。
  - *提示：高帧率动作更流畅，但生成的纹理会变大。*
- **Use Half Precision**: 推荐开启。使用 `RGBAHalf` 格式存储纹理，显存占用减少 50%。
- **Auto Find Clips**: 自动从 Prefab 上的 `Animator Controller` 中提取所有动画片段。
- **Clips List**: 手动管理需要烘焙的动画列表。

#### 输出文件
烘焙完成后，会在 `Assets/BakedAnimations` 目录下生成两个文件：
1. **`{Name}_AnimTex`**: 存储骨骼矩阵的纹理文件。
2. **`{Name}_AnimData`**: `AnimationDataAsset` 资源文件，记录了每个动画片段在纹理中的位置信息。

---

### 2. 运行时设置 (Runtime Setup)

#### ⚡️ 智能一键转换 (Smart Setup)
插件提供了极简的自动化配置功能，将繁琐的手动步骤合并为**一次点击**。

1. **准备角色**：将烘焙过的角色 Prefab 拖入场景。
2. **添加组件**：移除原有的 `Animator`，添加 `InstancedAnimator` 组件。
3. **一键配置**：
   - 点击 Inspector 上的 **"Smart Convert (Auto Setup)"** 按钮。
   - 插件会自动执行以下操作：
     - ✅ 自动查找并关联 `_AnimData` 和 `_AnimTex`（根据物体名称）。
     - ✅ 自动创建 GPU Instancing 材质。
     - ✅ 自动转换 MeshRenderer 组件。
     - ✅ 自动设置 Default Clip。

4. **Unlit 支持**:
   - 如果原材质使用 `Custom/CharacterUnlit`，插件会自动切换到 `AnimationInstancing/CharacterUnlit_GPUSkinning`。
   - 如果原材质使用其他 Unlit Shader，插件会自动选择 `AnimationInstancing/GPUSkinningUnlit`。
   - 适用于不需要光照计算的角色或风格化渲染。

> **提示**：为了让智能查找生效，请确保场景中的物体名称包含 Prefab 的原始名称（例如 "Robot" 或 "Robot(Clone)"），且烘焙产物未被重命名。

#### 📝 手动设置 (备选)
如果智能转换无法找到资源，你可以手动执行以下步骤：
1. 点击 "Convert" 按钮转换组件。
2. 手动将 `_AnimData` 拖入 **Data Asset** 属性。
3. 创建新材质，使用 `AnimationInstancing/GPUSkinning` Shader。
4. 将 `_AnimTex` 拖入材质，并勾选 **Enable GPU Instancing**。

---

### 3. 颜色定制 (Color Customization)

插件支持强大的颜色定制功能，允许你通过遮罩和全局配置来控制角色的外观。

#### 🎨 RGB 遮罩系统
使用一张遮罩贴图 (`Color Mask`) 的 R、G、B 三个通道来分别控制角色身上三个不同区域的颜色。
- **红色通道 (R)**: 控制区域 A 的颜色混合。
- **绿色通道 (G)**: 控制区域 B 的颜色混合。
- **蓝色通道 (B)**: 控制区域 C 的颜色混合。
- **黑色区域**: 保持原始贴图颜色。

#### ⚙️ 全局配置 (Centralized Config)
使用 `AnimationInstancingConfig` 资源来集中管理颜色设置，无需逐个修改物体。

1. **创建配置**: 右键 `Create > Animation Instancing > Config`。
2. **配置参数**:
   - **Color Mask**: 指定 RGB 遮罩贴图。
   - **Channel R/G/B**: 为每个通道开启随机化 (`Randomize`) 并设置颜色池 (`Random Colors`)。

#### 🧠 智能管理器 (Manager & Mapping)
`AnimationInstancingManager` 单例负责在运行时自动分发配置。

1. **创建管理器**: 在场景中创建一个空物体，挂载 `AnimationInstancingManager` 组件。
2. **配置映射 (Character Configs)**:
   - 为了解决重新烘焙后物体丢失配置的问题，我们使用**名称映射**系统。
   - 在 `Character Configs` 列表中添加规则：
     - **Key**: 角色名称关键字 (例如 "Archer")。
     - **Config**: 对应的配置文件 (例如 `ArcherConfig`)。
   - **自动匹配**: 运行时，系统会自动检查角色的 `Data Asset` 名称或 `GameObject` 名称。如果包含 "Archer"，就会自动应用 `ArcherConfig`。

---

## 💻 编程接口 (API)

`InstancedAnimator` 组件提供了简单的 API 来控制动画播放：

```csharp
using AnimationInstancing;

public class CrowdController : MonoBehaviour
{
    public InstancedAnimator character;

    void Start()
    {
        // 播放名为 "Run" 的动画
        character.Play("Run");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // 在 0.2 秒内平滑过渡到 Jump 动画
            character.CrossFade("Jump", 0.2f);
        }
    }
}
```

---

### 4. 高级特性

#### 🔄 动画混合 (Cross-fade)
支持在两个动画之间进行平滑过渡，避免动作突变。
- **API**: `animator.CrossFade(clipName, duration)`
- **预览**: 在 Inspector 中点击 **CrossFade** 按钮可直接预览过渡效果（仅限 Play Mode）。

#### 🚀 异步烘焙 (Async Baking)
烘焙过程现在完全异步化，不会卡死编辑器。
- **流畅体验**：烘焙大量动画时，编辑器保持响应。
- **进度反馈**：实时显示当前处理的 Clip 和总体进度。
- **随时取消**：点击进度条旁的 "Cancel" 按钮可立即停止烘焙。

#### 📱 性能优化建议
- **移动端**：务必开启 `Use Half Precision` (FP16)。
- **帧率**：对于远景角色，可以将烘焙 FPS 降低到 15 或 20。
- **GPU Instancing**：确保材质勾选了 `Enable GPU Instancing`，否则无法合批。

---

## 🎨 Shader 集成指南

如果你需要将 GPU Skinning 集成到自己的 Shader 中，请参考 `GPUSkinning.shader`。

核心逻辑在于顶点着色器 (Vertex Shader)：

1. **定义属性**:
   ```hlsl
   UNITY_INSTANCING_BUFFER_START(Props)
       UNITY_DEFINE_INSTANCED_PROP(float4, _AnimInfo) // x: start, y: offset, z: length
       UNITY_DEFINE_INSTANCED_PROP(float4, _InstanceColor)  // R Channel Color
       UNITY_DEFINE_INSTANCED_PROP(float4, _InstanceColorG) // G Channel Color
       UNITY_DEFINE_INSTANCED_PROP(float4, _InstanceColorB) // B Channel Color
   UNITY_INSTANCING_BUFFER_END(Props)
   ```

2. **采样矩阵**:
   使用 `GetBoneMatrix` 函数，根据 `boneIndex` 和 `frameIndex` 从 `_AnimTex` 中读取矩阵。

3. **应用蒙皮**:
   ```hlsl
   float4x4 skinMatrix = 
       GetBoneMatrix(indices.x, frame) * weights.x +
       GetBoneMatrix(indices.y, frame) * weights.y + ...;
   
   float4 positionWS = mul(skinMatrix, positionOS);
   ```

4. **应用颜色 (Fragment Shader)**:
   ```hlsl
   // 获取实例颜色
   float4 colR = UNITY_ACCESS_INSTANCED_PROP(Props, _InstanceColor);
   float4 colG = UNITY_ACCESS_INSTANCED_PROP(Props, _InstanceColorG);
   float4 colB = UNITY_ACCESS_INSTANCED_PROP(Props, _InstanceColorB);
   
   // 采样遮罩
   float3 mask = SAMPLE_TEXTURE2D(_ColorMask, sampler_ColorMask, input.uv).rgb;
   
   // 混合颜色
   baseColor.rgb *= lerp(float3(1,1,1), colR.rgb, mask.r);
   baseColor.rgb *= lerp(float3(1,1,1), colG.rgb, mask.g);
   baseColor.rgb *= lerp(float3(1,1,1), colB.rgb, mask.b);
   ```

---

## ❓ 常见问题 (FAQ)

**Q: 为什么角色变成了静态的？**
A: 请检查材质是否勾选了 **Enable GPU Instancing**，以及 `InstancedAnimator` 是否正确指定了 `Data Asset`。

**Q: 动画看起来有锯齿或抖动？**
A: 确保 `_AnimTex` 的 Filter Mode 设置为 **Point (no filter)**。烘焙工具会自动设置，但如果手动修改过可能会导致此问题。

**Q: 移动端支持吗？**
A: 支持。请确保开启 **Use Half Precision** 以节省显存和带宽。

**Q: 最大支持多少骨骼？**
A: 取决于纹理宽度限制。通常建议不超过 256 根骨骼。
