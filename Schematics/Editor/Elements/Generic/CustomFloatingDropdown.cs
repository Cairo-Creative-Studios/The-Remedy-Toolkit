using UnityEditor;
using UnityEngine;

public class CustomFloatingDropdown : EditorWindow
{
    public System.Action<object> OnSelect;
    public object[] Options;

    public static void Show(Rect position, object[] options, System.Action<object> callback)
    {
        var window = CreateInstance<CustomFloatingDropdown>();
        window.Options = options;
        window.OnSelect = callback;
        window.ShowAsDropDown(position, new Vector2(150, Mathf.Min(200, options.Length * 20)));
    }

    void OnGUI()
    {
        for (int i = 0; i < Options.Length; i++)
        {
            if (GUILayout.Button(Options[i]?.ToString()))
            {
                OnSelect?.Invoke(Options[i]);
                Close();
            }
        }
    }
}
