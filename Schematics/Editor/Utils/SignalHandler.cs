using SchematicAssets;
using System;
using UnityEditor;
using UnityEngine;

[Serializable]
public class SignalHandler
{
    public GlobalObjectId EventAssetID;
    public SerializableProperty Property;

    /// <summary>
    /// Called when the Original Event Asset is replaced, so that extra reference update functionality can be invoked for it. 
    /// </summary>
    public Action<GlobalObjectId, GlobalObjectId> EventAssetReplaced;
    /// <summary>
    /// A Property that gets the Event Asset from it's Global ID, casted to ScriptableEventBase
    /// </summary>
    public SignalData SignalAsset => (SignalData)GlobalObjectId.GlobalObjectIdentifierToObjectSlow(EventAssetID) ?? EnsureAssetExists();

    public string EventName
    {
        get
        {
            try
            {
                var ev = SignalAsset;

                if (ev == null)
                {
                    EnsureAssetExists(_defaultName);
                    ev = SignalAsset;
                }

                return ev.name;
            }
            catch
            {
                return string.Empty;
            }
        }
    }

    public Type EventType
    {
        get
        {
            var ev = SignalAsset;

            if (ev == null)
            {
                EnsureAssetExists(_defaultName);
                ev = SignalAsset;
            }

            return ev.GetType();
        }
    }

    [SerializeField]
    private SerializableType _defaultType;
    [SerializeField]
    private string _defaultName;
    [SerializeField]
    private string _fieldName;
    public string FieldName => _fieldName;

    public SignalHandler(GlobalObjectId componentID, string propertyPath, FieldOrPropertyInfo field)
    {
        _fieldName = field.Name;

        if (string.IsNullOrEmpty(propertyPath))
            Property = new SerializableProperty(componentID, _fieldName);
        else
            Property = new SerializableProperty(componentID, propertyPath + "." + _fieldName);

        //EventAssetReplaced += eventAssetReplacedCallback;

        SignalAsset.Property = Property;
    }

    /// <summary>
    /// Changes the name of the Scriptable event Asset this References
    /// </summary>
    /// <param name="name"></param>
    internal void ChangeName(string oldName, string newName)
    {
        SchematicAssetManager.Rename(Property.Obj, Property.Path, "", oldName, newName);
    }

    private SignalData EnsureAssetExists(string name = "")
    {
        var asset = TryGetExistingEvent();

        if (asset == null)
            asset = CreateEventAsset(); // Create new 
        else
            EventAssetID = GlobalObjectId.GetGlobalObjectIdSlow(asset); // Init from loaded

        return asset;
    }

    private SignalData TryGetExistingEvent()
    {
        var loadedData = SchematicAssetManager.LoadAll<SignalData>(Property.Obj, Property.Path, "");
        if(loadedData != null && loadedData.Length > 0)
        {
            var loaded = loadedData[0];
            return loaded;
        }
        return null;
    }

    private SignalData CreateEventAsset()
    {
        try
        {
            var oldID = EventAssetID;
            var obj = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(Property.ObjID);
            var eventAsset = SchematicAssetManager.Create<SignalData>(obj, Property.Path, "", _fieldName);

            EventAssetID = GlobalObjectId.GetGlobalObjectIdSlow(eventAsset);

            var newID = EventAssetID;

            EventAssetReplaced?.Invoke(oldID, newID);

            SignalAsset.Property = Property;

            return eventAsset;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        return null;
    }
}
