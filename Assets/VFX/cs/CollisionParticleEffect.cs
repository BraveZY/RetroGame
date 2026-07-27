using UnityEngine;
/// <summary>
/// 碰撞粒子触发器
/// 挂载于任意有 Collider 的物体上
/// 仅在碰撞到特定物体或标签时触发，并支持位置偏移
/// </summary>
public class CollisionParticleEffect : MonoBehaviour
{
    // 🎈 粒子预制体 (必填)
    [Header("粒子特效 Prefab")]
    [Tooltip("将你的粒子预制体拖拽到此处")]
    public GameObject particlePrefab;
// 🎯 特定物体筛选 (选填)
    [Header("碰撞过滤设置")]
    [Tooltip("将特定的 GameObject 拖拽到此处")]
    public GameObject targetObject; // 直接绑定特定对象
[Tooltip("设置目标物体的 Tag (留空则不使用 Tag 过滤)")]
    public string targetTag; // 使用标签过滤
// 📍 位置偏移设置
    [Header("位置偏移设置")]
    [Tooltip("粒子特效相对于挂载物体的偏移量")]
    public Vector3 positionOffset = Vector3.zero;
// ⚙️ 选项：是否只在第一次碰撞时触发
    [Header("其他配置")]
    public bool triggerOnlyOnce = true;
// 🎈 记录是否已经触发过
    private bool hasTriggered = false;
// -------------------------------------------------
    // 当使用 Is Trigger = true 时，请改用 OnTriggerEnter 方法
    private void OnCollisionEnter(Collision collision)
    {
        // --------------------------------------------
        // 🔄 防止多次触发
        // --------------------------------------------
        if (triggerOnlyOnce && hasTriggered) return;
// --------------------------------------------
        // 🎯 1. 检查是否碰撞到目标物体
        // --------------------------------------------
        // 优先判断目标物体 (targetObject)
        if (targetObject != null && collision.gameObject != targetObject)
        {
            // 碰撞到的不是指定的目标对象，直接返回
            return;
        }
        // 其次判断标签 (targetTag)
        if (!string.IsNullOrEmpty(targetTag) && !collision.gameObject.CompareTag(targetTag))
        {
            // 碰撞到的对象标签不匹配，直接返回
            return;
        }
// --------------------------------------------
        // ✅ 2. 检查粒子预制体是否设置
        // --------------------------------------------
        if (particlePrefab == null)
        {
            Debug.LogError("[CollisionParticleEffect] 粒子预制体未绑定！");
            return;
        }
// --------------------------------------------
        // 🚀 3. 实例化粒子特效
        // --------------------------------------------
        // 计算最终位置：挂载物体的位置 + 偏移
        Vector3 spawnPosition = transform.position + positionOffset;
        GameObject particleInstance = Instantiate(particlePrefab, spawnPosition, Quaternion.identity);
// --------------------------------------------
        // 🧹 4. 自动销毁粒子实例
        // --------------------------------------------
        // 使用泛型参数 <ParticleSystem> 获取组件
        ParticleSystem ps = particleInstance.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            // 计算完整的持续时间 (包括延迟和寿命)
            float totalDuration = ps.main.duration + ps.main.startLifetime.constantMax;
            Destroy(particleInstance, totalDuration);
        }
        else
        {
            // 如果预制体里有子物体是 ParticleSystem
            ParticleSystem childPs = particleInstance.GetComponentInChildren<ParticleSystem>();
            if (childPs != null)
            {
                float totalDuration = childPs.main.duration + childPs.main.startLifetime.constantMax;
                Destroy(particleInstance, totalDuration);
            }
            else
            {
                // 兜底：默认 5 秒后销毁
                Destroy(particleInstance, 5f);
            }
        }
// --------------------------------------------
        // 🎉 5. 标记已触发 (如果需要)
        // --------------------------------------------
        hasTriggered = true;
    }
}
