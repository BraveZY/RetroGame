using UnityEditor;
using UnityEngine;

namespace CinematicCameraPro
{
    static class EasingSelectorGUI
    {
        const float CurveHeight = 96f;

        static readonly EasingType[] Presets =
        {
            EasingType.Linear,
            EasingType.EaseIn,
            EasingType.EaseOut,
            EasingType.EaseInOut,
            EasingType.Smooth
        };

        public static bool Draw(CinematicPath path, GUIContent label, Object undoTarget)
        {
            if (path == null)
            {
                return false;
            }

            EnsureCurve(path);

            bool changed = false;
            bool undoRecorded = false;
            EasingType? activePreset = GetMatchedPreset(path.easingCurve);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, EditorStyles.label);
            GUIStyle badgeStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleRight
            };
            EditorGUILayout.LabelField(
                new GUIContent(GetModeLabel(activePreset), "Preset 表示当前曲线仍是标准预设；Custom 表示你已经手动编辑过曲线。"),
                badgeStyle,
                GUILayout.Width(120f));
            EditorGUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();
            AnimationCurve editedCurve = EditorGUILayout.CurveField(
                new GUIContent(string.Empty, "双击可添加控制点，拖拽可精细调整镜头节奏。"),
                path.easingCurve,
                new Color(0.55f, 0.82f, 1f, 1f),
                new Rect(0f, 0f, 1f, 1f),
                GUILayout.Height(CurveHeight));
            if (EditorGUI.EndChangeCheck())
            {
                RecordUndo(undoTarget, ref undoRecorded);
                path.easingCurve = SanitizeCurve(editedCurve);
                changed = true;
                activePreset = GetMatchedPreset(path.easingCurve);
            }

            EditorGUILayout.LabelField(
                new GUIContent("可直接在上方曲线中加点，做出前段慢、中段快、末段再收住的镜头节奏。"),
                EditorStyles.miniLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(new GUIContent("Quick Presets", "点击快速切回常用缓动。保持当前主界面快速调参体验。"));
            for (int i = 0; i < Presets.Length; i++)
            {
                var preset = Presets[i];
                bool isSelected = activePreset.HasValue && activePreset.Value == preset;
                GUIStyle buttonStyle = GetPresetButtonStyle(i, Presets.Length);
                Color previousColor = GUI.backgroundColor;
                if (isSelected)
                {
                    GUI.backgroundColor = new Color(0.38f, 0.62f, 0.98f, 1f);
                }

                if (GUILayout.Button(new GUIContent(GetPresetShortLabel(preset), GetPresetTooltip(preset)), buttonStyle, GUILayout.Width(58f)))
                {
                    RecordUndo(undoTarget, ref undoRecorded);
                    path.SetEasingPreset(preset);
                    changed = true;
                    activePreset = preset;
                }

                GUI.backgroundColor = previousColor;
            }

            if (GUILayout.Button(new GUIContent("Reset", "将曲线恢复为当前预设对应的默认形态。"), EditorStyles.miniButtonRight, GUILayout.Width(52f)))
            {
                RecordUndo(undoTarget, ref undoRecorded);
                path.SetEasingPreset(activePreset ?? path.easingType);
                changed = true;
                activePreset = GetMatchedPreset(path.easingCurve);
            }
            EditorGUILayout.EndHorizontal();

            if (changed)
            {
                path.easingType = DetectBestPreset(path.easingCurve, path.easingType);
            }

            EditorGUILayout.EndVertical();
            return changed;
        }

        static void RecordUndo(Object undoTarget, ref bool undoRecorded)
        {
            if (undoRecorded || undoTarget == null)
            {
                return;
            }

            Undo.RecordObject(undoTarget, "Edit Easing Curve");
            undoRecorded = true;
        }

        static void EnsureCurve(CinematicPath path)
        {
            if (path.easingCurve == null || path.easingCurve.length == 0)
            {
                path.SetEasingPreset(path.easingType);
                return;
            }

            path.easingCurve = SanitizeCurve(path.easingCurve);
        }

