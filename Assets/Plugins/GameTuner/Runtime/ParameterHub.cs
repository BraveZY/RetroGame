using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 参数注册中心（线程安全的静态字典）。
/// 
/// - 注册和读写均通过 lock 保护，HTTP 线程和主线程均可安全调用。
/// - 不依赖任何 MonoBehaviour 生命周期，插件卸载前保持全局有效。
/// </summary>
public static class ParameterHub
{
    private static readonly Dictionary<string, ParameterEntry> _params =
        new Dictionary<string, ParameterEntry>();

    private static readonly object _lock = new object();

    /// <summary>注册参数。若 id 已存在则覆盖。</summary>
    public static void Register(ParameterEntry entry)
    {
        if (entry == null || string.IsNullOrEmpty(entry.id)) return;
        lock (_lock) { _params[entry.id] = entry; }
    }

    /// <summary>注销参数。</summary>
    public static void Unregister(string id)
    {
        if (string.IsNullOrEmpty(id)) return;
        lock (_lock) { _params.Remove(id); }
    }

    /// <summary>获取参数当前值。不存在时返回 0。</summary>
    public static float GetValue(string id)
    {
        lock (_lock)
        {
            return _params.TryGetValue(id, out var e) ? e.currentValue : 0f;
        }
    }

    /// <summary>设置参数值，自动夹紧到 [min, max]。返回是否成功。</summary>
    public static bool SetValue(string id, float value)
    {
        lock (_lock)
        {
            if (!_params.TryGetValue(id, out var e)) return false;
            e.currentValue = Mathf.Clamp(value, e.minValue, e.maxValue);
            return true;
        }
    }

    /// <summary>将单个参数重置为默认值。</summary>
    public static bool Reset(string id)
    {
        lock (_lock)
        {
            if (!_params.TryGetValue(id, out var e)) return false;
            e.currentValue = e.defaultValue;
            return true;
        }
    }

    /// <summary>重置所有已注册的参数为默认值。</summary>
    public static void ResetAll()
    {
        lock (_lock)
        {
            foreach (var e in _params.Values)
                e.currentValue = e.defaultValue;
        }
    }

    /// <summary>返回所有已注册参数的快照列表（线程安全副本）。</summary>
    public static List<ParameterEntry> GetAll()
    {
        lock (_lock) { return new List<ParameterEntry>(_params.Values); }
    }
}
