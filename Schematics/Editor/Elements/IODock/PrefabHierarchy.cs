using Remedy.Framework;
using SchematicAssets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// A custom Unity UI Toolkit element that displays a prefab's GameObjects and components in a foldout hierarchy 
/// with interactive input/output docking for visual programming.
/// </summary>
/// <remarks>
/// Designed for use in editor extensions like <see cref="SchematicGraphEditorWindow"/>. 
/// Provides prefab visualization with component tabs, docking interfaces, and undo/redo support.<br/>
/// <br/>
/// Features:<br/>
/// - Foldout hierarchy with persistent state<br/>
/// - Component tabs with add/remove support<br/>
/// - I/O docking via <see cref="IODockRegistry"/><br/>
/// - Selection synchronization with Unity's inspector<br/>
/// - Lazy loading for performance optimization<br/>
/// - Cached component and type data<br/>
/// </remarks>
public class PrefabHierarchy : VisualElement
{
    private GameObject _prefab;
    private string _assetPath;
    SerializableDictionary<Transform, SerializableDictionary<SerializableType, VisualElement>> evMap = new();

    // Caching structures
    private static List<Type> _availableComponentTypes;
    private static bool _componentTypesInitialized = false;

    // Lazy loading tracking
    private SchematicGraphEditorWindow _schematicWindow;
    private EditorWindow _editorWindow;
    private bool _includeSelf;
    private List<LazyListFoldout> _foldouts = new();

    /// <summary>
    /// Creates a new prefab hierarchy view.
    /// </summary>
    /// <param name="prefab">Prefab root GameObject to display.</param>
    /// <param name="editorWindow">Editor window hosting the view, used for state and selection handling.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="prefab"/> or <paramref name="editorWindow"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if <paramref name="prefab"/> is not a valid prefab instance.</exception>
    public PrefabHierarchy(GameObject prefab, EditorWindow editorWindow)
    {
        _prefab = prefab;
        _assetPath = AssetDatabase.GetAssetPath(prefab);

        // Initialize caching structures
        var container = new VisualElement();

        if (editorWindow is SchematicGraphEditorWindow asSchematicWindow)
        {
            _schematicWindow = asSchematicWindow;
        }

        RebuildHierarchy(_prefab.transform);

        PrefabRefreshUtility.ReimportAndResetPrefab(prefab);

        Add(container);
    }


    public void RebuildHierarchy(Transform transform)
    {
        Clear();
        var goFoldout = CreateGameObjectFoldout(transform, _schematicWindow, _editorWindow, _includeSelf);
        Add(goFoldout);
    }

