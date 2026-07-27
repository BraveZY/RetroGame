using UnityEngine;
using System.Collections;
/// <summary>
/// 框架主入口
/// </summary>
public class Main : MonoBehaviour
{
	[SerializeField]
	private bool LoadResBundle = true;

    public static bool useBundle
    {
        get
        {
            if (Instance != null)
                return Instance.LoadResBundle;
            return false;
        }
    }


	private static Main Instance;

	void Awake()
	{
		Instance = this;
		//防止销毁自己
		DontDestroyOnLoad(gameObject);
	}



}