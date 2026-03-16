using BlueGraph;
using BlueGraph.Editor;
using Remedy.Schematics;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

[CustomNodeView(typeof(GetChild))]
public class GetChildNodeView : NodeView
{
    Label labelElement;
    protected override void OnInitialize()
    {
        base.OnInitialize();

        var node = Target as GetChild;
        var schematic = node.Schematic;
        var prefab = schematic.Prefab;

        var children = prefab.transform.Cast<Transform>().ToList();

        if (children.Count == 0) return;

        var childrenDropdown = new PopupField<Transform>(
            children,          
            0                  
        );

        childrenDropdown.RegisterValueChangedCallback(evt =>
        {
            node.Child = evt.newValue;
            node.IsDirty = true;
        });

        var portView = this.Q<PortView>();

        if(portView != null)
        {
            labelElement = portView.Q<Label>();
            
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.Add(childrenDropdown);

            portView.Add(container);

            //labelElement.text = "";
        }
    }
}