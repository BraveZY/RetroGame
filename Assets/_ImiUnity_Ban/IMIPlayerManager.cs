using UnityEngine;

/// <summary>
/// 用户数据管理
/// antai
/// </summary>
public class IMIPlayerManager : MonoBehaviour
{
    static IMIPlayerManager instance;
    public static IMIPlayerManager Instance
    {
        get
        {

            if (instance == null)
                instance = FindObjectOfType<IMIPlayerManager>();
            if (instance == null)
            { 
                GameObject aaa = new GameObject("IMIPlayerManager");
                instance = aaa.AddComponent<IMIPlayerManager>();
            }
            if (instance != null)
                DontDestroyOnLoad(instance.gameObject);
            return instance;
        }
    }
 

    void Awake()
    {
        if (instance == null)
            instance = this;
        if (instance == this)
            DontDestroyOnLoad(gameObject);
    }

    // Use this for initialization
    void Start()
    {
 
    }

    void OnDestroy()
    {
  
    }



    public skeleton GetMainPlayerInfo2()
    {
        //Debug.Log(SkeletonCenter.Instance);
        //Debug.Log(SkeletonCenter.Instance.Human);
        //Debug.Log(SkeletonCenter.Instance.Human.skeletons);
        if (SkeletonCenter.Instance.Human == null)
            return null;
        if (SkeletonCenter.Instance.Human.skeletons == null)
            return null;
        return SkeletonCenter.Instance.Human.skeletons[0];
    }

    public skeleton GetSubPlayerInfo2()
    {
        //Debug.Log("sub=" + SkeletonCenter.Instance);
        //Debug.Log("sub=" + SkeletonCenter.Instance.Human);
        //Debug.Log("sub=" + SkeletonCenter.Instance.Human.skeletons);
        if (SkeletonCenter.Instance.Human == null)
            return null;
        if (SkeletonCenter.Instance.Human.skeletons == null)
            return null;
        return SkeletonCenter.Instance.Human.skeletons[1];
    }
}
