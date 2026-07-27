using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// 测距工具编辑器插件
/// 功能：在Scene视图中点击地面或物体生成标记点，连接两点显示距离
/// </summary>
public class DistanceMeasureToolEditor : EditorWindow
{
    [MenuItem("Tools/测距工具")]
    static void Init()
    {
        DistanceMeasureToolEditor window = GetWindow<DistanceMeasureToolEditor>("测距工具");
        window.Show();
    }

    private List<Vector3> points = new List<Vector3>();
    private List<float> distances = new List<float>();
    private Vector3? previewPoint = null;
    private bool isMeasuring = false;
    private int selectedPointIndex = -1;
    private bool isDragging = false;
    private Color selectedMarkerColor = Color.cyan;

    [Header("标记点设置")]
    private float markerSize = 0.2f;
    private Color markerColor = Color.red;

    [Header("线段设置")]
    private float lineWidth = 2f;
    private Color lineColor = Color.yellow;

    [Header("距离显示设置")]
    private bool showDistanceText = true;
    private int fontSize = 14;
    private Color textColor = Color.white;

    [Header("射线检测设置")]
    private LayerMask raycastLayer = -1;
    private float maxRaycastDistance = 1000f;
    
    [Header("地面平面设置")]
    [Tooltip("地面平面高度（Y坐标），当没有碰撞体时使用")]
    private float groundPlaneHeight = 0f;
    
    [Tooltip("优先使用碰撞体检测，失败时使用地面平面")]
    private bool useGroundPlaneFallback = true;

    private GUIStyle labelStyle;

    void OnEnable()
    {
        #if UNITY_2019_1_OR_NEWER
        SceneView.duringSceneGui += OnSceneGUI;
        #else
        SceneView.onSceneGUIDelegate += OnSceneGUI;
        #endif
    }

    void OnDisable()
    {
        #if UNITY_2019_1_OR_NEWER
        SceneView.duringSceneGui -= OnSceneGUI;
        #else
        SceneView.onSceneGUIDelegate -= OnSceneGUI;
        #endif
    }

