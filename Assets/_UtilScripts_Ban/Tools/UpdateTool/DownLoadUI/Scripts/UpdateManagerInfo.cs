using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class UpdateManagerInfo : MonoBehaviour {

    public static UpdateManagerInfo Instance;
    public int DownloadType = 0;
    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
