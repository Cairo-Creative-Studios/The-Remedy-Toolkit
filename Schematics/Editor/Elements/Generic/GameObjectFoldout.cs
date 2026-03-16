using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Linq;

public class GameObjectFoldout : VisualElement
{
    public PersistentFoldout Foldout { get; private set; }
    public event Action<string> OnRenameRequested; // callback for renaming

    public GameObjectFoldout(GameObject gameObject, string GUID, string id, string labelText)
    {

        // Layout container
        var container = new VisualElement();
        container.style.flexDirection = FlexDirection.Row;
        container.style.alignItems = Align.FlexStart;

        // Icon
        Texture2D icon = EditorGUIUtility.IconContent("GameObject Icon").image as Texture2D;
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
        

        // Foldout
        Foldout = new PersistentFoldout(GUID, id)
        {
            text = labelText,
            value = true,
            style =
            {
                flexDirection = FlexDirection.Column,
                flexGrow = 1,
                minHeight = 0,
                unityFontStyleAndWeight = FontStyle.Bold
            }
        };
        Foldout.style.marginTop = 0;

        var contentContainer = Foldout.Q<VisualElement>("unity-content");
        if (contentContainer != null)
        {
            contentContainer.style.marginLeft = 0;
            contentContainer.style.paddingLeft = 0;
        }

        var indent = new VisualElement()
        {
            style =
            {
                width = 8
            }
        };

        // Add elements
        container.Add(indent);
        container.Add(Foldout);

        var toggle = container.Q<Toggle>();
        var header = toggle.Children().ElementAt(0);

        var children = header.Children();

        var handle = children.ElementAt(0);
        var label = children.ElementAt(1);

        header.Clear();
        handle.style.marginRight = 0;

        header.Add(handle);
        header.Add(iconImage);
        header.Add(label);

        this.Add(container);

        // Enable inline renaming
        EnableInlineRename(Foldout);
    }

    public void Add(GameObjectFoldout goFoldout)
    {
        Foldout.Add(goFoldout);
    }

    private void EnableInlineRename(Foldout foldout)
    {
        var label = foldout.Q<Label>(); 

        if (label == null)
            return;

        label.RegisterCallback<MouseDownEvent>(evt =>
        {
            evt.StopImmediatePropagation();

            if (evt.clickCount == 2 && evt.button == 0) // Double left-click
            {
                BeginRename(label);
                evt.StopImmediatePropagation();
            }
        });
    }

    private void BeginRename(Label label)
    {
        string oldName = label.text;

        var textField = new TextField
        {
            value = oldName
        };
        textField.style.flexGrow = 1;
        textField.SelectAll();

        var parent = label.parent;
        int index = parent.IndexOf(label);
        parent.Insert(index, textField);
        label.RemoveFromHierarchy();

        // Confirm rename on enter or blur
        void EndRename()
        {
            string newName = textField.value.Trim();
            if (!string.IsNullOrEmpty(newName) && newName != oldName)
            {
                OnRenameRequested?.Invoke(newName); 
            }

            label.text = newName;
            parent.Insert(index, label);
            textField.RemoveFromHierarchy();
        }

        textField.RegisterCallback<FocusOutEvent>(_ => EndRename());
        textField.RegisterCallback<KeyDownEvent>(evt =>
        {
            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
            {
                EndRename();
                evt.StopPropagation();
            }
            else if (evt.keyCode == KeyCode.Escape)
            {
                // Cancel
                parent.Insert(index, label);
                textField.RemoveFromHierarchy();
                evt.StopPropagation();
            }
        });

        textField.Focus();
    }
}
