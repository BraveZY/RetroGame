using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
public class GetAssetGUIDEditor : Editor {
	
	[MenuItem("Assets/GUID Tools/Get Asset GUID",false,999)]
	static void GetAssetGUID()
	{
		TextEditor t = new TextEditor();
		t.text = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(Selection.activeObject));

		t.OnFocus();
		t.Copy();

	}

	[MenuItem("Assets/GUID Tools/Get Asset FileID",false,999)]
	static void GetAssetFileID()
	{
		TextEditor t = new TextEditor();
		t.text = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(Selection.activeObject));

		t.OnFocus();
		t.Copy();

	}
	[MenuItem("Assets/GUID Tools/Get Asset Dependencies",false,999)]
	static void GetDependencies()
	{
		string[] str =AssetDatabase.GetDependencies(new string[]{AssetDatabase.GetAssetPath(Selection.activeObject)});
		System.Array.Sort(str);
		foreach (var item in str) {
			Debug.Log(item);

		}
	}

}
