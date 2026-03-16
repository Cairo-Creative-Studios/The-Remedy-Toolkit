using System;

/// <summary>
/// Tells the Schematic Editor that a ScriptableObject is a container for Properties for the Component to render it's data.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class SchematicPropertiesAttribute : Attribute
{
    public SchematicPropertiesAttribute()
    { }
}
