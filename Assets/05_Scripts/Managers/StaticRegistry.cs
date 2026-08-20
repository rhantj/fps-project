using System;
using System.Collections.Generic;
using UnityEngine;

public static class StaticRegistry
{
    private static readonly Dictionary<Type, UnityEngine.Object> _register = new();

    // 도메인 리로드를 끈 상태로 플레이 모드를 다시 시작하면 static 필드가 그대로
    // 남아 파괴된 오브젝트를 계속 참조한다. 진입 시점마다 비운다.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetRegistry() => _register.Clear();

    public static void Add<T>(T obj) where T : UnityEngine.Object
    {
        var type = typeof(T);
        if (!_register.ContainsKey(type))
        {
            _register.Add(type, obj);
        }
    }

    public static void Remove<T>(T obj) where T : UnityEngine.Object
    {
        var type = typeof(T);
        if(_register.TryGetValue(type, out var unregister) && ReferenceEquals(obj, unregister))
        {
            _register.Remove(type);
        }
    }

    public static void Clear()
    {
        _register.Clear();
    }

    public static T Find<T>() where T : UnityEngine.Object
    {
        return _register.TryGetValue(typeof(T), out var obj) ? obj as T : null;
    }
}
