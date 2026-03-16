// project armada

#pragma warning disable 0414

using BlueGraph;
using UnityEngine;
using System;

namespace Remedy.Schematics
{
    [Serializable]
    [Node(Path = "Conditions"), Tags("Default")] 
    public class If : SchematicActionNode
    {
        [Input("Condition")]
        public bool Condition;
        [Editable]
        public bool Not;
        [Editable]
        public bool TriggerOnce;
        private bool _triggered = false;

        protected override void OnTrigger(GameObject instance, bool awaiting = false)
        {
            bool ogValue = GetInputValue<bool>(nameof(Condition)) != Not;
            
            if(TriggerOnce)
            {
                if (_triggered)
                    ogValue = false;
                else if(!ogValue)
                    _triggered = false;
            }

            if (ogValue)
                _processChildren = ogValue;
        }
    }
}