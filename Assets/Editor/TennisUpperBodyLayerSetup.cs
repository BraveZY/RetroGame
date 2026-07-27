#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// 网球：从角色 FBX 生成上身 AvatarMask，并在各 AnimatorController 上新增独立 Upper Body 层（Override + Mask），
/// 与 Base 共用 Trigger；Base 可先切入 Move，本层挥拍播完再回到 Empty，仅影响 Mask 内骨骼。
/// </summary>
public static class TennisUpperBodyLayerSetup
{
    const string NewFolderRoot = "Assets/CoreGameAssets/Tennis_Assets/Modes/New Folder";
    const string HeroFbxPath = "Assets/CoreGameAssets/Tennis_Assets/Modes/New Folder/hero/hero.fbx";
    /// <summary>与旧版兼容：共用 Mask（仅 hero 骨架）；新流程优先使用角色目录下 *_UpperBody.mask。</summary>
    const string SharedFallbackMaskPath = "Assets/CoreGameAssets/Tennis_Assets/Modes/New Folder/TennisUpperBody.mask";
    const string UpperLayerName = "Upper Body";
    const string EmptyStateName = "Empty (Upper)";

    static readonly (string stateName, string triggerName)[] UpperBindings =
    {
        ("HitLeft", "HitLeft"),
        ("HitRight", "HitRight"),
        ("HitTop", "HitTop"),
        ("HitLeftLunge1", "HitLeftLunge1"),
        ("HitLeftLunge2", "HitLeftLunge2"),
        ("HitRightLunge1", "HitRightLunge1"),
        ("HitRightLunge2", "HitRightLunge2"),
        ("Save", "Save"),
        ("Smash", "Smash"),
        ("SmashPlayer", "SmashPlayer"),
        ("Serve", "Serve"),
        ("ServePlayer", "ServePlayer"),
    };

    [MenuItem("Tools/Tennis/Create Or Update Tennis Upper Body Mask (Hero Only, Shared)")]
    public static void CreateOrUpdateMask()
    {
        if (!CreateOrUpdateMaskAtPath(HeroFbxPath, SharedFallbackMaskPath, "TennisUpperBody"))
        {
            EditorUtility.DisplayDialog("Tennis Upper Body", "未能从 hero.fbx 生成 Mask，请检查路径与模型。", "OK");
            return;
        }

        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Tennis Upper Body", $"已保存共用 Mask:\n{SharedFallbackMaskPath}", "OK");
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(SharedFallbackMaskPath);
    }

    /// <summary>
    /// 扫描 New Folder 下一级子目录：若存在 目录名/目录名.fbx，则生成 目录名/目录名_UpperBody.mask（人形与四足 Generic 均适用，骨骼路径按各自 FBX）。
    /// </summary>
    [MenuItem("Tools/Tennis/Create Upper Body Masks For All Characters (New Folder)")]
    public static void CreateMasksForAllCharactersInNewFolder()
    {
        CreateMasksForAllCharactersInNewFolder_Internal(logDialog: true);
    }

    static void CreateMasksForAllCharactersInNewFolder_Internal(bool logDialog)
    {
        if (!Directory.Exists(NewFolderRoot))
        {
            if (logDialog)
                EditorUtility.DisplayDialog("Tennis Upper Body", $"未找到目录:\n{NewFolderRoot}", "OK");
            return;
        }

        int ok = 0;
        var lines = new List<string>();
        foreach (string subDir in Directory.GetDirectories(NewFolderRoot))
        {
            string folderName = Path.GetFileName(subDir);
            string fbxPath = $"{NewFolderRoot}/{folderName}/{folderName}.fbx".Replace("\\", "/");
            if (!File.Exists(fbxPath))
                continue;

            string maskPath = $"{NewFolderRoot}/{folderName}/{folderName}_UpperBody.mask".Replace("\\", "/");
            string maskName = $"{folderName}_UpperBody";
            if (CreateOrUpdateMaskAtPath(fbxPath, maskPath, maskName))
            {
                ok++;
                lines.Add(maskPath);
            }
            else
                Debug.LogWarning($"[TennisUpperBody] 跳过（无法生成）: {fbxPath}");
        }

        AssetDatabase.SaveAssets();
        foreach (var l in lines)
            Debug.Log($"[TennisUpperBody] Mask: {l}");
        if (logDialog)
            EditorUtility.DisplayDialog("Tennis Upper Body",
                $"已处理 {ok} 个角色 Mask（约定：子目录/目录名_UpperBody.mask）。\n详见 Console 列表。",
                "OK");
    }

