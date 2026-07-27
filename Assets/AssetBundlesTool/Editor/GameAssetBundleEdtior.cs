using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;
public class GameAssetBundleEdtior  {
	//[MenuItem("打包工具/GameAsset/Clear Bundle Name",false,9999)]
	static void ClearBundleName()
	{
		var guids = AssetDatabase.FindAssets ("t:Object", new string[]{"Assets/GameAssets"});
		List<string> clearPaths = new List<string> ();
		foreach (var item in guids) {
			string assetPath = AssetDatabase.GUIDToAssetPath (item).Replace ("\\", "/");
			AssetImporter assetImporter = AssetImporter.GetAtPath (assetPath);
			if (assetImporter != null) {
				if (clearPaths.Contains (assetImporter.assetBundleName) == false) {
					clearPaths.Add (assetImporter.assetBundleName);
				}
				assetImporter.assetBundleName = "";
			}
		}
		foreach (var item in clearPaths) {

			AssetDatabase.RemoveAssetBundleName (item, true);
			Debug.Log ("RemoveAssetBundleName : " + item);
		}
		AssetDatabase.Refresh ();
	}

	//[MenuItem("打包工具/GameAsset/Set Bundle Name",false,9999)]
	static void SetBundleName()
	{
		var guids = AssetDatabase.FindAssets ("t:Object", new string[]{"Assets/GameAssets"});
		Dictionary<string,List<string>> dict = new Dictionary<string, List<string>> ();
		foreach (var item in guids) {
			string assetPath = AssetDatabase.GUIDToAssetPath (item).Replace("\\","/");

			AssetImporter assetImporter = AssetImporter.GetAtPath (assetPath);
			if (assetImporter != null) {
				if (string.IsNullOrEmpty (assetImporter.assetBundleName)) {
					System.Type mainAssetType = AssetDatabase.GetMainAssetTypeAtPath (assetPath);

					if (mainAssetType != typeof(UnityEditor.DefaultAsset)) {
						string parentPath = assetPath.Substring (0, assetPath.LastIndexOf ('/'));
						parentPath = parentPath.Substring (parentPath.IndexOf ('/') + 1);
						string fileName = System.IO.Path.GetFileNameWithoutExtension (assetPath);
						assetImporter.assetBundleName = parentPath + "/" + fileName;
//						List<string> ls = null;
//						dict.TryGetValue (parentPath, out ls);
//						if (ls == null) {
//							ls = new List<string> ();
//							dict.Add (parentPath, ls);
//						}
//						if (ls.Contains (assetPath) == false) {
//							ls.Add (assetPath);
//						} else {
//							Debug.Log ("Duplicate path : " + assetPath + " t:"+ mainAssetType.ToString());
//						}


					}
				}
			}
		}

//		foreach (var item in dict) {
//			StringBuilder sBuilder = new StringBuilder ();
//			sBuilder.Append (item.Key);
//			sBuilder.Append ("\n------------------------\n");
//			foreach (var v in item.Value) {
//				System.Type mainAssetType = AssetDatabase.GetMainAssetTypeAtPath (v);
//				sBuilder.Append (v);
//				sBuilder.Append ("     t:");
//				sBuilder.Append (mainAssetType);
//				sBuilder.Append ("\n");
//				AssetImporter assetImport = AssetImporter.GetAtPath (v);
//				assetImport.assetBundleName = item.Key;
//			}
//			Debug.Log (sBuilder.ToString ());
//		}
		AssetDatabase.Refresh ();

	}

	//[MenuItem("打包工具/GameAsset/Show Unused AssetBundleNames",false,9999)]
	static void ShowUnusedAssetBundleNames()
	{
		string[] unUsedassetBundlesNames  = AssetDatabase.GetUnusedAssetBundleNames ();
		foreach (var item in unUsedassetBundlesNames) {
			Debug.Log (item);
		}
	}

	//[MenuItem("打包工具/GameAsset/Clear Unused AssetBundleNames",false,9999)]
	static void ClearUnusedAssetBundleNames()
	{
		if (EditorUtility.DisplayDialog ("Clear Unused AssetBundleNames", "确定要清理没使用的AssetBundleNames吗？", "确定", "取消")) {
			string[] unUsedassetBundlesNames = AssetDatabase.GetUnusedAssetBundleNames ();
			foreach (var item in unUsedassetBundlesNames) {
				AssetDatabase.RemoveAssetBundleName (item, true);
			}
		} else {
			Debug.Log ("不清理没使用的AssetBundleNames");
		}
	}

}
