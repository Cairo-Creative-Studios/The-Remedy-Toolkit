using UnityEngine.UIElements;
using UnityEngine;

namespace UnityEditor
{
    public static class EditorExtensions
    {
        public static Rect WorldBoundToScreen(this VisualElement element)
        {
            Rect world = element.worldBound;

            // Convert panel coordinates to screen coordinates
            Vector2 screenPos =
                GUIUtility.GUIToScreenPoint(world.position);

            return new Rect(screenPos, world.size);
        }
    }
}