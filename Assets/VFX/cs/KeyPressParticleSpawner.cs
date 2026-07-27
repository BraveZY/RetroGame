using UnityEngine;
using System.Collections.Generic;
/// <summary>
/// 键盘触发对象生成器
/// 挂载于任意空物体上，支持多键位配置
/// </summary>
public class KeyPressObjectSpawner : MonoBehaviour
{
    // ==============================
    // 📦 配置结构体定义
    // ==============================
    [System.Serializable]
    public class ObjectConfig
    {
        [Header("⚙️ 键位配置")]
        [Tooltip("设置触发键位 (建议避免使用 Unity 编辑器快捷键)")]
        public KeyCode key = KeyCode.F;
        [Header("📦 对象 Prefab")]
        [Tooltip("需要在 Inspector 中绑定")]
        public GameObject prefab;
        [Header("📍 位置偏移")]
        [Tooltip("对象相对于挂载物体的偏移量")]
        public Vector3 positionOffset = Vector3.zero;
        [Header("🔧 选项")]
        [Tooltip("如果勾选，使用 Unity 编辑器的快捷键 F 锁定视图功能")]
        public bool useUnityShortcut = true;
    }
    // ==============================
    // 📋 多键位配置列表
    // ==============================
    [Header("🔐 多键位配置列表")]
    [Tooltip("添加多组键位和对象 Prefab")]
    public List<ObjectConfig> objectConfigs = new List<ObjectConfig>();
    // ==============================
    // 🕹️ 主循环：检测按键
    // ==============================
    private void Update()
    {
        // 遍历所有配置
        foreach (var config in objectConfigs)
        {
            // 跳过未绑定 Prefab 的配置
            if (config.prefab == null) continue;
            // 检测键盘输入
            if (Input.GetKeyDown(config.key))
            {
                // 检查 F 键冲突
                if (config.key == KeyCode.F && config.useUnityShortcut) continue;
                // ==============================
                // ⚙️ 核心逻辑：生成对象
                // ==============================
                Debug.Log($"[KeyPressObjectSpawner] 检测到按键 {config.key}，准备生成对象。");
                // 计算生成位置：挂载物体的位置 + 偏移
                Vector3 spawnPosition = transform.position + config.positionOffset;
                // 实例化对象
                GameObject instance = Instantiate(config.prefab, spawnPosition, Quaternion.identity);
                // 确保实例激活（如果 Prefab 原本是隐藏状态）
                if (!instance.activeSelf)
                {
                    instance.SetActive(true);
                    Debug.Log("[KeyPressObjectSpawner] 对象实例已激活。");
                }
                // ==============================
                // 🚀 生成成功日志
                // ==============================
                Debug.Log($"[KeyPressObjectSpawner] 已成功生成对象: {instance.name}");
            }
        }
    }
}