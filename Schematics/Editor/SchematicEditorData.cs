using Remedy.Framework;
using Remedy.Schematics;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static SignalData;
using SchematicAssets;
using UnityEditor;
using NaughtyAttributes;
using Remedy.Schematics.Utils;

public class SchematicEditorData : SingletonData<SchematicEditorData>
{
    [Tooltip("If enabled, load times of processes of the Editor will be logged in the console")]
    [SerializeField]
    internal bool _profileEditor = true;
    internal static bool Profile => Instance._profileEditor;

    [SerializeField]
    internal List<SchematicPrefabData> _prefabData = new();

    [SerializeField]
    internal List<SchematicGraph> Graphs;
/*    [SerializeField]
    internal List<SchematicScope> Scopes;*/
/*
    public List<IOBase> IOBases = new();

    public SerializableDictionary<IOBase, List<SchematicScope.ScriptableEventReference>> WorkingEventSetDebug;*/

    public List<SignalHandler> ScriptableEventReferenceDebug = new();

    /// <summary>
    /// A cache storing the Invoke Nodes of the Schematic with the ScriptableEventReference that their Event is from so they can be properly updated.
    /// </summary>
    public SerializableDictionary<SignalHandler, List<SendSignalNode>> _invokeNodesToReferenceCache = new();
    /// <summary>
    /// A cache storing the OnInvoke Nodes of the Schematic with the ScriptableEventReference that their Event is from so they can be properly updated.
    /// </summary>
    public SerializableDictionary<SignalHandler, List<OnSignalReceivedNode>> _onInvokeNodesToReferenceCache = new();

    [SerializeField]
    private List<EditorTypeSetting> _editorTypeSettings = new();
    internal static List<EditorTypeSetting> EditorTypeSettings => Instance._editorTypeSettings;

    [Serializable]
    public class EditorTypeSetting
    {
        [HideInInspector]
        public string TypeName;
        [Dropdown("GetTypes")]
        public string Type;
        public Color ColorInEditor;
        private static DropdownList<string> _cachedList = new();

        public DropdownList<string> GetTypes()
        {
            if(_cachedList.Count() == 0)
            {
                var types = Union.TypeLookup.Values;
                
                foreach(var type in types)
                {
                    if (type == null) continue;
                    _cachedList.Add(type.Name, type.AssemblyQualifiedName);
                }
            }

            return _cachedList;
        }
    }

    /// <summary>
    /// Adds Schematic Data for the given item
    /// 
    /// This is used in the SchematicGenerationPrefabPostProcessor when asset importing has finished processing, to ensure that Ghost folders
    /// are not retained for Prefabs that no longer exist. 
    /// </summary>
    internal static void AddSchematicData(GameObject prefab)
    {
        var globalId = GlobalObjectId.GetGlobalObjectIdSlow(prefab).ToString();
        var guid = GetPrefabGuid(prefab);

        if(!Instance._prefabData.Any(data => data.GlobalID == globalId))
        {
            Instance._prefabData.Add(new(globalId));
        }
    }

    internal static void DeleteSchematicPrefabs(string[] deleted)
    {
        foreach (var toDelPath in deleted)
        {
            var toDelID = AssetDatabase.GUIDFromAssetPath(toDelPath).ToString();

            SchematicAssetManager.DeleteObjectFolder(toDelID);

            Instance._prefabData.RemoveAll(data => data.GlobalID == toDelID);
        }
    }

    internal static void DeleteComponentData(UnityEngine.GameObject prefab, string componentPath, Type componentType)
    {
        var obj = prefab.transform.Find(componentPath).gameObject.GetComponent(componentType);
        var objID = GlobalObjectId.GetGlobalObjectIdSlow(obj).ToString();
        SchematicAssetManager.DeleteObjectFolder(objID);
    }

    private static string GetPrefabGuid(GameObject prefab)
    {
        if (prefab == null) return null;

        string assetPath = AssetDatabase.GetAssetPath(prefab);
        if (string.IsNullOrEmpty(assetPath))
            return null;

        string guid = AssetDatabase.AssetPathToGUID(assetPath);
        return guid;
    }

    [Serializable]
    public class SchematicPrefabData
    {
        public string GlobalID;
        //public SchematicScope Scope;
        public List<SchematicGraph> Graphs = new();

        public SchematicPrefabData(string globalID)
        {
            GlobalID = globalID;
        }
    }

}