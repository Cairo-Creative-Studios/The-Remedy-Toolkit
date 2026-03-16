using Remedy.Framework;
using System;
using System.Collections.Generic;
//using System.Linq;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor;
using System.Linq;
using Remedy.Schematics.Utils;
using SchematicAssets;

[FieldRendererTarget(typeof(EventContainerRendererAttribute))]
public class SignalRenderer : FieldRenderer
{
    private static List<SignalRenderer> _eventLinkWaitPool = new(); // Events waiting for the rest of the Component to finish displaying before attempting to draw
    private EventContainerRendererAttribute _evAttr;
    private SchematicEventLinkAttribute _linkAttr;
    private static Texture2D _oIcon;
    private static Texture2D _outputIcon => _oIcon ??= Resources.Load<Texture2D>("Icons/Output");
    private static Texture2D _iIcon;
    private static Texture2D _inputIcon => _iIcon ??= Resources.Load<Texture2D>("Icons/Input");

    // Reflection caches
    private static readonly Dictionary<Type, Type[]> _genericArgumentsCache = new();
    private static readonly Dictionary<Type, Type> _baseTypeCache = new();
    private static readonly Dictionary<Type, Type> _eventTypeForArgumentCache = new();
    private static readonly Dictionary<Type, bool> _isAssignableFromCache = new();
    private static readonly Dictionary<(Type, Type), bool> _typeAssignabilityCache = new();

    public SignalRenderer(SchematicGraphEditorWindow window, UnityEngine.Object obj, object parent, object target, FieldOrPropertyInfo fieldInfo, CustomFieldRendererAttribute attr, string path, List<Action> onModified) : base(window, obj, parent, target, fieldInfo, attr, path, onModified)
    {
        _evAttr = (EventContainerRendererAttribute)attr;

        SchematicEventLinkAttribute linkAttr = null;

        if (fieldInfo != null)
        {
            var attributes = fieldInfo.GetCustomAttributes();
            for (int i = 0; i < attributes.Count(); i++)
            {
                var attribute = attributes.ElementAt(i) as SchematicEventLinkAttribute;
                if (attribute != null)
                {
                    linkAttr = attribute;
                    break;
                }
            }
        }

        _linkAttr = linkAttr;

        if (_evAttr == null)
            _evAttr = new EventContainerRendererAttribute(typeof(SignalBase), _field.Name, false);

        if (_linkAttr != null)
            DrawEventContainer();
        else
            _eventLinkWaitPool.Add(this);


    }

    /// <summary>
    /// Cached wrapper for Type.GetGenericArguments()
    /// </summary>
    private static Type[] GetGenericArguments(Type type)
    {
        if (!_genericArgumentsCache.TryGetValue(type, out var args))
        {
            args = type.GetGenericArguments();
            _genericArgumentsCache[type] = args;
        }
        return args;
    }

    /// <summary>
    /// Cached wrapper for Type.BaseType
    /// </summary>
    private static Type GetBaseType(Type type)
    {
        if (!_baseTypeCache.TryGetValue(type, out var baseType))
        {
            baseType = type.BaseType;
            _baseTypeCache[type] = baseType;
        }
        return baseType;
    }

    /// <summary>
    /// Cached wrapper for Type.IsAssignableFrom()
    /// </summary>
    private static bool IsAssignableFrom(Type targetType, Type sourceType)
    {
        var key = (targetType, sourceType);
        if (!_typeAssignabilityCache.TryGetValue(key, out var result))
        {
            result = targetType.IsAssignableFrom(sourceType);
            _typeAssignabilityCache[key] = result;
        }
        return result;
    }

    /// <summary>
    /// Clears all reflection caches. Call this if types are reloaded or modified at runtime.
    /// </summary>
    public static void ClearReflectionCaches()
    {
        _genericArgumentsCache.Clear();
        _baseTypeCache.Clear();
        _eventTypeForArgumentCache.Clear();
        _isAssignableFromCache.Clear();
        _typeAssignabilityCache.Clear();
    }

    /// <summary>
    /// Draws the Events that were waiting for the rest of the Component to render before they themselves rendered.
    /// </summary>
    public static void DrawDelayedEvents()
    {
        foreach (var eventContainer in _eventLinkWaitPool)
        {
            eventContainer.DrawEventContainer();
        }
        _eventLinkWaitPool.Clear();
    }

