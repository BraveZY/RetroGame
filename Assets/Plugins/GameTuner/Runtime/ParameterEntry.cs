using System;

/// <summary>
/// 单个可调参数的完整描述，包含元数据与当前值。
/// 所有字段均为值类型，可跨线程安全读取。
/// </summary>
[Serializable]
public class ParameterEntry
{
    public string id;           // 唯一标识，格式建议：{game}_{module}_{name}
    public string category;     // 分类显示名，如「篮球-玩家」
    public string name;         // 人类可读的参数名称
    public string description;  // 参数用途说明
    public float minValue;      // 允许的最小值
    public float maxValue;      // 允许的最大值
    public float defaultValue;  // 初始默认值，用于重置
    public float currentValue;  // 当前生效值
    public float step;          // 滑杆步进精度
}
