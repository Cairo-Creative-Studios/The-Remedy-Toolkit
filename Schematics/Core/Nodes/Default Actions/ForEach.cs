// project armada

#pragma warning disable 0414

using BlueGraph;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using System;
using Remedy.Schematics.Utils;

namespace Remedy.Schematics
{
    [Serializable]
    [Node(Path = "Loops"), Tags("Default")] 
    public class ForEach : SchematicActionNode
    {
        [Input("Items")]
        public List<Union> Items;
        [Output("Index")]
        public int Index;
        [Output("This")]
        public object CurrentItem;

        protected override void OnTrigger(GameObject instance, bool awaiting = false)
        {
            Items = GetInputValue<List<Union>>("Pool");
            _processChildren = false;
            
            for(int i = 0; i < Items.Count - 1; i++)
            {
                SetOutputValue(nameof(CurrentItem), Items[i]);
                SetOutputValue(nameof(Index), i);
                ProcessChildren(instance);
            }
        }
    }
}