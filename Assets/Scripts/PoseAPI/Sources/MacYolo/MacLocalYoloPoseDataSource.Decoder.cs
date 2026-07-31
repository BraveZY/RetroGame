using UnityEngine;

namespace PoseAI
{
    /// <summary>负责 YOLO 候选筛选、COCO 关键点解码与 PoseFrame20 映射。</summary>
    public sealed partial class MacLocalYoloPoseDataSource
    {
        private PoseFrame20 Decode(
            float[] output,
            int sourceWidth,
            int sourceHeight,
            long frameId)
        {
            candidates.Clear();
            selectedCandidates.Clear();
            float scale = Mathf.Min((float)InputSize / sourceWidth, (float)InputSize / sourceHeight);
            float padX = (InputSize - sourceWidth * scale) * 0.5f;
            float padY = (InputSize - sourceHeight * scale) * 0.5f;

            for (int candidateIndex = 0; candidateIndex < CandidateCount; candidateIndex++)
            {
                float score = ValueAt(output, 4, candidateIndex);
                if (score < confidenceThreshold)
                {
                    continue;
                }

                float centerX = (ValueAt(output, 0, candidateIndex) - padX) / scale;
                float centerY = (ValueAt(output, 1, candidateIndex) - padY) / scale;
                float width = ValueAt(output, 2, candidateIndex) / scale;
                float height = ValueAt(output, 3, candidateIndex) / scale;
                candidates.Add(new Candidate(candidateIndex, score, centerX, centerY, width, height));
            }

            candidates.Sort((left, right) => right.score.CompareTo(left.score));
            foreach (Candidate candidate in candidates)
            {
                bool overlaps = false;
                foreach (Candidate existing in selectedCandidates)
                {
                    if (IntersectionOverUnion(candidate, existing) > 0.45f)
                    {
                        overlaps = true;
                        break;
                    }
                }

                if (!overlaps)
                {
                    selectedCandidates.Add(candidate);
                    if (selectedCandidates.Count >= Mathf.Clamp(maxPlayers, 1, 2))
                    {
                        break;
                    }
                }
            }

            PoseFrame20 frame20 = new PoseFrame20
            {
                timestamp = Time.time,
                frameId = frameId,
                sourceAspectRatio = sourceHeight > 0 ? (float)sourceWidth / sourceHeight : 0f
            };

            foreach (Candidate candidate in selectedCandidates)
            {
                frame20.skeletons.Add(CreateSkeleton20(output, candidate, sourceWidth, sourceHeight, scale, padX, padY));
            }

            return frame20;
        }

        /// <summary>从 COCO 17 点生成 PoseAPI 骨架 UI 使用的标准化 20 点。</summary>
        private PoseSkeleton20 CreateSkeleton20(
            float[] output,
            Candidate candidate,
            int sourceWidth,
            int sourceHeight,
            float scale,
            float padX,
            float padY)
        {
            var skeleton = new PoseSkeleton20();
            SetCocoJoint(skeleton, PoseJoint20Index.Head, 0, output, candidate.index, sourceWidth, sourceHeight, scale, padX, padY);
            SetCocoJoint(skeleton, PoseJoint20Index.ShoulderLeft, 5, output, candidate.index, sourceWidth, sourceHeight, scale, padX, padY);
            SetCocoJoint(skeleton, PoseJoint20Index.ShoulderRight, 6, output, candidate.index, sourceWidth, sourceHeight, scale, padX, padY);
            SetCocoJoint(skeleton, PoseJoint20Index.ElbowLeft, 7, output, candidate.index, sourceWidth, sourceHeight, scale, padX, padY);
            SetCocoJoint(skeleton, PoseJoint20Index.ElbowRight, 8, output, candidate.index, sourceWidth, sourceHeight, scale, padX, padY);
            SetCocoJoint(skeleton, PoseJoint20Index.WristLeft, 9, output, candidate.index, sourceWidth, sourceHeight, scale, padX, padY);
            SetCocoJoint(skeleton, PoseJoint20Index.WristRight, 10, output, candidate.index, sourceWidth, sourceHeight, scale, padX, padY);
            SetCocoJoint(skeleton, PoseJoint20Index.HipLeft, 11, output, candidate.index, sourceWidth, sourceHeight, scale, padX, padY);
            SetCocoJoint(skeleton, PoseJoint20Index.HipRight, 12, output, candidate.index, sourceWidth, sourceHeight, scale, padX, padY);
            SetCocoJoint(skeleton, PoseJoint20Index.KneeLeft, 13, output, candidate.index, sourceWidth, sourceHeight, scale, padX, padY);
            SetCocoJoint(skeleton, PoseJoint20Index.KneeRight, 14, output, candidate.index, sourceWidth, sourceHeight, scale, padX, padY);
            SetCocoJoint(skeleton, PoseJoint20Index.AnkleLeft, 15, output, candidate.index, sourceWidth, sourceHeight, scale, padX, padY);
            SetCocoJoint(skeleton, PoseJoint20Index.AnkleRight, 16, output, candidate.index, sourceWidth, sourceHeight, scale, padX, padY);
            CopyApproximateJoint(skeleton, PoseJoint20Index.HandLeft, PoseJoint20Index.WristLeft);
            CopyApproximateJoint(skeleton, PoseJoint20Index.HandRight, PoseJoint20Index.WristRight);
            CopyApproximateJoint(skeleton, PoseJoint20Index.FootLeft, PoseJoint20Index.AnkleLeft);
            CopyApproximateJoint(skeleton, PoseJoint20Index.FootRight, PoseJoint20Index.AnkleRight);
            SetCenterJoint(skeleton, PoseJoint20Index.ShoulderCenter, PoseJoint20Index.ShoulderLeft, PoseJoint20Index.ShoulderRight);
            SetCenterJoint(skeleton, PoseJoint20Index.HipCenter, PoseJoint20Index.HipLeft, PoseJoint20Index.HipRight);
            SetCenterJoint(skeleton, PoseJoint20Index.Spine, PoseJoint20Index.ShoulderCenter, PoseJoint20Index.HipCenter);
            return skeleton;
        }

