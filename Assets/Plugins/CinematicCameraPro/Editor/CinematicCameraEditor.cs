using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace CinematicCameraPro
{
    [CustomEditor(typeof(CinematicCamera))]
    public class CinematicCameraEditor : Editor
    {
        const float AnchorButtonSize = 0.18f;
        static readonly Vector2 AnchorGuiButtonOffset = new Vector2(34f, -42f);
        static readonly Vector2 AnchorGuiButtonSize = new Vector2(54f, 24f);
        static readonly Vector3 AnchorSceneHandleOffset = new Vector3(0.58f, 0.42f, 0f);

        private CinematicCamera targetCamera;
        private int selectedShotIndex = -1;
        private int selectedAnchorIndex = -1;
        private bool sceneDragUndoRecorded;
        private bool sceneDragDirtyPending;

        static GUIContent GC(string text, string tooltip)
        {
            return new GUIContent(text, tooltip);
        }

        void OnEnable()
        {
            targetCamera = (CinematicCamera)target;
            EnsureValidSelection();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EnsureValidSelection();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(GC("Cinematic Camera Pro", "通用单机位过场控制器。用于管理镜头段落、路径运动、预览和事件。"), EditorStyles.boldLabel);
            DrawScenePathEditingToggle();
            EditorGUILayout.Space();

            DrawPlaybackControls();
            EditorGUILayout.Space();

            DrawShotsList();
            EditorGUILayout.Space();

            DrawSelectedShotDetails();
            EditorGUILayout.Space();

            DrawEvents();

            bool serializedPropertiesChanged = serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck() || serializedPropertiesChanged)
            {
                MarkTargetDirty();
            }
        }

        void DrawPlaybackControls()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(targetCamera.IsPlaying ? GC("⏸ Pause", "暂停当前镜头预览或运行时播放。") : GC("▶ Play", "播放当前相机的镜头序列。")))
            {
                if (targetCamera.IsPlaying)
                    targetCamera.Pause();
                else
                    targetCamera.Play();
            }

            if (GUILayout.Button(GC("⏹ Stop", "停止播放，并重置当前镜头播放状态。")))
            {
                targetCamera.Stop();
            }

            if (GUILayout.Button(GC("⟲ Rewind", "跳回时间起点，方便反复预览镜头开头。")))
            {
                Undo.RecordObject(targetCamera, "Rewind Cinematic Camera");
                Undo.RecordObject(targetCamera.transform, "Rewind Cinematic Camera Transform");
                var cameraComponent = targetCamera.GetComponent<Camera>();
                if (cameraComponent != null)
                {
                    Undo.RecordObject(cameraComponent, "Rewind Cinematic Camera FOV");
                }
                targetCamera.Seek(0f);
                EditorUtility.SetDirty(targetCamera);
                EditorUtility.SetDirty(targetCamera.transform);
                if (cameraComponent != null)
                {
                    EditorUtility.SetDirty(cameraComponent);
                }
                if (targetCamera.gameObject.scene.IsValid())
                {
                    EditorSceneManager.MarkSceneDirty(targetCamera.gameObject.scene);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(GC("Speed:", "播放速度倍率。1 为正常速度。"), GUILayout.Width(50));
            targetCamera.playbackSpeed = EditorGUILayout.Slider(targetCamera.playbackSpeed, 0.1f, 3f);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(GC("Play Mode:", "镜头序列的播放模式，如单次、循环或往返。"));
            targetCamera.playMode = (PlayMode)EditorGUILayout.EnumPopup(GC("", "镜头序列的播放模式，如单次、循环或往返。"), targetCamera.playMode);
            EditorGUILayout.EndHorizontal();
        }

        void DrawScenePathEditingToggle()
        {
            EditorGUILayout.Space(4f);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(
                GC("Scene Path Editing", "控制 Scene 视图中的路径点、偏移移动轴、Bezier 手柄和锚点标签。开启时会优先编辑路径点，避免相机图标抢选。"),
                EditorStyles.boldLabel);
            targetCamera.showSceneHandles = EditorGUILayout.Toggle(
                GC("", "开启后可在 Scene 视图编辑路径点；关闭后隐藏路径编辑控件，并恢复普通场景对象选择。"),
                targetCamera.showSceneHandles,
                GUILayout.Width(22));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField(
                targetCamera.showSceneHandles
                    ? "Scene 锚点编辑已开启：优先拖路径点/偏移轴；需要选相机时先关闭。"
                    : "Scene 锚点编辑已关闭：隐藏路径点控件，恢复普通场景选择。",
                EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
        }

        void DrawShotsList()
        {
            EditorGUILayout.LabelField(GC("Shots", "镜头段落列表。每个 Shot 表示一段独立的路径镜头。"), EditorStyles.boldLabel);

            for (int i = 0; i < targetCamera.shots.Count; i++)
            {
                var shot = targetCamera.shots[i];

                EditorGUILayout.BeginHorizontal();
                shot.enabled = EditorGUILayout.ToggleLeft(
                    GC("On", "是否启用这个 Shot。关闭后会在正式播放、总时长计算和顺序播放里被跳过。"),
                    shot.enabled,
                    GUILayout.Width(40));

                bool isSelected = selectedShotIndex == i;
                GUIStyle shotButtonStyle = isSelected ? EditorStyles.miniButtonMid : EditorStyles.miniButton;
                string shotLabel = shot.enabled ? $"{i + 1}. {shot.name}" : $"{i + 1}. {shot.name} (Disabled)";
                if (GUILayout.Button(GC(shotLabel, "点击这里选中当前 Shot 进行编辑和预览。"), shotButtonStyle, GUILayout.Width(140)))
                {
                    SelectShot(i);
                }

                shot.duration = Mathf.Max(0f, EditorGUILayout.FloatField(shot.duration, GUILayout.Width(45)));
                EditorGUILayout.LabelField("s", GUILayout.Width(10));

                EditorGUI.BeginDisabledGroup(!targetCamera.CanPreviewShot(i));
                if (GUILayout.Button(GC("▶", "仅播放这个 Shot，方便快速检查单段镜头。"), GUILayout.Width(25)))
                {
                    SelectShot(i);
                    targetCamera.PlayShot(i);
                }
                EditorGUI.EndDisabledGroup();

                if (GUILayout.Button(GC("▲", "将当前 Shot 上移一位。"), GUILayout.Width(25)) && i > 0)
                {
                    Undo.RecordObject(targetCamera, "Move Shot Up");
                    var temp = shot;
                    targetCamera.shots[i] = targetCamera.shots[i - 1];
                    targetCamera.shots[i - 1] = temp;
                    selectedShotIndex = i - 1;
                    EditorUtility.SetDirty(targetCamera);
                    PreviewSelectedShot(selectedShotIndex);
                    break;
                }

                if (GUILayout.Button(GC("▼", "将当前 Shot 下移一位。"), GUILayout.Width(25)) && i < targetCamera.shots.Count - 1)
                {
                    Undo.RecordObject(targetCamera, "Move Shot Down");
                    var temp = shot;
                    targetCamera.shots[i] = targetCamera.shots[i + 1];
                    targetCamera.shots[i + 1] = temp;
                    selectedShotIndex = i + 1;
                    EditorUtility.SetDirty(targetCamera);
                    PreviewSelectedShot(selectedShotIndex);
                    break;
                }

                if (GUILayout.Button(GC("✕", "删除当前 Shot。"), GUILayout.Width(25)))
                {
                    Undo.RecordObject(targetCamera, "Remove Shot");
                    targetCamera.RemoveShot(i);
                    if (selectedShotIndex == i) selectedShotIndex = -1;
                    EnsureValidSelection();
                    EditorUtility.SetDirty(targetCamera);
                    break;
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();

            if (GUILayout.Button(GC("+ Add Shot", "新增一个空白 Shot，并自动创建两个默认路径点。")))
            {
                Undo.RecordObject(targetCamera, "Add Shot");
                var shot = targetCamera.AddNewShot($"Shot {targetCamera.shots.Count + 1}");
                SelectShot(targetCamera.shots.Count - 1);
                EditorUtility.SetDirty(targetCamera);
            }

            if (GUILayout.Button(GC("+ Add Shot from Template", "从内置模板快速生成一个常用镜头段落。")))
            {
                ShowTemplateMenu();
            }
        }

        void ShowTemplateMenu()
        {
            var menu = new GenericMenu();
            var templates = BuiltInTemplates.GetAll();

            foreach (var template in templates)
            {
                menu.AddItem(new GUIContent($"{template.category}/{template.name} - {template.description}"), false, () =>
                {
                    Undo.RecordObject(targetCamera, "Add Shot From Template");
                    var shot = template.GenerateShot(null, template.defaultDuration);
                    targetCamera.shots.Add(shot);
                    SelectShot(targetCamera.shots.Count - 1);
                    EditorUtility.SetDirty(targetCamera);
                });
            }

            menu.ShowAsContext();
        }

        void DrawSelectedShotDetails()
        {
            if (selectedShotIndex < 0 || selectedShotIndex >= targetCamera.shots.Count)
                return;

            var shot = targetCamera.shots[selectedShotIndex];

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(GC($"Shot {selectedShotIndex + 1} Details", "当前 Shot 的详细配置区域。"), EditorStyles.boldLabel);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(GC("Path Settings", "配置当前 Shot 的路径类型和插值缓动。"), EditorStyles.miniBoldLabel);

            PathType newPathType = (PathType)EditorGUILayout.EnumPopup(GC("Path Type:", "路径插值类型。Linear 为直线，Bezier 为手柄曲线，Catmull-Rom 为平滑样条。"), shot.path.pathType);
            if (newPathType != shot.path.pathType)
            {
                shot.path.pathType = newPathType;
                shot.path.InvalidateCache();
                EditorUtility.SetDirty(targetCamera);
                PreviewSelectedShot(selectedShotIndex);
            }
            if (EasingSelectorGUI.Draw(shot.path, GC("Easing:", "使用 Unity 原生曲线编辑器控制镜头节奏。上方可自由加点编辑，下方保留紧凑预设快捷切换。"), targetCamera))
            {
                EditorUtility.SetDirty(targetCamera);
                PreviewSelectedShot(selectedShotIndex);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(GC("Path Points", "当前 Shot 的路径点列表。位置与 Bezier 手柄请直接在 Scene 视图中拖拽编辑。"), EditorStyles.miniBoldLabel);

            DrawAnchorsList(shot);
            DrawSelectedAnchorDetails(shot);

            EditorGUILayout.Space();

            if (GUILayout.Button(GC("+ Add Anchor", "在当前路径末尾追加一个新的路径点。")))
            {
                shot.path.AddKeyframe(null, 0f);
                selectedAnchorIndex = shot.path.keyframes.Count - 1;
                EditorUtility.SetDirty(targetCamera);
                PreviewSelectedShot(selectedShotIndex);
            }

            if (GUILayout.Button(GC("⟲ Auto Calculate Tangents", "根据相邻路径点自动生成 Bezier 手柄，快速得到平滑曲线。")))
            {
                shot.path.AutoCalculateTangents();
                EditorUtility.SetDirty(targetCamera);
                PreviewSelectedShot(selectedShotIndex);
            }

            EditorGUILayout.Space();
            DrawLookAtSettings(shot);

            EditorGUILayout.Space();
            using (new EditorGUI.DisabledScope(shot.HasLookAtTargets))
            {
                DrawPathFacingSettings(shot);
            }

            if (shot.HasLookAtTargets)
            {
                EditorGUILayout.HelpBox("当前 Shot 已设置 LookAt 目标，Path Facing 会被 LookAt 接管。清空 Target A/B 后，这里的 Mirror Facing 与 Yaw Offset 才会生效。", MessageType.Info);
            }
        }

        void DrawEvents()
        {
            EditorGUILayout.LabelField(GC("Events", "控制器级事件。可在播放、暂停、停止、完成时触发业务逻辑。"), EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onPlay"), GC("On Play", "开始播放时触发。"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onPause"), GC("On Pause", "暂停播放时触发。"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onStop"), GC("On Stop", "停止播放时触发。"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onComplete"), GC("On Complete", "整个镜头序列播放完成时触发。"));
        }

        void DrawPathFacingSettings(CinematicShot shot)
        {
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

        void DrawAnchorsList(CinematicShot shot)
        {
            for (int i = 0; i < shot.path.keyframes.Count; i++)
            {
                var anchor = shot.path.keyframes[i];

                EditorGUILayout.BeginHorizontal();

                bool isSelected = selectedAnchorIndex == i;
                GUIStyle pointButtonStyle = isSelected ? EditorStyles.miniButtonMid : EditorStyles.label;
                if (GUILayout.Button(GC($"Point {i + 1}", "选中这个路径点，并在 Scene 视图中直接拖拽位置、输入精确数值或预览画面。"), pointButtonStyle, GUILayout.Width(90)))
                {
                    SelectAnchor(selectedShotIndex, i, true);
                }

                EditorGUILayout.LabelField(GC("FOV", "该路径点的相机视场角。镜头运动到下一个路径点时会平滑过渡。"), GUILayout.Width(28));
                float newFov = EditorGUILayout.FloatField(anchor.fov, GUILayout.Width(42));
                newFov = Mathf.Clamp(newFov, 1f, 179f);
                if (!Mathf.Approximately(newFov, anchor.fov))
                {
                    Undo.RecordObject(targetCamera, "Edit Anchor FOV");
                    anchor.fov = newFov;
                    shot.path.InvalidateCache();
                    EditorUtility.SetDirty(targetCamera);
                    PreviewSelectedAnchor();
                }

                if (GUILayout.Button(GC("✕", "删除这个路径点。"), GUILayout.Width(25)))
                {
                    shot.path.RemoveKeyframe(i);
                    if (selectedAnchorIndex == i) selectedAnchorIndex = -1;
                    EditorUtility.SetDirty(targetCamera);
                    PreviewSelectedShot(selectedShotIndex);
                    break;
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        void DrawSelectedAnchorDetails(CinematicShot shot)
        {
            if (shot?.path == null || selectedAnchorIndex < 0 || selectedAnchorIndex >= shot.path.keyframes.Count)
            {
                return;
            }

            PathPoint anchor = shot.path.keyframes[selectedAnchorIndex];
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(GC($"Selected Anchor A{selectedAnchorIndex}", "当前选中锚点的精确数值。这里可避开 Scene 中相机图标重合导致的误选。"), EditorStyles.miniBoldLabel);

            EditorGUI.BeginChangeCheck();
            Vector3 newPosition = EditorGUILayout.Vector3Field(GC("Position", "锚点世界坐标。适合精确输入，避免 Scene 里拖拽误选相机。"), anchor.position);
            float newFov = EditorGUILayout.FloatField(GC("FOV", "该锚点对应的相机视场角。"), anchor.fov);
            newFov = Mathf.Clamp(newFov, 1f, 179f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(targetCamera, "Edit Anchor Values");
                anchor.position = newPosition;
                anchor.fov = newFov;
                shot.path.InvalidateCache();
                MarkTargetDirty();
                PreviewSelectedAnchor();
                SceneView.RepaintAll();
            }

            if (shot.path.pathType == PathType.Bezier)
            {
                EditorGUI.BeginChangeCheck();
                Vector3 tangentIn = EditorGUILayout.Vector3Field(GC("Tangent In", "Bezier 入手柄，相对锚点位置。"), anchor.tangentIn);
                Vector3 tangentOut = EditorGUILayout.Vector3Field(GC("Tangent Out", "Bezier 出手柄，相对锚点位置。"), anchor.tangentOut);
                bool lockTangents = EditorGUILayout.Toggle(GC("Lock Tangents", "拖动一个手柄时是否镜像另一个手柄。"), anchor.lockTangents);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(targetCamera, "Edit Anchor Tangents");
                    anchor.tangentIn = tangentIn;
                    anchor.tangentOut = tangentOut;
                    anchor.lockTangents = lockTangents;
                    shot.path.InvalidateCache();
                    MarkTargetDirty();
                    PreviewSelectedAnchor();
                    SceneView.RepaintAll();
                }
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(GC("Preview Anchor", "把当前相机预览到这个锚点对应的画面。")))
            {
                PreviewSelectedAnchor();
            }

            if (GUILayout.Button(GC("Snap To Camera", "把锚点移动到当前相机位置。")))
            {
                Undo.RecordObject(targetCamera, "Snap Anchor To Camera");
                anchor.position = targetCamera.transform.position;
                shot.path.InvalidateCache();
                MarkTargetDirty();
                PreviewSelectedAnchor();
                SceneView.RepaintAll();
            }
            EditorGUILayout.EndHorizontal();
        }

        void EnsureValidSelection()
        {
            if (targetCamera == null)
                return;

            if (targetCamera.shots.Count == 0)
            {
                selectedShotIndex = -1;
                selectedAnchorIndex = -1;
                return;
            }

            if (selectedShotIndex < 0 || selectedShotIndex >= targetCamera.shots.Count)
            {
                SelectShot(0);
            }
        }

        void SelectShot(int index)
        {
            if (targetCamera == null)
                return;
            if (index < 0 || index >= targetCamera.shots.Count)
                return;

            selectedShotIndex = index;
            selectedAnchorIndex = -1;
            PreviewSelectedShot(index);
        }

        void SelectAnchor(int shotIndex, int anchorIndex, bool preview)
        {
            if (targetCamera == null || shotIndex < 0 || shotIndex >= targetCamera.shots.Count)
            {
                return;
            }

            CinematicShot shot = targetCamera.shots[shotIndex];
            if (shot?.path == null || anchorIndex < 0 || anchorIndex >= shot.path.keyframes.Count)
            {
                return;
            }

            selectedShotIndex = shotIndex;
            selectedAnchorIndex = anchorIndex;
            if (preview)
            {
                PreviewSelectedAnchor();
            }
            Repaint();
        }

        void PreviewSelectedShot(int index)
        {
            if (targetCamera == null || targetCamera.shots.Count == 0)
                return;
            if (index < 0 || index >= targetCamera.shots.Count)
                return;
            targetCamera.PreviewShotStart(index);
        }

        void PreviewSelectedAnchor()
        {
            PreviewSelectedAnchor(true, true);
        }

        void PreviewSelectedAnchor(bool repaintAllViews, bool forceRepaint)
        {
            if (targetCamera == null || selectedShotIndex < 0 || selectedShotIndex >= targetCamera.shots.Count)
            {
                return;
            }

            CinematicShot shot = targetCamera.shots[selectedShotIndex];
            if (shot?.path == null || selectedAnchorIndex < 0 || selectedAnchorIndex >= shot.path.keyframes.Count)
            {
                PreviewSelectedShot(selectedShotIndex);
                return;
            }

            float duration = Mathf.Max(shot.path.Duration, 0.0001f);
            float pathTime = Mathf.Clamp01(shot.path.keyframes[selectedAnchorIndex].time / duration);
            targetCamera.PreviewShotAtPathTime(selectedShotIndex, pathTime, repaintAllViews, forceRepaint);
        }

        void MarkTargetDirty()
        {
            if (targetCamera == null)
            {
                return;
            }

            EditorUtility.SetDirty(targetCamera);
            if (targetCamera.gameObject.scene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(targetCamera.gameObject.scene);
            }
        }

        void OnSceneGUI()
        {
            if (targetCamera == null) return;
            if (!targetCamera.showSceneHandles) return;

            CompleteSceneDragIfNeeded();
            ReserveSceneSelectionForAnchorEditing();
            DrawSceneHandles();
        }

        void CompleteSceneDragIfNeeded()
        {
            EventType eventType = Event.current.type;
            if (eventType != EventType.MouseUp && eventType != EventType.Ignore)
            {
                return;
            }

            sceneDragUndoRecorded = false;
            if (!sceneDragDirtyPending || targetCamera == null)
            {
                return;
            }

            sceneDragDirtyPending = false;
            MarkTargetDirty();
        }

        void RecordSceneDragUndo(string undoName)
        {
            if (sceneDragUndoRecorded)
            {
                return;
            }

            Undo.RecordObject(targetCamera, undoName);
            sceneDragUndoRecorded = true;
        }

        void MarkSceneDragDirty()
        {
            EditorUtility.SetDirty(targetCamera);
            sceneDragDirtyPending = true;
        }

        void ReserveSceneSelectionForAnchorEditing()
        {
            if (Event.current.type != EventType.Layout)
            {
                return;
            }

            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        }

        void DrawSceneHandles()
        {
            for (int i = 0; i < targetCamera.shots.Count; i++)
            {
                var shot = targetCamera.shots[i];

                for (int j = 0; j < shot.path.keyframes.Count; j++)
                {
                    var anchor = shot.path.keyframes[j];
                    if (!shot.enabled)
                    {
                        Handles.color = new Color(1f, 0.8f, 0.2f, 0.2f);
                    }
                    else
                    {
                        Handles.color = selectedShotIndex == i && selectedAnchorIndex == j
                            ? Color.white
                            : new Color(1f, 0.8f, 0.2f, 0.95f);
                    }

                    EditorGUI.BeginDisabledGroup(!shot.enabled);
                    DrawAnchorSelectionHandle(anchor, i, j);
                    if (selectedShotIndex == i && selectedAnchorIndex == j)
                    {
                        DrawSelectedAnchorMoveHandle(anchor, shot, i, j);
                    }

                    Handles.Label(anchor.position, $"A{j}");

                    if (shot.path.pathType != PathType.Bezier)
                    {
                        EditorGUI.EndDisabledGroup();
                        continue;
                    }

                    DrawBezierHandles(anchor, i, j);
                    EditorGUI.EndDisabledGroup();
                }
            }
        }

        void DrawAnchorSelectionHandle(PathPoint anchor, int shotIndex, int anchorIndex)
        {
            float handleSize = HandleUtility.GetHandleSize(anchor.position);
            Quaternion viewRotation = SceneView.currentDrawingSceneView != null
                ? SceneView.currentDrawingSceneView.camera.transform.rotation
                : Quaternion.identity;
            float mainSize = handleSize * AnchorButtonSize;

            Handles.color = selectedShotIndex == shotIndex && selectedAnchorIndex == anchorIndex
                ? Color.white
                : new Color(1f, 0.8f, 0.2f, 0.95f);

            if (Handles.Button(anchor.position, viewRotation, mainSize, mainSize, Handles.SphereHandleCap))
            {
                SelectAnchor(shotIndex, anchorIndex, true);
            }

            DrawAnchorGuiButton(anchor.position, $"A{anchorIndex}", () => SelectAnchor(shotIndex, anchorIndex, true));
        }

        void DrawSelectedAnchorMoveHandle(PathPoint anchor, CinematicShot shot, int shotIndex, int anchorIndex)
        {
            float handleSize = HandleUtility.GetHandleSize(anchor.position);
            Quaternion viewRotation = SceneView.currentDrawingSceneView != null
                ? SceneView.currentDrawingSceneView.camera.transform.rotation
                : Quaternion.identity;
            Vector3 offsetPosition = GetOffsetHandlePosition(anchor.position, viewRotation, handleSize);

            Handles.color = Color.white;

            EditorGUI.BeginChangeCheck();
            Vector3 newOffsetPosition = Handles.PositionHandle(offsetPosition, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                RecordSceneDragUndo("Move Anchor");
                anchor.position += newOffsetPosition - offsetPosition;
                shot.path.InvalidateCache();
                SelectAnchor(shotIndex, anchorIndex, false);
                MarkSceneDragDirty();
                PreviewSelectedAnchor(false, false);
                Repaint();
            }
        }

        static Vector3 GetOffsetHandlePosition(Vector3 anchorPosition, Quaternion viewRotation, float handleSize)
        {
            return anchorPosition + (viewRotation * AnchorSceneHandleOffset) * handleSize;
        }

        void DrawAnchorGuiButton(Vector3 worldPosition, string label, System.Action onClick)
        {
            Vector2 guiPoint = HandleUtility.WorldToGUIPoint(worldPosition);
            Rect buttonRect = new Rect(guiPoint + AnchorGuiButtonOffset, AnchorGuiButtonSize);

            Handles.BeginGUI();
            Handles.color = new Color(1f, 0.8f, 0.2f, 0.95f);
            Handles.DrawLine(guiPoint, buttonRect.center);
            if (GUI.Button(buttonRect, GC(label, "点击选中这个锚点。用于锚点与相机图标重合时避开误选。"), EditorStyles.miniButton))
            {
                onClick?.Invoke();
            }
            Handles.EndGUI();
        }

        void DrawBezierHandles(PathPoint anchor, int shotIndex, int anchorIndex)
        {
            Vector3 handleOutPos = anchor.position + anchor.tangentOut;
            Vector3 handleInPos = anchor.position + anchor.tangentIn;
            float handleSize = HandleUtility.GetHandleSize(anchor.position) * 0.09f;

            Handles.color = new Color(1f, 0.65f, 0.2f, 0.9f);
            Handles.DrawLine(anchor.position, handleOutPos);
            Handles.DrawLine(anchor.position, handleInPos);

            EditorGUI.BeginChangeCheck();
            var fmh_803_73_639210228350760900 = Quaternion.identity; Vector3 newHandleOut = Handles.FreeMoveHandle(handleOutPos, handleSize, Vector3.zero, Handles.DotHandleCap);
            if (EditorGUI.EndChangeCheck())
            {
                RecordSceneDragUndo("Move Anchor Handle Out");
                anchor.tangentOut = newHandleOut - anchor.position;
                if (anchor.lockTangents)
                {
                    anchor.tangentIn = -anchor.tangentOut;
                }
                targetCamera.shots[shotIndex].path.InvalidateCache();
                SelectAnchor(shotIndex, anchorIndex, false);
                MarkSceneDragDirty();
                PreviewSelectedAnchor(false, false);
                Repaint();
            }

            EditorGUI.BeginChangeCheck();
            var fmh_820_71_639210228350781040 = Quaternion.identity; Vector3 newHandleIn = Handles.FreeMoveHandle(handleInPos, handleSize, Vector3.zero, Handles.DotHandleCap);
            if (EditorGUI.EndChangeCheck())
            {
                RecordSceneDragUndo("Move Anchor Handle In");
                anchor.tangentIn = newHandleIn - anchor.position;
                if (anchor.lockTangents)
                {
                    anchor.tangentOut = -anchor.tangentIn;
                }
                targetCamera.shots[shotIndex].path.InvalidateCache();
                SelectAnchor(shotIndex, anchorIndex, false);
                MarkSceneDragDirty();
                PreviewSelectedAnchor(false, false);
                Repaint();
            }
        }
    }
}
