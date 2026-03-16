using System.Linq;
using System;
using UnityEngine.UIElements;
using UnityEngine;

/// <summary>
/// A container that manages a list of items that includes add/remove functionality, and enables custom rendering of the items in the list where it's implemented.
/// </summary>
public class LazyListFoldout : LazyFoldout
{
    private AnyCollection _collection;
    public AnyCollection Collection => _collection;

    private string _listClassName; 
    private string _title;

    // Callbacks >>
    private Action<Action<object>> _createItem;
    private Func<int, object, VisualElement> _createItemContent;
    private Func<object, string> _getItemName;

    private Action<object> _itemCreationFinished;

    private Action<object> _onItemAdded;
    private Action<object> _onItemRemoved;

    private Action<MouseDownEvent, object, Action> _itemLabelClicked;

    private Func<object, Texture2D> _getItemIcon;

    /// <summary>
    /// for assigning classes to list containers. X Depth toggles once after a list of items is drawn, where Y Depth is toggled for each drawn item.
    /// </summary>
    private static int _listItem = 0;
    private bool _modifiable = true;

    /// <summary>
    /// Creates a Lazy Loaded List Foldout with add/remove functionality
    /// </summary>
    /// <param name="collection">The collection to create this editor for</param>
    /// <param name="title">Title for the main foldout</param>
    /// <param name="onItemAdded">Callback when add button is clicked - should return the newly created item</param>
    /// <param name="onItemRemoved">Callback when remove button is clicked on an item</param>
    /// <param name="createItemInstance">Factory for creating each actual Item Instance, passing an Action to call when Creation is complete</param>
    /// <param name="getItemName">Gets the name of the Item from the class that implements the List</param>
    public LazyListFoldout
    (
        object collection,
        string className,
        string title,

        Action<object> onItemAdded = null,
        Action<object> onItemRemoved = null,

        Action<Action<object>> createItemInstance = null,
        Func<int, object, VisualElement> createItemContent = null,

        Func<object, string> getItemName = null,
        Action<MouseDownEvent, object, Action> itemLabelClicked = null,
        Func<Texture2D> getListIcon = null,
        Func<object, Texture2D> getItemIcon = null,
        bool contentModifiable = true,
        bool startExpanded = false
    )
    {
        _collection = new AnyCollection(collection);
        _listClassName = className;
        _title = title;

        // callbacks
        _createItem = createItemInstance;
        _createItemContent = createItemContent;
        _getItemName = getItemName;

        _onItemAdded = onItemAdded;
        _onItemAdded += (object val) => RenderContent(true);
        _onItemRemoved = onItemRemoved;
        _onItemRemoved += (object val) => RenderContent(true);

        _itemLabelClicked = itemLabelClicked;

        _getItemIcon = getItemIcon;
        _modifiable = contentModifiable;

        // Set up the Finish Callback (send on createItem to be invoked externally whenever an item is ready to be added.
        _itemCreationFinished = (object value) =>
        {
            Collection.Add(value);
            _createItemContent?.Invoke(Collection.ToList().IndexOf(value), value);
            _onItemAdded?.Invoke(value);
        };

        AddToClassList("list-container");
        AddToClassList("list-container-" + _listClassName);
        AddToClassList("list-" + _title);

        // Set up the Foldout Factory
        _contentFactory = () =>
        {
            var content = new VisualElement();

            var customizationBar = new VisualElement()
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.FlexEnd,
                    justifyContent = Justify.FlexEnd,
                }
            };

            customizationBar.AddToClassList("list-customizationbar");
            customizationBar.AddToClassList("list-customizationbar-" + _title);
            customizationBar.AddToClassList("list-customizationbar-" + _listClassName);

            content.Add(customizationBar);
            content.Add(CreateListElements());

