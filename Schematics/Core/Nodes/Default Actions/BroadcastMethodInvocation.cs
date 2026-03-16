using BlueGraph;
using Remedy.Framework;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

namespace Remedy.Schematics
{
    [Serializable]
    [Node(Path = "Objects"), Tags("Object")]
    public class BroadcastMethodInvocation : SchematicActionNode
    {
        [Input("Target")]
        public GameObject Target;
        [Input("Name")]
        public string MethodName;
        [Input("Arguments")]
        public List<object> MethodArguments;

        protected override void OnTrigger(GameObject instance, bool awaiting = false)
        {
            Target = GetOutputValue<GameObject>("Target");
            MethodName = GetOutputValue<string>("Name");
            MethodArguments = GetOutputValue<List<object>>("Arguments");

            var components = Target.GetComponents<MonoBehaviour>();
           
            foreach(var component in components)
            {
                component.CallMethod(MethodName, MethodArguments.ToArray());
            }
        }
    }
} 