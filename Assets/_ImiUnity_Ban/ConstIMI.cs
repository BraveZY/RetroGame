using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
*FileName:   ConstIMI.cs
*Author：    JaenyJon
*Date:       #CreateTime#
*/
public class ConstIMI
{
    public static readonly float minDis = 0.16f;  //0.06
    public static readonly float timeGap = 5f;  //3

    public static int mainId = -1;
    public static Vector3 mainPos = Vector3.zero;
    public static float mainTime = 0;

    public static int subId = -2;
    public static Vector3 subPos = Vector3.zero;
    public static float subTime = 0;


    public static void SetNewMainData(int mId, Vector3 mPos)
    {
        if (mId == -1)
        {
            //Debug.LogError("异常数据，不更新");
            return;
        }

        mainId = mId;
        mainPos = mPos;
        mainTime = Time.time;
    }

    public static bool IsMainInited()
    {
        if (mainId == -1 || mainPos == Vector3.zero || mainTime == 0)
        {  
            return false;
        }
        return true;
    }

    public static bool IsMainInLostTime()
    {
        if (Time.time - mainTime > timeGap)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public static bool IsMainInDistance(Vector3 curPos)
    {
        if (curPos == Vector3.zero)
        {
            //Debug.LogError("-------------------传入坐标错误,为0，异常");
            return false;
        }

        if (mainPos == Vector3.zero)
        {
           // Debug.LogError("---------------------Main Pos 未初始化，默认满足条件");
            return true;
        }

        if (Vector3.Distance(mainPos, curPos) < minDis)
        {
         //   Debug.LogError("-----------------------距离满足条件，判断在原始位置");
            return true;
        }

       // Debug.LogError("--------------------------false");
        return false;
    }
}