    /// <summary>
    /// 一键：New Folder 下各角色生成/更新 *_UpperBody.mask + 为 Tennis 下所有未加层的 Controller 加 Upper Body 层。
    /// </summary>
    [MenuItem("Tools/Tennis/One Step — All Masks + Upper Body Layers")]
    public static void OneStepAllMasksAndLayers()
    {
        CreateMasksForAllCharactersInNewFolder_Internal(logDialog: false);
        AddUpperBodyLayerToAllTennisControllers_Internal(showFinishDialog: true, operationTitle: "One Step — Masks + Layers", allowProceedWithoutSharedFallback: true);
    }

    [MenuItem("Tools/Tennis/Add Upper Body Layer To All Tennis AnimatorControllers")]
    public static void AddUpperBodyLayerToAllTennisControllers()
    {
        AddUpperBodyLayerToAllTennisControllers_Internal(showFinishDialog: true, operationTitle: "Tennis Upper Body", allowProceedWithoutSharedFallback: false);
    }

    static void AddUpperBodyLayerToAllTennisControllers_Internal(bool showFinishDialog, string operationTitle, bool allowProceedWithoutSharedFallback)
    {
        if (AssetDatabase.LoadAssetAtPath<AvatarMask>(SharedFallbackMaskPath) == null && !allowProceedWithoutSharedFallback)
        {
            if (!EditorUtility.DisplayDialog(operationTitle,
                    "未找到共用 Fallback Mask。建议先执行：\n" +
                    "1) Create Upper Body Masks For All Characters\n" +
                    "或 2) Create Or Update (Hero Only)\n\n仍继续将仅用各目录已有 Mask；完全没有则可能失败。",
                    "继续", "取消"))
                return;
        }

        string[] guids = AssetDatabase.FindAssets("t:AnimatorController", new[] { "Assets/CoreGameAssets/Tennis_Assets" });
        int touched = 0;
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var ac = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if (ac == null || ac.layers == null || ac.layers.Length == 0)
                continue;
            if (HasUpperBodyLayer(ac))
            {
                Debug.Log($"[TennisUpperBody] Skip (already has layer): {path}");
                continue;
            }

            AvatarMask mask = ResolveMaskForControllerPath(path);
            if (mask == null)
            {
                Debug.LogError($"[TennisUpperBody] 无可用 Mask，请先生成: {path}");
                continue;
            }

            try
            {
                AddUpperBodyLayer(ac, mask);
                EditorUtility.SetDirty(ac);
                touched++;
                Debug.Log($"[TennisUpperBody] Added layer: {path}  Mask={AssetDatabase.GetAssetPath(mask)}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TennisUpperBody] Failed: {path}\n{ex}");
            }
        }

