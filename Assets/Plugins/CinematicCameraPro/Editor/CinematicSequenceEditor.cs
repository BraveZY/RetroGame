using UnityEditor;
using UnityEngine;

namespace CinematicCameraPro
{
    [CustomEditor(typeof(CinematicSequence))]
    public class CinematicSequenceEditor : Editor
    {
        const float AnchorButtonSize = 0.18f;
        static readonly Vector2 AnchorGuiButtonOffset = new Vector2(34f, -42f);
        static readonly Vector2 AnchorGuiButtonSize = new Vector2(54f, 24f);
        static readonly Vector3 AnchorSceneHandleOffset = new Vector3(0.58f, 0.42f, 0f);

        CinematicSequence targetSequence;
        int selectedClipIndex = -1;
        int selectedEmbeddedPointIndex = -1;
        bool sceneDragUndoRecorded;
        bool sceneDragDirtyPending;

        static GUIContent GC(string text, string tooltip)
        {
            return new GUIContent(text, tooltip);
        }

        void OnEnable()
        {
            targetSequence = (CinematicSequence)target;
            EnsureValidSelection();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EnsureValidSelection();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(GC("Cinematic Sequence", "多相机序列控制器。用于按时间切换不同机位，并可为每个镜头片段配置内嵌路径镜头。"), EditorStyles.boldLabel);
            EditorGUILayout.Space();

            DrawPlaybackControls();
            EditorGUILayout.Space();

            DrawSettings();
            EditorGUILayout.Space();

            DrawSmartActions();
            EditorGUILayout.Space();

            DrawClipList();
            EditorGUILayout.Space();

            DrawSelectedClipDetails();
            EditorGUILayout.Space();

            DrawEvents();

            serializedObject.ApplyModifiedProperties();
        }

        void DrawPlaybackControls()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(targetSequence.IsPlaying ? GC("⏸ Pause", "暂停当前多相机序列。") : GC("▶ Play", "播放当前多相机序列。")))
            {
                if (targetSequence.IsPlaying)
                {
                    targetSequence.Pause();
                }
                else
                {
                    targetSequence.Play();
                }
            }

            if (GUILayout.Button(GC("⏹ Stop", "停止序列播放，并恢复默认相机状态。")))
            {
                targetSequence.Stop();
            }

            if (GUILayout.Button(GC("⟲ Rewind", "跳回序列起点，方便检查开场切换。")))
            {
                targetSequence.Seek(0f);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(GC("Speed:", "序列整体播放速度倍率。"), GUILayout.Width(50));
            targetSequence.playbackSpeed = EditorGUILayout.Slider(targetSequence.playbackSpeed, 0.1f, 3f);
            EditorGUILayout.EndHorizontal();
        }

        void DrawSettings()
        {
            EditorGUILayout.LabelField(GC("Settings", "多相机序列的基础播放设置。"), EditorStyles.boldLabel);
            targetSequence.playMode = (SequencePlayMode)EditorGUILayout.EnumPopup(GC("Play Mode:", "序列播放模式。当前支持单次与循环。"), targetSequence.playMode);
            targetSequence.playOnStart = EditorGUILayout.Toggle(GC("Play On Start:", "进入运行时后是否自动播放该序列。"), targetSequence.playOnStart);
            targetSequence.defaultCamera = (Camera)EditorGUILayout.ObjectField(GC("Default Camera:", "序列停止后默认恢复启用的相机。"), targetSequence.defaultCamera, typeof(Camera), true);
            targetSequence.restoreDefaultCameraOnStop = EditorGUILayout.Toggle(GC("Restore Default:", "停止序列时是否自动恢复默认相机。"), targetSequence.restoreDefaultCameraOnStop);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(GC("Total Duration:", "所有镜头片段拼接后的总时长。"), GUILayout.Width(95));
            EditorGUILayout.LabelField($"{targetSequence.TotalDuration:F2}s");
            EditorGUILayout.EndHorizontal();
        }