    /// <summary>
    /// Creates a GameObjectFoldout with buttons for a given Transform.
    /// </summary>
    private VisualElement CreateGameObjectFoldout(Transform tf, SchematicGraphEditorWindow schematicWindow, EditorWindow editorWindow, bool includeSelf)
    {
        if (tf == null) return null;

        var childListContainer = new VisualElement();

        var childListFoldout = new LazyListFoldout(collection: tf.GetComponentsInImmediateChildren<Transform>(),
                                                    className: "game-object",
                                                    title: tf.name,

                                                    onItemAdded: (object item) =>
                                                    {
                                                    },

                                                    onItemRemoved: (object item) =>
                                                    {
                                                        var tfPath = _prefab.transform.GetRelativePath((Transform)item);
                                                        var copy = PrefabUtility.LoadPrefabContents(_assetPath);

                                                        var tfCopy = copy.transform.Find(tfPath);
                                                        if (tfCopy != null)
                                                        {
                                                            GameObject.DestroyImmediate(tfCopy.gameObject);
                                                            ApplyChangesToPrefabAsset(copy);
                                                        }
                                                    },

                                                    // Setup Item/VisualElements
                                                    createItemInstance: (Action<object> onCreationFinished) =>
                                                    {
                                                        var childObject = new GameObject().transform;

                                                        Add(new InlineIdentifierEditor(popupRect: childListContainer.Q<Toggle>().WorldBoundToScreen(),
                                                                                        identifierType: ListIdentifierType.Name,
                                                                                        getOriginalValue: () => childObject.name,
                                                                                        onFinishedEditting: (bool success, object value) =>
                                                                                        {
                                                                                            childObject.name = value.ToString();

                                                                                            var tfPath = _prefab.transform.GetRelativePath(tf);
                                                                                            var copy = PrefabUtility.LoadPrefabContents(_assetPath);

                                                                                            var tfCopy = copy.transform.Find(tfPath);
                                                                                            if (tfCopy != null)
                                                                                            {
                                                                                                childObject.SetParent(tfCopy, false);
                                                                                                ApplyChangesToPrefabAsset(copy);
                                                                                            }

                                                                                            onCreationFinished?.Invoke(tfCopy);
                                                                                        }));
                                                    },

                                                    createItemContent: (int index, object item) =>
                                                    {
                                                        var objectContainer = CreateGameObjectFoldout(((Transform)item), schematicWindow, editorWindow, includeSelf);

                                                        var toggle = objectContainer.Q<Toggle>();
                                                        //var toggleParent = toggle.parent;

                                                        var headerContainer = new VisualElement()
                                                        {
                                                            style =
                                                            {
                                                                flexDirection = FlexDirection.Column,
                                                                width =  Length.Percent(100)
                                                            }
                                                        };
                                                        headerContainer.AddToClassList("child-header");

                                                        var newToggle = new VisualElement()
                                                        {
                                                            style =
                                                            {
                                                                flexDirection = FlexDirection.Row
                                                            }
                                                        };
                                                        var componentRow = new VisualElement()
                                                        {
                                                            style =
                                                            {
                                                                flexDirection = FlexDirection.Row
                                                            }
                                                        };

                                                        componentRow.Add(CreateFoldoutContent(((Transform)item), schematicWindow));

                                                        componentRow.Children().ElementAt(0).style.width = Length.Percent(100);

                                                        foreach(var child in toggle.Children().ToList())
                                                        {
                                                            newToggle.Add(child);
                                                        }

                                                        headerContainer.Add(newToggle);
                                                        headerContainer.Add(componentRow);

                                                        toggle.Add(headerContainer);

                                                        //toggleParent.Insert(0, headerContainer);

                                                        return objectContainer;
                                                    },

                                                    getItemName: (object item) =>
                                                    {
                                                        return ((Transform)item) == null ? "" : ((Transform)item).name;
                                                    },

                                                    itemLabelClicked: (MouseDownEvent evt, object item, Action onItemEditted) =>
                                                    {
                                                        if (evt.clickCount == 2 && evt.button == 0)
                                                        {
                                                            var identifierEditor = new InlineIdentifierEditor(popupRect: childListContainer.Q<Toggle>().WorldBoundToScreen(),
                                                                                                                identifierType: ListIdentifierType.Name,
                                                                                                                getOriginalValue: () => { return ((Transform)item).name; },
                                                                                                                onFinishedEditting: (bool change, object newValue) =>
                                                                                                                {
                                                                                                                    var childObject = ((Transform)item);

                                                                                                                    var tfPath = _prefab.transform.GetRelativePath(childObject);
                                                                                                                    var copy = PrefabUtility.LoadPrefabContents(_assetPath);

                                                                                                                    var tfCopy = copy.transform.Find(tfPath);
                                                                                                                    if (tfCopy != null)
                                                                                                                    {
                                                                                                                        tfCopy.name = newValue.ToString();
                                                                                                                        ApplyChangesToPrefabAsset(copy);
                                                                                                                    }

                                                                                                                    onItemEditted?.Invoke();
                                                                                                                });
                                                        }
                                                    },
                                                    getListIcon: () => { return EditorGUIUtility.IconContent("GameObject Icon").image as Texture2D; } );


        _foldouts.Add(childListFoldout);

        childListContainer.Add(childListFoldout);

        return childListContainer;
    }

    private VisualElement CreateFoldoutContent(Transform tf, SchematicGraphEditorWindow schematicWindow)
    {
        var componentsOnObject = tf.gameObject.GetComponents<MonoBehaviour>();

        var foldout = new LazyFoldout(() =>
        {
            var container = new VisualElement();
            var ioGroup = new VisualElement();

            TabView tabView = new("", "components", true, !schematicWindow.ShowComponentNames);

            int compCount = 0;
            foreach (var component in componentsOnObject)
            {
                var type = component.GetType();
                var typeName = ObjectNames.NicifyVariableName(type.Name);
                var icon = EditorGUIUtility.ObjectContent(null, type).image;

                schematicWindow.SchematicGraph.Components.Add(GlobalObjectId.GetGlobalObjectIdSlow(_prefab));

                tabView.AddTab(typeName, icon).UseFactory(() => CreateTabContent(component, type, tf, schematicWindow));

                compCount++;
            }

            // Add "+" tab for adding components
            CreateAddComponentButton(tabView, tf);

            container.Add(tabView);

            return container;
        })
        {
            text = "components"
        };

        foldout.AddToClassList("component-foldout");

        return foldout;
    }

