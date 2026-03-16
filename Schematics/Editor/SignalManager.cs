using Remedy.Schematics;
using Remedy.Schematics.Utils;
using SchematicAssets;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

internal class SignalManager
{
    private List<SignalHandler> _workingSet;
    public List<SignalHandler> WorkingSet => _workingSet ??= new();

    private SchematicGraph _graph;

    internal SignalManager(SchematicGraph graph)
    {
        _graph = graph;
    }

    internal SignalHandler GetOrCreateEventReference(UnityEngine.Object obj, string propertyPath, FieldOrPropertyInfo field)
    {
        var objID = GlobalObjectId.GetGlobalObjectIdSlow(obj);

        var existing = FindEventReference(obj, propertyPath, field.Name);
        if (existing != null)
            return existing;
        else
        {
            var newEventRef = new SignalHandler(objID, propertyPath, field);
            WorkingSet.Add(newEventRef);
            return newEventRef;
        }
    }

    private SignalHandler FindEventReference(UnityEngine.Object obj, string propertyPath, string fieldName)
    {
        var objID = GlobalObjectId.GetGlobalObjectIdSlow(obj).targetObjectId;

        foreach (var ser in WorkingSet)
        {
            if (ser.Property.ObjID.targetObjectId == objID && ser.Property.Path == propertyPath && ser.FieldName == fieldName)
                return ser;
        }

        return null;
    }
}