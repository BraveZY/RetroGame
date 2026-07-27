using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonView : MonoBehaviour
{
    skeleton iplayerInfos1;
    skeleton iplayerInfos2;
    public List<Transform> obj1;
    public List<Transform> obj2;

    // Start is called before the first frame update
    void Start()
    {
        iplayerInfos1 = IMIPlayerManager.Instance.GetMainPlayerInfo2();
        iplayerInfos2 = IMIPlayerManager.Instance.GetSubPlayerInfo2();


    }
    public void Update1()
    {
        if (iplayerInfos1 == null)
            return;
        if (iplayerInfos1.points == null)
            return;
        if (!iplayerInfos1.IsTracked)
            return;
        obj1[0].localPosition = new Vector3(0, 0, 0);
    }
    public void Update2()
    {
        if (iplayerInfos2 == null)
            return;
        if (iplayerInfos2.points == null)
            return;
        if (!iplayerInfos2.IsTracked)
            return;
        obj2[0].localPosition = new Vector3(0, 0, 0);
    }
    // Update is called once per frame
    void Update()
    {
        Update1();
        Update2();

    }
}
