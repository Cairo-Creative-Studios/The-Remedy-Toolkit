
using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class SerializableType : ISerializationCallbackReceiver
{
    private static Dictionary<Type, SerializableType> _cache = new();
    [SerializeField] private string typeName;

    [NonSerialized] private Type cachedType;
    [NonSerialized] private string cachedFullName;
    [NonSerialized] private int cachedHashCode;

    public Type Type
    {
        get => cachedType;
        set
        {
            cachedType = value;
            cachedFullName = value?.FullName; 
            cachedHashCode = value?.GetHashCode() ?? 0;
            typeName = value?.AssemblyQualifiedName;
        }
    }

    public static implicit operator Type(SerializableType serializableType) => serializableType?.Type;

    public static implicit operator SerializableType(Type type) => _cache.ContainsKey(type) ? _cache[type] : _cache[type] = new SerializableType { Type = type };

    public override string ToString() => cachedFullName ?? "null";

    public override bool Equals(object obj)
    {
        if (obj is SerializableType other)
            return cachedType == other.cachedType; 
        if (obj is Type type)
            return cachedType == type;           
        return false;
    }

    public override int GetHashCode() => cachedHashCode;

    public static bool operator ==(SerializableType a, SerializableType b)
    {
        if (ReferenceEquals(a, b)) return true;
        if (a is null || b is null) return false;
        return a.cachedType == b.cachedType;
    }

    public static bool operator !=(SerializableType a, SerializableType b) => !(a == b);

    // Ensures cache is restored after Unity deserializes
    public void OnAfterDeserialize()
    {
        cachedType = string.IsNullOrEmpty(typeName) ? null : Type.GetType(typeName);
        cachedFullName = cachedType?.FullName;
        cachedHashCode = cachedType?.GetHashCode() ?? 0;
    }

    public void OnBeforeSerialize() { } // not needed
}
