using System;

/// <summary>
/// Links a ScriptableEvent to another such that the EventContainer actually displays the ScriptableEvent created for that other reference Field/Property
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class SchematicEventLinkAttribute : Attribute
{
    public Type Type;
    public string FieldName;

    public SchematicEventLinkAttribute(Type type, string fieldName)
    {
        Type = type;
        FieldName = fieldName;
    }
}