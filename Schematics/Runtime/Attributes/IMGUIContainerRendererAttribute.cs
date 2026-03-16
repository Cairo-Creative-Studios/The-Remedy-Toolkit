using System;

/// <summary>
/// Draws an IMGUIContainer for the Field or Property in the Schematic Editor.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class IMGUIContainerRendererAttribute : CustomFieldRendererAttribute
{
    public IMGUIContainerRendererAttribute()
    {
    }
}
