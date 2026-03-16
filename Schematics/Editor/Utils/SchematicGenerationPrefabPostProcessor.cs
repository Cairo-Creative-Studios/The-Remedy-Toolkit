using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SchematicGenerationPrefabPostProcessor : AssetPostprocessor
{
    static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        // --- CLEANUP PASS ---
        bool modified = false;

        if (modified)
        {
            EditorUtility.SetDirty(SchematicEditorData.Instance);
            AssetDatabase.SaveAssets();
        }

        // --- PREFAB PROCESSING ---
        foreach (string assetPath in importedAssets)
        {
            if (!assetPath.EndsWith(".prefab")) continue;

            if (!PrefabDragTracker.WasRecentDrag())
                continue;

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab == null) continue;

            var created = SchematicGenerator.CreateSchematicForPrefab(prefab);

            if (created)
                SchematicEditorData.AddSchematicData(prefab);
        }

        SchematicEditorData.DeleteSchematicPrefabs(deletedAssets);
    }
}

public static class PrefabDragTracker
{
    private static double _lastDragTime;

    static PrefabDragTracker()
    {
        DragAndDrop.AddDropHandler(OnProjectBrowserDrop);
    }

    public static bool WasRecentDrag()
    {
        return EditorApplication.timeSinceStartup - _lastDragTime < 1.0;
    }

    private static DragAndDropVisualMode OnProjectBrowserDrop(
        int instanceId,
        string path,
        bool perform)
    {
        if (perform)
            _lastDragTime = EditorApplication.timeSinceStartup;

        return DragAndDropVisualMode.None;
    }
}