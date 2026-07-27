using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using PoseAI;

namespace PoseAI.Editor
{
    /// <summary>
    /// PoseSmootherUI编辑器辅助工具
    /// 用于自动创建UI控制元素
    /// </summary>
    [CustomEditor(typeof(PoseSmootherUI))]
    public class PoseSmootherUIEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            PoseSmootherUI smootherUI = (PoseSmootherUI)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("快速设置", EditorStyles.boldLabel);

            if (GUILayout.Button("自动创建UI元素"))
            {
                Undo.SetCurrentGroupName("创建平滑器UI元素");
                CreateUIElements(smootherUI);
                Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("点击按钮将自动创建所有UI元素并关联到脚本字段。如果UI元素已存在，会先清理再重新创建。", MessageType.Info);
        }

        private void CreateUIElements(PoseSmootherUI smootherUI)
        {
            // 获取Canvas
            Canvas canvas = smootherUI.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                EditorUtility.DisplayDialog("错误", "未找到Canvas，请确保GameObject在Canvas下", "确定");
                Debug.LogError("PoseSmootherUIEditor: 未找到Canvas，请确保GameObject在Canvas下");
                return;
            }

            // 清理已存在的UI元素（如果存在）
            Transform existingPanel = smootherUI.transform.Find("SmootherPanel");
            if (existingPanel != null)
            {
                Undo.DestroyObjectImmediate(existingPanel.gameObject);
            }

            // 创建容器Panel
            GameObject panelObj = new GameObject("SmootherPanel");
            Undo.RegisterCreatedObjectUndo(panelObj, "创建SmootherPanel");
            panelObj.transform.SetParent(smootherUI.transform, false);
            
            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(1, 1);
            panelRect.anchorMax = new Vector2(1, 1);
            panelRect.pivot = new Vector2(1, 1);
            panelRect.sizeDelta = new Vector2(300, 320);
            panelRect.anchoredPosition = new Vector2(-20, -20);

            Image panelImage = panelObj.AddComponent<Image>();
            panelImage.color = new Color(0.12f, 0.12f, 0.14f, 0.98f);

            // 添加阴影效果
            UnityEngine.UI.Outline panelOutline = panelObj.AddComponent<UnityEngine.UI.Outline>();
            panelOutline.effectColor = new Color(0, 0, 0, 0.5f);
            panelOutline.effectDistance = new Vector2(2, -2);

            // 创建标题区域背景
            GameObject headerBgObj = new GameObject("HeaderBackground");
            Undo.RegisterCreatedObjectUndo(headerBgObj, "创建标题背景");
            headerBgObj.transform.SetParent(panelObj.transform, false);
            RectTransform headerBgRect = headerBgObj.AddComponent<RectTransform>();
            headerBgRect.anchorMin = new Vector2(0, 1);
            headerBgRect.anchorMax = new Vector2(1, 1);
            headerBgRect.pivot = new Vector2(0, 1);
            headerBgRect.sizeDelta = new Vector2(0, 50);
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
            dividerRect.anchoredPosition = new Vector2(0, -50);
            Image dividerImage = dividerObj.AddComponent<Image>();
            dividerImage.color = new Color(0.35f, 0.35f, 0.4f, 0.6f);

