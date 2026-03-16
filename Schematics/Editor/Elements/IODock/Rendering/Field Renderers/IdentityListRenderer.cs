using Remedy.Framework;
using System.Collections;
using System;
using System.Linq;
using UnityEngine.UIElements;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal.Profiling.Memory.Experimental;

[FieldRendererTarget(typeof(IdentityListRendererAttribute))]
public class IdentityListContainerRenderer : FieldRenderer
{
    private IdentityListRendererAttribute _listAttr;

    private VisualElement _container;
    private LazyListFoldout _containerFoldout;

    private Color _borderShade = new Color(0, 0, 0, 0.25f);

    public IdentityListContainerRenderer(SchematicGraphEditorWindow window, UnityEngine.Object obj, object parent, object target, FieldOrPropertyInfo fieldInfo, CustomFieldRendererAttribute attr, string path, List<Action> onModified) : base(window, obj, parent, target, fieldInfo, attr, path, onModified)
    {
        // Update the original field when it's children are modified 
        _onModified.Add(() =>
        {
            _field.SetValue(_parent, _containerFoldout.Collection.ToArray()); 
            AttemptPrefabRefresh();
        });

        _listAttr = (IdentityListRendererAttribute)_attr;

        _container = new VisualElement();

        Redraw();
        SignalRenderer.DrawDelayedEvents();
    }


    protected override void Redraw()
    {
        if (_listAttr == null) return;

        /// <summary>
        /// Creates a Lazy Loaded List Foldout with add/remove functionality
        /// </summary>
        /// <param name="collection">The collection to create this editor for</param>
        /// <param name="title">Title for the main foldout</param>
        /// <param name="onItemAdded">Callback when add button is clicked - should return the newly created item</param>
        /// <param name="onItemRemoved">Callback when remove button is clicked on an item</param>
        /// <param name="createItem">Factory for creating each actual Item Instance, passing an Action to call when Creation is complete</param>
        /// <param name="getItemName">Gets the name of the Item from the class that implements the List</param>
        _containerFoldout = new LazyListFoldout(collection: _value,
                                                    className: _listAttr.FoldoutTitle,
                                                    title: _listAttr.FoldoutTitle,

                                                    // List Modified 
                                                    onItemAdded: (object item) =>
                                                    {
                                                        OnModified();
                                                    },
                                                    onItemRemoved: (object item) =>
                                                    {
                                                        OnModified();
                                                    },

                                                    // Setup Item/VisualElements
                                                    createItemInstance: (Action<object> onCreationFinished) =>
                                                    {
                                                        var elementType = _value.GetElementType();
                                                        var newItem = Activator.CreateInstance(elementType);

                                                        Add(new InlineIdentifierEditor(popupRect: _containerFoldout.contentContainer.parent.Q<Toggle>().WorldBoundToScreen(),
                                                                                        identifierType: ListIdentifierType.Name,
                                                                                        getOriginalValue: () => newItem.GetType().GetFieldOrProperty(_listAttr.Identifier).GetValue(newItem),
                                                                                        onFinishedEditting: (bool success, object value) =>
                                                                                        {
                                                                                            newItem.GetType().GetFieldOrProperty(_listAttr.Identifier).SetValue(newItem, value);
                                                                                            onCreationFinished?.Invoke(newItem);
                                                                                            _containerFoldout.RenderContent(true);
                                                                                        }));
                                                    },
                                                    createItemContent: (int index, object item) =>
                                                    {
                                                        return CreateItemContent(index, item);
                                                    },
                                                    getItemName: (object item) =>
                                                    {
                                                        if(_listAttr.ItemLabel != string.Empty)
                                                            return _listAttr.ItemLabel + ": " + (string)item.GetType().GetFieldOrProperty(_listAttr.Identifier).GetValue(item);
                                                        return (string)item.GetType().GetFieldOrProperty(_listAttr.Identifier).GetValue(item);
                                                    },
                                                    itemLabelClicked: (MouseDownEvent evt, object item, Action onComplete) =>
                                                    {
                                                        if (evt.clickCount == 2 && evt.button == 0)
                                                        {
                                                            ChangeValue(item, onComplete);
                                                        }
                                                    });
                    

        _container.Add(_containerFoldout);
        Add(_container);
    }

    private VisualElement CreateItemContent(int index, object item)
    {
        var elementPath = PathManager.GetArrayPath(_path, _field, index);
        (bool drawFields, VisualElement fieldsContainer) = IODockRegistry.RenderMultipleFields(_window, _object, item, elementPath, onModified: _onModified);
        return drawFields ? fieldsContainer : null;
    }

    private void ChangeValue(object item, Action onItemEditted)
    {
        var identifierEditor = new InlineIdentifierEditor(popupRect: _containerFoldout.contentContainer.parent.Q<Toggle>().WorldBoundToScreen(),
                                                            identifierType: _listAttr.IdentifierType,
                                                            getOriginalValue: () => { return _listAttr.IdentifierType == ListIdentifierType.Name ? item.GetType().GetFieldOrProperty(_listAttr.Identifier).GetValue(item) : item; },
                                                            onFinishedEditting: (bool change, object newValue) =>
                                                            {
                                                                item.GetType().GetFieldOrProperty(_listAttr.Identifier).SetValue(item, newValue);
                                                                onItemEditted?.Invoke();
                                                                _containerFoldout.RenderContent(true);

                                                            },
                                                            popupOptions: GetPopupOptions());
    }

    private AnyCollection GetPopupOptions()
    {
        if (_listAttr.Options == null) return null;

        var optionsProp = _parent.GetType().GetFieldOrProperty(_listAttr.Options);

        if (optionsProp == null) return null;

        var optionsObj = optionsProp.GetValue(_parent);

        // Search for list options
        if (_listAttr.Options.StartsWith("./"))
        {
            optionsObj = _object.GetType().GetProperty(_listAttr.Options.Replace("./", ""))?.GetValue(_object);
        }
        if (optionsObj == null)
        {
            optionsObj = _parent.GetType().GetMethod(_listAttr.Options)?.Invoke(_parent, null);
        }

        // create popupfield
        if (optionsObj != null)
        {
            var enumerable = optionsObj as IEnumerable;
            var optionList = enumerable?.Cast<object>().ToList();

            var defaultValue = optionList.FirstOrDefault();

            if (optionList != null && optionList.Count > 0)
            {
                return optionList;
            }
        }
        return null;
    }
}