using UnityEngine;
using UnityEngine.UI;

namespace PoseAI
{
    /// <summary>负责网格、坐标轴、刻度和关键点 UI 的绘制与复用。</summary>
    internal partial class CoordinateRendererInstance
    {
        #region 绘制逻辑主要流程
        /// <summary>
        /// 绘制网格线（包含细网格、粗网格）
        /// </summary>
        private void DrawGrid()
        {
            int idx = 0;
            float minX = parent.coordXMin * currentUnitLength, maxX = parent.coordXMax * currentUnitLength;
            float minY = parent.coordYMin * currentUnitLength, maxY = parent.coordYMax * currentUnitLength;

            // 绘制细网格
            for (float x = parent.coordXMin; x <= parent.coordXMax + FLOAT_EPSILON; x += parent.fineGridSpacing)
            {
                if (Mathf.Abs(x % parent.coarseGridSpacing) < FLOAT_EPSILON) continue;
                GetOrCreateGridLine(ref idx, new Vector2(x * currentUnitLength, minY), new Vector2(x * currentUnitLength, maxY), parent.fineGridColor, parent.gridLineWidth);
            }
            for (float y = parent.coordYMin; y <= parent.coordYMax + FLOAT_EPSILON; y += parent.fineGridSpacing)
            {
                if (Mathf.Abs(y % parent.coarseGridSpacing) < FLOAT_EPSILON) continue;
                GetOrCreateGridLine(ref idx, new Vector2(minX, y * currentUnitLength), new Vector2(maxX, y * currentUnitLength), parent.fineGridColor, parent.gridLineWidth);
            }
            // 绘制粗网格
            for (float x = parent.coordXMin; x <= parent.coordXMax + FLOAT_EPSILON; x += parent.coarseGridSpacing)
                GetOrCreateGridLine(ref idx, new Vector2(x * currentUnitLength, minY), new Vector2(x * currentUnitLength, maxY), parent.coarseGridColor, parent.gridLineWidth * GRID_COARSE_MULTIPLIER);
            for (float y = parent.coordYMin; y <= parent.coordYMax + FLOAT_EPSILON; y += parent.coarseGridSpacing)
                GetOrCreateGridLine(ref idx, new Vector2(minX, y * currentUnitLength), new Vector2(maxX, y * currentUnitLength), parent.coarseGridColor, parent.gridLineWidth * GRID_COARSE_MULTIPLIER);

            // 隐藏多余 UI 线对象
            for (int i = idx; i < gridLines.Count; i++) if (gridLines[i] != null) gridLines[i].gameObject.SetActive(false);
        }

        /// <summary>
        /// 绘制坐标轴线
        /// </summary>
        private void DrawAxes()
        {
            int idx = 0;
            GetOrCreateAxisLine(ref idx, new Vector2(parent.coordXMin * currentUnitLength, 0), new Vector2(parent.coordXMax * currentUnitLength, 0), parent.xAxisColor, AXIS_WIDTH);
            GetOrCreateAxisLine(ref idx, new Vector2(0, parent.coordYMin * currentUnitLength), new Vector2(0, parent.coordYMax * currentUnitLength), parent.yAxisColor, AXIS_WIDTH);

            // 隐藏多余 UI 线对象
            for (int i = idx; i < axisLines.Count; i++) if (axisLines[i] != null) axisLines[i].gameObject.SetActive(false);
        }

        /// <summary>
        /// 绘制坐标刻度线与标签
        /// </summary>
        private void DrawTicks()
        {
            int tIdx = 0, lIdx = 0;
            for (float x = parent.coordXMin; x <= parent.coordXMax + FLOAT_EPSILON; x += parent.tickSpacing)
            {
                float px = x * currentUnitLength;
                GetOrCreateTickLine(ref tIdx, new Vector2(px, 0), new Vector2(px, -parent.tickLength), parent.xAxisColor);
                if (parent.showTickLabels) GetOrCreateTickLabel(ref lIdx, new Vector2(px, -parent.tickLength + TICK_LABEL_X_OFFSET), x.ToString("F1"), parent.xAxisColor, true);
            }
            for (float y = parent.coordYMin; y <= parent.coordYMax + FLOAT_EPSILON; y += parent.tickSpacing)
            {
                float py = y * currentUnitLength;
                GetOrCreateTickLine(ref tIdx, new Vector2(0, py), new Vector2(-parent.tickLength, py), parent.yAxisColor);
                if (parent.showTickLabels) GetOrCreateTickLabel(ref lIdx, new Vector2(-parent.tickLength + TICK_LABEL_Y_OFFSET, py), y.ToString("F1"), parent.yAxisColor, false);
            }
            // 隐藏多余的UI对象
            for (int i = tIdx; i < tickLines.Count; i++) if (tickLines[i] != null) tickLines[i].gameObject.SetActive(false);
            for (int i = lIdx; i < tickLabels.Count; i++) if (tickLabels[i] != null) tickLabels[i].gameObject.SetActive(false);
        }

