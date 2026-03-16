// project armada

#pragma warning disable 0414

using BlueGraph;
using UnityEngine;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using System;

namespace Remedy.Schematics
{
    [Serializable]
    [Node(Path = "Debug"), Tags("Default")] 
    public class Log : SchematicActionNode
    {
        public enum DebugType
        {
            Normal,
            Error,
            Warning
        }


        [Editable]
        public string Text;
        [Editable]
        public DebugType Type = DebugType.Normal;

        protected override void OnTrigger(GameObject instance, bool awaiting = false)
        {
                if (Type == DebugType.Normal)
                    Debug.Log(Text);
                else if (Type == DebugType.Error)
                    Debug.LogError(Text);
                else if (Type == DebugType.Warning)
                    Debug.LogWarning(Text);
        }
    }
}