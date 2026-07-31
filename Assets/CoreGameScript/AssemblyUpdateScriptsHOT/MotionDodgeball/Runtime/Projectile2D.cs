using UnityEngine;

namespace MotionDodgeball.Gameplay
{
    /// <summary>
    /// 表示一枚会直线飞行、命中带生命对象后造成伤害的 2D 子弹。
    ///
    /// 职责：
    /// - 启用后按寿命自动销毁，避免场景里残留子弹。
    /// - 按发射方向和速度推进位置。
    /// - 碰到 Health 时扣除伤害并结束本次飞行。
    /// </summary>
    public sealed class Projectile2D : MonoBehaviour
    {
        [SerializeField] private float speed = 8f;
        [SerializeField] private int damage = 1;
        [SerializeField] private float lifetime = 3f;

        private Vector2 direction = Vector2.right;

        private void OnEnable()
        {
            Destroy(gameObject, lifetime);
        }

        private void Update()
        {
            transform.position += (Vector3)(direction * speed * Time.deltaTime);
        }

        /// <summary>设置本次飞行方向；传入零向量时默认向右飞。</summary>
        public void Launch(Vector2 launchDirection)
        {
            direction = launchDirection.sqrMagnitude > 0f ? launchDirection.normalized : Vector2.right;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent<Health>(out var health))
            {
                health.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
    }
}
