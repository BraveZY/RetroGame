using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompareData
{
    public int gestureId;
    public double compareTime;    
}


public class DanceGesture
{
    public int gestureId;

    public double compareTime;

    public List<Vector3> bonePos;

    public double startCheckTime;

    public double endCheckTime;
}

