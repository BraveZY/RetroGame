using UnityEngine;

namespace MotionDodgeball.Gameplay
{
    /// <summary>
    /// 让玩家物体按方向键或摇杆输入在 2D 平面移动。
    ///
    /// 职责：
    /// - 每帧读取横向与纵向输入，并避免斜向移动比直线更快。
    /// - 按配置速度把输入转换成世界坐标位移。
    /// </summary>
    public sealed class PlayerMover2D : MonoBehaviour
    {
        [SerializeField] private float speed = 5f;

        private void Update()
        {
            var input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (input.sqrMagnitude > 1f)
            {
                input.Normalize();
            }

            transform.position += (Vector3)(input * speed * Time.deltaTime);
        }
    }
}
