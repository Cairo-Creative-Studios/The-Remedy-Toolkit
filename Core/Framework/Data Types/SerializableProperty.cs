#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Pairs the GlobalObjectId of a Unity Object with a Path to a Property. 
/// </summary>
[Serializable]
public class SerializableProperty
{
    [SerializeField]
    private string _path;
    public string Path => _path;
    [SerializeField]
    private GlobalObjectId _objID;
    public GlobalObjectId ObjID => _objID;
    public UnityEngine.Object Obj => GlobalObjectId.GlobalObjectIdentifierToObjectSlow(_objID);

    private SerializedObject _serializedObject;
    private SerializedProperty _cachedProperty;

    public SerializableProperty(GlobalObjectId objID, string path)
    {
        _path = path;
        _objID = objID;
    }

    /// <summary>
    /// Gets the SerializedProperty for this path on the target object.
    /// Caches the SerializedObject and SerializedProperty for repeated access.
    /// </summary>
    public SerializedProperty GetProperty()
    {
        if (_cachedProperty != null) return _cachedProperty;

        var targetObj = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(_objID);
        if (targetObj == null) return null;

        if (_serializedObject == null || _serializedObject.targetObject != targetObj)
            _serializedObject = new SerializedObject(targetObj);

        _cachedProperty = _serializedObject.FindProperty(_path);
        return _cachedProperty;
    }

    /// <summary>
    /// Gets the value of this property as an object.
    /// Works for UnityEngine.Object references, structs, and normal classes.
    /// </summary>
    public object GetValue()
    {
        var property = GetProperty();
        if (property == null) return null;

        // Shortcut for UnityEngine.Object references
        if (property.propertyType == SerializedPropertyType.ObjectReference)
            return property.objectReferenceValue;

        // Fallback: use reflection for other types
        object target = property.serializedObject.targetObject;
        string path = property.propertyPath.Replace(".Array.data[", "["); // normalize arrays
        string[] elements = path.Split('.');

        foreach (string element in elements)
        {
            if (element.Contains("["))
            {
                string elementName = element.Substring(0, element.IndexOf("["));
                int index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                target = target.GetType()
                               .GetField(elementName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                               ?.GetValue(target);
                if (target is IList list)
                    target = list[index];
                else
                    return null; // invalid path
            }
            else
            {
                target = target.GetType()
                               .GetField(element, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                               ?.GetValue(target);
            }

            if (target == null) return null; // early exit if path is invalid
        }

        return target;
    }

    /// <summary>
    /// Generic version of GetValue for convenience.
    /// </summary>
    public T GetValue<T>() where T : class
    {
        return GetValue() as T;
    }

    /// <summary>
    /// Gets the Type of the Field that this Property is rendering
    /// </summary>
    /// <returns></returns>
    public Type GetFieldType()
    {
        var property = GetProperty();
        var objType = property.serializedObject.targetObject.GetType();
        var field = objType.GetField(property.propertyPath,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        return field?.FieldType;
    }

    public IEnumerable<Attribute> GetAttributes()
    {
        var property = GetProperty();
        if (property == null)
            yield break;

        Type type = property.serializedObject.targetObject.GetType();
        FieldInfo field = null;

        string path = property.propertyPath.Replace(".Array.data[", "[");
        string[] elements = path.Split('.');

        foreach (var element in elements)
        {
            if (element.Contains("["))
            {
                string fieldName = element.Substring(0, element.IndexOf("["));
                field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (field == null)
                    yield break;

                type = field.FieldType;

                // unwrap array / list element type
                if (type.IsArray)
                    type = type.GetElementType();
                else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                    type = type.GetGenericArguments()[0];
            }
            else
            {
                field = type.GetField(element, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (field == null)
                    yield break;

                type = field.FieldType;
            }
        }

        if (field == null)
            yield break;

        foreach (var attr in field.GetCustomAttributes(true))
            yield return (Attribute)attr;
    }
}
#endif