            // 创建标题
            GameObject titleObj = CreateTextElement(panelObj.transform, "Title", "平滑参数", 20, new Color(1f, 1f, 1f, 1f));
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 1);
            titleRect.anchorMax = new Vector2(0, 1);
            titleRect.pivot = new Vector2(0, 1);
            titleRect.sizeDelta = new Vector2(200, 24);
            titleRect.anchoredPosition = new Vector2(18, -15);
            Text titleText = titleObj.GetComponent<Text>();
            titleText.fontStyle = FontStyle.Bold;
            UnityEngine.UI.Outline titleOutline = titleObj.AddComponent<UnityEngine.UI.Outline>();
            titleOutline.effectColor = new Color(0, 0, 0, 0.3f);
            titleOutline.effectDistance = new Vector2(1, -1);

            // 创建启用平滑Toggle
            float startY = -70;
            float lineHeight = 60;
            CreateToggleRow(panelObj.transform, "EnableSmoothing", "启用平滑", ref smootherUI.enableSmoothingToggle, startY);

            // 创建MinCutoff滑块行
            CreateSliderRow(panelObj.transform, "MinCutoff", "最小截止频率", 0.1f, 5.0f, ref smootherUI.minCutoffSlider, ref smootherUI.minCutoffValueText, startY - lineHeight * 1);

            // 创建Beta滑块行
            CreateSliderRow(panelObj.transform, "Beta", "速度系数", 0.001f, 0.1f, ref smootherUI.betaSlider, ref smootherUI.betaValueText, startY - lineHeight * 2);

            // 创建DCutoff滑块行
            CreateSliderRow(panelObj.transform, "DCutoff", "导数截止频率", 0.1f, 5.0f, ref smootherUI.dCutoffSlider, ref smootherUI.dCutoffValueText, startY - lineHeight * 3);

            EditorUtility.SetDirty(smootherUI);
            Debug.Log("PoseSmootherUIEditor: UI元素创建完成");
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

        private void CreateToggleRow(Transform parent, string name, string labelText, ref Toggle toggleRef, float yPos)
        {
            // 创建行背景
            GameObject rowBgObj = new GameObject(name + "RowBg");
            Undo.RegisterCreatedObjectUndo(rowBgObj, "创建行背景: " + name);
            rowBgObj.transform.SetParent(parent, false);
            Image rowBgImage = rowBgObj.AddComponent<Image>();
            rowBgImage.color = new Color(0.16f, 0.16f, 0.18f, 0.6f);

            RectTransform rowBgRect = rowBgObj.GetComponent<RectTransform>();
            rowBgRect.anchorMin = new Vector2(0, 1);
            rowBgRect.anchorMax = new Vector2(1, 1);
            rowBgRect.pivot = new Vector2(0, 1);
            rowBgRect.sizeDelta = new Vector2(0, 48);
            rowBgRect.anchoredPosition = new Vector2(0, yPos + 24);

            // 创建标签
            GameObject labelObj = CreateTextElement(parent, name + "Label", labelText + ":", 13, new Color(0.85f, 0.85f, 0.9f, 1f));
            RectTransform labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 1);
            labelRect.anchorMax = new Vector2(0, 1);
            labelRect.pivot = new Vector2(0, 0.5f);
            labelRect.sizeDelta = new Vector2(110, 26);
            labelRect.anchoredPosition = new Vector2(18, yPos);

            // 创建iOS风格开关容器
            GameObject toggleContainerObj = new GameObject(name + "ToggleContainer");
            Undo.RegisterCreatedObjectUndo(toggleContainerObj, "创建开关容器: " + name);
            toggleContainerObj.transform.SetParent(parent, false);
            RectTransform containerRect = toggleContainerObj.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0, 1);
            containerRect.anchorMax = new Vector2(0, 1);
            containerRect.pivot = new Vector2(0, 0.5f);
            containerRect.sizeDelta = new Vector2(50, 28);
            containerRect.anchoredPosition = new Vector2(130, yPos);

            // 创建Toggle背景（圆角矩形，iOS风格）
            GameObject toggleBgObj = new GameObject(name + "ToggleBg");
            Undo.RegisterCreatedObjectUndo(toggleBgObj, "创建Toggle背景: " + name);
            toggleBgObj.transform.SetParent(toggleContainerObj.transform, false);
            RectTransform bgRect = toggleBgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0, 0);
            bgRect.anchorMax = new Vector2(1, 1);
            bgRect.sizeDelta = Vector2.zero;
            bgRect.anchoredPosition = Vector2.zero;
            
            Image bgImage = toggleBgObj.AddComponent<Image>();
            bgImage.color = new Color(0.35f, 0.35f, 0.4f, 1f);
            bgImage.raycastTarget = true;

            // 创建Toggle（确保Toggle在最上层，可以接收点击）
            GameObject toggleObj = new GameObject(name + "Toggle");
            Undo.RegisterCreatedObjectUndo(toggleObj, "创建Toggle: " + name);
            toggleObj.transform.SetParent(toggleContainerObj.transform, false);
            // 确保Toggle在最后创建，这样它在层级中在最上层
            toggleObj.transform.SetAsLastSibling();
            RectTransform toggleRect = toggleObj.AddComponent<RectTransform>();
            toggleRect.anchorMin = new Vector2(0, 0);
            toggleRect.anchorMax = new Vector2(1, 1);
            toggleRect.sizeDelta = Vector2.zero;
            toggleRect.anchoredPosition = Vector2.zero;

            // 添加一个透明的Image来确保整个区域都可以点击
            Image toggleClickArea = toggleObj.AddComponent<Image>();
            toggleClickArea.color = new Color(1, 1, 1, 0);
            toggleClickArea.raycastTarget = true;

            Toggle toggle = toggleObj.AddComponent<Toggle>();
            toggle.isOn = true;
            toggle.interactable = true;
            
            // 禁用Toggle的ColorTint过渡，避免与动画脚本冲突
            ColorBlock colors = toggle.colors;
            colors.colorMultiplier = 0f;
            toggle.colors = colors;
            toggle.transition = Selectable.Transition.None;
            // 设置透明Image为targetGraphic，使整个Toggle区域都可以点击
            toggle.targetGraphic = toggleClickArea;
            // 清除graphic属性，避免干扰
            toggle.graphic = null;

            // 创建圆形滑块（Handle）
            GameObject handleObj = new GameObject("Handle");
            Undo.RegisterCreatedObjectUndo(handleObj, "创建开关滑块: " + name);
            handleObj.transform.SetParent(toggleContainerObj.transform, false);
            RectTransform handleRect = handleObj.AddComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(22, 22);
            // 初始位置：开启状态在右侧（x=11），确保手柄完全可见
            handleRect.anchoredPosition = new Vector2(11, 0);
            
            Image handleImage = handleObj.AddComponent<Image>();
            handleImage.color = Color.white;
            // 禁用手柄的射线检测，避免阻挡点击事件
            handleImage.raycastTarget = false;

            // 添加开关动画脚本（控制滑块位置和背景颜色）
            // 使用反射添加组件，因为编辑器脚本无法直接引用运行时类型
            System.Type animatorType = System.Type.GetType("PoseAI.iOSStyleToggleAnimator, Assembly-CSharp");
            if (animatorType == null)
            {
                // 尝试其他可能的程序集名称
                animatorType = System.Type.GetType("PoseAI.iOSStyleToggleAnimator");
            }
            
            if (animatorType != null)
            {
                Component animator = toggleObj.AddComponent(animatorType);
                var toggleField = animatorType.GetField("toggle", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                var bgImageField = animatorType.GetField("backgroundImage", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                var handleRectField = animatorType.GetField("handleRect", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                var containerRectField = animatorType.GetField("containerRect", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                
                if (toggleField != null) toggleField.SetValue(animator, toggle);
                if (bgImageField != null) bgImageField.SetValue(animator, bgImage);
                if (handleRectField != null) handleRectField.SetValue(animator, handleRect);
                if (containerRectField != null) containerRectField.SetValue(animator, containerRect);
            }
            else
            {
                Debug.LogWarning("PoseSmootherUIEditor: 无法找到iOSStyleToggleAnimator类型，开关将使用默认样式。请确保iOSStyleToggleAnimator.cs已编译。");
            }

            toggleRef = toggle;
        }

        private void CreateSliderRow(Transform parent, string name, string labelText, float minValue, float maxValue, ref Slider sliderRef, ref Text valueTextRef, float yPos)
        {
            // 创建行背景
            GameObject rowBgObj = new GameObject(name + "RowBg");
            Undo.RegisterCreatedObjectUndo(rowBgObj, "创建行背景: " + name);
            rowBgObj.transform.SetParent(parent, false);
            Image rowBgImage = rowBgObj.AddComponent<Image>();
            rowBgImage.color = new Color(0.16f, 0.16f, 0.18f, 0.6f);

            RectTransform rowBgRect = rowBgObj.GetComponent<RectTransform>();
            rowBgRect.anchorMin = new Vector2(0, 1);
            rowBgRect.anchorMax = new Vector2(1, 1);
            rowBgRect.pivot = new Vector2(0, 1);
            rowBgRect.sizeDelta = new Vector2(0, 48);
            rowBgRect.anchoredPosition = new Vector2(0, yPos + 24);

            // 创建标签
            GameObject labelObj = CreateTextElement(parent, name + "Label", labelText + ":", 13, new Color(0.85f, 0.85f, 0.9f, 1f));
            RectTransform labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 1);
            labelRect.anchorMax = new Vector2(0, 1);
            labelRect.pivot = new Vector2(0, 0.5f);
            labelRect.sizeDelta = new Vector2(110, 26);
            labelRect.anchoredPosition = new Vector2(18, yPos);

            // 创建滑块背景（缩短长度）
            GameObject sliderBgObj = new GameObject(name + "SliderBg");
            Undo.RegisterCreatedObjectUndo(sliderBgObj, "创建滑块背景: " + name);
            sliderBgObj.transform.SetParent(parent, false);
            Image bgImage = sliderBgObj.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.22f, 1f);

            RectTransform bgRect = sliderBgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0, 1);
            bgRect.anchorMax = new Vector2(0, 1);
            bgRect.pivot = new Vector2(0, 0.5f);
            bgRect.sizeDelta = new Vector2(80, 28);
            bgRect.anchoredPosition = new Vector2(130, yPos);

            // 添加边框高光
            UnityEngine.UI.Outline bgOutline = sliderBgObj.AddComponent<UnityEngine.UI.Outline>();
            bgOutline.effectColor = new Color(0.4f, 0.4f, 0.45f, 0.3f);
            bgOutline.effectDistance = new Vector2(1, -1);

            // 创建Slider
            GameObject sliderObj = new GameObject(name + "Slider");
            Undo.RegisterCreatedObjectUndo(sliderObj, "创建Slider: " + name);
            sliderObj.transform.SetParent(sliderBgObj.transform, false);
            RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0, 0);
            sliderRect.anchorMax = new Vector2(1, 1);
            sliderRect.sizeDelta = Vector2.zero;
            sliderRect.anchoredPosition = Vector2.zero;

            Slider slider = sliderObj.AddComponent<Slider>();
            slider.minValue = minValue;
            slider.maxValue = maxValue;
            slider.value = (minValue + maxValue) * 0.5f;
            slider.wholeNumbers = false;

            // 创建Background
            GameObject bgSliderObj = new GameObject("Background");
            bgSliderObj.transform.SetParent(sliderObj.transform, false);
            RectTransform bgSliderRect = bgSliderObj.AddComponent<RectTransform>();
            bgSliderRect.anchorMin = new Vector2(0, 0);
            bgSliderRect.anchorMax = new Vector2(1, 1);
            bgSliderRect.sizeDelta = Vector2.zero;
            bgSliderRect.anchoredPosition = Vector2.zero;
            Image bgSliderImage = bgSliderObj.AddComponent<Image>();
            bgSliderImage.color = new Color(0.15f, 0.15f, 0.17f, 1f);
            slider.targetGraphic = bgSliderImage;

            // 创建Fill Area
            GameObject fillAreaObj = new GameObject("Fill Area");
            fillAreaObj.transform.SetParent(sliderObj.transform, false);
            RectTransform fillAreaRect = fillAreaObj.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0, 0);
            fillAreaRect.anchorMax = new Vector2(1, 1);
            fillAreaRect.sizeDelta = Vector2.zero;
            fillAreaRect.anchoredPosition = Vector2.zero;

            // 创建Fill
            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(fillAreaObj.transform, false);
            RectTransform fillRect = fillObj.AddComponent<RectTransform>();
            fillRect.sizeDelta = Vector2.zero;
            Image fillImage = fillObj.AddComponent<Image>();
            fillImage.color = new Color(0.2f, 0.6f, 0.9f, 1f);
            slider.fillRect = fillRect;

            // 创建Handle Slide Area
            GameObject handleSlideAreaObj = new GameObject("Handle Slide Area");
            handleSlideAreaObj.transform.SetParent(sliderObj.transform, false);
            RectTransform handleSlideAreaRect = handleSlideAreaObj.AddComponent<RectTransform>();
            handleSlideAreaRect.anchorMin = new Vector2(0, 0);
            handleSlideAreaRect.anchorMax = new Vector2(1, 1);
            handleSlideAreaRect.sizeDelta = Vector2.zero;
            handleSlideAreaRect.anchoredPosition = Vector2.zero;

            // 创建Handle（扁平化设计）
            GameObject handleObj = new GameObject("Handle");
            handleObj.transform.SetParent(handleSlideAreaObj.transform, false);
            RectTransform handleRect = handleObj.AddComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(16, 12);
            Image handleImage = handleObj.AddComponent<Image>();
            handleImage.color = new Color(0.9f, 0.9f, 0.95f, 1f);
            slider.handleRect = handleRect;

            sliderRef = slider;

            // 创建值显示文本（更靠近滑块）
            GameObject valueObj = CreateTextElement(parent, name + "Value", slider.value.ToString("F2"), 13, new Color(0.95f, 0.95f, 0.98f, 1f));
            RectTransform valueRect = valueObj.GetComponent<RectTransform>();
            valueRect.anchorMin = new Vector2(0, 1);
            valueRect.anchorMax = new Vector2(0, 1);
            valueRect.pivot = new Vector2(0, 0.5f);
            valueRect.sizeDelta = new Vector2(50, 26);
            valueRect.anchoredPosition = new Vector2(218, yPos);
            Text valueText = valueObj.GetComponent<Text>();
            valueText.alignment = TextAnchor.MiddleLeft;
            valueText.fontStyle = FontStyle.Normal;
            
            // 为值文本添加背景
            GameObject valueBgObj = new GameObject(name + "ValueBg");
            Undo.RegisterCreatedObjectUndo(valueBgObj, "创建值背景: " + name);
            valueBgObj.transform.SetParent(parent, false);
            Image valueBgImage = valueBgObj.AddComponent<Image>();
            valueBgImage.color = new Color(0.22f, 0.22f, 0.24f, 0.8f);
            
            RectTransform valueBgRect = valueBgObj.GetComponent<RectTransform>();
            valueBgRect.anchorMin = new Vector2(0, 1);
            valueBgRect.anchorMax = new Vector2(0, 1);
            valueBgRect.pivot = new Vector2(0, 0.5f);
            valueBgRect.sizeDelta = new Vector2(50, 26);
            valueBgRect.anchoredPosition = new Vector2(218, yPos);
            valueBgRect.SetAsFirstSibling();
            
            UnityEngine.UI.Outline valueBgOutline = valueBgObj.AddComponent<UnityEngine.UI.Outline>();
            valueBgOutline.effectColor = new Color(0.3f, 0.3f, 0.35f, 0.4f);
            valueBgOutline.effectDistance = new Vector2(1, -1);

            valueTextRef = valueText;
        }
    }
}

