// project armada

#pragma warning disable 0414

using BlueGraph;
using System;
using UnityEngine;
using UnityEngineInternal;

namespace Remedy.Schematics
{
    [Serializable]
    [Node(Path = "Collider"), Tags("Object")]
    public class HasColliderForward : SchematicConditionalNode
    {
        [Input("Object")]
        public GameObject Object;
        [Input("Other Tag", Editable = true)]
        public string Tag = "";
        [Input("Distance", Editable = true)]
        public float Distance = 5.0f;
        [Input("Radius", Editable = true)]
        public float Radius = 1.0f;

        [Output("Other GameObject")]
        public GameObject OtherGameObject;

        public override bool Condition()
        {
            if (GetPort("OtherTag").ConnectionCount != 0)
                Tag = GetInputValue<string>("OtherTag");

            if (GetPort("Distance").ConnectionCount != 0)
                Distance = GetInputValue<float>("Distance");

            if (GetPort("Radius").ConnectionCount != 0)
                Radius = GetInputValue<float>("Radius");

            var transform = Object.transform;
            RaycastHit hit;

            if (Physics.SphereCast(transform.position, Radius, transform.forward, out hit))
            {
                if(Tag == "" || hit.collider.tag == Tag)
                {
                    SetOutputValue("Other GameObject", hit.collider.gameObject);
                    return true;
                }
            }
            return false;
        }

        public override object OnRequestValue(Port port)
        {
            if (port.Name == "Other GameObject")
                return OtherGameObject;

            return base.OnRequestValue(port);
        }
    }
}