using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using PoseAI;

namespace PoseAI.Editor
{
    /// <summary>
    /// InferenceResultDisplayUI编辑器辅助工具
    /// 用于自动创建UI显示元素
    /// </summary>
    [CustomEditor(typeof(InferenceResultDisplayUI))]
    public class InferenceResultDisplayUIEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            InferenceResultDisplayUI displayUI = (InferenceResultDisplayUI)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("快速设置", EditorStyles.boldLabel);

            if (GUILayout.Button("自动创建UI元素"))
            {
                Undo.SetCurrentGroupName("创建推理结果UI元素");
                CreateUIElements(displayUI);
                Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("点击按钮将自动创建所有UI元素并关联到脚本字段。如果UI元素已存在，会先清理再重新创建。", MessageType.Info);
        }

        private void CreateUIElements(InferenceResultDisplayUI displayUI)
        {
            // 获取Canvas
            Canvas canvas = displayUI.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                EditorUtility.DisplayDialog("错误", "未找到Canvas，请确保GameObject在Canvas下", "确定");
                Debug.LogError("InferenceResultDisplayUIEditor: 未找到Canvas，请确保GameObject在Canvas下");
                return;
            }

            // 清理已存在的UI元素（如果存在）
            Transform existingPanel = displayUI.transform.Find("OutputPanel");
            if (existingPanel != null)
            {
                Undo.DestroyObjectImmediate(existingPanel.gameObject);
            }

