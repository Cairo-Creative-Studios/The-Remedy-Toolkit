using BlueGraph;
using System;
using Remedy.Schematics.Utils;
using UnityEngine;

namespace Remedy.Schematics
{
    [Serializable]
    [Output("▶", typeof(ActionPort), enableCasting: false, Multiple = true)]
    public class SchematicEventNode : SchematicGraphNode
    {
        protected bool _blocked = false;


        public override object OnRequestValue(Port port)
        {
            return null;
        }

        public void Trigger(GameObject instance, params Union[] arguments)
        {
            int i = 0;
            foreach(var kvp in _cachedOutputsByName)
            {
                _cachedOutputsByName[kvp.Key] = arguments[i];
                i++;
            }

            OnTrigger(instance);

            if(!_blocked)
                ProcessChildren(instance);
        }

        protected virtual void OnTrigger(GameObject instance, params Union[] arguments)
        { }

        public void ProcessChildren(GameObject instance, bool awaiting = false, bool parallel = true)
        {
            if (IsDirty)
                UpdateCaches();

            if (_cachedChildren.Count == 0) return;

                for (int i = 0; i < _cachedChildren.Count; i++)
                {
                    var child = _cachedChildren[i];
                    child.Trigger(instance, false);
                }
        }
    }
}
