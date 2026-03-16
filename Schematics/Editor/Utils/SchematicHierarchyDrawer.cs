using BlueGraph.Editor;
using Remedy.Schematics;
using SchematicAssets;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class SchematicHierarchyDrawer
{
    static SchematicHierarchyDrawer()
    {
        EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
    }

    static void OnHierarchyGUI(int instanceID, Rect selectionRect)
    {
        GameObject go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
        if (go == null) return;

        var controller = go.GetComponent<SchematicInstanceController>();
        if (controller == null/* || controller?.SchematicGraphs.Length > 0*/) return;

        var prefab = PrefabUtility.GetCorrespondingObjectFromSource(go);

        if (controller.SchematicGraph == null)
            controller.SchematicGraph = SchematicAssetManager.Load<SchematicGraph>(prefab,"","");
        if (controller.SchematicGraph == null)
            return;

        controller.SchematicGraph.Prefab = PrefabUtility.GetCorrespondingObjectFromSource(go);

        Rect buttonRect = new Rect(selectionRect.xMax - 20, selectionRect.y, 18, selectionRect.height);
        if (GUI.Button(buttonRect, "§", EditorStyles.miniButton))
        {
            GraphAssetHandler.OnOpenGraph(controller.SchematicGraph);
        }
    }
}