        /// <summary>
        /// 刷新关键点（默认渲染左/右手腕）
        /// </summary>
        private void UpdateKeypoints()
        {
            float[] features = normalizationHandler.GetNormalizedFeatures(skeletonIndex);
            if (features == null || features.Length == 0) { ClearKeypoints(); return; }

            int featureDim = features.Length == 36 ? 3 : 2;
            int imgIdx = 0;

            // 局部函数：只渲染主要的手腕点，也可扩展
            void UpdateOneKeypoint(int kIdx)
            {
                int baseIdx = kIdx * featureDim;
                if (baseIdx + 1 >= features.Length) return;
                float x = features[baseIdx], y = features[baseIdx + 1];
                if (Mathf.Abs(x) < VALIDITY_THRESHOLD && Mathf.Abs(y) < VALIDITY_THRESHOLD) return;

                if (imgIdx >= keypointImages.Count) CreateKeypointImage();
                Image img = keypointImages[imgIdx++];
                img.rectTransform.anchoredPosition = new Vector2(x * currentUnitLength, y * currentUnitLength);
                img.gameObject.SetActive(true);
            }

            foreach (var img in keypointImages) img.gameObject.SetActive(false);
            UpdateOneKeypoint(LEFT_WRIST_INDEX);
            UpdateOneKeypoint(RIGHT_WRIST_INDEX);
        }

        /// <summary>
        /// 绘制单条线段的UI（各种直线通用）
        /// </summary>
        private void DrawUILine(RectTransform rect, Image img, Vector2 start, Vector2 end, float width)
        {
            if (rect == null || img == null) return;
            Vector2 dir = end - start;
            float dist = dir.magnitude;
            if (dist < FLOAT_EPSILON) { img.gameObject.SetActive(false); return; }
            rect.anchorMin = rect.anchorMax = Vector2.zero;
            rect.pivot = new Vector2(0, 0.5f);
            rect.anchoredPosition = start;
            rect.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
            rect.sizeDelta = new Vector2(dist, width);
        }
        #endregion

        #region UI 对象工厂与复用
        /// <summary> 网格线工厂/复用 </summary>
        private void GetOrCreateGridLine(ref int index, Vector2 start, Vector2 end, Color color, float width)
        {
            Image img;
            if (index < gridLines.Count) img = gridLines[index];
            else { img = CreateUIImage("GridLine", gridContainer.transform); gridLines.Add(img); }
            img.gameObject.SetActive(true);
            img.color = color;
            DrawUILine(img.rectTransform, img, start, end, width);
            index++;
        }

        /// <summary> 坐标轴工厂 </summary>
        private void GetOrCreateAxisLine(ref int index, Vector2 start, Vector2 end, Color color, float width)
        {
            Image img;
            if (index < axisLines.Count) img = axisLines[index];
            else { img = CreateUIImage("AxisLine", axesContainer.transform); axisLines.Add(img); }
            img.gameObject.SetActive(true);
            img.color = color;
            DrawUILine(img.rectTransform, img, start, end, width);
            index++;
        }

        /// <summary> 刻度线工厂 </summary>
        private void GetOrCreateTickLine(ref int index, Vector2 start, Vector2 end, Color color)
        {
            Image img;
            if (index < tickLines.Count) img = tickLines[index];
            else { img = CreateUIImage("TickLine", ticksContainer.transform); tickLines.Add(img); }
            img.gameObject.SetActive(true);
            img.color = color;
            DrawUILine(img.rectTransform, img, start, end, parent.tickWidth);
            index++;
        }

        /// <summary> 刻度标签工厂 </summary>
        private void GetOrCreateTickLabel(ref int index, Vector2 pos, string text, Color color, bool isX)
        {
            Text txt;
            if (index < tickLabels.Count) txt = tickLabels[index];
            else
            {
                GameObject obj = new GameObject("TickLabel");
                obj.transform.SetParent(ticksContainer.transform, false);
                RectTransform rect = obj.AddComponent<RectTransform>();
                rect.anchorMin = rect.anchorMax = Vector2.zero;
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(TICK_LABEL_WIDTH, TICK_LABEL_HEIGHT);
                txt = obj.AddComponent<Text>();
                txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                tickLabels.Add(txt);
            }
            txt.gameObject.SetActive(true);
            txt.text = text;
            txt.fontSize = parent.tickLabelFontSize;
            txt.color = color;
            txt.alignment = isX ? TextAnchor.MiddleCenter : TextAnchor.MiddleRight;
            txt.rectTransform.anchoredPosition = pos;
            index++;
        }

        /// <summary>
        /// 创建关键点可视化点（Image），并解决 Sprite 重复分配问题
        /// </summary>
        private void CreateKeypointImage()
        {
            Image img = CreateUIImage("Keypoint", keypointsContainer.transform);
            img.rectTransform.sizeDelta = new Vector2(parent.keypointSize, parent.keypointSize);
            img.color = parent.keypointColor;

            // 只分配一次Sprite，防止Texture泄漏
            if (defaultKeypointSprite == null)
            {
                Texture2D tex = new Texture2D(1, 1);
                tex.SetPixel(0, 0, Color.white);
                tex.Apply();
                defaultKeypointSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
            }
            img.sprite = defaultKeypointSprite;
            keypointImages.Add(img);
        }

        /// <summary>
        /// 原始Image工厂
        /// </summary>
        private Image CreateUIImage(string name, Transform parent)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.AddComponent<RectTransform>();
            return obj.AddComponent<Image>();
        }
        #endregion
    }
}
