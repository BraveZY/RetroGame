using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;
public class SceneManagerEditor : Editor {

    //[MenuItem("PrefabTools/DisconnectPrefabInstance")]
    //static void DisconnectPrefabInstance()
    //{
    //    if (Selection.activeGameObject != null) {
    //        PrefabUtility.DisconnectPrefabInstance (Selection.activeGameObject);
    //    }
    //}

    //[MenuItem("PrefabTools/ResetPrefabInstance")]
    //static void ResetPrefabInstance()
    //{
    //    if (Selection.activeGameObject != null) {
    //        PrefabUtility.ResetToPrefabState (Selection.activeGameObject);
    //        Debug.Log (PrefabUtility.GetPrefabType (Selection.activeGameObject));
    //    }
    //}

    //[MenuItem("Open Scene/选中场景")]
    //static void SelectedScene()
    //{
    //    Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset> (EditorSceneManager.GetActiveScene ().path);
    //}
	//[MenuItem("热更模式切换/热更新场景")]
	static void LoadBeginScene()
	{

		EditorSceneManager.SaveModifiedScenesIfUserWantsTo (new UnityEngine.SceneManagement.Scene[]{EditorSceneManager.GetActiveScene ()});
//		EditorSceneManager.OpenScene ("Assets/Scenes/UpdateScene.unity");
        string openSceneName = "DownLoad.unity";
		string[] activeList = EditorBuildSettingsScene.GetActiveSceneList (EditorBuildSettings.scenes);
		foreach (var item in activeList) {
			if (item.EndsWith (openSceneName)) {
				EditorSceneManager.OpenScene (item);
				break;
			}
		}
        System.IO.Directory.Move("Assets/Resources", "Assets/ResourcesCopy");
        System.IO.Directory.Move("Assets/ResourcesDown", "Assets/Resources");
        UnityEditor.AssetDatabase.Refresh();

	}
    //[MenuItem("热更模式切换/本地场景")]
	static void LoadLoginScene()
	{

		EditorSceneManager.SaveModifiedScenesIfUserWantsTo (new UnityEngine.SceneManagement.Scene[]{EditorSceneManager.GetActiveScene ()});
//		EditorSceneManager.OpenScene ("Assets/Scenes/MainLogin.unity");
        string openSceneName = "S_First.unity";
		string[] activeList = EditorBuildSettingsScene.GetActiveSceneList (EditorBuildSettings.scenes);
		foreach (var item in activeList) {
			if (item.EndsWith (openSceneName)) {
				EditorSceneManager.OpenScene (item);
				break;
			}
		}
        System.IO.Directory.Move("Assets/Resources", "Assets/ResourcesDown");
        System.IO.Directory.Move("Assets/ResourcesCopy", "Assets/Resources");
        UnityEditor.AssetDatabase.Refresh();
	}

	//[MenuItem("Open Scene/主菜单场景")]
	static void LoadMainScene()
	{
		EditorSceneManager.SaveModifiedScenesIfUserWantsTo (new UnityEngine.SceneManagement.Scene[]{EditorSceneManager.GetActiveScene ()});
		//		EditorSceneManager.OpenScene ("Assets/Scenes/MainMenu.unity");
		string openSceneName = "MainMenu.unity";
		string[] activeList = EditorBuildSettingsScene.GetActiveSceneList (EditorBuildSettings.scenes);
		foreach (var item in activeList) {
			if (item.EndsWith (openSceneName)) {
				EditorSceneManager.OpenScene (item);
				return;
			}
		}
	}

