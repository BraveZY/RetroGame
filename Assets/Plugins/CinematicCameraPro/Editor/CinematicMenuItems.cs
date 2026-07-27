using UnityEngine;
using UnityEditor;

namespace CinematicCameraPro
{
    public static class CinematicMenuItems
    {
        [MenuItem("GameObject/Cinematic Camera/Cinematic Camera", false, 10)]
        public static void CreateCinematicCamera(MenuCommand menuCommand)
        {
            GameObject go = new GameObject("Cinematic Camera");
            go.AddComponent<CinematicCamera>();
            
            if (go.GetComponent<Camera>() == null)
            {
                go.AddComponent<Camera>();
            }

            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Selection.activeGameObject = go;
            
            EditorGUIUtility.PingObject(go);
        }

        [MenuItem("GameObject/Cinematic Camera/Cinematic Sequence", false, 11)]
        public static void CreateCinematicSequence(MenuCommand menuCommand)
        {
            GameObject go = new GameObject("Cinematic Sequence");
            go.AddComponent<CinematicSequence>();

            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Selection.activeGameObject = go;

            EditorGUIUtility.PingObject(go);
        }

        [MenuItem("GameObject/Cinematic Camera/Template/Linear", false, 20)]
        public static void CreateLinearTemplate(MenuCommand menuCommand)
        {
            CreateFromTemplate("Linear", menuCommand);
        }

        [MenuItem("GameObject/Cinematic Camera/Template/Orbit", false, 21)]
        public static void CreateOrbitTemplate(MenuCommand menuCommand)
        {
            CreateFromTemplate("Orbit", menuCommand);
        }

        [MenuItem("GameObject/Cinematic Camera/Template/Focus Push", false, 22)]
        public static void CreateFocusPushTemplate(MenuCommand menuCommand)
        {
            CreateFromTemplate("Focus Push", menuCommand);
        }

        [MenuItem("GameObject/Cinematic Camera/Template/Flythrough", false, 23)]
        public static void CreateFlythroughTemplate(MenuCommand menuCommand)
        {
            CreateFromTemplate("Flythrough", menuCommand);
        }

        [MenuItem("GameObject/Cinematic Camera/Template/Figure 8", false, 24)]
        public static void CreateFigure8Template(MenuCommand menuCommand)
        {
            CreateFromTemplate("Figure 8", menuCommand);
        }

        [MenuItem("GameObject/Cinematic Camera/Template/Arc", false, 25)]
        public static void CreateArcTemplate(MenuCommand menuCommand)
        {
            CreateFromTemplate("Arc", menuCommand);
        }

        static void CreateFromTemplate(string templateName, MenuCommand menuCommand)
        {
            GameObject go = new GameObject($"Cinematic Camera - {templateName}");
            var camera = go.AddComponent<CinematicCamera>();
            
            if (go.GetComponent<Camera>() == null)
            {
                go.AddComponent<Camera>();
            }

            var templates = BuiltInTemplates.GetAll();
            foreach (var template in templates)
            {
                if (template.name == templateName)
                {
                    var shot = template.GenerateShot(null, template.defaultDuration);
                    camera.shots.Add(shot);
                    break;
                }
            }

            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Selection.activeGameObject = go;
            
            EditorGUIUtility.PingObject(go);
        }
    }
}
