using Remedy.Framework;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// A Visual Element class with a resizable Sidebar Content and Main Content 
/// </summary>
public class SidebarAndContent : VisualElement
{
    public VisualElement SidebarContainer = new();
    public VisualElement ContentContainer = new();
    private VisualElement _resizer;
    private ScrollView _scrollView;

    public SidebarAndContent(string name)
    {
        style.flexDirection = FlexDirection.Row;
        style.flexGrow = 1;

        // Sidebar Container 
        var _sidebar = new VisualElement();

        _sidebar.AddToClassList(name + "-sidebar");

        _scrollView = new ScrollView(ScrollViewMode.Vertical);
        _scrollView.style.minHeight = Length.Percent(100);

        _sidebar.Add(_scrollView);

        _resizer = new VisualElement
        {
            name = "SidebarResizer",
            style =
            {
                width = 4,
                cursor = new StyleCursor(UnityDefaultCursor.DefaultCursor(UnityDefaultCursor.CursorType.ResizeHorizontal)),
            }
        };
        _resizer.AddToClassList(name + "-divider");

        _scrollView.Add(SidebarContainer);

        Add(_sidebar);
        Add(_resizer);
        Add(ContentContainer);

        // Callbacks
        float minSidebarWidth = 150f;
        float maxSidebarWidth = 1200f;
        bool isDragging = false;

        _resizer.RegisterCallback<MouseDownEvent>(evt =>
        {
            isDragging = true;
            _resizer.CaptureMouse();
            evt.StopPropagation();
        });

        _resizer.RegisterCallback<MouseMoveEvent>(evt =>
        {
            if (!isDragging) return;

            var localMouse = evt.localMousePosition;
            float newWidth = _sidebar.resolvedStyle.width + evt.mouseDelta.x;
            newWidth = Mathf.Clamp(newWidth, minSidebarWidth, maxSidebarWidth);
            _sidebar.style.width = newWidth;
            evt.StopPropagation();
        });

        _resizer.RegisterCallback<MouseUpEvent>(evt =>
        {
            if (!isDragging) return;
            isDragging = false;
            _resizer.ReleaseMouse();
            evt.StopPropagation();
        });

        _resizer.RegisterCallback<MouseEnterEvent>(_ => _resizer.AddToClassList(name + "-divider-hover"));
        _resizer.RegisterCallback<MouseLeaveEvent>(_ => _resizer.RemoveFromClassList(name + "-divider-hover"));

    }
}
