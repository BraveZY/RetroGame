using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 残影特效控制器
/// 自动适配 URP 渲染管线，修复缩放问题，并优化性能
/// </summary>
public class GhostTrailEffect : MonoBehaviour
{
    [Header("残影基础设置")]
    [Tooltip("残影材质 (通常使用透明/半透明材质，留空则自动创建)")]
    public Material ghostMaterial;

    [Tooltip("生成残影的时间间隔 (秒)")]
    [Range(0.01f, 0.2f)]
    public float spawnInterval = 0.05f;

    [Tooltip("每个残影的持续时间 (秒)")]
    [Range(0.1f, 2f)]
    public float fadeDuration = 0.5f;

    [Header("残影外观")]
    [Tooltip("残影颜色")]
    public Color ghostColor = new Color(0.5f, 0.8f, 1f, 0.5f);

    [Tooltip("残影所在的层级名称 (默认Default，可设置为Ignore Raycast等以避免干扰)")]
    public string ghostLayerName = "Default";

    [Tooltip("残影是否使用物体当前材质颜色")]
    public bool useOriginalColor = false;

    [Tooltip("残影初始缩放倍率")]
    public float initialScaleMultiplier = 1.0f;

    [Tooltip("残影消失时的缩放倍率")]
    public float endScaleMultiplier = 1.0f;

    [Tooltip("残影大小变化曲线 (X轴: 时间0-1, Y轴: 缩放比例)")]
    public AnimationCurve scaleCurve = AnimationCurve.Linear(0, 1, 1, 1);

    [Header("生成条件")]
    [Tooltip("移动速度阈值，大于此速度时才生成残影")]
    [Range(0.1f, 10f)]
    public float minSpeedThreshold = 0.5f;

    [Tooltip("是否需要按住特定按键时才生成残影")]
    public bool requireKeyPress = false;

    [Tooltip("触发残影的按键")]
    public KeyCode triggerKey = KeyCode.LeftShift;

    [Header("性能优化")]
    [Tooltip("最大同时存在的残影数量")]
    [Range(2, 50)]
    public int maxGhosts = 10;

    [Tooltip("是否使用对象池 (推荐开启)")]
    public bool useObjectPooling = true;

    // 私有变量
    private float spawnTimer = 0f;
    private Vector3 lastPosition;
    private List<GhostInstance> activeGhosts = new List<GhostInstance>();
    private Queue<GameObject> ghostPool = new Queue<GameObject>();

    // 缓存不同类型的渲染器
    private MeshRenderer meshRenderer;
    private SkinnedMeshRenderer skinnedMeshRenderer;
    private SpriteRenderer spriteRenderer;
    private bool hasMeshRenderer, hasSkinnedMeshRenderer, hasSpriteRenderer;

    // 残影容器，用于统一管理残影，避免在 Hierarchy 中散乱
    private Transform ghostContainer;

    // 残影实例类
    private class GhostInstance
    {
        public GameObject gameObject;
        public float fadeTimer;
        public Material materialInstance;
        public Mesh mesh; // 用于存储网格副本
        public Vector3 initialScale; // 记录初始缩放，避免覆盖
    }

    void Start()
    {
        // 获取组件渲染器引用
        meshRenderer = GetComponent<MeshRenderer>();
        skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        hasMeshRenderer = meshRenderer != null;
        hasSkinnedMeshRenderer = skinnedMeshRenderer != null;
        hasSpriteRenderer = spriteRenderer != null;

        // 如果没有指定残影材质，使用默认的透明材质
        if (ghostMaterial == null)
        {
            CreateDefaultMaterial();
        }
        else
        {
            // [新增] 如果用户手动指定了材质，为避免修改原资源，创建一个运行时副本
            ghostMaterial = new Material(ghostMaterial);

        }

        // [修改] 确保在预热池之前初始化容器
        InitGhostContainer();

        lastPosition = transform.position;

        // [修改] 强制预热对象池，在初始化时创建所有残影
        // 移除 if (useObjectPooling) 判断，因为现在要求必须预先创建
        for (int i = 0; i < maxGhosts; i++)
        {
            CreateGhostForPool();
        }
    }

