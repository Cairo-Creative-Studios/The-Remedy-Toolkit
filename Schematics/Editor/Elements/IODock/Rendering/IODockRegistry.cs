using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using System.Reflection;
using Remedy.Framework;
using UnityEditor;
using System.IO;
using Remedy.Schematics;
using PlasticPipe.PlasticProtocol.Messages;

public static class IODockRegistry
{
    private static Dictionary<Type, IIODockRenderer> _componentRenderers;
    private static Dictionary<Type, Type> _fieldElementTypes;
    private static IIODockRenderer _defaultRenderer;

    static IODockRegistry()
    {
        _componentRenderers = new Dictionary<Type, IIODockRenderer>();

        var rendererTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => !t.ContainsGenericParameters && typeof(IIODockRenderer).IsAssignableFrom(t) && !t.IsAbstract);

        foreach (var type in rendererTypes)
        {
            var attr = type.GetCustomAttributes(typeof(IODockTargetAttribute), false)
                           .FirstOrDefault() as IODockTargetAttribute;

            if (attr != null)
            {
                var instance = (IIODockRenderer)Activator.CreateInstance(type);
                _componentRenderers[attr.TargetType] = instance;
            }

            _defaultRenderer = (DefaultIODockRenderer)Activator.CreateInstance(typeof(DefaultIODockRenderer));
        }


        _fieldElementTypes = new Dictionary<Type, Type>();

        var fieldRendererTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(VisualElement).IsAssignableFrom(t) && !t.IsAbstract);

        foreach (var type in fieldRendererTypes)
        {
            var attr = type.GetCustomAttributes(typeof(FieldRendererTargetAttribute), false)
                           .FirstOrDefault() as FieldRendererTargetAttribute;

            if (attr != null)
            {
                _fieldElementTypes[attr.TargetType] = type;
            }
        }
    }

    public static IIODockRenderer GetComponentRenderer(Type componentType)
    {
        if (_componentRenderers.TryGetValue(componentType, out var renderer))
            return renderer;

        return _defaultRenderer;
    }

    public static (bool, VisualElement) RenderObjectDock(Type type, SchematicGraphEditorWindow schematicWindow, UnityEngine.Object component, Transform prefabTransform, string specifiedPath = "")
    {
        return GetComponentRenderer(type).Render(schematicWindow, component, prefabTransform, specifiedPath);
    }

    /// <summary>
    /// Uses reflection to render each field instance within a target object as defined within the Attributes added to the Fields in that Target's Class.
    /// </summary>
    /// <param name="parent"></param>
    /// <returns></returns>
    public static (bool, VisualElement) RenderMultipleFields(SchematicGraphEditorWindow window, UnityEngine.Object obj, object parent, string propertyPath = "", List<Action> onModified = null, bool displayObjectName = false)
    {
        var container = new VisualElement()
        {
            name = obj.name + " Fields Container",
            style =
            {
                minWidth = Length.Percent(100),
                flexDirection = FlexDirection.Column
            }
        };

        List<FieldOrPropertyInfo> ioMembers = new();

        foreach(var field in parent.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            ioMembers.Add(field);
        }

        foreach (var prop in parent.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            ioMembers.Add(prop);
        }

        bool draw = false;
        foreach (var field in ioMembers)
        {
            try
            {
                bool fieldDrawn = false;
                VisualElement element = null;

                var attr = field.GetCustomAttributes().FirstOrDefault(t => typeof(CustomFieldRendererAttribute).IsAssignableFrom(t.GetType()));

                if (attr == null)
                {
                    if (typeof(SignalBase).IsAssignableFrom(field.MemberType))
                    {
                        (fieldDrawn, element) = RenderElementForField(window, obj, parent, field.GetValue(parent), field, propertyPath, onModified, attr, typeof(SignalRenderer));

                        container.Add(element);
                        draw = true;
                        fieldDrawn = true;
                    }
                }
                else
                {
                    (fieldDrawn, element) = RenderElementForField(window, obj, parent, field.GetValue(parent), field, propertyPath, onModified, attr);

                    container.Add(element);
                    fieldDrawn = true;

                    draw = true;
                }
            }
            catch(Exception e)
            {
                Debug.LogException(e);
            }
        }

        return (draw, container);
    }

    /// <summary>
    /// Uses reflection to render the Field Instance for the given Target based on it's defined <see cref="CustomFieldRendererAttribute"/> if it has one.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="field"></param>
    /// <returns></returns>
    public static (bool, VisualElement) RenderElementForField(SchematicGraphEditorWindow window, UnityEngine.Object obj, object parent, object target, FieldOrPropertyInfo field, string path, List<Action> onModified, Attribute attr, Type specificRenderer = null)
    {
        if(attr != null)
        {
            Type attrType = attr.GetType();

            if (_fieldElementTypes.TryGetValue(attrType, out var renderType))
            {
                return (true, (VisualElement)Activator.CreateInstance(renderType, new[] { window, obj, parent, target, field, attr, path, onModified }));
            }
        }
        else if (specificRenderer != null)
        {
            return (true, (VisualElement)Activator.CreateInstance(specificRenderer, new[] { window, obj, parent, target, field, attr, path, onModified }));
        }

        return (false, null);
    }
}
