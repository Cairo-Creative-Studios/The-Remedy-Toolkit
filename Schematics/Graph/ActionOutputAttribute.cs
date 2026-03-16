using BlueGraph;
using System.Collections;
using UnityEngine;

namespace Remedy.Schematics
{
    [BlockCast]
    public class ActionOutputAttribute : OutputAttribute
    {
        public bool NodeInput;

        public ActionOutputAttribute(string name = null) : base(name)
        {
            NodeInput = true;
        }
    }
}