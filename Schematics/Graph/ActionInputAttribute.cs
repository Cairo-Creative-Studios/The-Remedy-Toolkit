﻿using BlueGraph;
using System.Collections;
using UnityEngine;

namespace Remedy.Schematics
{
    public class ActionInputAttribute : InputAttribute
    {
        public bool NodeInput;

        public ActionInputAttribute(string name = null) : base(name)
        {
            NodeInput = true;
        }
    }
}