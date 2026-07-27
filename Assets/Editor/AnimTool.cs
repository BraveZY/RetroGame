#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class AnimTool : AssetPostprocessor
{
    [MenuItem("Assets/AnimTool/CreateCtrl")]
    static void CreateCtrl()
    {
        string assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
        string ctrlPath = assetPath + "/" + Path.GetFileNameWithoutExtension(assetPath) + ".controller";
        AnimatorController ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(ctrlPath);
        if (ctrl == null)
            ctrl = AnimatorController.CreateAnimatorControllerAtPath(ctrlPath);
        foreach (Object obj in Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets))
        {
            if (!(obj is GameObject))
                continue;
            GameObject model = obj as GameObject;
            string modelPath = AssetDatabase.GetAssetPath(model);
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(modelPath))
            {
                if (asset.GetType() == typeof(AnimationClip) && !asset.name.Contains("__preview__"))
                {
                    var clip = (AnimationClip)asset;
                    AnimatorControllerLayer layer;
                    if (ctrl.layers.Length > 0)
                    {
                        layer = ctrl.layers[0];
                    }
                    else
                    {
                        layer = new AnimatorControllerLayer { name = "Base Layer", stateMachine = new AnimatorStateMachine() };
                        ctrl.layers = new AnimatorControllerLayer[] { layer };
                    }
                    bool exists = false;
                    foreach (var child in layer.stateMachine.states)
                    {
                        var childClip = child.state.motion as AnimationClip;
                        if (childClip != null && childClip == clip)
                        {
                            exists = true;
                            break;
                        }
                    }
                    if (!exists)
                    {
                        AnimatorState state = ctrl.AddMotion(clip, 0);
                        state.name = obj.name;
                    }
                }
            }
        }
        AssetDatabase.Refresh();
    }
}
#endif