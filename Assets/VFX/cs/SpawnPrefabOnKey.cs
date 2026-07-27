using UnityEngine;

/// <summary>
/// 按下H键生成指定预制体的脚本
/// </summary>
public class SpawnPrefabOnKey : MonoBehaviour
{
    // 你需要在Inspector面板中拖入的自定义预制体
    [Header("生成设置")]
    [Tooltip("需要生成的自定义预制体")]
    public GameObject customPrefab;

    [Tooltip("预制体生成的位置（默认在世界坐标原点）")]
    public Vector3 spawnPosition = Vector3.zero;

    [Tooltip("预制体生成的旋转角度（默认无旋转）")]
    public Quaternion spawnRotation = Quaternion.identity;

    [Tooltip("生成按键（默认H键）")]
    public KeyCode spawnKey = KeyCode.H;

    // Update每一帧都会执行，用于检测按键输入
    void Update()
    {
        // 检测是否按下指定按键（GetKeyDown只在按下瞬间触发一次）
        if (Input.GetKeyDown(spawnKey))
        {
            // 安全校验：防止未赋值预制体导致空引用错误
            if (customPrefab == null)
            {
                Debug.LogError("请在Inspector面板中为customPrefab赋值自定义预制体！");
                return;
            }

            // 生成预制体到指定位置和旋转角度
            GameObject spawnedObject = Instantiate(
                customPrefab, 
                spawnPosition, 
                spawnRotation
            );

            // 可选：给生成的物体命名，方便在Hierarchy面板识别
            spawnedObject.name = $"Spawned_{customPrefab.name}_{Time.time}";
            
            Debug.Log($"成功生成预制体：{spawnedObject.name}");
        }
    }
}