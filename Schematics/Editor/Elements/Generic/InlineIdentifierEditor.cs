using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class InlineIdentifierEditor : VisualElement
{

    public InlineIdentifierEditor(Rect popupRect, 
                                    ListIdentifierType identifierType,
                                    Func<object> getOriginalValue,
                                    Action<bool, object> onFinishedEditting,
                                    AnyCollection popupOptions = null)
    {
        var originalValue = getOriginalValue?.Invoke();

        // Initialize TextField Type
        if(identifierType == ListIdentifierType.Name)
        {
            CustomFloatingTextInput.Show(popupRect, originalValue?.ToString() ?? "Default text", result =>
            {
                onFinishedEditting?.Invoke(true, result);
            });
        }
        else
        {
            CustomFloatingDropdown.Show(popupRect, popupOptions.ToArray().Cast<object>().ToArray(), selected =>
            {
                onFinishedEditting?.Invoke(true, selected);
            });
        }

        AddToClassList("inline-identifier-editor");
    }
}
