// project armada

#pragma warning disable 0414

using BlueGraph;
using System;
using UnityEngine;

namespace Remedy.Schematics
{
    [Serializable]
    [Node(Path = "Schematic"), Tags("Default")]
    public class GetChild : SchematicGraphNode
    {
        [Output]
        public Transform Child;

        public override object OnRequestValue(Port port)
        {
            return Child;
        }
    }
}