        void DrawSmartActions()
        {
            EditorGUILayout.LabelField(GC("Quick Actions", "快速生成和整理镜头片段，减少手工配置。"), EditorStyles.boldLabel);

            if (GUILayout.Button(GC("+ Add Clip From Selected Camera", "把当前选中的场景相机快速加入序列，并尝试智能推断默认时长与内嵌镜头。")))
            {
                Camera selectedCamera = Selection.activeGameObject != null ? Selection.activeGameObject.GetComponent<Camera>() : null;
                if (selectedCamera != null)
                {
                    Undo.RecordObject(targetSequence, "Add Clip From Selected Camera");
                    targetSequence.AddSmartClip(selectedCamera);
                    targetSequence.AutoArrangeClips();
                    selectedClipIndex = targetSequence.clips.Count - 1;
                    EditorUtility.SetDirty(targetSequence);
                }
            }

            if (GUILayout.Button(GC("⟲ Auto Arrange Clips", "自动按当前列表顺序重新排布每个镜头片段的开始时间。")))
            {
                Undo.RecordObject(targetSequence, "Auto Arrange Clips");
                targetSequence.AutoArrangeClips();
                EditorUtility.SetDirty(targetSequence);
            }
        }

        void DrawClipList()
        {
            EditorGUILayout.LabelField(GC("Clips", "镜头片段列表。每个片段对应一段时间内使用的输出机位。"), EditorStyles.boldLabel);

            for (int i = 0; i < targetSequence.clips.Count; i++)
            {
                CameraTrackClip clip = targetSequence.clips[i];
                if (clip == null)
                {
                    continue;
                }

                EditorGUILayout.BeginHorizontal();

                bool isSelected = selectedClipIndex == i;
                GUIStyle selectStyle = isSelected ? EditorStyles.miniButtonMid : EditorStyles.label;
                if (GUILayout.Button(GC(GetClipLabel(i, clip), "选择当前镜头片段并同步预览。"), selectStyle, GUILayout.Width(180)))
                {
                    SelectClip(i);
                }

                clip.duration = Mathf.Max(0.1f, EditorGUILayout.FloatField(clip.duration, GUILayout.Width(45)));
                EditorGUILayout.LabelField("s", GUILayout.Width(10));

                if (GUILayout.Button(GC("▶", "跳到当前镜头片段起点预览。"), GUILayout.Width(25)))
                {
                    SelectClip(i);
                    targetSequence.PreviewClip(i);
                }

                if (GUILayout.Button(GC("▲", "将当前镜头片段上移一位。"), GUILayout.Width(25)) && i > 0)
                {
                    Undo.RecordObject(targetSequence, "Move Clip Up");
                    targetSequence.MoveClip(i, i - 1);
                    selectedClipIndex = i - 1;
                    EditorUtility.SetDirty(targetSequence);
                    break;
                }

                if (GUILayout.Button(GC("▼", "将当前镜头片段下移一位。"), GUILayout.Width(25)) && i < targetSequence.clips.Count - 1)
                {
                    Undo.RecordObject(targetSequence, "Move Clip Down");
                    targetSequence.MoveClip(i, i + 1);
                    selectedClipIndex = i + 1;
                    EditorUtility.SetDirty(targetSequence);
                    break;
                }

                if (GUILayout.Button(GC("✕", "删除当前镜头片段。"), GUILayout.Width(25)))
                {
                    Undo.RecordObject(targetSequence, "Remove Clip");
                    targetSequence.RemoveClip(i);
                    EnsureValidSelection();
                    EditorUtility.SetDirty(targetSequence);
                    break;
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();

            if (GUILayout.Button(GC("+ Add Empty Clip", "新增一个空白镜头片段，稍后再指定相机和时长。")))
            {
                Undo.RecordObject(targetSequence, "Add Empty Clip");
                targetSequence.AddClip(null);
                targetSequence.AutoArrangeClips();
                selectedClipIndex = targetSequence.clips.Count - 1;
                EditorUtility.SetDirty(targetSequence);
            }
        }

        void DrawSelectedClipDetails()
        {
            if (selectedClipIndex < 0 || selectedClipIndex >= targetSequence.clips.Count)
            {
                return;
            }

            CameraTrackClip clip = targetSequence.clips[selectedClipIndex];
            if (clip == null)
            {
                return;
            }

            EditorGUILayout.LabelField(GC($"Clip {selectedClipIndex + 1} Details", "当前镜头片段的详细配置。"), EditorStyles.boldLabel);

            clip.name = EditorGUILayout.TextField(GC("Name:", "镜头片段名称，仅用于编辑器识别和整理。"), clip.name);
            clip.sourceCamera = (Camera)EditorGUILayout.ObjectField(GC("Source Camera:", "该镜头片段实际输出画面的相机。"), clip.sourceCamera, typeof(Camera), true);
            clip.startTime = Mathf.Max(0f, EditorGUILayout.FloatField(GC("Start Time:", "该镜头片段在整条序列中的开始时间。"), clip.startTime));
            clip.duration = Mathf.Max(0.1f, EditorGUILayout.FloatField(GC("Duration:", "该镜头片段持续时间。"), clip.duration));
            EditorGUILayout.LabelField(GC("Transition:", "当前版本多相机切换仅支持 Cut 硬切。"));
            EditorGUILayout.LabelField("Cut");

            EditorGUILayout.Space();
            clip.useEmbeddedShot = EditorGUILayout.Toggle(GC("Use Embedded Shot:", "是否在这个镜头片段内启用内嵌路径镜头，而不是只做静态机位切换。"), clip.useEmbeddedShot);

            if (clip.useEmbeddedShot)
            {
                if (GUILayout.Button(GC("Use First Shot From Source Camera", "自动读取源相机上的 CinematicCamera，并复制它的第一个 Shot 作为当前片段的内嵌镜头。")))
                {
                    TryApplySourceCameraShot(clip);
                }

                clip.embeddedShot.name = EditorGUILayout.TextField(GC("Shot Name:", "内嵌镜头名称，仅用于编辑器识别。"), clip.embeddedShot.name);
                DrawLookAtSettings(clip.embeddedShot);
                if (!clip.embeddedShot.HasLookAtTargets)
                {
                    DrawPathFacingSettings(clip.embeddedShot);
                }

                EditorGUILayout.Space();
                DrawEmbeddedShotDetails(clip);
            }

            EditorGUILayout.Space();
            if (GUILayout.Button(GC("Preview This Clip", "预览当前镜头片段起点的相机状态。")))
            {
                targetSequence.PreviewClip(selectedClipIndex);
            }
        }

        void DrawEvents()
        {
            EditorGUILayout.LabelField(GC("Events", "多相机序列的控制和切换事件。"), EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onPlay"), GC("On Play", "序列开始播放时触发。"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onPause"), GC("On Pause", "序列暂停时触发。"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onStop"), GC("On Stop", "序列停止时触发。"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onComplete"), GC("On Complete", "序列完整播放结束时触发。"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onCameraSwitch"), GC("On Camera Switch", "切换到新的输出机位时触发。"));
        }

        void DrawPathFacingSettings(CinematicShot shot)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(GC("Path Facing", "未设置 LookAt 时，相机默认沿路径前进方向朝向。这里可以控制是否镜像朝向，或附加左右偏转。"), EditorStyles.miniBoldLabel);

            shot.mirrorPathFacing = EditorGUILayout.Toggle(
                GC("Mirror Facing:", "开启后，未设置 LookAt 时相机会朝向路径反方向。适合做拉远但镜头回看的运镜。"),
                shot.mirrorPathFacing);
            shot.pathFacingYawOffset = EditorGUILayout.Slider(
                GC("Yaw Offset:", "未设置 LookAt 时，在路径朝向基础上附加水平偏转角度。可用于左 90 度、右 90 度等侧拍。"),
                shot.pathFacingYawOffset,
                -180f,
                180f);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(GC("Quick Presets:", "常用拍摄方向预设。只有在未设置 LookAt 时生效。"));
            if (GUILayout.Button(GC("Forward", "沿路径正方向拍摄。"), EditorStyles.miniButtonLeft))
            {
                shot.mirrorPathFacing = false;
                shot.pathFacingYawOffset = 0f;
            }
            if (GUILayout.Button(GC("Backward", "沿路径反方向拍摄。"), EditorStyles.miniButtonMid))
            {
                shot.mirrorPathFacing = true;
                shot.pathFacingYawOffset = 0f;
            }
            if (GUILayout.Button(GC("Left 90", "相机拍摄方向相对路径左转 90 度。"), EditorStyles.miniButtonMid))
            {
                shot.mirrorPathFacing = false;
                shot.pathFacingYawOffset = -90f;
            }
            if (GUILayout.Button(GC("Right 90", "相机拍摄方向相对路径右转 90 度。"), EditorStyles.miniButtonRight))
            {
                shot.mirrorPathFacing = false;
                shot.pathFacingYawOffset = 90f;
            }
            EditorGUILayout.EndHorizontal();
        }

        void DrawLookAtSettings(CinematicShot shot)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(GC("LookAt", "支持一个或两个注视目标。两个目标时可分配各自持续时间，并通过过渡时长平滑切换。"), EditorStyles.miniBoldLabel);
            shot.lookAtTarget = (Transform)EditorGUILayout.ObjectField(GC("Target A:", "第一个注视目标。只有一个目标时会占满整个 Shot。"), shot.lookAtTarget, typeof(Transform), true);
            shot.secondaryLookAtTarget = (Transform)EditorGUILayout.ObjectField(GC("Target B:", "第二个注视目标。设置后可为两个目标分配时间并平滑过渡。"), shot.secondaryLookAtTarget, typeof(Transform), true);

            if (shot.lookAtTarget == null && shot.secondaryLookAtTarget != null)
            {
                shot.lookAtTarget = shot.secondaryLookAtTarget;
                shot.secondaryLookAtTarget = null;
            }

            if (shot.lookAtTarget != null && shot.secondaryLookAtTarget != null)
            {
                float shotDuration = Mathf.Max(0.1f, shot.Duration);
                shot.primaryLookAtDuration = EditorGUILayout.Slider(
                    GC("Target A Time:", "第一个目标占用的时间。剩余时间会自动分配给第二个目标。"),
                    shot.GetPrimaryLookAtDuration(),
                    0f,
                    shotDuration);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(GC("Target B Time:", "第二个目标自动获得剩余时间。"));
                EditorGUILayout.LabelField($"{shot.SecondaryLookAtDuration:F2}s");
                EditorGUILayout.EndHorizontal();

                float maxTransition = Mathf.Min(shot.GetPrimaryLookAtDuration(), shot.SecondaryLookAtDuration);
                if (maxTransition > 0.01f)
                {
                    shot.lookAtTransitionDuration = EditorGUILayout.Slider(
                        GC("Transition:", "两个目标之间的平滑过渡时长。值越大，镜头切换越柔和。"),
                        shot.GetLookAtTransitionDuration(),
                        0f,
                        maxTransition);
                }
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel(GC("Transition:", "两个目标没有可用于过渡的重叠时间，因此当前不启用平滑切换。"));
                    EditorGUILayout.LabelField("0.00s");
                    EditorGUILayout.EndHorizontal();
                }

                shot.lookAtLeadTime = EditorGUILayout.Slider(
                    GC("Lead Time:", "在理论切换点之前，提前多长时间开始轻微引导到第二目标。值越大，镜头越早开始转向。"),
                    shot.lookAtLeadTime,
                    0f,
                    Mathf.Min(shot.Duration, 1.5f));
                shot.lookAtRotationSmoothTime = EditorGUILayout.Slider(
                    GC("Rotation Smooth:", "镜头朝向跟随注视目标变化的阻尼时间。值越大，转向越柔和。"),
                    shot.lookAtRotationSmoothTime,
                    0.02f,
                    0.6f);
                shot.maxLookAtTurnSpeed = EditorGUILayout.Slider(
                    GC("Max Turn Speed:", "镜头每秒最大转向角速度。值越低越稳重，值越高越灵活。"),
                    shot.maxLookAtTurnSpeed,
                    45f,
                    360f);
            }
            else if (shot.lookAtTarget != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(GC("Active Time:", "单目标时默认占满整个 Shot 时长。"));
                EditorGUILayout.LabelField($"{shot.Duration:F2}s");
                EditorGUILayout.EndHorizontal();

                shot.lookAtRotationSmoothTime = EditorGUILayout.Slider(
                    GC("Rotation Smooth:", "镜头朝向跟随注视目标变化的阻尼时间。值越大，转向越柔和。"),
                    shot.lookAtRotationSmoothTime,
                    0.02f,
                    0.6f);
                shot.maxLookAtTurnSpeed = EditorGUILayout.Slider(
                    GC("Max Turn Speed:", "镜头每秒最大转向角速度。值越低越稳重，值越高越灵活。"),
                    shot.maxLookAtTurnSpeed,
                    45f,
                    360f);
            }
        }