	//[MenuItem("Open Scene/比赛场场景")]
	static void LoadMainRank()
	{
		EditorSceneManager.SaveModifiedScenesIfUserWantsTo (new UnityEngine.SceneManagement.Scene[]{EditorSceneManager.GetActiveScene ()});
		//		EditorSceneManager.OpenScene ("Assets/Scenes/MainMenu.unity");
		string openSceneName = "MainRank.unity";
		string[] activeList = EditorBuildSettingsScene.GetActiveSceneList (EditorBuildSettings.scenes);
		foreach (var item in activeList) {
			if (item.EndsWith (openSceneName)) {
				EditorSceneManager.OpenScene (item);
				return;
			}
		}
	}
//
//	[MenuItem("Open Scene/南宁麻将场景")]
//	static void LoadMainGame_NNMJ()
//	{
//		EditorSceneManager.SaveModifiedScenesIfUserWantsTo (new UnityEngine.SceneManagement.Scene[]{EditorSceneManager.GetActiveScene ()});
////		EditorSceneManager.OpenScene ("Assets/Scenes/MainGame_NNMJ.unity");
//		string openSceneName = "MainGame_NNMJ.unity";
//		string[] activeList = EditorBuildSettingsScene.GetActiveSceneList (EditorBuildSettings.scenes);
//		foreach (var item in activeList) {
//			if (item.EndsWith (openSceneName)) {
//				EditorSceneManager.OpenScene (item);
//				return;
//			}
//		}
//	}
//
//	[MenuItem("Open Scene/红中麻将场景")]
//	static void LoadMainGame_HZMJ()
//	{
//		EditorSceneManager.SaveModifiedScenesIfUserWantsTo (new UnityEngine.SceneManagement.Scene[]{EditorSceneManager.GetActiveScene ()});
////		EditorSceneManager.OpenScene ("Assets/Scenes/MainGame_HZMJ.unity");
//		string openSceneName = "MainGame_HZMJ.unity";
//		string[] activeList = EditorBuildSettingsScene.GetActiveSceneList (EditorBuildSettings.scenes);
//		foreach (var item in activeList) {
//			if (item.EndsWith (openSceneName)) {
//				EditorSceneManager.OpenScene (item);
//				return;
//			}
//		}
//	}
//
//	[MenuItem("Open Scene/DDZ场景")]
//	static void LoadMainGame_DDZ()
//	{
//		EditorSceneManager.SaveModifiedScenesIfUserWantsTo (new UnityEngine.SceneManagement.Scene[]{EditorSceneManager.GetActiveScene ()});
////		EditorSceneManager.OpenScene ("Assets/Scenes/MainGame_DDZ.unity");
//		string openSceneName = "MainGame_DDZ.unity";
//		string[] activeList = EditorBuildSettingsScene.GetActiveSceneList (EditorBuildSettings.scenes);
//		foreach (var item in activeList) {
//			if (item.EndsWith (openSceneName)) {
//				EditorSceneManager.OpenScene (item);
//				return;
//			}
//		}
//	}
//
//	[MenuItem("Open Scene/斗地主场景")]
//	static void LoadMainGame_DouDiZhu()
//	{
//		EditorSceneManager.SaveModifiedScenesIfUserWantsTo (new UnityEngine.SceneManagement.Scene[]{EditorSceneManager.GetActiveScene ()});
//		EditorSceneManager.OpenScene ("Assets/Scenes/MainGame_DouDiZhu.unity");
//	}
//
//	[MenuItem("Open Scene/高安麻将场景")]
//	static void LoadMainGame_GAMJ()
//	{
//		EditorSceneManager.SaveModifiedScenesIfUserWantsTo (new UnityEngine.SceneManagement.Scene[]{EditorSceneManager.GetActiveScene ()});
//		//		EditorSceneManager.OpenScene ("Assets/Scenes/MainGame_DDZ.unity");
//		string openSceneName = "MainGame_GAMJ.unity";
//		string[] activeList = EditorBuildSettingsScene.GetActiveSceneList (EditorBuildSettings.scenes);
//		foreach (var item in activeList) {
//			if (item.EndsWith (openSceneName)) {
//				EditorSceneManager.OpenScene (item);
//				return;
//			}
//		}
//	}
//
//	[MenuItem("Open Scene/高安清混麻将场景")]
//	static void LoadMainGame_GAQH()
//	{
//		EditorSceneManager.SaveModifiedScenesIfUserWantsTo (new UnityEngine.SceneManagement.Scene[]{EditorSceneManager.GetActiveScene ()});
//		//		EditorSceneManager.OpenScene ("Assets/Scenes/MainGame_DDZ.unity");
//		string openSceneName = "MainGame_GAQH.unity";
//		string[] activeList = EditorBuildSettingsScene.GetActiveSceneList (EditorBuildSettings.scenes);
//		foreach (var item in activeList) {
//			if (item.EndsWith (openSceneName)) {
//				EditorSceneManager.OpenScene (item);
//				return;
//			}
//		}
//	}

	//[MenuItem("Open Scene/打开最后一个应用场景")]
	static void LoadMainGame_Last()
	{
		EditorSceneManager.SaveModifiedScenesIfUserWantsTo (new UnityEngine.SceneManagement.Scene[]{EditorSceneManager.GetActiveScene ()});
		string[] activeList = EditorBuildSettingsScene.GetActiveSceneList (EditorBuildSettings.scenes);
		int index = activeList.Length ;
		while (index > 1) {
			index--;
			if (activeList [index].Contains ("MainGame")) {
				EditorSceneManager.OpenScene (activeList [index]);
				return;
			}
		}
	}
}
