using BlueGraph;
using Remedy.Schematics;
using System;
using UnityEngine;
using Remedy.Schematics.Utils;

[Serializable]
[Node(Name = "Send a Signal", Path = "ScriptableEvents/Events"), Tags("Object")]
public class SendSignalNode : SchematicActionNode
{
    [Editable]
    public SignalData Signal;

    protected override void OnTrigger(GameObject instance, bool awaiting = false)
    {
        var input = GetInputValue<Union>(nameof(Input), default);
        Signal?.Invoke(input);
    }
}

[Serializable]
[Node(Name = "On Signal Received"), Tags("Object")]
public class OnSignalReceivedNode : SchematicEventNode
{
    [Editable]
    public SignalData Signal;
}
 
[Node(Name = "Set Variable", Path = "Schematic/Variables"), Tags("Object")]
public class SetScriptableVariable : SchematicActionNode
{
    [Editable]
    public ScriptableVariable Target;
    [Input]
    public Union Value;

    protected override void OnTrigger(GameObject instance, bool awaiting = false)
    {
        var newVal = GetInputValue<Union>(nameof(Value), default);
        Target.Value = newVal;
    } 
}

[Node(Name = "Get Variable", Path = "Schematic/Variables"), Tags("Object")]
public class GetScriptableVariable : SchematicActionNode
{
    [Editable]
    public ScriptableVariable Target;
    [Output]
    public Union Value;

    protected override void OnTrigger(GameObject instance, bool awaiting = false)
    {
        SetOutputValue(nameof(Value), Target.Value);
    }
}