        void EnsureValidSelection()
        {
            if (targetSequence == null)
            {
                return;
            }

            if (targetSequence.clips.Count == 0)
            {
                selectedClipIndex = -1;
                selectedEmbeddedPointIndex = -1;
                return;
            }

            if (selectedClipIndex < 0 || selectedClipIndex >= targetSequence.clips.Count)
            {
                SelectClip(0);
            }
        }

        void SelectClip(int index)
        {
            if (targetSequence == null || index < 0 || index >= targetSequence.clips.Count)
            {
                return;
            }

            selectedClipIndex = index;
            selectedEmbeddedPointIndex = -1;
            targetSequence.PreviewClip(index);
            Repaint();
        }

        void SelectEmbeddedPoint(int pointIndex, bool preview)
        {
            if (!TryGetSelectedEmbeddedShot(out CinematicShot embeddedShot))
            {
                return;
            }

            if (pointIndex < 0 || pointIndex >= embeddedShot.path.keyframes.Count)
            {
                return;
            }

            selectedEmbeddedPointIndex = pointIndex;
            if (preview)
            {
                PreviewSelectedEmbeddedPoint();
            }
            Repaint();
        }

        void PreviewSelectedEmbeddedPoint()
        {
            PreviewSelectedEmbeddedPoint(true, true);
        }

