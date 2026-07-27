using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using CDL.Test;

namespace CDL.Test.Editor
{
    /// <summary>
    /// TestDisplayUI 编辑器辅助工具
    /// 验证 UI 创建流程
    /// </summary>
    [CustomEditor(typeof(TestDisplayUI))]
    public class TestDisplayUIEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            TestDisplayUI displayUI = (TestDisplayUI)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("测试功能", EditorStyles.boldLabel);

            if (GUILayout.Button("自动创建UI元素"))
            {
                Undo.SetCurrentGroupName("创建测试UI元素");
                CreateUIElements(displayUI);
                Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("点击按钮将自动创建测试用UI元素并关联到脚本字段。", MessageType.Info);
        }

        private void CreateUIElements(TestDisplayUI displayUI)
        {
            // 获取Canvas
            Canvas canvas = displayUI.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                EditorUtility.DisplayDialog("错误", "未找到Canvas，请确保GameObject在Canvas下", "确定");
                Debug.LogError("TestDisplayUIEditor: 未找到Canvas");
                return;
            }

            // 清理已存在的UI元素
            Transform existingPanel = displayUI.transform.Find("TestPanel");
            if (existingPanel != null)
            {
                Undo.DestroyObjectImmediate(existingPanel.gameObject);
            }

            // 布局参数
            float startY = -50;      // 第一行起始Y
            float lineHeight = 35;    // 行高
            float lineSpacing = 5;   // 行间距
            float labelWidth = 70;   // 标签宽度
            float valueWidth = 150;  // 值宽度

            // 创建容器Panel
            GameObject panelObj = new GameObject("TestPanel");
            Undo.RegisterCreatedObjectUndo(panelObj, "创建TestPanel");
            panelObj.transform.SetParent(displayUI.transform, false);

            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 1);
            panelRect.anchorMax = new Vector2(0, 1);
            panelRect.pivot = new Vector2(0, 1);
            // Panel高度 = 标题高度(30) + 行数 * 行高 + 间距
            panelRect.sizeDelta = new Vector2(250, 160);
            panelRect.anchoredPosition = new Vector2(20, -20);

            Image panelImage = panelObj.AddComponent<Image>();
            panelImage.color = new Color(0.12f, 0.12f, 0.14f, 0.98f);

            // 创建标题
            GameObject titleObj = CreateTextElement(panelObj.transform, "Title", "测试面板", 18, Color.white);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 1);
            titleRect.anchorMax = new Vector2(0, 1);
            titleRect.pivot = new Vector2(0, 1);
            titleRect.sizeDelta = new Vector2(200, 30);
            titleRect.anchoredPosition = new Vector2(15, -15);
            Text titleText = titleObj.GetComponent<Text>();
            titleText.fontStyle = FontStyle.Bold;
            displayUI.titleText = titleText;

            // 创建状态行 (标题底部 -30 - 间距5 = -35)
            CreateFieldRow(panelObj.transform, "Status", ref displayUI.statusText, startY, labelWidth, valueWidth);

            // 创建值行 (上一行 - 行高 - 间距)
            CreateFieldRow(panelObj.transform, "Value", ref displayUI.valueText, startY - lineHeight - lineSpacing, labelWidth, valueWidth);

            EditorUtility.SetDirty(displayUI);
            Debug.Log("TestDisplayUIEditor: UI元素创建完成");
        }

        private GameObject CreateTextElement(Transform parent, string name, string text, int fontSize, Color color)
        {
            GameObject obj = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(obj, "创建文本: " + name);
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
            rect.sizeDelta = new Vector2(150, fontSize + 10);

            return obj;
        }

        private void CreateFieldRow(Transform parent, string labelName, ref Text valueTextRef, float yPos, float labelWidth, float valueWidth)
        {
            // 创建标签
            GameObject labelObj = CreateTextElement(parent, labelName + "Label", labelName + ":", 13, new Color(0.85f, 0.85f, 0.9f, 1f));
            RectTransform labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 1);
            labelRect.anchorMax = new Vector2(0, 1);
            labelRect.pivot = new Vector2(0, 0.5f);
            labelRect.sizeDelta = new Vector2(labelWidth, 26);
            labelRect.anchoredPosition = new Vector2(15, yPos);

            // 创建值背景
            GameObject valueBgObj = new GameObject(labelName + "ValueBg");
            Undo.RegisterCreatedObjectUndo(valueBgObj, "创建值背景: " + labelName);
            valueBgObj.transform.SetParent(parent, false);
            Image bgImage = valueBgObj.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.22f, 1f);

            RectTransform bgRect = valueBgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0, 1);
            bgRect.anchorMax = new Vector2(0, 1);
            bgRect.pivot = new Vector2(0, 0.5f);
            bgRect.sizeDelta = new Vector2(valueWidth, 28);
            bgRect.anchoredPosition = new Vector2(labelWidth + 10, yPos);

            // 创建值文本
            GameObject valueObj = CreateTextElement(parent, labelName + "Value", "0", 13, new Color(0.95f, 0.95f, 0.98f, 1f));
            RectTransform valueRect = valueObj.GetComponent<RectTransform>();
            valueRect.anchorMin = new Vector2(0, 1);
            valueRect.anchorMax = new Vector2(0, 1);
            valueRect.pivot = new Vector2(0, 0.5f);
            valueRect.sizeDelta = new Vector2(valueWidth - 12, 24);
            valueRect.anchoredPosition = new Vector2(labelWidth + 18, yPos);

            valueTextRef = valueObj.GetComponent<Text>();
        }
    }
}