        private void SetCocoJoint(
            PoseSkeleton20 skeleton,
            PoseJoint20Index targetIndex,
            int cocoIndex,
            float[] output,
            int candidateIndex,
            int sourceWidth,
            int sourceHeight,
            float scale,
            float padX,
            float padY)
        {
            DecodeCocoJoint(output, candidateIndex, cocoIndex, sourceWidth, sourceHeight, scale, padX, padY,
                out float x, out float y, out float confidence);
            if (confidence <= 0f)
            {
                return;
            }

            skeleton.Set(targetIndex, new PoseJoint20(x, y, 0f, confidence));
        }

        /// <summary>把模型输出统一转换为 PoseAPI 使用的左上原点坐标。</summary>
        private void DecodeCocoJoint(
            float[] output,
            int candidateIndex,
            int cocoIndex,
            int sourceWidth,
            int sourceHeight,
            float scale,
            float padX,
            float padY,
            out float x,
            out float y,
            out float confidence)
        {
            int offset = KeypointOffset + cocoIndex * KeypointStride;
            float rawX = (ValueAt(output, offset, candidateIndex) - padX) / scale / sourceWidth;
            float rawY = (ValueAt(output, offset + 1, candidateIndex) - padY) / scale / sourceHeight;

            x = Mathf.Clamp01(mirror ? 1f - rawX : rawX);
            y = Mathf.Clamp01(rawY);
            confidence = Mathf.Clamp01(ValueAt(output, offset + 2, candidateIndex));
        }

        private static void CopyApproximateJoint(PoseSkeleton20 skeleton, PoseJoint20Index targetIndex, PoseJoint20Index sourceIndex)
        {
            if (!skeleton.TryGet(sourceIndex, out PoseJoint20 sourceJoint))
            {
                return;
            }

            sourceJoint.approximate = true;
            skeleton.Set(targetIndex, sourceJoint);
        }

        private static void SetCenterJoint(PoseSkeleton20 skeleton, PoseJoint20Index targetIndex, PoseJoint20Index firstIndex, PoseJoint20Index secondIndex)
        {
            if (!skeleton.TryGet(firstIndex, out PoseJoint20 first) || !skeleton.TryGet(secondIndex, out PoseJoint20 second))
            {
                return;
            }

            skeleton.Set(targetIndex, new PoseJoint20(
                (first.x + second.x) * 0.5f,
                (first.y + second.y) * 0.5f,
                (first.z + second.z) * 0.5f,
                Mathf.Min(first.confidence, second.confidence)));
        }

        private static float ValueAt(float[] output, int channel, int candidate)
        {
            return output[channel * CandidateCount + candidate];
        }

        private static float IntersectionOverUnion(Candidate left, Candidate right)
        {
            float leftMinX = left.centerX - left.width * 0.5f;
            float leftMinY = left.centerY - left.height * 0.5f;
            float leftMaxX = left.centerX + left.width * 0.5f;
            float leftMaxY = left.centerY + left.height * 0.5f;
            float rightMinX = right.centerX - right.width * 0.5f;
            float rightMinY = right.centerY - right.height * 0.5f;
            float rightMaxX = right.centerX + right.width * 0.5f;
            float rightMaxY = right.centerY + right.height * 0.5f;
            float overlapWidth = Mathf.Max(0f, Mathf.Min(leftMaxX, rightMaxX) - Mathf.Max(leftMinX, rightMinX));
            float overlapHeight = Mathf.Max(0f, Mathf.Min(leftMaxY, rightMaxY) - Mathf.Max(leftMinY, rightMinY));
            float intersection = overlapWidth * overlapHeight;
            float union = left.width * left.height + right.width * right.height - intersection;
            return union <= 0f ? 0f : intersection / union;
        }

    }
}