    protected void DrawEventContainer()
    {
        var handler = new SignalHandler(GlobalObjectId.GetGlobalObjectIdSlow(_object), _path, _field);

        var column = new VisualElement
        {
            style =
            {
                flexDirection = FlexDirection.Column,

                borderBottomColor = Color.gray4,
                borderTopColor = Color.gray4,

                borderRightColor = Color.black,
                borderLeftColor = Color.black
            }
        };

        var row = new VisualElement
        {
            style =
            {
                flexDirection = FlexDirection.Row,
                borderBottomWidth = 0,
                paddingLeft = 0
            }
        };

        if (_linkAttr != null)
        {
            var otherComponent = ((MonoBehaviour)_object).gameObject.GetComponent(_linkAttr.Type);
            var otherField = _linkAttr.Type.GetField(_linkAttr.FieldName);
            if (otherComponent != null && otherField != null)
            {
                var parentLabel = new Label
                {
                    text = "> " + _linkAttr.Type + _linkAttr.FieldName,
                    style =
                    {
                        fontSize = 8,
                        color = Color.gray8,

                        marginTop = 2,
                        marginLeft = 2
                    }
                };
                parentLabel.AddToClassList("event-link-label");

                column.Add(parentLabel);

                _object = otherComponent;
                _field = otherField;
            }
        }

        // Name field
        var nameField = new TextField { value = _field.Name, style = { flexGrow = 1, paddingLeft = 0 } };

        nameField.Children().ElementAt(0).AddToClassList("event-name");

        nameField.RegisterCallback<MouseEnterEvent>(_ => nameField.Children().ElementAt(0).AddToClassList("event-name-hover"));
        nameField.RegisterCallback<MouseLeaveEvent>(_ => nameField.Children().ElementAt(0).RemoveFromClassList("event-name-hover"));

        string oldName = nameField.value;
        string newName = nameField.value;

        nameField.RegisterValueChangedCallback(evt =>
        {
            newName = evt.newValue;
        });

        nameField.RegisterCallback<FocusInEvent>(evt =>
        {
            Undo.RecordObject(_window, "Edit Event Name");
            oldName = nameField.value;
            _window.Canvas.Reload();
        });

        nameField.RegisterCallback<FocusOutEvent>((EventCallback<FocusOutEvent>)(evt =>
        {
            Undo.RecordObject(_window, "Edit Event Name");
            handler.ChangeName(oldName, newName);
            _window.Canvas.Reload();
        }));

        // Global toggle
        var globalToggle = new Toggle { value = handler.SignalAsset.Global };

        globalToggle.RegisterValueChangedCallback(evt =>
        {
            Undo.RecordObject(_window, "Toggle Global");
            handler.SignalAsset.Global = evt.newValue;
            _window.Canvas.Reload();
        });

        bool signalType = _field.Attributes.Any(attr => attr.AttributeType == typeof(OutputSignalAttribute));

        // Drag Handle
        Texture2D myTexture = signalType ? myTexture = _outputIcon : myTexture = _inputIcon;

        Image imageElement = new Image();

        imageElement.style.cursor = new StyleCursor(UnityDefaultCursor.DefaultCursor(UnityDefaultCursor.CursorType.Link));
        imageElement.RegisterCallback<MouseDownEvent>(evt =>
        {
            DragAndDrop.PrepareStartDrag();
            DragAndDrop.objectReferences = new UnityEngine.Object[] { handler.SignalAsset };
            DragAndDrop.StartDrag("Drag ScriptableEvent");
            evt.StopPropagation();
        });

        imageElement.RegisterCallback<MouseEnterEvent>(evt =>
        {
            column.AddToClassList("event-container-hover");
        });

        imageElement.RegisterCallback<MouseLeaveEvent>(evt =>
        {
            column.RemoveFromClassList("event-container-hover");
        });

        imageElement.image = myTexture;
        imageElement.style.width = 32;
        imageElement.style.height = 24;
        imageElement.style.opacity = 0.75f;

        imageElement.tooltip = signalType ? "Output Signal" : "Input Signal";

        row.Add(nameField);
        row.Add(imageElement);

        column.AddToClassList("event-container");
        column.AddToClassList("event-container-" + (signalType ? "output" : "input"));

        var signalData = handler.SignalAsset;

        _window.SchematicGraph.AddSignalToCache(signalData);

        var paramAttrs = _field.GetCustomAttributes().OfType<ParameterAttribute>();

        signalData.Parameters.Contents.Clear();

        foreach(var param in paramAttrs)
        {
            if(!signalData.Parameters.Contents.Any(item => item.Name == param.Name))
            {
                signalData.Parameters.AddParameter(param.Name, new Union(param.Type));
                var paramInSignal = signalData.Parameters.Contents.FirstOrDefault(item => item.Name == param.Name);
                paramInSignal.Value.Type = Union.ReverseTypeLookup[param.Type];
            }
        }
        foreach (var param in paramAttrs)
        {
            if (signalData.Parameters.Contents.Any(item => item.Name == param.Name))
            {
                var paramInSignal = signalData.Parameters.Contents.FirstOrDefault(item => item.Name == param.Name);
                paramInSignal.Value.Type = Union.ReverseTypeLookup[param.Type];
            }
        }

        var paramList = new LazyListFoldout(collection: signalData.Parameters.Contents,
                                            className: "event-parameters",
                                            title: "Parameters",
                                            onItemAdded: (object item) => 
                                            { 
                                                signalData.Parameters.Contents.Add((SignalData.Parameter)item);
                                                signalData.RuntimeArgs = new Union[signalData.Parameters.Contents.Count];
                                            },
                                            onItemRemoved: (object item) => 
                                            { 
                                                signalData.Parameters.Contents.Remove((SignalData.Parameter)item);
                                                signalData.RuntimeArgs = new Union[signalData.Parameters.Contents.Count];
                                            },
                                            createItemInstance: (Action<object> onFinished) =>
                                            {
                                                var value = new SignalData.Parameter();
                                                onFinished?.Invoke(value);
                                            },
                                            createItemContent: (int index, object value) =>
                                            {
                                                var param = signalData.Parameters[index];
                                                var paramType = param.Type;

                                                var typePopup = new PopupField<Type>(
                                                    Union.TypeLookup.Values.Where(type => type != null).ToList(),
                                                    Union.TypeLookup[param.Type],
                                                    t => t?.Name ?? "???",
                                                    t => t?.Name ?? "???"
                                                );

                                                typePopup.SetEnabled(_evAttr.ParamTypeModifiable);

                                                var nameField = new TextField { value = ((SignalData.Parameter)value).Name , style = { flexGrow = 1, paddingLeft = 0 } };

                                                nameField.Children().ElementAt(0).AddToClassList("event-name");

                                                nameField.RegisterCallback<MouseEnterEvent>(_ => nameField.Children().ElementAt(0).AddToClassList("event-name-hover"));
                                                nameField.RegisterCallback<MouseLeaveEvent>(_ => nameField.Children().ElementAt(0).RemoveFromClassList("event-name-hover"));

                                                string oldName = nameField.value;
                                                string newName = nameField.value;
                                                                
                                                nameField.RegisterValueChangedCallback(evt =>
                                                {
                                                    newName = evt.newValue;
                                                });

                                                nameField.SetEnabled(_evAttr.ParamNameModifiable);

                                                var paramRow = new VisualElement
                                                {
                                                    style =
                                                    {
                                                        flexDirection = FlexDirection.Row,
                                                        borderBottomWidth = 0,
                                                        paddingLeft = 0
                                                    }
                                                };

                                                paramRow.AddToClassList("event-parameter-container");
                                                paramRow.RegisterCallback<MouseEnterEvent>(_ => nameField.Children().ElementAt(0).AddToClassList("event-parameter-container-hover"));
                                                paramRow.RegisterCallback<MouseLeaveEvent>(_ => nameField.Children().ElementAt(0).RemoveFromClassList("event-parameter-container-hover"));

                                                paramRow.Add(typePopup);
                                                paramRow.Add(nameField);

                                                var typeDef = SchematicEditorData.EditorTypeSettings.FirstOrDefault(item => Type.GetType(item.Type) == typePopup.value);
                                                if (typeDef != null)
                                                    paramRow.style.backgroundColor = typeDef.ColorInEditor;
                                                else
                                                {
                                                    Debug.LogWarning("Type Definition does not exist for: " + typePopup.value);
                                                }

                                                return paramRow;
                                            },
                                            getItemName: (object item) => { return ((SignalData.Parameter)item).Name; },
                                            contentModifiable: _evAttr.ParametersEditable,
                                            startExpanded: true
                                            );

        signalData.RuntimeArgs = new Union[signalData.Parameters.Contents.Count];

        _window.SchematicGraph.AddSignalToCache(signalData);

        column.Add(row);
        column.Add(paramList);

        Add(column);
    }


    protected IMGUIContainer DrawGUIContainer()
    {
        SerializedObject seriializedObject = new SerializedObject(_object);

        var valPath = _path + "." + _field.Name + "." + nameof(SignalData);
        var property = seriializedObject.FindProperty(valPath);

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

        return imguiContainer;
    }
}