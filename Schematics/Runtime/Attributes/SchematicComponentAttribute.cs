using System;

/// <summary>
/// Tells the Schematic Editor to add this Component/MonoBehavior and it's editors specified within the attributes of it's children.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class SchematicComponentAttribute : Attribute
{
    public string Path { get; }
    public bool HideInSelector { get; }

    public SchematicComponentAttribute(string path, bool hideInSelector = false)
    {
        Path = path;
        HideInSelector = hideInSelector;
    }
}