            return content;
        };
        _isLazy = true;

        contentContainer.AddToClassList("list-content");
        contentContainer.AddToClassList("list-content-" + _listClassName);

        var parent = contentContainer.parent;

        var toggle = parent.Q<Toggle>();
        toggle.AddToClassList("list-toggle");
        toggle.AddToClassList("list-toggle-" + className);

        var listHeader = toggle.Children().ElementAt(0);
        if (listHeader != null)
        {
            listHeader.style.flexGrow = 0;

            var label = new Label()
            {
                text = (getListIcon == null ? "... " : "") + title,
            };

            if(getListIcon != null)
            {
                // Icon
                Texture2D icon = getListIcon?.Invoke();

                // Icon image
                var iconImage = new Image
                {
                    image = icon,
                    scaleMode = ScaleMode.ScaleToFit,
                    style =
                    {
                        minWidth = 16,
                        minHeight = 16,
                        maxWidth = 16,
                        maxHeight = 16,
                        marginRight = 4
                    }
                };

                listHeader.Add(iconImage);
            }

            listHeader.Add(label);

            listHeader.AddToClassList("list-header");
            label.AddToClassList("list-label");
            label.AddToClassList("list-label-" + title.Replace(" ", "-"));
        }

        listHeader.AddToClassList("list-toggle-container");
        listHeader.Children().ElementAt(0).AddToClassList("list-toggle");

        if(contentModifiable)
        {
            var addButton = CreateButton("add", "+", () =>
            {
                _createItem?.Invoke(_itemCreationFinished);
                RenderContent(true);
            });
            listHeader.Add(addButton);
        }

        this.RegisterValueChangedCallback(OnFoldoutToggled);

        if (startExpanded)
        {
            RenderContent(true);
            value = true;
        }
    } 

    private VisualElement CreateListElements()
    {
        var listContainer = new VisualElement();

        for (int i = 0; i < Collection.Count(); i++)
        {
            int index = i;
            var item = Collection[index];
            
            var container = new VisualElement();

            var itemContainer = new VisualElement()
            {
                style =
                {
                    flexDirection = FlexDirection.Column
                }
            };
            itemContainer.AddToClassList("list-item-" + _listItem);
            itemContainer.AddToClassList("list-item-" + _listClassName);

            itemContainer?.contentContainer.AddToClassList("list-item-content");

            var itemContent = _createItemContent?.Invoke(index, item);

            var itemHeader = new VisualElement();
            itemHeader.AddToClassList("list-item-header");

            var label = new Label()
            {
                text = _getItemName?.Invoke(item),
                style =
                {
                    width = 200
                }
            };
            label.RegisterCallback((MouseDownEvent evt) => _itemLabelClicked?.Invoke(evt, item, () => RenderContent()));
            label.AddToClassList("list-item-label");

            itemHeader.Add(label);

            //itemContainer.Add(itemHeader);
            itemContainer.Add(itemContent);


            var removeButton = CreateButton("remove",
                                                "-",
                                                () =>
                                                {
                                                    _onItemRemoved(item);
                                                    Collection.RemoveAt(index);
                                                    RenderContent(true);
                                                });

            container.Add(itemContainer);

            listContainer.Add(container);
            itemHeader.Add(removeButton);

            _listItem = _listItem == 0 ? 1 : 0;
        }

        return listContainer;
    }

    // TODO: implement this for Add: _createItem.Invoke(_itemCreationFinished)
    private Button CreateButton(string className, string label, Action action)
    {
        var button = new Button(() =>
        {
            action?.Invoke();
        })
        {
            text = label
        };

        button.AddToClassList("list-button-" + _listClassName + "-" + className);
        button.AddToClassList("list-button-" + _listClassName + "-" + className);

        button.AddToClassList("list-button-" + className);
        button.AddToClassList("list-button" + className);

        button.RegisterCallback<MouseEnterEvent>(evt => button.AddToClassList("list-button-" + className + "-hover"));
        button.RegisterCallback<MouseLeaveEvent>(evt => button.RemoveFromClassList("list-button-" + className + "-hover"));

        return button;
    }
}
