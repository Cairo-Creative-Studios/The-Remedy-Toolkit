using BlueGraph;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

namespace Remedy.Schematics
{
    [Serializable]
    [Node(Path = "Objects/Transform"), Tags("Object")]
    public class SetPosition : SchematicActionNode
    {
        [Input("Target")]
        public GameObject Target;
        [Input("Position")]
        public Vector3 NewPosition;

        protected override void OnTrigger(GameObject instance, bool awaiting = false)
        {
            Target = GetInputValue<GameObject>("Target");
            Position = GetInputValue<Vector3>("Position");

            Target.transform.position = Position;
        }
    }
}