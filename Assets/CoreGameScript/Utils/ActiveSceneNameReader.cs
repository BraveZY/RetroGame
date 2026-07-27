using UnityEngine;
using UnityEngine.SceneManagement;

// Unity MCP utility: read the current active scene name
public static class ActiveSceneNameReader
{
    public static string GetActiveSceneName()
    {
        UnityEngine.SceneManagement.Scene scene = SceneManager.GetActiveScene();
        return scene.IsValid() ? scene.name : string.Empty;
    }
}
