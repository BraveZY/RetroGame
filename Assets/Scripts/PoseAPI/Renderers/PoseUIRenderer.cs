using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace PoseAI
{
    /// <summary>
    /// PoseAPI 标准化 20 点骨架的 UI 渲染器。
    ///
    /// 职责：
    /// - 只使用项目统一的 20 点索引，不读取 MediaPipe 33 点数组。
    /// - 在 Canvas 上绘制核心关节和连线，并在低置信度时隐藏它们。
    /// - 保持输入坐标为左上原点的 0..1，避免重复镜像或坐标翻转。
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
        
        [Header("样式设置")]
        [Tooltip("关键点大小（像素）")]
        public float landmarkSize = 8f;
        
        [Tooltip("骨架线条宽度（像素）")]
        public float lineWidth = 3f;
        
        [Header("颜色设置")]
        public Color landmarkColor = Color.yellow;
        public Color landmarkColor2 = Color.cyan;
        public Color skeletonColor = Color.green;
        public Color skeletonColor2 = Color.cyan;
        [HideInInspector]
        public bool useFullScreen = true;
        
        [HideInInspector]
        public float displayWidth = 800f;
        
        [HideInInspector]
        public float displayHeight = 600f;

        [HideInInspector]
        public float sourceAspectRatio = 1.7778f;

        // 标准 20 点骨架连接；不连接 Mac COCO 近似生成的手和脚，避免零长度线条。
        private static readonly int[][] SKELETON_CONNECTIONS = new int[][]
        {
            // 躯干核心连接
            new int[] {(int)PoseJoint20Index.ShoulderLeft, (int)PoseJoint20Index.ShoulderRight},
            new int[] {(int)PoseJoint20Index.ShoulderLeft, (int)PoseJoint20Index.HipLeft},
            new int[] {(int)PoseJoint20Index.ShoulderRight, (int)PoseJoint20Index.HipRight},
            new int[] {(int)PoseJoint20Index.HipLeft, (int)PoseJoint20Index.HipRight},
            
            // 左臂连接（到手腕）
            new int[] {(int)PoseJoint20Index.ShoulderLeft, (int)PoseJoint20Index.ElbowLeft},
            new int[] {(int)PoseJoint20Index.ElbowLeft, (int)PoseJoint20Index.WristLeft},
            
            // 右臂连接（到手腕）
            new int[] {(int)PoseJoint20Index.ShoulderRight, (int)PoseJoint20Index.ElbowRight},
            new int[] {(int)PoseJoint20Index.ElbowRight, (int)PoseJoint20Index.WristRight},
            
            // 左腿连接（到脚踝）
            new int[] {(int)PoseJoint20Index.HipLeft, (int)PoseJoint20Index.KneeLeft},
            new int[] {(int)PoseJoint20Index.KneeLeft, (int)PoseJoint20Index.AnkleLeft},
            
            // 右腿连接（到脚踝）
            new int[] {(int)PoseJoint20Index.HipRight, (int)PoseJoint20Index.KneeRight},
            new int[] {(int)PoseJoint20Index.KneeRight, (int)PoseJoint20Index.AnkleRight}
        };
        
        // 保持旧 UI 的核心显示范围：头、肩、肘、腕、髋、膝、踝。
        private static readonly int[] VISIBLE_LANDMARK_INDICES = new int[]
        {
            (int)PoseJoint20Index.Head,
            (int)PoseJoint20Index.ShoulderLeft, (int)PoseJoint20Index.ShoulderRight,
            (int)PoseJoint20Index.ElbowLeft, (int)PoseJoint20Index.ElbowRight,
            (int)PoseJoint20Index.WristLeft, (int)PoseJoint20Index.WristRight,
            (int)PoseJoint20Index.HipLeft, (int)PoseJoint20Index.HipRight,
            (int)PoseJoint20Index.KneeLeft, (int)PoseJoint20Index.KneeRight,
            (int)PoseJoint20Index.AnkleLeft, (int)PoseJoint20Index.AnkleRight
        };

        private PoseFrame20 currentPoseFrame;
        private RectTransform canvasRect;
        private RectTransform containerRect;
        
        // UI元素池 - 改为支持多套骨架
        private List<Image[]> landmarkImagesList = new List<Image[]>();
        private List<Image[]> skeletonLineImagesList = new List<Image[]>();
        private List<RectTransform[]> skeletonLineRectsList = new List<RectTransform[]>();
        
        // 最大支持骨架数量
        private const int MAX_SKELETONS = 2;
        
        private void Start()
        {
            InitializeCanvas();
            InitializeRenderers();
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
                    Image[] landmarkImages = new Image[PoseSkeleton20.JointCount];
                    for (int i = 0; i < PoseSkeleton20.JointCount; i++)
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

        }

        /// <summary>
        /// 用标准化 20 点姿态帧更新骨架 UI。
        /// </summary>
        public void UpdatePoseFrame(PoseFrame20 frame)
        {
            currentPoseFrame = frame;
            if (frame != null && frame.sourceAspectRatio > 0f)
            {
                sourceAspectRatio = frame.sourceAspectRatio;
            }
            
            if (frame != null && frame.Detected)
            {
                for (int i = 0; i < MAX_SKELETONS; i++)
                {
                    if (i < frame.skeletons.Count)
                    {
                        RenderSkeletonIndex(i, frame.skeletons[i]);
                    }
                    else
                    {
                        HideSkeletonIndex(i);
                    }
                }
            }
            else
            {
                ClearPose();
            }

        }

        private void RenderSkeletonIndex(int index, PoseSkeleton20 skeleton)
        {
            if (index >= MAX_SKELETONS) return;

            // 渲染关键点
            if (showLandmarks && index < landmarkImagesList.Count)
            {
                RenderLandmarks(skeleton, landmarkImagesList[index]);
            }

            // 渲染骨架
            if (showSkeleton && index < skeletonLineImagesList.Count && index < skeletonLineRectsList.Count)
            {
                RenderSkeleton(skeleton, skeletonLineImagesList[index], skeletonLineRectsList[index]);
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
            currentPoseFrame = null;

            // 隐藏所有骨架
            for (int i = 0; i < MAX_SKELETONS; i++)
            {
                HideSkeletonIndex(i);
            }

        }

        private void RenderLandmarks(PoseSkeleton20 skeleton, Image[] landmarkImages)
        {
            if (skeleton == null || landmarkImages == null || canvasRect == null)
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
                if (idx < landmarkImages.Length && skeleton.joints[idx].tracked &&
                    skeleton.joints[idx].confidence > visibilityThreshold)
                {
                    Vector2 screenPos = NormalizedToScreenPosition(skeleton.joints[idx].x, skeleton.joints[idx].y);
                    RectTransform rect = landmarkImages[idx].GetComponent<RectTransform>();
                    rect.anchoredPosition = screenPos;
                    landmarkImages[idx].gameObject.SetActive(true);
                }
            }
        }

        private void RenderSkeleton(PoseSkeleton20 skeleton, Image[] skeletonLineImages, RectTransform[] skeletonLineRects)
        {
            if (skeleton == null || skeletonLineImages == null || canvasRect == null)
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

            for (int i = 0; i < SKELETON_CONNECTIONS.Length && i < skeletonLineImages.Length; i++)
            {
                int startIdx = SKELETON_CONNECTIONS[i][0];
                int endIdx = SKELETON_CONNECTIONS[i][1];

                if (skeleton.joints[startIdx].tracked && skeleton.joints[endIdx].tracked)
                {
                    float visibilityThreshold = 0.3f;
                    if (skeleton.joints[startIdx].confidence > visibilityThreshold &&
                        skeleton.joints[endIdx].confidence > visibilityThreshold)
                    {
                        Vector2 startPos = NormalizedToScreenPosition(
                            skeleton.joints[startIdx].x,
                            skeleton.joints[startIdx].y
                        );
                        Vector2 endPos = NormalizedToScreenPosition(
                            skeleton.joints[endIdx].x,
                            skeleton.joints[endIdx].y
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