    /// <summary>
    /// Creates the actual content for a component tab.
    /// </summary>
    private VisualElement CreateTabContent(Component component, Type type, Transform tf, SchematicGraphEditorWindow schematicWindow)
    {
        var dock = new VisualElement();

        if (evMap.ContainsKey(tf) && evMap[tf].ContainsKey(type))
        {
            return evMap[tf][type];
        }

        var label = new Label(type.Name);
        dock.Add(label);

        var (draw, dockElement) = IODockRegistry.RenderObjectDock(type, schematicWindow, component, _prefab.transform);
        SignalRenderer.DrawDelayedEvents();

        FieldInfo propertiesField = GetPropertiesField(type);

        if (draw || propertiesField != null)
        {
            if (dockElement == null)
                dockElement = new VisualElement();

            if (propertiesField != null)
            {
                var propertiesFoldout = CreatePropertiesFoldout(component, type, propertiesField, tf);
                dockElement.Add(propertiesFoldout);
            }

            dock.Add(dockElement);
        }

        return dock;
    }

    /// <summary>
    /// Gets the properties field from a component type using reflection (cached).
    /// </summary>
    private FieldInfo GetPropertiesField(Type type)
    {
        var fields = type.GetFields();
        foreach (var field in fields)
        {
            if (!typeof(ScriptableObject).IsAssignableFrom(field.FieldType))
                continue;

            var attributes = field.GetCustomAttributes();
            foreach (var attr in attributes)
            {
                if (attr.GetType() == typeof(SchematicPropertiesAttribute))
                {
                    return field;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Creates a foldout for component properties.
    /// </summary>
    private Foldout CreatePropertiesFoldout(Component component, Type type, FieldInfo propertiesField, Transform tf)
    {
        var propertiesFoldout = new Foldout()
        {
            text = "Properties",
            value = false,
            style =
            {
                width = Length.Percent(100),
                flexGrow = 0,
                flexShrink = 0
            }
        };

        propertiesFoldout.AddManipulator(new ContextualMenuManipulator(evt =>
        {
            evt.menu.AppendAction("Reset Properties", _ =>
            {
                var newProperties = ScriptableObject.CreateInstance(propertiesField.FieldType);
                newProperties.name = "Properties";
                AssetDatabase.AddObjectToAsset(newProperties, _assetPath);
                AssetDatabase.SaveAssets();

                propertiesField.SetValue(component, newProperties);

                var copy = PrefabUtility.LoadPrefabContents(_assetPath);
                var tfPath = _prefab.transform.GetRelativePath(tf);
                var tfCopy = copy.transform.Find(tfPath);
                if (tfCopy != null)
                {
                    var componentCopy = tfCopy.gameObject.GetComponent(type);
                    propertiesField.SetValue(componentCopy, newProperties);
                    ApplyChangesToPrefabAsset(copy);
                }
            });
        }));

        var propertiesObject = (ScriptableObject)propertiesField.GetValue(component);

        if (propertiesObject == null && propertiesField != null)
        {
            propertiesObject = SchematicAssetManager.Create(component, "", "", "Properties", propertiesField.FieldType);

            var tfPath = _prefab.transform.GetRelativePath(tf);
            var copy = PrefabUtility.LoadPrefabContents(_assetPath);

            var tfCopy = copy.transform.Find(tfPath);
            if (tfCopy != null)
            {
                var componentCopy = tfCopy.gameObject.GetComponent(type);
                propertiesField.SetValue(componentCopy, propertiesObject);
                ApplyChangesToPrefabAsset(copy);
            }
        }

        var container = new IMGUIContainer(() =>
        {
            var soEditor = Editor.CreateEditor(propertiesObject);
            soEditor.OnInspectorGUI();
        });

        propertiesFoldout.Add(container);
        return propertiesFoldout;
    }

    /// <summary>
    /// Creates the remove component button.
    /// </summary>
    private Button CreateRemoveComponentButton(Type type, Transform tf, GameObjectFoldout goFoldout)
    {
        var removeComponentButton = new Button(() =>
        {
            var tfPath = _prefab.transform.GetRelativePath(tf);
            var copy = PrefabUtility.LoadPrefabContents(_assetPath);
            var tfCopy = copy.transform.Find(tfPath);
            if (tfCopy != null)
            {
                var component = tfCopy.gameObject.GetComponent(type);
                SchematicEditorData.DeleteComponentData(copy, tfPath, type);
                Component.DestroyImmediate(component);
                ApplyChangesToPrefabAsset(copy);
            }
        })
        {
            text = "-"
        };

        removeComponentButton.style.minWidth = 10;
        removeComponentButton.style.minHeight = 10;
        removeComponentButton.style.maxWidth = 10;
        removeComponentButton.style.maxHeight = 10;
        removeComponentButton.style.backgroundColor = SchematicGraphEditorWindow.ColorLookupFunctional["Remove"];
        removeComponentButton.RegisterCallback<MouseEnterEvent>(evt => removeComponentButton.style.backgroundColor = SchematicGraphEditorWindow.ColorLookupFunctional["RemoveHighlight"]);
        removeComponentButton.RegisterCallback<MouseLeaveEvent>(evt => removeComponentButton.style.backgroundColor = SchematicGraphEditorWindow.ColorLookupFunctional["Remove"]);

        return removeComponentButton;
    }

    /// <summary>
    /// Creates the "Add Component" button with cached component type list.
    /// </summary>
    private void CreateAddComponentButton(TabView tabView, Transform tf)
    {
        tabView.AddTab("+").UseFactory(() =>
        {
            // Initialize component types cache if needed
            if (!_componentTypesInitialized)
            {
                _availableComponentTypes = BuildAvailableComponentTypesList();
                _componentTypesInitialized = true;
            }

            var menu = new GenericMenu();

            foreach (var type in _availableComponentTypes)
            {
                var attr = type.GetCustomAttribute<SchematicComponentAttribute>();
                string menuPath = attr != null ? $"{attr.Path}" : "Unsorted/" + type.FullName;

                menu.AddItem(new GUIContent(menuPath), false, () =>
                {
                    var tfPath = _prefab.transform.GetRelativePath(tf);
                    var copy = PrefabUtility.LoadPrefabContents(_assetPath);
                    var tfCopy = copy.transform.Find(tfPath);

                    if (tfCopy != null)
                    {
                        var component = tfCopy.gameObject.AddComponent(type);
                        ApplyChangesToPrefabAsset(copy);
                    }
                });
            }

            menu.ShowAsContext();

            return null;
        });

        var addTabButton = tabView;

        addTabButton.AddToClassList("tab-add-component");
    }

    /// <summary>
    /// Builds a cached list of available component types that can be added.
    /// </summary>
    private static List<Type> BuildAvailableComponentTypesList()
    {
        var allTypes = new List<Type>();
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var assembly in assemblies)
        {
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                types = e.Types;
            }

            if (types == null) continue;

            foreach (var type in types)
            {
                if (type == null) continue;
                if (!typeof(MonoBehaviour).IsAssignableFrom(type)) continue;

                bool include = false;

                // Case 1: renderer override
                var renderer = IODockRegistry.GetComponentRenderer(type);
                if (renderer != null && renderer.GetType() != typeof(DefaultIODockRenderer))
                {
                    include = true;
                }

                // Case 2: field IOBase
                if (!include)
                {
                    var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    foreach (var field in fields)
                    {
                        if (typeof(SignalData).IsAssignableFrom(field.FieldType))
                        {
                            include = true;
                            break;
                        }
                    }
                }

                // Case 3: property IOBase
                if (!include)
                {
                    var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    foreach (var property in properties)
                    {
                        if (typeof(SignalData).IsAssignableFrom(property.PropertyType))
                        {
                            include = true;
                            break;
                        }
                    }
                }

                if (include)
                    allTypes.Add(type);
            }
        }

        return allTypes;
    }

    /// <summary>
    /// Applies changes to the prefab asset and triggers rebuild if needed.
    /// </summary>
    private async void ApplyChangesToPrefabAsset(GameObject copy)
    {
        PrefabUtility.SaveAsPrefabAsset(copy, _assetPath);
        PrefabUtility.UnloadPrefabContents(copy);

        AssetDatabase.Refresh();

        await EditorUtilities.NextEditorFrame();

        PrefabRefreshUtility.ReimportAndResetPrefab(_prefab);
        RebuildHierarchy(_prefab.transform);
    }
}