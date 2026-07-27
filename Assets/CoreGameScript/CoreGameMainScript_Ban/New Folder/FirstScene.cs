using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FirstScene : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        if (AssetBundlesInit.Ins != null && AssetBundlesInit.Ins.isUpdate)
        {
            ResourceManager.LoadAssetBundle("CoreGameInit");
        }
        SceneManager.LoadScene("CoreGameInit");
    }

    // Update is called once per frame
    void Update()
    {

    }
}
