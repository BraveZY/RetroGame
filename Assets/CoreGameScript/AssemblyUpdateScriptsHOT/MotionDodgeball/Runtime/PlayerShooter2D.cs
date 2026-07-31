using UnityEngine;

namespace MotionDodgeball.Gameplay
{
    /// <summary>
    /// 让玩家按下空格时从指定位置发射一枚 2D 子弹。
    ///
    /// 职责：
    /// - 监听发射输入，并在子弹预制体存在时创建子弹。
    /// - 优先使用枪口位置，未配置时退回到玩家当前位置。
    /// - 统一把新子弹朝右侧发射，保持最小样例的射击规则简单可测。
    /// </summary>
    public sealed class PlayerShooter2D : MonoBehaviour
    {
        [SerializeField] private Projectile2D projectilePrefab;
        [SerializeField] private Transform spawnPoint;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Shoot();
            }
        }

        private void Shoot()
        {
            if (projectilePrefab == null)
            {
                return;
            }

            var origin = spawnPoint != null ? spawnPoint.position : transform.position;
            var projectile = Instantiate(projectilePrefab, origin, Quaternion.identity);
            projectile.Launch(Vector2.right);
        }
    }
}