        void PreviewSelectedEmbeddedPoint(bool repaintAllViews, bool forceRepaint)
        {
            if (!TryGetSelectedEmbeddedShot(out CinematicShot embeddedShot))
            {
                return;
            }

            if (selectedEmbeddedPointIndex < 0 || selectedEmbeddedPointIndex >= embeddedShot.path.keyframes.Count)
            {
                targetSequence.PreviewClip(selectedClipIndex);
                return;
            }

            float duration = Mathf.Max(embeddedShot.path.Duration, 0.0001f);
            float pathTime = Mathf.Clamp01(embeddedShot.path.keyframes[selectedEmbeddedPointIndex].time / duration);
            targetSequence.PreviewEmbeddedShotAtPathTime(selectedClipIndex, pathTime, repaintAllViews, forceRepaint);
        }

        bool TryGetSelectedEmbeddedShot(out CinematicShot embeddedShot)
        {
            embeddedShot = null;
            if (targetSequence == null || selectedClipIndex < 0 || selectedClipIndex >= targetSequence.clips.Count)
            {
                return false;
            }

            CameraTrackClip clip = targetSequence.clips[selectedClipIndex];
            if (clip == null || !clip.useEmbeddedShot || clip.embeddedShot == null || clip.embeddedShot.path == null)
            {
                return false;
            }

            embeddedShot = clip.embeddedShot;
            return true;
        }

