using System;

/// <summary>
/// Marking a field or property with this creates a Schematic Variable for this Variable and makes it modifiable and accessible within the scope of the Schematic. <br></br>
/// Prefer to make fields and properties decorated with this Attribute as private, as the Unity Inspector will display the actual Field value not the Schematic's value.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class ScriptableVariableAttribute : CustomFieldRendererAttribute
{
    public ScriptableVariableAttribute()
    {

    }
}