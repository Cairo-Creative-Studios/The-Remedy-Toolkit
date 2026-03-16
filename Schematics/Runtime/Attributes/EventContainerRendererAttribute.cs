using System;

/// <summary>
/// Draws an Event Container UI within the Schematic Editor for this Field
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class EventContainerRendererAttribute : CustomFieldRendererAttribute
{
    public Type DefaultType;
    public string DefaultName;
    public bool ParametersEditable;
    public bool ParamTypeModifiable;
    public bool ParamNameModifiable;
    public SignalData IOBase;

    public EventContainerRendererAttribute(Type defaultType, string defaultName, bool parametersEditable, bool paramTypeModifiable = false, bool paramNameModifiable = false)
    {
        DefaultType = defaultType;
        DefaultName = defaultName;

        ParametersEditable = parametersEditable;
        ParamTypeModifiable = paramTypeModifiable;
        ParamNameModifiable = paramNameModifiable;
    }
}