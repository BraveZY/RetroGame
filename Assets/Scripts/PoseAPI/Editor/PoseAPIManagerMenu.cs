using UnityEditor;
using UnityEngine;

namespace PoseAI.Editor
{
    /// <summary>
    /// Pose API 创建菜单只建立最小核心对象，并保留完整 Undo。
    /// </summary>
    internal static class PoseAPIManagerMenu
    {
        [MenuItem("GameObject/Pose API/Pose API Manager", false, 10)]
        private static void CreatePoseAPIManager(MenuCommand command)
        {
            var managerObject = new GameObject("Pose API Manager");
            Undo.RegisterCreatedObjectUndo(managerObject, "Create Pose API Manager");

            if (command.context is GameObject parent)
            {
                GameObjectUtility.SetParentAndAlign(managerObject, parent);
            }

            Undo.AddComponent<PoseDataSourceManager>(managerObject);
            Undo.AddComponent<PoseDataManager>(managerObject);
            Selection.activeGameObject = managerObject;
            EditorGUIUtility.PingObject(managerObject);
        }
    }
}
