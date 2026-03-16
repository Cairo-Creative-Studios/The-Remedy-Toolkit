using BlueGraph;
using System;
using UnityEngine;

namespace Remedy.Schematics
{
    [Serializable]
    [Output("▶", typeof(ActionPort), enableCasting: false, Multiple = true)]
    public abstract class SchematicActionNode : SchematicActionNodeBase
    { }

    [Serializable]
    [Input("▷", typeof(ActionPort), enableCasting: false, Multiple = true)]
    public abstract class SchematicActionNodeBase : SchematicGraphNode
    {
        protected bool _processChildren = true;

        private string _errorMessage;
        public string ErrorMessage => _errorMessage;

        public override object OnRequestValue(Port port)
        {
            return null;
        }


        public void Trigger(GameObject instance, bool awaiting = false)
        {
            OnTrigger(instance, false);

            if (_processChildren)
            {
                ProcessChildren(instance, false);
            }
        }

        protected virtual void OnTrigger(GameObject instance, bool awaiting = false)
        {
        }

        public void ProcessChildren(GameObject instance, bool awaiting = false, bool parallel = false)
        {
            if (IsDirty)
                UpdateCaches();

            if (_cachedChildren.Count == 0) return;

            for (int i = 0; i < _cachedChildren.Count; i++)
            {
                var child = _cachedChildren[i];
                child.Trigger(instance, awaiting);
            }
        }
    }
}
