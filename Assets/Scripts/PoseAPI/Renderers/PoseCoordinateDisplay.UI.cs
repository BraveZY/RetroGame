using UnityEngine;
using UnityEngine.UI;

namespace PoseAI
{
    /// <summary>负责自动创建坐标标签 UI，运行时数据更新保留在主文件。</summary>
    public partial class PoseCoordinateDisplay
    {
        private void CreateUIElements()
        {
            // 查找或创建Canvas
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("PoseCoordinateCanvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 100; // 设置较高的排序顺序，确保显示在最上层

                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);

                canvasObj.AddComponent<GraphicRaycaster>();
            }
            else
            {
                // 如果Canvas已存在，确保设置正确
                if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                {
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                }
                // 确保Canvas在最上层
                if (canvas.sortingOrder < 100)
                {
                    canvas.sortingOrder = 100;
                }
            }

            Font font = GetBuiltinFont();
            int maxSkeletons = 2; // 支持最多2个骨架

            for (int i = 0; i < maxSkeletons; i++)
            {
                // 创建左手腕坐标文本
                if (showWrists)
                {
                    Color color = (i == 0) ? new Color(0f, 1f, 0f, 1f) : new Color(0f, 1f, 1f, 1f);
                    Text text = CreateTextObject(canvas.transform, $"LeftWristText_{i}", font, color);
                    text.text = $"LW{i}: init";
                    leftWristTexts.Add(text);
                    if (i == 0) leftWristText = text;
                    else leftWristText2 = text;
                }

                // 创建右手腕坐标文本
                if (showWrists)
                {
                    Color color = (i == 0) ? new Color(0f, 1f, 1f, 1f) : new Color(0f, 0.5f, 1f, 1f);
                    Text text = CreateTextObject(canvas.transform, $"RightWristText_{i}", font, color);
                    text.text = $"RW{i}: init";
                    rightWristTexts.Add(text);
                    if (i == 0) rightWristText = text;
                    else rightWristText2 = text;
                }

                // 创建左脚腕坐标文本
                if (showAnkles)
                {
                    Color color = (i == 0) ? new Color(1f, 1f, 0f, 1f) : new Color(1f, 0.8f, 0f, 1f);
                    Text text = CreateTextObject(canvas.transform, $"LeftAnkleText_{i}", font, color);
                    text.text = $"LA{i}: init";
                    leftAnkleTexts.Add(text);
                    if (i == 0) leftAnkleText = text;
                    else leftAnkleText2 = text;
                }

                // 创建右脚腕坐标文本
                if (showAnkles)
                {
                    Color color = (i == 0) ? new Color(1f, 0.5f, 0f, 1f) : new Color(1f, 0.2f, 0f, 1f);
                    Text text = CreateTextObject(canvas.transform, $"RightAnkleText_{i}", font, color);
                    text.text = $"RA{i}: init";
                    rightAnkleTexts.Add(text);
                    if (i == 0) rightAnkleText = text;
                    else rightAnkleText2 = text;
                }
            }
        }

        /// <summary>
        /// 创建文本对象
        /// </summary>
        private Text CreateTextObject(Transform parent, string name, Font font, Color color)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);

            Text text = obj.AddComponent<Text>();
            text.font = font;
            text.fontSize = 19; // 24 * 0.8 = 19.2，取整为19
            text.color = color;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            // 添加轮廓
            Outline outline = obj.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(1, 1);

            // 设置 RectTransform - 与 PoseUIRenderer 一致，锚点在左下角 (0,0)
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0f); // pivot 在底部中心，文本显示在手腕上方
            rect.sizeDelta = new Vector2(200, 30);

            return text;
        }

    }
}