        static AnimationCurve SanitizeCurve(AnimationCurve source)
        {
            if (source == null || source.length < 2)
            {
                return CinematicPath.CreatePresetCurve(EasingType.EaseOut);
            }

            Keyframe[] sourceKeys = source.keys;
            Keyframe[] keys = new Keyframe[sourceKeys.Length];
            for (int i = 0; i < sourceKeys.Length; i++)
            {
                Keyframe key = sourceKeys[i];
                key.time = Mathf.Clamp01(key.time);
                key.value = Mathf.Clamp01(key.value);
                keys[i] = key;
            }

            System.Array.Sort(keys, (a, b) => a.time.CompareTo(b.time));

            keys[0].time = 0f;
            keys[0].value = 0f;
            keys[keys.Length - 1].time = 1f;
            keys[keys.Length - 1].value = 1f;

            float runningValue = keys[0].value;
            for (int i = 1; i < keys.Length; i++)
            {
                runningValue = Mathf.Max(runningValue, keys[i].value);
                keys[i].value = runningValue;
            }

            var sanitizedCurve = new AnimationCurve(keys)
            {
                preWrapMode = WrapMode.ClampForever,
                postWrapMode = WrapMode.ClampForever
            };
            return sanitizedCurve;
        }

        static bool MatchesPreset(AnimationCurve curve, EasingType preset)
        {
            AnimationCurve presetCurve = CinematicPath.CreatePresetCurve(preset);
            if (curve == null || curve.length != presetCurve.length)
            {
                return false;
            }

            Keyframe[] a = curve.keys;
            Keyframe[] b = presetCurve.keys;
            for (int i = 0; i < a.Length; i++)
            {
                if (Mathf.Abs(a[i].time - b[i].time) > 0.001f ||
                    Mathf.Abs(a[i].value - b[i].value) > 0.001f ||
                    Mathf.Abs(a[i].inTangent - b[i].inTangent) > 0.05f ||
                    Mathf.Abs(a[i].outTangent - b[i].outTangent) > 0.05f)
                {
                    return false;
                }
            }

            return true;
        }

        static EasingType DetectBestPreset(AnimationCurve curve, EasingType fallback)
        {
            EasingType? matchedPreset = GetMatchedPreset(curve);
            if (matchedPreset.HasValue)
            {
                return matchedPreset.Value;
            }

            return fallback;
        }

        static EasingType? GetMatchedPreset(AnimationCurve curve)
        {
            foreach (var preset in Presets)
            {
                if (MatchesPreset(curve, preset))
                {
                    return preset;
                }
            }

            return null;
        }

        static GUIStyle GetPresetButtonStyle(int index, int total)
        {
            if (index == 0)
            {
                return EditorStyles.miniButtonLeft;
            }

            if (index == total - 1)
            {
                return EditorStyles.miniButtonMid;
            }

            return EditorStyles.miniButtonMid;
        }

        static string GetModeLabel(EasingType? activePreset)
        {
            if (!activePreset.HasValue)
            {
                return "Custom";
            }

            return "Preset: " + GetPresetShortLabel(activePreset.Value);
        }

        static string GetPresetShortLabel(EasingType preset)
        {
            switch (preset)
            {
                case EasingType.Linear:
                    return "Linear";
                case EasingType.EaseIn:
                    return "In";
                case EasingType.EaseOut:
                    return "Out";
                case EasingType.EaseInOut:
                    return "InOut";
                case EasingType.Smooth:
                    return "Smooth";
                default:
                    return preset.ToString();
            }
        }

        static string GetPresetTooltip(EasingType preset)
        {
            switch (preset)
            {
                case EasingType.Linear:
                    return "匀速。";
                case EasingType.EaseIn:
                    return "前慢后快。";
                case EasingType.EaseOut:
                    return "前快后慢。";
                case EasingType.EaseInOut:
                    return "两端平滑。";
                case EasingType.Smooth:
                    return "整体更柔和自然。";
                default:
                    return preset.ToString();
            }
        }
    }
}
