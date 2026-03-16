using System;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.UIElements;

public class CustomFloatingTextInput : EditorWindow
{
    public Action<string> OnSubmit;

    private string _text = "";
    private TextField _textField;

    public static void Show(Rect position, string initialValue, Action<string> callback)
    {
        var window = CreateInstance<CustomFloatingTextInput>();
        window._text = initialValue ?? "";
        window.OnSubmit = callback;

        window.ShowAsDropDown(
            position,
            new Vector2(220, 28)
        );
    }

    private void OnDisable()
    {
        OnSubmit?.Invoke(_text);
    }

    public void CreateGUI()
    {
        // Root
        var root = rootVisualElement;
        root.style.flexDirection = FlexDirection.Row;
        root.style.paddingLeft = 4;
        root.style.paddingRight = 4;
        root.style.alignItems = Align.Center;

        // TextField
        _textField = new TextField
        {
            value = _text
        };

        _textField.style.flexGrow = 1;
        _textField.isDelayed = false;

        root.Add(_textField);

        // Keep backing value synced
        _textField.RegisterValueChangedCallback(evt =>
        {
            _text = evt.newValue;
        });

        // Key handling

        _textField.RegisterCallback<KeyDownEvent>(evt =>
        {
            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Escape)
            {
                Close();
                evt.StopPropagation();
            }
        });

        // Focus must still be delayed one frame
        root.schedule.Execute(() =>
        {
            _textField.Focus();
            _textField.SelectAll();
        });
    }
}
