using System.Collections.Generic;
using UnityEngine;

namespace MotionSport.Tools
{
    /// <summary>
    /// 手动引导线路径：本地空间折点顺序连接，用于沿路径拼接 Box/Capsule 碰撞体。
    /// 应挂在与 MeshFilter 同一物体上，使 localPoints 与网格顶点空间一致。
    /// </summary>
    [DisallowMultipleComponent]
    public class AutoColliderGuidePath : MonoBehaviour
    {
        [Tooltip("相对本物体 Transform 的折点，按索引顺序连接成折线")]
        public List<Vector3> localPoints = new List<Vector3>();

        [Tooltip("勾选后使用下方宽高；不勾选则按本物体及子级渲染/网格包围盒自动估算截面")]
        public bool manualSectionSize = false;

        [Tooltip("自定义：按约世界空间单位（米）；生成时会按宿主 lossyScale 换算到 Collider 局部。自动模式失败时的备用值同左。")]
        public float segmentWidth = 0.6f;

        [Tooltip("自定义：按约世界空间单位（米）；生成时按缩放换算。备用高同左。")]
        public float segmentHeight = 0.35f;

        [Tooltip("开启后 Scene 内无修饰键左键在表面加点；易误触，编辑完请关闭。与 Unity Shift 多选冲突故默认用 Ctrl/Cmd。")]
        public bool sceneLeftClickAddsPoint = false;
    }
}