        Camera ResolveSelectedClipCamera()
        {
            if (targetSequence == null || selectedClipIndex < 0 || selectedClipIndex >= targetSequence.clips.Count)
            {
                return null;
            }

            return targetSequence.clips[selectedClipIndex]?.sourceCamera;
        }

        void TryApplySourceCameraShot(CameraTrackClip clip)
        {
            if (clip?.sourceCamera == null)
            {
                return;
            }

            CinematicCamera sourceController = clip.sourceCamera.GetComponent<CinematicCamera>();
            if (sourceController == null || sourceController.shots.Count == 0)
            {
                return;
            }

            Undo.RecordObject(targetSequence, "Use Source Camera Shot");
            clip.embeddedShot = sourceController.shots[0].Clone();
            clip.duration = Mathf.Max(0.1f, sourceController.TotalDuration);
            EditorUtility.SetDirty(targetSequence);
        }

        void DrawEmbeddedShotDetails(CameraTrackClip clip)
        {
            CinematicShot embeddedShot = clip.embeddedShot;
            if (embeddedShot == null)
            {
                return;
            }

            EditorGUILayout.LabelField(GC("Embedded Shot", "当前镜头片段内附带的路径镜头配置。"), EditorStyles.boldLabel);
            PathType newPathType = (PathType)EditorGUILayout.EnumPopup(GC("Path Type:", "内嵌镜头的路径插值类型。"), embeddedShot.path.pathType);
            if (newPathType != embeddedShot.path.pathType)
            {
                embeddedShot.path.pathType = newPathType;
                embeddedShot.path.InvalidateCache();
                EditorUtility.SetDirty(targetSequence);
                targetSequence.PreviewClip(selectedClipIndex);
            }
            if (EasingSelectorGUI.Draw(embeddedShot.path, GC("Easing:", "使用 Unity 原生曲线编辑器控制镜头节奏。上方可自由加点编辑，下方保留紧凑预设快捷切换。"), targetSequence))
            {
                EditorUtility.SetDirty(targetSequence);
                targetSequence.PreviewClip(selectedClipIndex);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(GC("Path Points", "内嵌镜头的路径点列表。位置与 Bezier 手柄请直接在 Scene 视图中拖拽编辑。"), EditorStyles.miniBoldLabel);

            for (int i = 0; i < embeddedShot.path.keyframes.Count; i++)
            {
                PathPoint point = embeddedShot.path.keyframes[i];
                EditorGUILayout.BeginHorizontal();

                bool isSelected = selectedEmbeddedPointIndex == i;
                GUIStyle pointButtonStyle = isSelected ? EditorStyles.miniButtonMid : EditorStyles.label;
                if (GUILayout.Button(GC($"Point {i + 1}", "选中这个内嵌镜头路径点，并在 Scene 视图中拖拽、输入精确数值或预览画面。"), pointButtonStyle, GUILayout.Width(90)))
                {
                    SelectEmbeddedPoint(i, true);
                }

                EditorGUILayout.LabelField(GC("FOV", "该路径点的相机视场角。镜头运动到下一个路径点时会平滑过渡。"), GUILayout.Width(28));
                float newFov = EditorGUILayout.FloatField(point.fov, GUILayout.Width(42));
                newFov = Mathf.Clamp(newFov, 1f, 179f);
                if (!Mathf.Approximately(newFov, point.fov))
                {
                    Undo.RecordObject(targetSequence, "Edit Embedded Point FOV");
                    point.fov = newFov;
                    embeddedShot.path.InvalidateCache();
                    EditorUtility.SetDirty(targetSequence);
                    PreviewSelectedEmbeddedPoint();
                }

                if (GUILayout.Button(GC("✕", "删除这个内嵌镜头路径点。"), GUILayout.Width(25)))
                {
                    embeddedShot.path.RemoveKeyframe(i);
                    if (selectedEmbeddedPointIndex == i)
                    {
                        selectedEmbeddedPointIndex = -1;
                    }
                    EditorUtility.SetDirty(targetSequence);
                    targetSequence.PreviewClip(selectedClipIndex);
                    break;
                }

                EditorGUILayout.EndHorizontal();
            }

            DrawSelectedEmbeddedPointDetails(embeddedShot);

            EditorGUILayout.Space();
            if (GUILayout.Button(GC("+ Add Path Point", "为当前内嵌镜头新增一个路径点。")))
            {
                embeddedShot.path.AddKeyframe((Vector3?)null, 0f);
                selectedEmbeddedPointIndex = embeddedShot.path.keyframes.Count - 1;
                EditorUtility.SetDirty(targetSequence);
                targetSequence.PreviewClip(selectedClipIndex);
            }

            if (GUILayout.Button(GC("⟲ Auto Calculate Tangents", "自动为内嵌镜头路径生成平滑 Bezier 手柄。")))
            {
                embeddedShot.path.AutoCalculateTangents();
                EditorUtility.SetDirty(targetSequence);
                targetSequence.PreviewClip(selectedClipIndex);
            }
        }

        void DrawSelectedEmbeddedPointDetails(CinematicShot embeddedShot)
        {
            if (embeddedShot?.path == null || selectedEmbeddedPointIndex < 0 || selectedEmbeddedPointIndex >= embeddedShot.path.keyframes.Count)
            {
                return;
            }

            PathPoint point = embeddedShot.path.keyframes[selectedEmbeddedPointIndex];
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(GC($"Selected Path Point P{selectedEmbeddedPointIndex}", "当前内嵌路径点的精确数值。"), EditorStyles.miniBoldLabel);

            EditorGUI.BeginChangeCheck();
            Vector3 newPosition = EditorGUILayout.Vector3Field(GC("Position", "路径点世界坐标。"), point.position);
            float newFov = EditorGUILayout.FloatField(GC("FOV", "该路径点对应的相机视场角。"), point.fov);
            newFov = Mathf.Clamp(newFov, 1f, 179f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(targetSequence, "Edit Embedded Point Values");
                point.position = newPosition;
                point.fov = newFov;
                embeddedShot.path.InvalidateCache();
                EditorUtility.SetDirty(targetSequence);
                PreviewSelectedEmbeddedPoint();
                SceneView.RepaintAll();
            }

            if (embeddedShot.path.pathType == PathType.Bezier)
            {
                EditorGUI.BeginChangeCheck();
                Vector3 tangentIn = EditorGUILayout.Vector3Field(GC("Tangent In", "Bezier 入手柄，相对路径点位置。"), point.tangentIn);
                Vector3 tangentOut = EditorGUILayout.Vector3Field(GC("Tangent Out", "Bezier 出手柄，相对路径点位置。"), point.tangentOut);
                bool lockTangents = EditorGUILayout.Toggle(GC("Lock Tangents", "拖动一个手柄时是否镜像另一个手柄。"), point.lockTangents);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(targetSequence, "Edit Embedded Point Tangents");
                    point.tangentIn = tangentIn;
                    point.tangentOut = tangentOut;
                    point.lockTangents = lockTangents;
                    embeddedShot.path.InvalidateCache();
                    EditorUtility.SetDirty(targetSequence);
                    PreviewSelectedEmbeddedPoint();
                    SceneView.RepaintAll();
                }
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(GC("Preview Point", "把源相机预览到这个路径点对应的画面。")))
            {
                PreviewSelectedEmbeddedPoint();
            }

            Camera sourceCamera = ResolveSelectedClipCamera();
            EditorGUI.BeginDisabledGroup(sourceCamera == null);
            if (GUILayout.Button(GC("Snap To Camera", "把路径点移动到当前片段源相机位置。")))
            {
                Undo.RecordObject(targetSequence, "Snap Embedded Point To Camera");
                point.position = sourceCamera.transform.position;
                embeddedShot.path.InvalidateCache();
                EditorUtility.SetDirty(targetSequence);
                PreviewSelectedEmbeddedPoint();
                SceneView.RepaintAll();
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }

        void OnSceneGUI()
        {
            if (targetSequence == null || selectedClipIndex < 0 || selectedClipIndex >= targetSequence.clips.Count)
            {
                return;
            }

            CameraTrackClip clip = targetSequence.clips[selectedClipIndex];
            if (clip == null || !clip.useEmbeddedShot || clip.embeddedShot == null)
            {
                return;
            }

            CompleteSceneDragIfNeeded();
            ReserveSceneSelectionForPointEditing();
            DrawEmbeddedShotSceneHandles(clip);
        }

        void CompleteSceneDragIfNeeded()
        {
            EventType eventType = Event.current.type;
            if (eventType != EventType.MouseUp && eventType != EventType.Ignore)
            {
                return;
            }

            sceneDragUndoRecorded = false;
            if (!sceneDragDirtyPending || targetSequence == null)
            {
                return;
            }

            sceneDragDirtyPending = false;
            EditorUtility.SetDirty(targetSequence);
            if (targetSequence.gameObject.scene.IsValid())
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(targetSequence.gameObject.scene);
            }
        }

