using UnityEngine;

namespace MotionDodgeball.Gameplay
{
    /// <summary>
    /// 管住一个对象还能承受几次伤害，以及什么时候算被击倒。
    ///
    /// 职责：
    /// - 入场时把生命值恢复到可用的最大值，保证至少有 1 点生命。
    /// - 受到有效伤害时扣血，并把生命值锁在 0 以上。
    /// - 对外提供死亡状态，方便子弹、规则和测试判断结果。
    /// </summary>
    public sealed class Health : MonoBehaviour
    {
        [SerializeField] private int maxHealth = 3;

        public int MaxHealth => maxHealth;
        public int CurrentHealth { get; private set; }
        public bool IsDead => CurrentHealth <= 0;

        private void Awake()
        {
            CurrentHealth = Mathf.Max(1, maxHealth);
        }

        /// <summary>扣除一次有效伤害；无效数值或已死亡时保持当前生命不变。</summary>
        public void TakeDamage(int amount)
        {
            if (amount <= 0 || IsDead)
            {
                return;
            }

            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        }
    }
}
