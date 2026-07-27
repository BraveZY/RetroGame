using UnityEngine;

// Example usage: print the active scene name on start
public class ActiveSceneNamePrinterExample : MonoBehaviour
{
    void Start()
    {
        Debug.Log(ActiveSceneNameReader.GetActiveSceneName());
    }
}
