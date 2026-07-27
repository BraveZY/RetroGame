using UnityEngine;
using System.Collections;
using UnityEditor;
public class AssetBundlePreview : ScriptableWizard {

    //[MenuItem ("Bundle/Preview",false,999)]
    //static void CreateWindow() {
    //    // Creates the wizard for display
    //    ScriptableWizard.DisplayWizard("AssetBundle Preview", 
    //                                   typeof(AssetBundlePreview), 
    //                                   "Close","Preview");
		
    //}
	public string basePath;
	public string subPath = "/AssetBundles/";
	public string assetBundleName="";
	public string extension = ".unity3d";
	public AssetBundle assetBundle;
	public Object[] objs;
	void OnEnable()
	{
		basePath = Application.dataPath.Replace("\\","/");
		basePath = basePath.Substring(0,basePath.LastIndexOf("/"));
	}
	void OnWizardCreate () {



	}

	void OnWizardOtherButton()
	{
		if (assetBundle != null) {
			assetBundle.Unload(true);
		}
		assetBundle = AssetBundle.LoadFromFile (basePath + subPath + assetBundleName + extension);
		if (assetBundle != null) 
		{
			objs = assetBundle.LoadAllAssets();
		}
	}

	void OnDisable()
	{
		if (assetBundle != null) 
		{
			assetBundle.Unload(true);
		}
	}

}
