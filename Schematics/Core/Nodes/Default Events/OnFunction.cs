// project armada

#pragma warning disable 0414

using BlueGraph;
using Remedy.Schematics.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


namespace Remedy.Schematics
{
    [Serializable]
    [Node(Path = "Events"), Tags("Object")]
    public class OnFunction : SchematicEventNode
    {
        [Editable]
        public string FunctionName;
        [Output]
        public List<FunctionParameter> FunctionParameters;

        protected override void OnTrigger(GameObject instance, params Union[] arguments)
        {
            if(FunctionName != arguments[0].GetValue<string>())
            {
                _blocked = true;
                return;
            }

            for(int i = 1; i < arguments.Length; i++)
            {
                _cachedOutputsByName[FunctionParameters[i].Name] = arguments[i];
            }
        }
    }
}