using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor;

public class FieldRenderer : VisualElement
{
    protected CustomFieldRendererAttribute _attr;
    protected UnityEngine.Object _object;
    protected string _prefabPath;
    protected object _parent;
    protected object _value;
    protected FieldOrPropertyInfo _field;
    protected string _path;
    protected SchematicGraphEditorWindow _window;
    protected List<Action> _onModified;

    public FieldRenderer(SchematicGraphEditorWindow window, UnityEngine.Object obj, object parent, object target, FieldOrPropertyInfo fieldInfo, CustomFieldRendererAttribute attr, string path, List<Action> onModifed)
    {
        _window = window;
        _object = obj;
        _parent = parent;
        _attr = attr;
        _value = target;
        _field = fieldInfo;
        _path = path;
        _onModified = new();
        _prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(_object);

        if (onModifed != null)
        {
            _onModified.AddRange(onModifed);
        }

        Redraw();
    }

    protected virtual void Redraw()
    { }

    protected virtual void OnModified()
    {
        foreach(var modCallback in _onModified)
        {
            modCallback?.Invoke();
        }
    }

    protected void AttemptPrefabRefresh()
    {
        if (string.IsNullOrEmpty(_prefabPath)) return;
        PrefabUtility.SaveAsPrefabAsset(_window.Prefab, _prefabPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        PrefabRefreshUtility.ReimportAndResetPrefab(_window.Prefab);
    }
}