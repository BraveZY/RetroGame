using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 把骨骼点和连线绘制到 UGUI 画布上的组件。
///
/// 职责：
/// - 读取 SkeletonCenter 的多人骨骼数据并决定显示哪些人。
/// - 把每个关节点换算成 UGUI anchoredPosition。
/// - 用 Image 生成圆点和线段，替换旧的 NGUI/UITexture 方案。
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class SkeletonRenderer1 : MonoBehaviour
{
    public string lineLayer = "UI";
    public float lineWidth = 0.1f;
    public int lineRenderQueue = 5002;
    public string pointLayer = "UI";
    public int pointWidth = 32;
    public Texture2D pointTexture;
    public RenderMode renderMode;
    public int SpecificIndex;

    RectTransform rootRectTransform;
    Sprite pointSprite;
    Texture2D generatedPointTexture;
    List<Skeleton> pool = new List<Skeleton>();
    List<Skeleton> skeletonList = new List<Skeleton>();

    void Awake()
    {
        rootRectTransform = transform as RectTransform;
    }

    void OnDestroy()
    {
        if (pointSprite != null)
            Destroy(pointSprite);

        if (generatedPointTexture != null)
            Destroy(generatedPointTexture);
    }

    void Update()
    {
        if (rootRectTransform == null || SkeletonCenter.Instance.Human == null)
            return;

        if (SkeletonCenter.Instance.Human.skeletonNum <= 0)
        {
            DespawnAll();
            return;
        }

        switch (renderMode)
        {
            case RenderMode.All:
                while (skeletonList.Count > SkeletonCenter.Instance.Human.skeletonNum)
                    Despawn();

                while (skeletonList.Count < SkeletonCenter.Instance.Human.skeletonNum)
                    Spawn();

                for (int i = 0; i < SkeletonCenter.Instance.Human.skeletonNum; i++)
                    skeletonList[i].SetPosition(SkeletonCenter.Instance.Human.skeletons[i].points, SkeletonCenter.Instance.Human.skeletons[i].IsTracked);
                break;

            case RenderMode.Specific:
                if (SpecificIndex < 0 || SpecificIndex > SkeletonCenter.Instance.Human.skeletonNum - 1)
                {
                    DespawnAll();
                    return;
                }

                while (skeletonList.Count > 1)
                    Despawn();

                if (skeletonList.Count <= 0)
                    Spawn();

                skeletonList[0].SetPosition(SkeletonCenter.Instance.Human.skeletons[SpecificIndex].points, SkeletonCenter.Instance.Human.skeletons[SpecificIndex].IsTracked);
                break;

            case RenderMode.None:
                DespawnAll();
                break;
        }
    }

    void DespawnAll()
    {
        while (skeletonList.Count > 0)
            Despawn();
    }

    void Spawn()
    {
        Skeleton skeleton;
        if (pool.Count > 0)
        {
            skeleton = pool[0];
            pool.RemoveAt(0);
        }
        else
        {
            skeleton = new Skeleton(this);
        }

        skeleton.SetActive(true);
        skeletonList.Add(skeleton);
    }

    void Despawn()
    {
        Skeleton skeleton = skeletonList[skeletonList.Count - 1];
        skeleton.SetActive(false);
        pool.Add(skeleton);
        skeletonList.RemoveAt(skeletonList.Count - 1);
    }

    Vector2 GetRenderSize()
    {
        return new Vector2(rootRectTransform.rect.width, rootRectTransform.rect.height);
    }

    Vector2 ConvertToUiPosition(point value)
    {
        float sourceWidth = SkeletonCenter.Instance.Width > 0 ? SkeletonCenter.Instance.Width : 1920f;
        float sourceHeight = SkeletonCenter.Instance.Height > 0 ? SkeletonCenter.Instance.Height : 1080f;
        Vector2 renderSize = GetRenderSize();
        float x = (value.x - sourceWidth * 0.5f) / sourceWidth * renderSize.x;
        float y = (value.y - sourceHeight * 0.5f) / sourceHeight * renderSize.y;
        //if (!GameResManager.instance.isSingle)
        //{
        //    if (SpecificIndex == 0)
        //    {
        //        return new Vector2(x+100, y + 40);
        //    }
        //    else
        //    {
        //        return new Vector2(x-100, y + 40);
        //    }
        //}
        return new Vector2(x, y + 80);
    }

    float GetUiLineWidth()
    {
        return Mathf.Max(1f, lineWidth);
    }

    Sprite GetPointSprite()
    {
        if (pointSprite != null)
            return pointSprite;

        if (pointTexture != null)
        {
            pointSprite = Sprite.Create(pointTexture, new Rect(0f, 0f, pointTexture.width, pointTexture.height), new Vector2(0.5f, 0.5f));
            return pointSprite;
        }

        int size = Mathf.Max(4, pointWidth);
        generatedPointTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float radius = size * 0.5f;
        Vector2 center = new Vector2(radius - 0.5f, radius - 0.5f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                generatedPointTexture.SetPixel(x, y, distance <= radius ? Color.white : Color.clear);
            }
        }

        generatedPointTexture.Apply();
        pointSprite = Sprite.Create(generatedPointTexture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f));
        return pointSprite;
    }

    void ApplyLayer(GameObject target, string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        if (layer >= 0)
            target.layer = layer;
    }

    static void ResetRectTransform(RectTransform rectTransform)
    {
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.localScale = Vector3.one;
        rectTransform.localRotation = Quaternion.identity;
        rectTransform.anchoredPosition = Vector2.zero;
    }

    static Color GetPointColor(int index)
    {
        switch (index)
        {
            case 4:
            case 10:
                return Color.red;
            case 7:
            case 13:
                return Color.blue;
            default:
                return Color.yellow;
        }
    }

    class Skeleton
    {
        readonly List<Line> lineList = new List<Line>();
        readonly List<Point> pointList = new List<Point>();

        public Skeleton(SkeletonRenderer1 renderer)
        {
            for (int i = 0; i < 20; i++)
                pointList.Add(new Point(renderer, i));

            List<int[]> linkIDList = new List<int[]>()
            {
                new int[] { 3, 2 },
                new int[] { 4, 5, 6 },
                new int[] { 8, 9, 10 },
                new int[] { 12, 13, 14 },
                new int[] { 16, 17, 18 },
                new int[] { 2, 4, 12, 16, 8, 2 },
            };

            for (int i = 0; i < linkIDList.Count; i++)
            {
                for (int j = 0; j < linkIDList[i].Length - 1; j++)
                    lineList.Add(new Line(renderer, pointList[linkIDList[i][j]], pointList[linkIDList[i][j + 1]]));
            }
        }

        public void SetPosition(point[] points, bool isTracked)
        {
            if (points == null)
                return;

            int count = Mathf.Min(pointList.Count, points.Length);
            for (int i = 0; i < count; i++)
                pointList[i].SetPosition(points[i], isTracked);

            for (int i = count; i < pointList.Count; i++)
                pointList[i].SetVisible(false);

            for (int i = 0; i < lineList.Count; i++)
                lineList[i].SetPosition();
        }

        public void SetActive(bool active)
        {
            for (int i = 0; i < pointList.Count; i++)
                pointList[i].gameObject.SetActive(active);

            for (int i = 0; i < lineList.Count; i++)
                lineList[i].gameObject.SetActive(active);
        }
    }

    class Line
    {
        public readonly GameObject gameObject;

        readonly RectTransform rectTransform;
        readonly Image image;
        readonly SkeletonRenderer1 renderer;
        readonly Point p1;
        readonly Point p2;

        public Line(SkeletonRenderer1 renderer, Point p1, Point p2)
        {
            this.renderer = renderer;
            this.p1 = p1;
            this.p2 = p2;

            gameObject = new GameObject("Line", typeof(RectTransform), typeof(Image));
            renderer.ApplyLayer(gameObject, renderer.lineLayer);
            rectTransform = gameObject.GetComponent<RectTransform>();
            rectTransform.SetParent(renderer.rootRectTransform, false);
            ResetRectTransform(rectTransform);

            image = gameObject.GetComponent<Image>();
            image.color = Color.white;
            image.raycastTarget = false;
        }

        public void SetPosition()
        {
            bool visible = p1.IsVisible && p2.IsVisible;
            image.enabled = visible;
            if (!visible)
                return;

            Vector2 from = p1.Position;
            Vector2 to = p2.Position;
            Vector2 delta = to - from;
            float distance = delta.magnitude;

            rectTransform.sizeDelta = new Vector2(distance, renderer.GetUiLineWidth());
            rectTransform.anchoredPosition = (from + to) * 0.5f;
            rectTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
        }
    }

    class Point
    {
        public point p;
        public readonly GameObject gameObject;

        readonly SkeletonRenderer1 renderer;
        readonly RectTransform rectTransform;
        readonly Image image;
        readonly KalmanFilter kfX;
        readonly KalmanFilter kfY;

        public Point(SkeletonRenderer1 renderer, int index)
        {
            kfX = new KalmanFilter();
            kfY = new KalmanFilter();
            this.renderer = renderer;

            gameObject = new GameObject("Point_" + index, typeof(RectTransform), typeof(Image));
            renderer.ApplyLayer(gameObject, renderer.pointLayer);
            rectTransform = gameObject.GetComponent<RectTransform>();
            rectTransform.SetParent(renderer.rootRectTransform, false);
            ResetRectTransform(rectTransform);
            rectTransform.sizeDelta = new Vector2(renderer.pointWidth, renderer.pointWidth);

            image = gameObject.GetComponent<Image>();
            image.sprite = renderer.GetPointSprite();
            image.color = GetPointColor(index);
            image.raycastTarget = false;
        }

        public Vector2 Position => rectTransform.anchoredPosition;

        public bool IsVisible => image.enabled;

        public void SetPosition(point p, bool isTracked)
        {
            this.p = p;
            Vector2 uiPosition = renderer.ConvertToUiPosition(p);
            float smoothedX = kfX.Update(uiPosition.x);
            float smoothedY = kfY.Update(uiPosition.y);

            rectTransform.anchoredPosition = new Vector2(smoothedX, smoothedY);
            rectTransform.sizeDelta = new Vector2(renderer.pointWidth, renderer.pointWidth);
            SetVisible(isTracked && p.detect);
        }

        public void SetVisible(bool visible)
        {
            image.enabled = visible;
        }
    }

    public enum TextureAspectMode
    {
        BaseWidth,
        BaseHeight,
        FillInside,
        FitOutside,
        None,
    }

    public enum RenderMode
    {
        All,
        Specific,
        None,
    }

    public class KalmanFilter
    {
        float Q = 0.1f;
        float R = 0.1f;
        float P = 1f;
        float X;
        float K;

        public float Update(float measurement)
        {
            P += Q;
            K = P / (P + R);
            X += K * (measurement - X);
            P = (1 - K) * P;
            return X;
        }
    }
}