            // 创建容器Panel（现代化深色背景）
            GameObject panelObj = new GameObject("OutputPanel");
            Undo.RegisterCreatedObjectUndo(panelObj, "创建OutputPanel");
            panelObj.transform.SetParent(displayUI.transform, false);
            
            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 1);
            panelRect.anchorMax = new Vector2(0, 1);
            panelRect.pivot = new Vector2(0, 1);
            // 宽度以容纳双人数据显示（格式：值1 | 值2）
            panelRect.sizeDelta = new Vector2(350, 460);
            panelRect.anchoredPosition = new Vector2(20, -20);

            Image panelImage = panelObj.AddComponent<Image>();
            panelImage.color = new Color(0.12f, 0.12f, 0.14f, 0.98f);

            // 添加阴影效果（使用Outline组件模拟）
            UnityEngine.UI.Outline panelOutline = panelObj.AddComponent<UnityEngine.UI.Outline>();
            panelOutline.effectColor = new Color(0, 0, 0, 0.5f);
            panelOutline.effectDistance = new Vector2(2, -2);

            // 创建标题区域背景（渐变效果模拟）
            GameObject headerBgObj = new GameObject("HeaderBackground");
            Undo.RegisterCreatedObjectUndo(headerBgObj, "创建标题背景");
            headerBgObj.transform.SetParent(panelObj.transform, false);
            RectTransform headerBgRect = headerBgObj.AddComponent<RectTransform>();
            headerBgRect.anchorMin = new Vector2(0, 1);
            headerBgRect.anchorMax = new Vector2(1, 1);
            headerBgRect.pivot = new Vector2(0, 1);
            headerBgRect.sizeDelta = new Vector2(0, 60);
            headerBgRect.anchoredPosition = new Vector2(0, 0);
            Image headerBgImage = headerBgObj.AddComponent<Image>();
            headerBgImage.color = new Color(0.18f, 0.18f, 0.22f, 1f);

            // 创建分隔线
            GameObject dividerObj = new GameObject("Divider");
            Undo.RegisterCreatedObjectUndo(dividerObj, "创建分隔线");
            dividerObj.transform.SetParent(panelObj.transform, false);
            RectTransform dividerRect = dividerObj.AddComponent<RectTransform>();
            dividerRect.anchorMin = new Vector2(0, 1);
            dividerRect.anchorMax = new Vector2(1, 1);
            dividerRect.pivot = new Vector2(0, 1);
            dividerRect.sizeDelta = new Vector2(0, 1);
            dividerRect.anchoredPosition = new Vector2(0, -60);
            Image dividerImage = dividerObj.AddComponent<Image>();
            dividerImage.color = new Color(0.35f, 0.35f, 0.4f, 0.6f);

            // 创建标题（更醒目的样式）
            GameObject titleObj = CreateTextElement(panelObj.transform, "Title", "推理结果", 22, new Color(1f, 1f, 1f, 1f));
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 1);
            titleRect.anchorMax = new Vector2(0, 1);
            titleRect.pivot = new Vector2(0, 1);
            titleRect.sizeDelta = new Vector2(250, 28);
            titleRect.anchoredPosition = new Vector2(18, -18);
            Text titleText = titleObj.GetComponent<Text>();
            titleText.fontStyle = FontStyle.Bold;
            UnityEngine.UI.Outline titleOutline = titleObj.AddComponent<UnityEngine.UI.Outline>();
            titleOutline.effectColor = new Color(0, 0, 0, 0.3f);
            titleOutline.effectDistance = new Vector2(1, -1);

            GameObject subtitleObj = CreateTextElement(panelObj.transform, "Subtitle", "Latest Inference Result", 13, new Color(0.75f, 0.75f, 0.8f, 1f));
            RectTransform subtitleRect = subtitleObj.GetComponent<RectTransform>();
            subtitleRect.anchorMin = new Vector2(0, 1);
            subtitleRect.anchorMax = new Vector2(0, 1);
            subtitleRect.pivot = new Vector2(0, 1);
            subtitleRect.sizeDelta = new Vector2(250, 18);
            subtitleRect.anchoredPosition = new Vector2(20, -42);

            // 创建字段标签和值（优化间距和样式）
            // 值区域宽度以容纳双人数据显示（格式：值1 | 值2）
            float startY = -80;
            float lineHeight = 40;
            float labelWidth = 100;
            float valueWidth = 220;

            CreateFieldRow(panelObj.transform, "Pose Label", ref displayUI.poseLabelText, startY, labelWidth, valueWidth);
            CreateFieldRow(panelObj.transform, "Confidence", ref displayUI.confidenceText, startY - lineHeight * 1, labelWidth, valueWidth);
            CreateFieldRow(panelObj.transform, "Speed", ref displayUI.speedText, startY - lineHeight * 2, labelWidth, valueWidth);
            CreateFieldRow(panelObj.transform, "State", ref displayUI.stateText, startY - lineHeight * 3, labelWidth, valueWidth);
            CreateFieldRow(panelObj.transform, "Hit Direction", ref displayUI.hitDirectionText, startY - lineHeight * 4, labelWidth, valueWidth);
            CreateFieldRow(panelObj.transform, "Hit Count", ref displayUI.hitCountText, startY - lineHeight * 5, labelWidth, valueWidth);
            CreateFieldRow(panelObj.transform, "Power", ref displayUI.powerText, startY - lineHeight * 6, labelWidth, valueWidth);
            CreateFieldRow(panelObj.transform, "Score", ref displayUI.scoreText, startY - lineHeight * 7, labelWidth, valueWidth);
            CreateFieldRow(panelObj.transform, "Event Type", ref displayUI.eventTypeText, startY - lineHeight * 8, labelWidth, valueWidth);

            EditorUtility.SetDirty(displayUI);
            Debug.Log("InferenceResultDisplayUIEditor: UI元素创建完成");
        }

        private GameObject CreateTextElement(Transform parent, string name, string text, int fontSize, Color color)
        {
            GameObject obj = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(obj, "创建UI文本: " + name);
            obj.transform.SetParent(parent, false);

            Text textComponent = obj.AddComponent<Text>();
            textComponent.text = text;
            textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            textComponent.fontSize = fontSize;
            textComponent.color = color;
            textComponent.alignment = TextAnchor.MiddleLeft;
            textComponent.horizontalOverflow = HorizontalWrapMode.Overflow;
            textComponent.verticalOverflow = VerticalWrapMode.Truncate;

            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(200, fontSize + 10);

            return obj;
        }

        private void CreateFieldRow(Transform parent, string labelName, ref Text valueTextRef, float yPos, float labelWidth, float valueWidth)
        {
            // 创建标签（优化样式）
            GameObject labelObj = CreateTextElement(parent, labelName + "Label", labelName + ":", 13, new Color(0.85f, 0.85f, 0.9f, 1f));
            RectTransform labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 1);
            labelRect.anchorMax = new Vector2(0, 1);
            labelRect.pivot = new Vector2(0, 0.5f);
            labelRect.sizeDelta = new Vector2(labelWidth, 26);
            labelRect.anchoredPosition = new Vector2(18, yPos);
            Text labelText = labelObj.GetComponent<Text>();
            labelText.fontStyle = FontStyle.Normal;

            // 创建值输入框背景（现代化深色输入框样式，带边框效果）
            GameObject valueBgObj = new GameObject(labelName + "ValueBg");
            Undo.RegisterCreatedObjectUndo(valueBgObj, "创建值背景: " + labelName);
            valueBgObj.transform.SetParent(parent, false);
            Image bgImage = valueBgObj.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.22f, 1f);

            RectTransform bgRect = valueBgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0, 1);
            bgRect.anchorMax = new Vector2(0, 1);
            bgRect.pivot = new Vector2(0, 0.5f);
            bgRect.sizeDelta = new Vector2(valueWidth, 30);
            bgRect.anchoredPosition = new Vector2(labelWidth + 12, yPos);

            // 添加边框高光效果（使用Outline组件）
            UnityEngine.UI.Outline bgOutline = valueBgObj.AddComponent<UnityEngine.UI.Outline>();
            bgOutline.effectColor = new Color(0.4f, 0.4f, 0.45f, 0.3f);
            bgOutline.effectDistance = new Vector2(1, -1);

            // 创建值文本（优化颜色和样式）
            GameObject valueObj = CreateTextElement(parent, labelName + "Value", "0", 13, new Color(0.95f, 0.95f, 0.98f, 1f));
            RectTransform valueRect = valueObj.GetComponent<RectTransform>();
            valueRect.anchorMin = new Vector2(0, 1);
            valueRect.anchorMax = new Vector2(0, 1);
            valueRect.pivot = new Vector2(0, 0.5f);
            valueRect.sizeDelta = new Vector2(valueWidth - 16, 26);
            valueRect.anchoredPosition = new Vector2(labelWidth + 20, yPos);
            
            // 设置文本对齐方式和样式
            Text valueText = valueObj.GetComponent<Text>();
            valueText.alignment = TextAnchor.MiddleLeft;
            valueText.fontStyle = FontStyle.Normal;
            valueText.horizontalOverflow = HorizontalWrapMode.Overflow;

            valueTextRef = valueObj.GetComponent<Text>();
        }
    }
}

