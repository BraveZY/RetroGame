using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace PoseAI
{
    /// <summary>
    /// UI屏幕坐标空间的骨架渲染器
    /// 在Canvas上绘制骨架线条和关键点
    /// </summary>
    public class PoseUIRenderer : MonoBehaviour
    {
        [Header("Canvas引用")]
        [Tooltip("目标Canvas（如果为空则自动查找）")]
        public Canvas targetCanvas;

        [Header("渲染设置")]
        [Tooltip("是否显示关键点")]
        public bool showLandmarks = true;
        
        [Tooltip("是否显示骨架连线")]
        public bool showSkeleton = true;
        
        [Tooltip("是否显示轨迹")]
        public bool showTrails = true;

        [Header("样式设置")]
        [Tooltip("关键点大小（像素）")]
        public float landmarkSize = 8f;
        
        [Tooltip("骨架线条宽度（像素）")]
        public float lineWidth = 3f;
        
        [Tooltip("轨迹线条宽度（像素）")]
        public float trailWidth = 2f;

        [Header("颜色设置")]
        public Color landmarkColor = Color.yellow;
        public Color landmarkColor2 = Color.cyan;
        public Color skeletonColor = Color.green;
        public Color skeletonColor2 = Color.cyan;
        public Color trailLColor = Color.cyan;
        public Color trailRColor = Color.magenta;

        [HideInInspector]
        public bool useFullScreen = true;
        
        [HideInInspector]
        public float displayWidth = 800f;
        
        [HideInInspector]
        public float displayHeight = 600f;

        [HideInInspector]
        public float sourceAspectRatio = 1.7778f;

        // MediaPipe骨架连接定义（仅保留需要的关键点）
        // 保留：0(nose), 11-16(肩膀到手腕), 23-28(髋部到脚踝)
        // 移除：1-10(面部其他), 17-22(手部), 29-32(脚部)
        private static readonly int[][] SKELETON_CONNECTIONS = new int[][]
        {
            // 躯干核心连接
            new int[] {11, 12},  // 左肩-右肩
            new int[] {11, 23},  // 左肩-左髋
            new int[] {12, 24},  // 右肩-右髋
            new int[] {23, 24},  // 左髋-右髋
            
            // 左臂连接（到手腕）
            new int[] {11, 13},  // 左肩-左肘
            new int[] {13, 15},  // 左肘-左腕
            
            // 右臂连接（到手腕）
            new int[] {12, 14},  // 右肩-右肘
            new int[] {14, 16},  // 右肘-右腕
            
            // 左腿连接（到脚踝）
            new int[] {23, 25},  // 左髋-左膝
            new int[] {25, 27},  // 左膝-左脚踝
            
            // 右腿连接（到脚踝）
            new int[] {24, 26},  // 右髋-右膝
            new int[] {26, 28}   // 右膝-右脚踝
        };
        
        // 需要显示的关键点索引：0(nose), 11-16(肩膀到手腕), 23-28(髋部到脚踝)
        private static readonly int[] VISIBLE_LANDMARK_INDICES = new int[]
        {
            0,   // nose
            11, 12, 13, 14, 15, 16,  // 肩膀到手腕
            23, 24, 25, 26, 27, 28   // 髋部到脚踝
        };

        private PoseInferenceResult currentPose = null;
        private RectTransform canvasRect;
        private RectTransform containerRect;
        
        // UI元素池 - 改为支持多套骨架
        private List<Image[]> landmarkImagesList = new List<Image[]>();
        private List<Image[]> skeletonLineImagesList = new List<Image[]>();
        private List<RectTransform[]> skeletonLineRectsList = new List<RectTransform[]>();
        
        // 最大支持骨架数量
        private const int MAX_SKELETONS = 2;
        
        // 轨迹渲染（使用Image组件）
        private List<Image> trailLImages = new List<Image>();
        private List<RectTransform> trailLRects = new List<RectTransform>();
        private List<Image> trailRImages = new List<Image>();
        private List<RectTransform> trailRRects = new List<RectTransform>();
        private RectTransform trailLContainer;
        private RectTransform trailRContainer;
        private PoseDataManager poseDataManager;

        private void Start()
        {
            InitializeCanvas();
            InitializeRenderers();

            // 优先从同 GameObject 获取组件引用
            if (poseDataManager == null)
            {
                poseDataManager = GetComponent<PoseDataManager>();
            }
            // 如果同 GameObject 上没有，再查找场景中的组件
            if (poseDataManager == null)
            {
                poseDataManager = FindObjectOfType<PoseDataManager>();
            }
        }

        private void InitializeCanvas()
        {
            // 查找Canvas
            if (targetCanvas == null)
            {
                targetCanvas = GetComponentInParent<Canvas>();
                if (targetCanvas == null)
                {
                    targetCanvas = FindObjectOfType<Canvas>();
                }
            }

            if (targetCanvas == null)
            {
                Debug.LogError("PoseUIRenderer: 未找到Canvas！请确保场景中存在Canvas或在Inspector中指定targetCanvas");
                return;
            }

            canvasRect = targetCanvas.GetComponent<RectTransform>();
            
            // 创建容器
            GameObject containerObj = new GameObject("PoseUIContainer");
            containerObj.transform.SetParent(canvasRect, false);
            containerRect = containerObj.AddComponent<RectTransform>();
            containerRect.anchorMin = Vector2.zero;
            containerRect.anchorMax = Vector2.one;
            containerRect.sizeDelta = Vector2.zero;
            containerRect.anchoredPosition = Vector2.zero;
        }

        private void InitializeRenderers()
        {
            if (containerRect == null)
            {
                Debug.LogError("PoseUIRenderer: containerRect未初始化");
                return;
            }

            // 创建关键点UI元素
            if (showLandmarks)
            {
                for (int s = 0; s < MAX_SKELETONS; s++)
                {
                    Image[] landmarkImages = new Image[33];
                    for (int i = 0; i < 33; i++)
                    {
                        GameObject landmarkObj = new GameObject($"Landmark_{s}_{i}");
                        landmarkObj.transform.SetParent(containerRect, false);
                        
                        Image img = landmarkObj.AddComponent<Image>();
                        img.color = s == 0 ? landmarkColor : landmarkColor2;
                        
                        RectTransform rect = landmarkObj.GetComponent<RectTransform>();
                        rect.sizeDelta = Vector2.one * landmarkSize;
                        rect.anchorMin = Vector2.zero;
                        rect.anchorMax = Vector2.zero;
                        rect.pivot = Vector2.one * 0.5f;
                        
                        // 创建圆形sprite
                        Texture2D texture = new Texture2D(64, 64);
                        Color[] colors = new Color[64 * 64];
                        float centerX = 32f;
                        float centerY = 32f;
                        float radius = 30f;
                        
                        for (int y = 0; y < 64; y++)
                        {
                            for (int x = 0; x < 64; x++)
                            {
                                float dist = Mathf.Sqrt((x - centerX) * (x - centerX) + (y - centerY) * (y - centerY));
                                colors[y * 64 + x] = dist <= radius ? Color.white : Color.clear;
                            }
                        }
                        texture.SetPixels(colors);
                        texture.Apply();
                        
                        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 64, 64), Vector2.one * 0.5f);
                        img.sprite = sprite;
                        
                        landmarkImages[i] = img;
                        landmarkObj.SetActive(false);
                    }
                    landmarkImagesList.Add(landmarkImages);
                }
            }

            // 创建骨架线条UI元素
            if (showSkeleton)
            {
                for (int s = 0; s < MAX_SKELETONS; s++)
                {
                    Image[] skeletonLineImages = new Image[SKELETON_CONNECTIONS.Length];
                    RectTransform[] skeletonLineRects = new RectTransform[SKELETON_CONNECTIONS.Length];
                    
                    for (int i = 0; i < SKELETON_CONNECTIONS.Length; i++)
                    {
                        GameObject lineObj = new GameObject($"SkeletonLine_{s}_{i}");
                        lineObj.transform.SetParent(containerRect, false);
                        
                        Image img = lineObj.AddComponent<Image>();
                        img.color = s == 0 ? skeletonColor : skeletonColor2;
                        
                        RectTransform rect = lineObj.GetComponent<RectTransform>();
                        rect.anchorMin = Vector2.zero;
                        rect.anchorMax = Vector2.zero;
                        // pivot 设置为左侧中心 (0, 0.5)，这样旋转时从起点开始延伸
                        rect.pivot = new Vector2(0f, 0.5f);
                        
                        // 创建白色sprite用于线条
                        Texture2D texture = new Texture2D(1, 1);
                        texture.SetPixel(0, 0, Color.white);
                        texture.Apply();
                        img.sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f);
                        
                        skeletonLineImages[i] = img;
                        skeletonLineRects[i] = rect;
                        lineObj.SetActive(false);
                    }
                    skeletonLineImagesList.Add(skeletonLineImages);
                    skeletonLineRectsList.Add(skeletonLineRects);
                }
            }

            // 创建轨迹容器
            if (showTrails)
            {
                // 左手腕轨迹容器
                GameObject trailLObj = new GameObject("TrailL");
                trailLObj.transform.SetParent(containerRect, false);
                trailLContainer = trailLObj.AddComponent<RectTransform>();
                trailLContainer.anchorMin = Vector2.zero;
                trailLContainer.anchorMax = Vector2.one;
                trailLContainer.sizeDelta = Vector2.zero;
                trailLContainer.anchoredPosition = Vector2.zero;

                // 右手腕轨迹容器
                GameObject trailRObj = new GameObject("TrailR");
                trailRObj.transform.SetParent(containerRect, false);
                trailRContainer = trailRObj.AddComponent<RectTransform>();
                trailRContainer.anchorMin = Vector2.zero;
                trailRContainer.anchorMax = Vector2.one;
                trailRContainer.sizeDelta = Vector2.zero;
                trailRContainer.anchoredPosition = Vector2.zero;
            }
        }

        /// <summary>
        /// 更新姿态数据并渲染
        /// </summary>
        public void UpdatePose(PoseInferenceResult result)
        {
            currentPose = result;
            
            // 优先使用 results 列表（多骨架）
            if (result != null && result.results != null && result.results.Count > 0)
            {
                // 渲染所有检测到的骨架
                for (int i = 0; i < MAX_SKELETONS; i++)
                {
                    if (i < result.results.Count)
                    {
                        var skeletonData = result.results[i];
                        RenderSkeletonIndex(i, skeletonData.landmarks);
                    }
                    else
                    {
                        // 隐藏多余的骨架UI
                        HideSkeletonIndex(i);
                    }
                }
            }
            // 回退到单骨架模式（兼容旧数据）
            else if (result != null && result.detected && result.result != null && result.result.landmarks != null)
            {
                RenderSkeletonIndex(0, result.result.landmarks);
                // 隐藏其他骨架
                for (int i = 1; i < MAX_SKELETONS; i++)
                {
                    HideSkeletonIndex(i);
                }
            }
            else
            {
                ClearPose();
                return;
            }

            // 渲染轨迹 (目前仅支持单人轨迹，或需要进一步扩展)
            if (showTrails)
            {
                RenderTrails(result);
            }
        }

        private void RenderSkeletonIndex(int index, Landmark[] landmarks)
        {
            if (index >= MAX_SKELETONS) return;

            // 渲染关键点
            if (showLandmarks && index < landmarkImagesList.Count)
            {
                RenderLandmarks(landmarks, landmarkImagesList[index]);
            }

            // 渲染骨架
            if (showSkeleton && index < skeletonLineImagesList.Count && index < skeletonLineRectsList.Count)
            {
                RenderSkeleton(landmarks, skeletonLineImagesList[index], skeletonLineRectsList[index]);
            }
        }

        private void HideSkeletonIndex(int index)
        {
            if (index >= MAX_SKELETONS) return;

            // 隐藏关键点
            if (index < landmarkImagesList.Count)
            {
                foreach (var img in landmarkImagesList[index])
                {
                    if (img != null) img.gameObject.SetActive(false);
                }
            }

            // 隐藏骨架线条
            if (index < skeletonLineImagesList.Count)
            {
                foreach (var img in skeletonLineImagesList[index])
                {
                    if (img != null) img.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// 清除渲染
        /// </summary>
        public void ClearPose()
        {
            currentPose = null;

            // 隐藏所有骨架
            for (int i = 0; i < MAX_SKELETONS; i++)
            {
                HideSkeletonIndex(i);
            }

            // 清除轨迹
            ClearTrail(trailLImages, trailLRects);
            ClearTrail(trailRImages, trailRRects);
        }

        private void RenderLandmarks(Landmark[] landmarks, Image[] landmarkImages)
        {
            if (landmarks == null || landmarkImages == null || canvasRect == null)
                return;

            float visibilityThreshold = 0.3f;
            // 先隐藏所有关键点
            for (int i = 0; i < landmarkImages.Length; i++)
            {
                landmarkImages[i].gameObject.SetActive(false);
            }
            
            // 只显示需要的关键点
            foreach (int idx in VISIBLE_LANDMARK_INDICES)
            {
                if (idx < landmarks.Length && idx < landmarkImages.Length && 
                    landmarks[idx] != null && landmarks[idx].visibility > visibilityThreshold)
                {
                    Vector2 screenPos = NormalizedToScreenPosition(landmarks[idx].x, landmarks[idx].y);
                    RectTransform rect = landmarkImages[idx].GetComponent<RectTransform>();
                    rect.anchoredPosition = screenPos;
                    landmarkImages[idx].gameObject.SetActive(true);
                }
            }
        }

        private void RenderSkeleton(Landmark[] landmarks, Image[] skeletonLineImages, RectTransform[] skeletonLineRects)
        {
            if (landmarks == null || skeletonLineImages == null || canvasRect == null)
            {
                if (skeletonLineImages != null)
                {
                    foreach (var img in skeletonLineImages)
                    {
                        if (img != null) img.gameObject.SetActive(false);
                    }
                }
                return;
            }

            if (landmarks.Length < 17)
            {
                foreach (var img in skeletonLineImages)
                {
                    if (img != null) img.gameObject.SetActive(false);
                }
                return;
            }

            for (int i = 0; i < SKELETON_CONNECTIONS.Length && i < skeletonLineImages.Length; i++)
            {
                int startIdx = SKELETON_CONNECTIONS[i][0];
                int endIdx = SKELETON_CONNECTIONS[i][1];

                if (startIdx < landmarks.Length && endIdx < landmarks.Length &&
                    landmarks[startIdx] != null && landmarks[endIdx] != null)
                {
                    float visibilityThreshold = 0.3f;
                    if (landmarks[startIdx].visibility > visibilityThreshold && 
                        landmarks[endIdx].visibility > visibilityThreshold)
                    {
                        Vector2 startPos = NormalizedToScreenPosition(
                            landmarks[startIdx].x, 
                            landmarks[startIdx].y
                        );
                        Vector2 endPos = NormalizedToScreenPosition(
                            landmarks[endIdx].x, 
                            landmarks[endIdx].y
                        );

                        DrawUILine(skeletonLineRects[i], skeletonLineImages[i], startPos, endPos, lineWidth);
                        skeletonLineImages[i].gameObject.SetActive(true);
                    }
                    else
                    {
                        skeletonLineImages[i].gameObject.SetActive(false);
                    }
                }
                else
                {
                    skeletonLineImages[i].gameObject.SetActive(false);
                }
            }
        }

        private void RenderTrails(PoseInferenceResult result)
        {
            ClearTrail(trailLImages, trailLRects);
            ClearTrail(trailRImages, trailRRects);
        }

        /// <summary>
        /// 将轨迹数据从 List<float[]> 转换为 List<Vector2>
        /// float[] 格式：[x, y] 归一化坐标
        /// </summary>
        private List<Vector2> ConvertTrailToVector2(List<float[]> trail)
        {
            List<Vector2> result = new List<Vector2>();
            if (trail == null || trail.Count == 0)
            {
                return result;
            }

            foreach (float[] point in trail)
            {
                if (point != null && point.Length >= 2)
                {
                    result.Add(new Vector2(point[0], point[1]));
                }
            }

            return result;
        }

        private void RenderTrail(List<Vector2> trail, List<Image> images, List<RectTransform> rects, RectTransform container, Color color)
        {
            if (trail == null || trail.Count < 2)
            {
                ClearTrail(images, rects);
                return;
            }

            // 确保有足够的Image对象
            int neededCount = trail.Count - 1; // 需要n-1条线段连接n个点
            while (images.Count < neededCount)
            {
                GameObject lineObj = new GameObject($"TrailLine_{images.Count}");
                lineObj.transform.SetParent(container, false);
                
                Image img = lineObj.AddComponent<Image>();
                img.color = color;
                
                RectTransform rect = lineObj.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.zero;
                // pivot 设置为左侧中心 (0, 0.5)，这样旋转时从起点开始延伸
                rect.pivot = new Vector2(0f, 0.5f);
                
                // 创建白色sprite用于线条
                Texture2D texture = new Texture2D(1, 1);
                texture.SetPixel(0, 0, Color.white);
                texture.Apply();
                img.sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f);
                
                images.Add(img);
                rects.Add(rect);
            }

            // 渲染轨迹线段
            for (int i = 0; i < neededCount; i++)
            {
                Vector2 startPos = NormalizedToScreenPosition(trail[i].x, trail[i].y);
                Vector2 endPos = NormalizedToScreenPosition(trail[i + 1].x, trail[i + 1].y);
                
                DrawUILine(rects[i], images[i], startPos, endPos, trailWidth);
                images[i].gameObject.SetActive(true);
            }

            // 隐藏多余的线段
            for (int i = neededCount; i < images.Count; i++)
            {
                images[i].gameObject.SetActive(false);
            }
        }

        private void ClearTrail(List<Image> images, List<RectTransform> rects)
        {
            foreach (var img in images)
            {
                if (img != null) img.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 将归一化坐标转换为Canvas屏幕坐标
        /// 使用统一的 CoordinateConverter 工具类
        /// </summary>
        private Vector2 NormalizedToScreenPosition(float x, float y)
        {
            if (canvasRect == null)
                return Vector2.zero;

            return CoordinateConverter.NormalizedToScreenPosition(
                x, y, canvasRect, useFullScreen, displayWidth, displayHeight, sourceAspectRatio
            );
        }

        /// <summary>
        /// 在UI上绘制线条（使用Image组件）
        /// </summary>
        private void DrawUILine(RectTransform rect, Image img, Vector2 startPos, Vector2 endPos, float width)
        {
            if (rect == null || img == null)
                return;

            Vector2 direction = endPos - startPos;
            float distance = direction.magnitude;
            
            if (distance < 0.01f)
            {
                img.gameObject.SetActive(false);
                return;
            }

            // 设置位置（起点）
            rect.anchoredPosition = startPos;
            
            // 设置旋转
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            rect.localEulerAngles = new Vector3(0, 0, angle);
            
            // 设置尺寸（长度和宽度）
            rect.sizeDelta = new Vector2(distance, width);
        }

        private void OnDestroy()
        {
            ClearPose();
        }
    }
}