        void RecordSceneDragUndo(string undoName)
        {
            if (sceneDragUndoRecorded)
            {
                return;
            }

            Undo.RecordObject(targetSequence, undoName);
            sceneDragUndoRecorded = true;
        }

        void MarkSceneDragDirty()
        {
            EditorUtility.SetDirty(targetSequence);
            sceneDragDirtyPending = true;
        }

        void ReserveSceneSelectionForPointEditing()
        {
            if (Event.current.type != EventType.Layout)
            {
                return;
            }

            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        }

        void DrawEmbeddedShotSceneHandles(CameraTrackClip clip)
        {
            CinematicShot embeddedShot = clip.embeddedShot;
            if (embeddedShot?.path == null)
            {
                return;
            }

            for (int i = 0; i < embeddedShot.path.keyframes.Count; i++)
            {
                PathPoint point = embeddedShot.path.keyframes[i];

                Handles.color = selectedEmbeddedPointIndex == i
                    ? Color.white
                    : new Color(1f, 0.8f, 0.2f, 0.95f);

                DrawEmbeddedPointSelectionHandle(point, i);
                if (selectedEmbeddedPointIndex == i)
                {
                    DrawSelectedEmbeddedPointMoveHandle(point, embeddedShot, i);
                }

                Handles.Label(point.position, $"P{i}");

                if (embeddedShot.path.pathType != PathType.Bezier)
                {
                    continue;
                }

                DrawEmbeddedBezierHandles(point, i);
            }
        }

