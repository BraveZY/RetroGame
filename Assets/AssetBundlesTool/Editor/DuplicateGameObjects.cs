using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class DuplicateGameObjects : ScriptableWizard {

	public GameObject parentGo;
	public string nameFormat;

	public int startIndex ;
	public int maxIndex;

	[MenuItem("GameObject/Duplicate GameObjects")]
	static void CreateWizard()
	{
		ScriptableWizard.DisplayWizard<DuplicateGameObjects>("Duplicate GameObjects", "Close", "Create");

	}


	void OnWizardCreate()
	{
	}


	// When the user presses the "Apply" button OnWizardOtherButton is called.
	void OnWizardOtherButton()
	{

		for (int i = startIndex; i <= maxIndex; i++) {
			GameObject go = Instantiate<GameObject>(parentGo);
			go.transform.parent = parentGo.transform.parent;
			go.transform.localScale = Vector3.one;
			go.transform.localPosition = Vector3.zero;
			go.name = string.Format(nameFormat,i);

		}

	}

}
