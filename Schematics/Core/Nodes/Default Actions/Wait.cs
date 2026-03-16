using BlueGraph;
using Cysharp.Threading.Tasks;
using Remedy.Framework;
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace Remedy.Schematics
{
    [Serializable]
    [Node(Path = "Time"), Tags("Default")]
    public class Wait : SchematicActionNode
    {
        [Input("Wait Time", typeof(float))]
        public float waitTime = 1f;

        protected override void OnTrigger(GameObject instance, bool awaiting = false)
        {
            _processChildren = false;
            //await UniTask.Delay((int)(GetInputValue<float>("Wait Time") * 1000));
            ProcessChildren(instance);
        }
    }
}