    void OnGUI()
    {
        if (labelStyle == null)
        {
            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = 12;
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("测距工具", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("操作说明：", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("1. 在Scene视图中左键点击地面或物体");
        EditorGUILayout.LabelField("2. 再次左键点击完成测量");
        EditorGUILayout.LabelField("3. 点击已有锚点可选中并拖拽移动");
        EditorGUILayout.LabelField("4. 右键点击取消当前测量");
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("标记点设置", EditorStyles.boldLabel);
        markerSize = EditorGUILayout.Slider("标记点大小", markerSize, 0.1f, 1f);
        markerColor = EditorGUILayout.ColorField("标记点颜色", markerColor);
        selectedMarkerColor = EditorGUILayout.ColorField("选中标记点颜色", selectedMarkerColor);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("线段设置", EditorStyles.boldLabel);
        lineWidth = EditorGUILayout.Slider("线段宽度", lineWidth, 1f, 10f);
        lineColor = EditorGUILayout.ColorField("线段颜色", lineColor);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("距离显示设置", EditorStyles.boldLabel);
        showDistanceText = EditorGUILayout.Toggle("显示距离文本", showDistanceText);
        if (showDistanceText)
        {
            fontSize = EditorGUILayout.IntSlider("字体大小", fontSize, 10, 30);
            textColor = EditorGUILayout.ColorField("文本颜色", textColor);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("射线检测设置", EditorStyles.boldLabel);
        
        string[] layerNames = new string[32];
        for (int i = 0; i < 32; i++)
        {
            string layerName = LayerMask.LayerToName(i);
            layerNames[i] = string.IsNullOrEmpty(layerName) ? "Layer " + i : layerName;
        }
        raycastLayer = EditorGUILayout.MaskField("检测层", raycastLayer, layerNames);
        
        maxRaycastDistance = EditorGUILayout.FloatField("最大距离", maxRaycastDistance);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("地面平面设置", EditorStyles.boldLabel);
        useGroundPlaneFallback = EditorGUILayout.Toggle("无碰撞体时使用地面平面", useGroundPlaneFallback);
        if (useGroundPlaneFallback)
        {
            groundPlaneHeight = EditorGUILayout.FloatField("地面高度(Y)", groundPlaneHeight);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField($"当前点数: {points.Count}", labelStyle);
        EditorGUILayout.LabelField($"测量次数: {distances.Count}", labelStyle);

        EditorGUILayout.Space();
        if (GUILayout.Button("清除所有", GUILayout.Height(30)))
        {
            ClearAll();
        }
    }

    void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;
        int controlID = GUIUtility.GetControlID(FocusType.Passive);

        if (e.type == EventType.Layout)
        {
            HandleUtility.AddDefaultControl(controlID);
        }

        if (e.type == EventType.MouseDown && e.button == 0 && !e.alt && !e.control && !e.shift)
        {
            int clickedPointIndex = GetClickedPointIndex(e.mousePosition);
            
            if (clickedPointIndex >= 0)
            {
                if (isMeasuring && points.Count % 2 == 1)
                {
                    points.RemoveAt(points.Count - 1);
                    isMeasuring = false;
                    previewPoint = null;
                }
                selectedPointIndex = clickedPointIndex;
                isDragging = true;
                GUIUtility.hotControl = controlID;
                e.Use();
                sceneView.Repaint();
                Repaint();
            }
            else
            {
                Vector3 hitPoint;
                if (GetHitPoint(e.mousePosition, out hitPoint))
                {
                    if (points.Count % 2 == 0)
                    {
                        points.Add(hitPoint);
                        isMeasuring = true;
                        previewPoint = hitPoint;
                        e.Use();
                        GUIUtility.hotControl = controlID;
                    }
                    else
                    {
                        points.Add(hitPoint);
                        UpdateDistance(points.Count - 2);
                        isMeasuring = false;
                        previewPoint = null;
                        e.Use();
                        GUIUtility.hotControl = 0;
                    }
                    sceneView.Repaint();
                    Repaint();
                }
            }
        }

        if (e.type == EventType.MouseDrag && isDragging && selectedPointIndex >= 0)
        {
            Vector3 hitPoint;
            if (GetHitPoint(e.mousePosition, out hitPoint))
            {
                points[selectedPointIndex] = hitPoint;
                UpdateDistancesForPoint(selectedPointIndex);
                e.Use();
                sceneView.Repaint();
                Repaint();
            }
        }

        if (e.type == EventType.MouseUp && e.button == 0)
        {
            if (isDragging)
            {
                isDragging = false;
                GUIUtility.hotControl = 0;
                e.Use();
                sceneView.Repaint();
                Repaint();
            }
            else if (GUIUtility.hotControl == controlID)
            {
                GUIUtility.hotControl = 0;
                e.Use();
            }
        }

        if (e.type == EventType.MouseDown && e.button == 1)
        {
            if (isMeasuring && points.Count % 2 == 1)
            {
                points.RemoveAt(points.Count - 1);
                isMeasuring = false;
                previewPoint = null;
                e.Use();
                sceneView.Repaint();
                Repaint();
            }
            else if (selectedPointIndex >= 0)
            {
                selectedPointIndex = -1;
                e.Use();
                sceneView.Repaint();
                Repaint();
            }
        }

        if (e.type == EventType.MouseMove && isMeasuring && !isDragging)
        {
            Vector3 hitPoint;
            if (GetHitPoint(e.mousePosition, out hitPoint))
            {
                previewPoint = hitPoint;
                sceneView.Repaint();
            }
        }

        DrawMeasurements();
    }

    void DrawMeasurements()
    {
        if (points.Count == 0)
            return;

        if (Event.current.type != EventType.Repaint)
            return;

        Camera cam = SceneView.lastActiveSceneView != null ? SceneView.lastActiveSceneView.camera : null;

        for (int i = 0; i < points.Count; i++)
        {
            Handles.color = (i == selectedPointIndex) ? selectedMarkerColor : markerColor;
            
            if (cam != null)
            {
                Vector3 toCamera = (cam.transform.position - points[i]).normalized;
                if (toCamera.sqrMagnitude > 0.01f)
                {
                    Handles.DrawSolidDisc(points[i], toCamera, markerSize * 0.5f);
                }
            }
            else
            {
                Handles.DrawSolidDisc(points[i], Vector3.up, markerSize * 0.5f);
            }
            
            if (i == selectedPointIndex && cam != null)
            {
                Handles.color = selectedMarkerColor;
                Vector3 toCamera = (cam.transform.position - points[i]).normalized;
                if (toCamera.sqrMagnitude > 0.01f)
                {
                    Handles.DrawWireDisc(points[i], toCamera, markerSize * 1.5f);
                }
            }
        }

        if (previewPoint != null && points.Count > 0 && points.Count % 2 == 1 && !isDragging)
        {
            Handles.color = lineColor;
            Handles.DrawLine(points[points.Count - 1], (Vector3)previewPoint, lineWidth);
        }

        Handles.color = lineColor;
        int distanceIndex = 0;
        for (int i = 0; i < points.Count - 1; i += 2)
        {
            if (i + 1 < points.Count)
            {
                Handles.DrawLine(points[i], points[i + 1], lineWidth);

                if (showDistanceText && distanceIndex < distances.Count)
                {
                    Vector3 midPoint = (points[i] + points[i + 1]) / 2f;
                    GUIStyle labelStyle = new GUIStyle();
                    labelStyle.normal.textColor = textColor;
                    labelStyle.fontSize = fontSize;
                    labelStyle.alignment = TextAnchor.MiddleCenter;
                    Handles.Label(midPoint, string.Format("{0:F2}m", distances[distanceIndex]), labelStyle);
                    distanceIndex++;
                }
            }
        }
    }

    bool GetHitPoint(Vector2 mousePosition, out Vector3 hitPoint)
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxRaycastDistance, raycastLayer))
        {
            hitPoint = hit.point;
            return true;
        }

        if (useGroundPlaneFallback)
        {
            Plane groundPlane = new Plane(Vector3.up, new Vector3(0, groundPlaneHeight, 0));
            float distance;
            if (groundPlane.Raycast(ray, out distance))
            {
                hitPoint = ray.GetPoint(distance);
                return true;
            }
        }

        hitPoint = Vector3.zero;
        return false;
    }

    int GetClickedPointIndex(Vector2 mousePosition)
    {
        Camera cam = SceneView.lastActiveSceneView.camera;
        if (cam == null) return -1;

        float minDistance = float.MaxValue;
        int closestIndex = -1;
        float pickRadiusPixels = 15f;

        for (int i = 0; i < points.Count; i++)
        {
            Vector2 screenPos = HandleUtility.WorldToGUIPoint(points[i]);
            float screenDistance = Vector2.Distance(mousePosition, screenPos);
            
            if (screenDistance < pickRadiusPixels && screenDistance < minDistance)
            {
                minDistance = screenDistance;
                closestIndex = i;
            }
        }

        return closestIndex;
    }

    void UpdateDistancesForPoint(int pointIndex)
    {
        if (pointIndex < 0 || pointIndex >= points.Count) return;

        int pairIndex = pointIndex / 2;
        if (pointIndex % 2 == 0)
        {
            if (pairIndex < distances.Count && pointIndex + 1 < points.Count)
            {
                distances[pairIndex] = Vector3.Distance(points[pointIndex], points[pointIndex + 1]);
            }
        }
        else
        {
            if (pairIndex < distances.Count && pointIndex - 1 >= 0)
            {
                distances[pairIndex] = Vector3.Distance(points[pointIndex - 1], points[pointIndex]);
            }
        }
    }

    void UpdateDistance(int startIndex)
    {
        if (startIndex >= 0 && startIndex + 1 < points.Count)
        {
            int pairIndex = startIndex / 2;
            while (distances.Count <= pairIndex)
            {
                distances.Add(0f);
            }
            distances[pairIndex] = Vector3.Distance(points[startIndex], points[startIndex + 1]);
        }
    }

    void ClearAll()
    {
        points.Clear();
        distances.Clear();
        isMeasuring = false;
        previewPoint = null;
        selectedPointIndex = -1;
        isDragging = false;
        SceneView.RepaintAll();
    }
}