        void DrawEmbeddedPointSelectionHandle(PathPoint point, int pointIndex)
        {
            float handleSize = HandleUtility.GetHandleSize(point.position);
            Quaternion viewRotation = SceneView.currentDrawingSceneView != null
                ? SceneView.currentDrawingSceneView.camera.transform.rotation
                : Quaternion.identity;
            float mainSize = handleSize * AnchorButtonSize;

            Handles.color = selectedEmbeddedPointIndex == pointIndex
                ? Color.white
                : new Color(1f, 0.8f, 0.2f, 0.95f);

            if (Handles.Button(point.position, viewRotation, mainSize, mainSize, Handles.SphereHandleCap))
            {
                SelectEmbeddedPoint(pointIndex, true);
            }

            DrawPointGuiButton(point.position, $"P{pointIndex}", () => SelectEmbeddedPoint(pointIndex, true));
        }

        void DrawSelectedEmbeddedPointMoveHandle(PathPoint point, CinematicShot embeddedShot, int pointIndex)
        {
            float handleSize = HandleUtility.GetHandleSize(point.position);
            Quaternion viewRotation = SceneView.currentDrawingSceneView != null
                ? SceneView.currentDrawingSceneView.camera.transform.rotation
                : Quaternion.identity;
            Vector3 offsetPosition = GetOffsetHandlePosition(point.position, viewRotation, handleSize);

            Handles.color = Color.white;

            EditorGUI.BeginChangeCheck();
            Vector3 newOffsetPosition = Handles.PositionHandle(offsetPosition, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                RecordSceneDragUndo("Move Embedded Path Point");
                point.position += newOffsetPosition - offsetPosition;
                embeddedShot.path.InvalidateCache();
                SelectEmbeddedPoint(pointIndex, false);
                MarkSceneDragDirty();
                PreviewSelectedEmbeddedPoint(false, false);
            }
        }

