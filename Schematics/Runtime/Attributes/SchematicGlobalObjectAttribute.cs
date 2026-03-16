using System;

[AttributeUsage(AttributeTargets.Class)]
public class SchematicGlobalObjectAttribute : Attribute
{
    /// <summary>
    /// Used for saving the Global Resources, as well as the Display name of the Object in the Global Schematics Tab
    /// </summary>
    public string Name;
    public SchematicGlobalObjectAttribute(string displayName)
    {
        Name = displayName;
    }
}
