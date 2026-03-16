using System;
using System.Diagnostics;
using UnityEngine.UIElements;

/// <summary>
/// A foldout that can render its content lazily when expanded, or accept pre-built content.
/// Useful for expensive rendering operations that should only happen when the user expands the foldout.
/// </summary>
public class LazyFoldout : Foldout
{
    protected Func<VisualElement> _contentFactory;
    private bool _hasRendered = false;
    protected bool _isLazy = false;

    /// <summary>
    /// Creates a lazy foldout that will call the factory function when first expanded
    /// </summary>
    public LazyFoldout(Func<VisualElement> contentFactory = null, bool startExpanded = false)
    {
        _isLazy = contentFactory != null;
        _contentFactory = contentFactory;

        value = startExpanded;

        this.RegisterValueChangedCallback(OnFoldoutToggled);

        if (startExpanded && _isLazy)
        {
            RenderContent();
        }
    }

    protected void OnFoldoutToggled(ChangeEvent<bool> evt)
    {
        if (evt.newValue && _isLazy && !_hasRendered)
        {
            RenderContent();
        }
    }

    public void RenderContent(bool force = false)
    {
        if ((_hasRendered || _contentFactory == null) && !force)
            return;

        var content = _contentFactory.Invoke();
        if (content != null)
        {
            Clear();
            Add(content);
        }

        _hasRendered = true;
    }
}