        static Vector3 GetOffsetHandlePosition(Vector3 pointPosition, Quaternion viewRotation, float handleSize)
        {
            return pointPosition + (viewRotation * AnchorSceneHandleOffset) * handleSize;
        }

        void DrawPointGuiButton(Vector3 worldPosition, string label, System.Action onClick)
        {
            Vector2 guiPoint = HandleUtility.WorldToGUIPoint(worldPosition);
            Rect buttonRect = new Rect(guiPoint + AnchorGuiButtonOffset, AnchorGuiButtonSize);

            Handles.BeginGUI();
            Handles.color = new Color(1f, 0.8f, 0.2f, 0.95f);
            Handles.DrawLine(guiPoint, buttonRect.center);
            if (GUI.Button(buttonRect, GC(label, "点击选中这个路径点。用于路径点与相机图标重合时避开误选。"), EditorStyles.miniButton))
            {
                onClick?.Invoke();
            }
            Handles.EndGUI();
        }

        void DrawEmbeddedBezierHandles(PathPoint point, int pointIndex)
        {
            Vector3 handleOutPos = point.position + point.tangentOut;
            Vector3 handleInPos = point.position + point.tangentIn;
            float handleSize = HandleUtility.GetHandleSize(point.position) * 0.09f;

            Handles.color = new Color(1f, 0.65f, 0.2f, 0.9f);
            Handles.DrawLine(point.position, handleOutPos);
            Handles.DrawLine(point.position, handleInPos);

            EditorGUI.BeginChangeCheck();
            var fmh_829_73_639210228350841020 = Quaternion.identity; Vector3 newHandleOut = Handles.FreeMoveHandle(handleOutPos, handleSize, Vector3.zero, Handles.DotHandleCap);
            if (EditorGUI.EndChangeCheck())
            {
                RecordSceneDragUndo("Move Embedded Handle Out");
                point.tangentOut = newHandleOut - point.position;
                if (point.lockTangents)
                {
                    point.tangentIn = -point.tangentOut;
                }
                targetSequence.clips[selectedClipIndex].embeddedShot.path.InvalidateCache();
                SelectEmbeddedPoint(pointIndex, false);
                MarkSceneDragDirty();
                PreviewSelectedEmbeddedPoint(false, false);
            }

            EditorGUI.BeginChangeCheck();
            var fmh_845_71_639210228350846450 = Quaternion.identity; Vector3 newHandleIn = Handles.FreeMoveHandle(handleInPos, handleSize, Vector3.zero, Handles.DotHandleCap);
            if (EditorGUI.EndChangeCheck())
            {
                RecordSceneDragUndo("Move Embedded Handle In");
                point.tangentIn = newHandleIn - point.position;
                if (point.lockTangents)
                {
                    point.tangentOut = -point.tangentIn;
                }
                targetSequence.clips[selectedClipIndex].embeddedShot.path.InvalidateCache();
                SelectEmbeddedPoint(pointIndex, false);
                MarkSceneDragDirty();
                PreviewSelectedEmbeddedPoint(false, false);
            }
        }

        static string GetClipLabel(int index, CameraTrackClip clip)
        {
            string cameraName = clip.sourceCamera != null ? clip.sourceCamera.name : "No Camera";
            string displayName = string.IsNullOrEmpty(clip.name) ? cameraName : clip.name;
            return $"{index + 1}. {displayName}";
        }
    }
}