    // 初始化残影容器
    private void InitGhostContainer()
    {
        if (ghostContainer != null) return;

        // 创建一个空的容器对象
        GameObject container = new GameObject(gameObject.name + "_GhostContainer");

        // 如果当前物体有父节点，将容器也挂在父节点下，作为兄弟节点
        if (transform.parent != null)
        {
            container.transform.SetParent(transform.parent);
        }

        // 重置容器的变换，防止受到父节点缩放等影响（位置设为0，但残影是世界坐标控制的）
        container.transform.localPosition = Vector3.zero;
        container.transform.localRotation = Quaternion.identity;
        container.transform.localScale = Vector3.one;

        ghostContainer = container.transform;
    }

    // 创建默认材质，适配 URP 和 Built-in 管线
    private void CreateDefaultMaterial()
    {
        // 尝试查找 URP Shader
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard"); // Fallback 到内置管线

        if (shader != null)
        {
            ghostMaterial = new Material(shader);

            // 设置混合模式为透明
            ghostMaterial.SetFloat("_Surface", 1); // SurfaceType: Transparent

            // [新增] 关键修复：启用透明表面类型关键字，否则 URP 可能不识别
            ghostMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            // [新增] 设置渲染类型标签，确保管线按透明物体处理
            ghostMaterial.SetOverrideTag("RenderType", "Transparent");

            // [新增] 兼容 Built-in Standard Shader 的透明模式设置
            ghostMaterial.SetFloat("_Mode", 3); // 3 = Transparent Mode
            ghostMaterial.SetFloat("_Blend", 0);   // BlendMode: Alpha
            ghostMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            ghostMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            ghostMaterial.SetInt("_ZWrite", 0); // 关闭深度写入，防止残影相互遮挡产生奇怪效果
            // 设置 ZTest 为 Less，确保残影在与实体完全重合时不被绘制（或只绘制在后面）
            // 这样当残影和角色位置一致时，优先显示角色（因为角色写入了深度）
            ghostMaterial.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Less);
            ghostMaterial.DisableKeyword("_ALPHATEST_ON");
            ghostMaterial.EnableKeyword("_ALPHABLEND_ON");
            ghostMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            // 将渲染队列设置为 2999 (Transparent - 1)
            // 这样残影会在普通透明物体之前渲染，或者我们可以用 3001 在之后渲染
            // 为了不遮挡"现有物体"（假设是实体），只要是 Transparent 队列 (3000+) 都会在 Geometry (2000) 之后渲染
            // 也就是会被实体遮挡。
            // 但如果"现有物体"也是透明的，我们需要精细控制 Queue。
            // 这里默认设置为 3000，通常透明物体不会写入深度，所以会被实体遮挡。
            ghostMaterial.renderQueue = 3000;
        }
        else
        {
            Debug.LogWarning("GhostTrailEffect: 找不到合适的 Shader，残影可能无法正常显示。");
        }
    }

    void Update()
    {
        // 计算当前速度
        float currentSpeed = Vector3.Distance(transform.position, lastPosition) / Time.deltaTime;

        // 检查是否满足生成条件
        bool shouldSpawn = currentSpeed > minSpeedThreshold;

        if (requireKeyPress)
        {
            shouldSpawn = shouldSpawn && Input.GetKey(triggerKey);
        }

        // 更新计时器
        spawnTimer += Time.deltaTime;

        // 生成残影
        if (shouldSpawn && spawnTimer >= spawnInterval)
        {
            SpawnGhost();
            spawnTimer = 0f;
        }

        // 更新所有活跃的残影
        for (int i = activeGhosts.Count - 1; i >= 0; i--)
        {
            GhostInstance ghost = activeGhosts[i];
            ghost.fadeTimer += Time.deltaTime;

            // 计算透明度 (从1到0)
            float alpha = 1f - (ghost.fadeTimer / fadeDuration);

            if (alpha <= 0f)
            {
                // 残影消失，回收回池
                FadeOutGhost(ghost, i);
            }
            else
            {
                // 更新残影透明度
                UpdateGhostAlpha(ghost, alpha);

                // 计算生命周期进度 (0: 刚生成, 1: 即将消失)
                float lifeProgress = ghost.fadeTimer / fadeDuration;

                // 计算缩放倍率
                // 使用曲线控制缩放变化，默认曲线为常数1
                float curveScale = scaleCurve.Evaluate(lifeProgress);

                // 结合初始和结束缩放倍率
                float currentScaleMultiplier = Mathf.Lerp(initialScaleMultiplier, endScaleMultiplier, lifeProgress) * curveScale;

                // 应用缩放
                ghost.gameObject.transform.localScale = ghost.initialScale * currentScaleMultiplier;
            }
        }

        // 记录上一帧位置
        lastPosition = transform.position;
    }

    // 生成一个残影
    private void SpawnGhost()
    {
        GameObject ghostObj = null;

        // [修改] 强制使用对象池逻辑
        // 优先从池中获取
        if (ghostPool.Count > 0)
        {
            ghostObj = ghostPool.Dequeue();
            ghostObj.SetActive(true);
        }
        else
        {
            // 如果池空了且已经达到最大数量限制，则复用最早的一个活跃残影
            if (activeGhosts.Count >= maxGhosts && activeGhosts.Count > 0)
            {
                GhostInstance oldGhost = activeGhosts[0];
                activeGhosts.RemoveAt(0); // 从活跃列表头部移除
                ghostObj = oldGhost.gameObject;

                // 重置该对象的状态以便复用
                // 注意：我们复用的是 GameObject，下面的逻辑会重新设置它的位置和属性
                // 但需要先从旧的 GhostInstance 中解绑（逻辑上解绑，实际上 GhostInstance 是轻量级数据类，丢弃即可）
            }
            else
            {
                // [注意] 理论上在 Start 中已经预热了 maxGhosts 数量的对象
                // 如果运行到这里，说明 activeGhosts.Count < maxGhosts 但 ghostPool 为空
                // 这可能是因为某些异常情况导致池中对象丢失，为了保险起见，这里补创建一个
                // 但在严格的"不动态创建"要求下，这里应该尽量避免
                ghostObj = CreateNewGhost();
            }
        }

        if (ghostObj == null) return;

        // 确保容器存在 (虽然 Start 中已初始化，但防止运行时被意外删除)
        if (ghostContainer == null)
        {
            InitGhostContainer();
        }

        // 设置层级 (Layer)
        int layerIndex = LayerMask.NameToLayer(ghostLayerName);
        if (layerIndex >= 0)
        {
            ghostObj.layer = layerIndex;
        }

        // 设置父节点为专门的容器
        // 注意：使用 worldPositionStays=true 确保在改变父节点时保持世界坐标不变
        if (ghostObj.transform.parent != ghostContainer)
        {
            ghostObj.transform.SetParent(ghostContainer, true);
        }

        // 设置残影位置、旋转和缩放
        ghostObj.transform.position = transform.position;
        ghostObj.transform.rotation = transform.rotation;
        ghostObj.transform.localScale = transform.localScale;

        // 查找或创建现有的 GhostInstance 数据
        // 这里每次都 new 一个 GhostInstance 可能会产生少量 GC，但为了代码清晰度暂时保留
        // 优化方向：可以考虑也池化 GhostInstance 类，或者直接在 GameObject 上挂载脚本来存储状态
        GhostInstance ghost = new GhostInstance
        {
            gameObject = ghostObj,
            fadeTimer = 0f,
            initialScale = transform.localScale // 关键：记录初始缩放
        };

        // 根据渲染器类型设置残影数据
        if (hasMeshRenderer && meshRenderer != null)
        {
            SetupMeshGhost(ghost);
        }
        else if (hasSkinnedMeshRenderer && skinnedMeshRenderer != null)
        {
            SetupSkinnedMeshGhost(ghost);
        }
        else if (hasSpriteRenderer && spriteRenderer != null)
        {
            SetupSpriteGhost(ghost);
        }

        // 设置初始透明度
        UpdateGhostAlpha(ghost, 1f);

        // 添加到活跃列表
        activeGhosts.Add(ghost);
    }

    // 为网格渲染器设置残影
    private void SetupMeshGhost(GhostInstance ghost)
    {
        MeshFilter originalMeshFilter = GetComponent<MeshFilter>();
        if (originalMeshFilter == null || originalMeshFilter.sharedMesh == null) return;

        // 获取或添加MeshFilter和MeshRenderer
        MeshFilter ghostMeshFilter = ghost.gameObject.GetComponent<MeshFilter>();
        MeshRenderer ghostMeshRenderer = ghost.gameObject.GetComponent<MeshRenderer>();

        if (ghostMeshFilter == null) ghostMeshFilter = ghost.gameObject.AddComponent<MeshFilter>();
        if (ghostMeshRenderer == null) ghostMeshRenderer = ghost.gameObject.AddComponent<MeshRenderer>();

        // 直接引用 sharedMesh，避免访问顶点数据导致 Not Readable 报错
        // 同时也提高了性能，因为对于静态网格不需要每帧复制数据
        ghostMeshFilter.sharedMesh = originalMeshFilter.sharedMesh;

        // 设置材质
        SetupGhostMaterial(ghost, meshRenderer);

        ghostMeshRenderer.material = ghost.materialInstance;
        ghostMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        ghostMeshRenderer.receiveShadows = false;
    }

    // 为蒙皮网格渲染器设置残影
    private void SetupSkinnedMeshGhost(GhostInstance ghost)
    {
        // 获取或添加MeshFilter和MeshRenderer
        MeshFilter ghostMeshFilter = ghost.gameObject.GetComponent<MeshFilter>();
        MeshRenderer ghostMeshRenderer = ghost.gameObject.GetComponent<MeshRenderer>();

        if (ghostMeshFilter == null) ghostMeshFilter = ghost.gameObject.AddComponent<MeshFilter>();
        if (ghostMeshRenderer == null) ghostMeshRenderer = ghost.gameObject.AddComponent<MeshRenderer>();

        // 为SkinnedMeshRenderer烘焙网格副本
        if (ghost.mesh == null)
        {
            ghost.mesh = new Mesh();
        }

        skinnedMeshRenderer.BakeMesh(ghost.mesh);
        ghostMeshFilter.mesh = ghost.mesh;

        // 设置材质
        SetupGhostMaterial(ghost, skinnedMeshRenderer);

        ghostMeshRenderer.material = ghost.materialInstance;
        ghostMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        ghostMeshRenderer.receiveShadows = false;
    }

    // 通用材质设置逻辑
    private void SetupGhostMaterial(GhostInstance ghost, Renderer sourceRenderer)
    {
        if (ghost.materialInstance == null)
        {
            ghost.materialInstance = new Material(ghostMaterial);
        }

        // 获取源颜色
        Color sourceColor = ghostColor;
        if (useOriginalColor)
        {
            if (sourceRenderer.sharedMaterial != null)
            {
                if (sourceRenderer.sharedMaterial.HasProperty("_BaseColor"))
                    sourceColor = sourceRenderer.sharedMaterial.GetColor("_BaseColor");
                else if (sourceRenderer.sharedMaterial.HasProperty("_Color"))
                    sourceColor = sourceRenderer.sharedMaterial.GetColor("_Color");
            }
        }

        // 应用颜色 (兼容 URP 和 Built-in)
        if (ghost.materialInstance.HasProperty("_BaseColor"))
        {
            ghost.materialInstance.SetColor("_BaseColor", sourceColor);
        }
        else if (ghost.materialInstance.HasProperty("_Color"))
        {
            ghost.materialInstance.SetColor("_Color", sourceColor);
        }
    }

    // 为Sprite渲染器设置残影
    private void SetupSpriteGhost(GhostInstance ghost)
    {
        SpriteRenderer ghostSpriteRenderer = ghost.gameObject.GetComponent<SpriteRenderer>();
        if (ghostSpriteRenderer == null) ghostSpriteRenderer = ghost.gameObject.AddComponent<SpriteRenderer>();

        ghostSpriteRenderer.sprite = spriteRenderer.sprite;
        ghostSpriteRenderer.flipX = spriteRenderer.flipX;
        ghostSpriteRenderer.flipY = spriteRenderer.flipY;
        ghostSpriteRenderer.sortingOrder = spriteRenderer.sortingOrder - 1; // 确保在原始层级之下

        // 设置颜色
        ghostSpriteRenderer.color = useOriginalColor ? spriteRenderer.color : ghostColor;
    }

    // 更新残影的透明度
    private void UpdateGhostAlpha(GhostInstance ghost, float alpha)
    {
        if (hasSpriteRenderer)
        {
            SpriteRenderer sr = ghost.gameObject.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color color = sr.color;
                color.a = alpha;
                sr.color = color;
            }
        }
        else if (ghost.materialInstance != null)
        {
            Color color = Color.white;
            bool colorFound = false;

            // 获取当前颜色
            if (ghost.materialInstance.HasProperty("_BaseColor"))
            {
                color = ghost.materialInstance.GetColor("_BaseColor");
                colorFound = true;
            }
            else if (ghost.materialInstance.HasProperty("_Color"))
            {
                color = ghost.materialInstance.GetColor("_Color");
                colorFound = true;
            }

            if (colorFound)
            {
                color.a = alpha;
                // 设置回材质
                if (ghost.materialInstance.HasProperty("_BaseColor"))
                    ghost.materialInstance.SetColor("_BaseColor", color);
                else if (ghost.materialInstance.HasProperty("_Color"))
                    ghost.materialInstance.SetColor("_Color", color);
            }
        }
    }

    // 残影回收逻辑
    private void FadeOutGhost(GhostInstance ghost, int index)
    {
        if (useObjectPooling)
        {
            // 回收到对象池
            ghost.gameObject.SetActive(false);
            ghostPool.Enqueue(ghost.gameObject);
        }
        else
        {
            // 直接销毁
            if (ghost.materialInstance != null)
            {
                Destroy(ghost.materialInstance);
            }
            Destroy(ghost.gameObject);
        }

        // 从活跃列表移除
        activeGhosts.RemoveAt(index);
    }

    // 创建新的残影对象
    private GameObject CreateNewGhost()
    {
        GameObject ghostObj = new GameObject("Ghost");

        // [新增] 确保创建时直接挂在容器下，保持 Hierarchy 整洁
        if (ghostContainer != null)
        {
            ghostObj.transform.SetParent(ghostContainer);
        }

        // 添加必要组件
        if (hasMeshRenderer || hasSkinnedMeshRenderer)
        {
            ghostObj.AddComponent<MeshFilter>();
            ghostObj.AddComponent<MeshRenderer>();
        }
        else if (hasSpriteRenderer)
        {
            ghostObj.AddComponent<SpriteRenderer>();
        }

        return ghostObj;
    }

    // 为对象池创建残影对象
    private void CreateGhostForPool()
    {
        GameObject ghostObj = CreateNewGhost();
        ghostObj.SetActive(false);
        ghostPool.Enqueue(ghostObj);
    }

    // 清除所有残影
    public void ClearAllGhosts()
    {
        for (int i = activeGhosts.Count - 1; i >= 0; i--)
        {
            GhostInstance ghost = activeGhosts[i];

            if (useObjectPooling)
            {
                ghost.gameObject.SetActive(false);
                ghostPool.Enqueue(ghost.gameObject);
            }
            else
            {
                if (ghost.materialInstance != null)
                {
                    Destroy(ghost.materialInstance);
                }
                Destroy(ghost.gameObject);
            }
        }

        activeGhosts.Clear();
    }

    // 手动触发残影效果
    public void TriggerGhostEffect(int count = 5, float interval = 0.05f)
    {
        StartCoroutine(SpawnGhostSequence(count, interval));
    }

    // 生成一系列残影的协程
    private System.Collections.IEnumerator SpawnGhostSequence(int count, float interval)
    {
        for (int i = 0; i < count; i++)
        {
            SpawnGhost();
            yield return new WaitForSeconds(interval);
        }
    }

    // 禁用组件时清除残影
    private void OnDisable()
    {
        ClearAllGhosts();
    }

    // 销毁组件时清除残影
    private void OnDestroy()
    {
        ClearAllGhosts();

        // 清理对象池
        while (ghostPool.Count > 0)
        {
            GameObject obj = ghostPool.Dequeue();
            if (obj != null)
            {
                Destroy(obj);
            }
        }
    }
}