        AssetDatabase.SaveAssets();
        if (showFinishDialog)
            EditorUtility.DisplayDialog(operationTitle, $"处理完成，共修改 {touched} 个 Controller。", "OK");
    }

    static AvatarMask ResolveMaskForControllerPath(string controllerPath)
    {
        string dir = Path.GetDirectoryName(controllerPath)?.Replace("\\", "/") ?? "";
        string baseName = Path.GetFileNameWithoutExtension(controllerPath);
        if (!string.IsNullOrEmpty(dir))
        {
            string perCharacter = $"{dir}/{baseName}_UpperBody.mask";
            var m = AssetDatabase.LoadAssetAtPath<AvatarMask>(perCharacter);
            if (m != null)
                return m;
        }

        var fallback = AssetDatabase.LoadAssetAtPath<AvatarMask>(SharedFallbackMaskPath);
        if (fallback != null)
        {
            Debug.LogWarning($"[TennisUpperBody] 使用共用 Fallback Mask: {controllerPath}");
            return fallback;
        }

        return null;
    }

    static bool CreateOrUpdateMaskAtPath(string fbxAssetPath, string maskAssetPath, string maskAssetName)
    {
        var mask = BuildMaskFromFbx(fbxAssetPath, maskAssetName);
        if (mask == null)
            return false;

        var existing = AssetDatabase.LoadAssetAtPath<AvatarMask>(maskAssetPath);
        if (existing != null)
            EditorUtility.CopySerialized(mask, existing);
        else
        {
            Directory.CreateDirectory(Path.GetDirectoryName(maskAssetPath) ?? "");
            AssetDatabase.CreateAsset(mask, maskAssetPath);
        }

        return true;
    }

    static bool HasUpperBodyLayer(AnimatorController ac)
    {
        foreach (var layer in ac.layers)
        {
            if (layer.name == UpperLayerName)
                return true;
        }
        return false;
    }

    static AvatarMask BuildMaskFromFbx(string fbxAssetPath, string maskDisplayName)
    {
        var main = AssetDatabase.LoadMainAssetAtPath(fbxAssetPath);
        if (main == null || !(main is GameObject fbxRoot))
            return null;

        GameObject instance = null;
        try
        {
            instance = PrefabUtility.InstantiatePrefab(fbxRoot) as GameObject;
            if (instance == null)
                return null;

            var animator = instance.GetComponentInChildren<Animator>();
            Transform hierarchyRoot = animator != null ? animator.transform : instance.transform;

            var mask = new AvatarMask { name = maskDisplayName };

            if (!TryBuildUpperBodyMaskBipedSubtree(hierarchyRoot, mask))
            {
                Debug.LogWarning($"[TennisUpperBody] 未识别为标准 Biped(Pelvis→Spine)，回退为按路径关键字排除腿脚: {fbxAssetPath}");
                BuildUpperBodyMaskByExcludingLegTokens(hierarchyRoot, mask);
            }

            AddRacketPropTransforms(hierarchyRoot, mask);
            return mask;
        }
        finally
        {
            if (instance != null)
                UnityEngine.Object.DestroyImmediate(instance);
        }
    }

    /// <summary>
    /// 3ds Max Biped：Pelvis → 躯干起点（多为 Spine），向下 DFS；凡子节点名为腿链则整枝不加入 Mask。
    /// 兼容腿挂在 Pelvis 下与挂在 Spine 下两种层级（后者若整段 AddTransformPath(recursive) 会把两腿全包进去）。
    /// </summary>
    static bool TryBuildUpperBodyMaskBipedSubtree(Transform hierarchyRoot, AvatarMask mask)
    {
        Transform pelvis = FindBipedPelvis(hierarchyRoot);
        if (pelvis == null)
            return false;

        Transform spineStart = FindFirstNonLegTorsoChild(pelvis);
        if (spineStart == null)
            return false;

        mask.AddTransformPath(pelvis, false);
        AddUpperBodySubtreeSkippingLegBranches(spineStart, mask);
        return true;
    }

    static void AddUpperBodySubtreeSkippingLegBranches(Transform t, AvatarMask mask)
    {
        if (t == null)
            return;
        mask.AddTransformPath(t, false);
        for (int i = 0; i < t.childCount; i++)
        {
            Transform c = t.GetChild(i);
            if (IsLegBoneName(c.name))
                continue;
            AddUpperBodySubtreeSkippingLegBranches(c, mask);
        }
    }

    static Transform FindBipedPelvis(Transform root)
    {
        foreach (var t in root.GetComponentsInChildren<Transform>(true))
        {
            string n = t.name;
            if (n.IndexOf("Pelvis", StringComparison.OrdinalIgnoreCase) < 0)
                continue;
            if (n.IndexOf("Breast", StringComparison.OrdinalIgnoreCase) >= 0)
                continue;
            return t;
        }
        return null;
    }

    static Transform FindFirstNonLegTorsoChild(Transform pelvis)
    {
        Transform fallback = null;
        for (int i = 0; i < pelvis.childCount; i++)
        {
            Transform c = pelvis.GetChild(i);
            if (IsLegBoneName(c.name))
                continue;

            string l = c.name.ToLowerInvariant();
            if (l.Contains("spine") || l.Contains("neck") || l.Contains("spine1"))
                return c;

            if (fallback == null)
                fallback = c;
        }
        return fallback;
    }

    static bool IsLegBoneName(string boneName)
    {
        string n = boneName.ToLowerInvariant();
        string[] hints =
        {
            "thigh", "calf", "foot", "toe", "upleg", "loleg", "upperleg", "lowerleg",
            "knee", "shin", "leg", "hoof", "paw", "horselink", "up leg", "lo leg", "footsteps"
        };
        foreach (var h in hints)
        {
            if (n.Contains(h))
                return true;
        }
        return false;
    }

    static void BuildUpperBodyMaskByExcludingLegTokens(Transform hierarchyRoot, AvatarMask mask)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var t in hierarchyRoot.GetComponentsInChildren<Transform>(true))
        {
            string path = AnimationUtility.CalculateTransformPath(t, hierarchyRoot);
            if (string.IsNullOrEmpty(path) || !seen.Add(path))
                continue;
            if (ShouldExcludeBoneFromUpperBodyMask(path))
                continue;
            mask.AddTransformPath(t, false);
        }
    }

    static void AddRacketPropTransforms(Transform hierarchyRoot, AvatarMask mask)
    {
        foreach (var t in hierarchyRoot.GetComponentsInChildren<Transform>(true))
        {
            string l = t.name.ToLowerInvariant();
            if (l.Contains("racket") || l.Contains("racquet") || l.Contains("weapon"))
                mask.AddTransformPath(t, true);
        }
    }

    static bool ShouldExcludeBoneFromUpperBodyMask(string bonePath)
    {
        string p = bonePath.Replace('\\', '/');
        string[] legTokens =
        {
            "Leg", "Thigh", "UpLeg", "LoLeg", "Calf", "Foot", "Toe", "Knee", "Shin",
            "leg", "foot", "toe", "calf", "thigh", "Hoof", "hoof", "Paw", "paw", "HorseLink", "Footsteps"
        };
        foreach (var tok in legTokens)
        {
            if (p.IndexOf(tok, StringComparison.Ordinal) >= 0)
                return true;
        }
        return false;
    }

    static void AddUpperBodyLayer(AnimatorController ac, AvatarMask mask)
    {
        AnimatorStateMachine baseSm = ac.layers[0].stateMachine;
        if (baseSm == null)
            throw new InvalidOperationException("Base layer state machine is null");

        var upperSm = new AnimatorStateMachine { name = UpperLayerName, hideFlags = HideFlags.HideInHierarchy };
        AssetDatabase.AddObjectToAsset(upperSm, ac);

        AnimatorState empty = upperSm.AddState(EmptyStateName);
        upperSm.defaultState = empty;

        foreach (var (stateName, triggerName) in UpperBindings)
        {
            if (!HasTriggerParameter(ac, triggerName))
                continue;

            AnimatorState src = FindStateByName(baseSm, stateName);
            if (src == null)
                continue;

            AnimatorState st = upperSm.AddState(stateName);
            st.motion = src.motion;
            CopyStatePlayback(src, st);

            AnimatorStateTransition anyToHit = upperSm.AddAnyStateTransition(st);
            anyToHit.canTransitionToSelf = false;
            anyToHit.duration = 0f;
            anyToHit.hasFixedDuration = true;
            anyToHit.interruptionSource = TransitionInterruptionSource.None;
            anyToHit.AddCondition(AnimatorConditionMode.If, 0, triggerName);

            AnimatorStateTransition hitToEmpty = st.AddTransition(empty);
            hitToEmpty.hasExitTime = true;
            hitToEmpty.exitTime = 0.92f;
            hitToEmpty.duration = 0.18f;
            hitToEmpty.hasFixedDuration = true;
            hitToEmpty.interruptionSource = TransitionInterruptionSource.None;
        }

        var layer = new AnimatorControllerLayer
        {
            name = UpperLayerName,
            stateMachine = upperSm,
            defaultWeight = 1f,
            avatarMask = mask,
            blendingMode = AnimatorLayerBlendingMode.Override,
            syncedLayerIndex = -1,
            iKPass = false,
        };

        ac.AddLayer(layer);
    }

    static bool HasTriggerParameter(AnimatorController ac, string name)
    {
        foreach (var p in ac.parameters)
        {
            if (p.name == name && p.type == AnimatorControllerParameterType.Trigger)
                return true;
        }
        return false;
    }

    static AnimatorState FindStateByName(AnimatorStateMachine sm, string name)
    {
        foreach (ChildAnimatorState cs in sm.states)
        {
            if (cs.state != null && cs.state.name == name)
                return cs.state;
        }
        foreach (ChildAnimatorStateMachine csm in sm.stateMachines)
        {
            AnimatorState found = FindStateByName(csm.stateMachine, name);
            if (found != null)
                return found;
        }
        return null;
    }

    static void CopyStatePlayback(AnimatorState from, AnimatorState to)
    {
        if (from == null || to == null)
            return;
        to.speed = from.speed;
        to.mirror = from.mirror;
        to.iKOnFeet = from.iKOnFeet;
        to.writeDefaultValues = from.writeDefaultValues;
        to.cycleOffset = from.cycleOffset;
        to.timeParameterActive = from.timeParameterActive;
        to.timeParameter = from.timeParameter;
        to.speedParameterActive = from.speedParameterActive;
        to.speedParameter = from.speedParameter;
        to.mirrorParameterActive = from.mirrorParameterActive;
        to.mirrorParameter = from.mirrorParameter;
        to.cycleOffsetParameterActive = from.cycleOffsetParameterActive;
        to.cycleOffsetParameter = from.cycleOffsetParameter;
    }
}
#endif
