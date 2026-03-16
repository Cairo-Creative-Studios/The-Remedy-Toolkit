using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine.UIElements;

[FieldRendererTarget(typeof(IMGUIContainerRendererAttribute))]
public class IMGUIContainerRenderer : FieldRenderer
{
    public IMGUIContainerRenderer(SchematicGraphEditorWindow window, UnityEngine.Object obj, object parent, object target, FieldOrPropertyInfo fieldInfo, CustomFieldRendererAttribute attr, string path, List<Action> onModified = null) : base(window, obj, parent, target, fieldInfo, attr, path, onModified)
    {
    }

    public string FieldPath { get; private set; }

    protected override void Redraw()
    {
        SerializedObject seriializedObject = new SerializedObject(_object);

        var property = seriializedObject.FindProperty(_path + "." + _field.Name);

        var imguiContainerContainer = new VisualElement();

        var imguiContainer = new IMGUIContainer()
        {
            style =
            {
                flexGrow = 0,
                flexShrink = 0
            }
        };

        imguiContainer.onGUIHandler = () =>
        {
            seriializedObject.Update();
            EditorGUILayout.PropertyField(property);

            // Ensure the value of the property is not null
            if (property.boxedValue == null)
                property.boxedValue = default;

            seriializedObject.ApplyModifiedProperties();

            EditorApplication.QueuePlayerLoopUpdate();
            imguiContainer.MarkDirtyRepaint();
        };

        EditorApplication.update += () =>
        {
            if (imguiContainer == null) return;
            imguiContainer.MarkDirtyRepaint();
        }; 

        imguiContainer.focusable = true;
        imguiContainer.tabIndex = 0;

        Add(imguiContainer);
    }
}