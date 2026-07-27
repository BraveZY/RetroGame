using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPosition
{
    private static PlayerPosition ins = null;
    public static PlayerPosition Ins
    {
        get
        {
            if (ins == null)
            {
                ins = new PlayerPosition();
            }
            return ins;
        }
    }

    private Dictionary<int, PlayerDirection> playerPositionDic = new Dictionary<int, PlayerDirection>();

    /// <summary>
    /// 玩家位置信息 key:从左到右的玩家排序 从0开始， value：位置信息
    /// </summary>
    public Dictionary<int, PlayerDirection> PlayerPositionDic
    {
        get
        {
            return playerPositionDic;
        }

        set
        {
            playerPositionDic = value;
        }
    }
    public void Init()
    {
        PlayerPositionDic.Clear();
    }

}
public class PlayerDirection
{
    public PlayerDirection()
    {
        X_Direction = PlayerPositionInfo.Center;
        Z_Direction = PlayerPositionInfo.Center;
    }
    public PlayerDirection(PlayerPositionInfo xDirection, PlayerPositionInfo zDirection)
    {
        X_Direction = xDirection;
        Z_Direction = zDirection;
    }
    /// <summary>
    /// 玩家在x轴方向的站位
    /// </summary>
    public PlayerPositionInfo X_Direction { get; set; }
    /// <summary>
    /// 玩家在z轴方向的站位
    /// </summary>
    public PlayerPositionInfo Z_Direction { get; set; }

}
public enum PlayerPositionInfo : int
{
    /// <summary>
    /// 无人
    /// </summary>
    None = 0,
    /// <summary>
    /// 处于当前应用区域的左边界
    /// </summary>
    Left,
    /// <summary>
    /// 处于当前应用区域的右边界
    /// </summary>
    Right,
    /// <summary>
    /// 处于当前应用区域的中心
    /// </summary>
    Center,
    /// <summary>
    /// 处于当前应用区域的前边界
    /// </summary>
    Forward,
    /// <summary>
    /// 处于当前应用区域的后边界
    /// </summary>
    Backward
}
