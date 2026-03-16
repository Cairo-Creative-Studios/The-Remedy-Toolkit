using BlueGraph.Editor;
using Remedy.Schematics;
using SchematicAssets;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// A Manager that oversees the setup of a Schematic Object. 
/// </summary>
public static class SchematicGenerator
{
    private static GameObject InstantiatingPrefab;

    /// <summary>
    /// Initiates the schematic creation workflow for a prefab asset. 
    /// This method is invoked by the <see cref="SchematicGenerationPrefabPostProcessor"/> 
    /// after a prefab is imported or modified. 
    /// </summary>
    /// <param name="prefab">The prefab to create a schematic for.</param>
    /// <param name="assetPath">The asset path where the prefab is stored in the project.</param>
    internal static bool CreateSchematicForPrefab(GameObject prefab)
    {
        var assetPath = AssetDatabase.GetAssetPath(prefab);

        //if (InstantiatingPrefab != null) return true;
        InstantiatingPrefab = prefab;

        //if (SchematicAssetManager.ObjectFolderExists(prefab)) return true;

        if (!EditorUtility.DisplayDialog(
            "Create Schematic?",
            $"Would you like to create a Schematic for '{prefab.name}'?",
            "Yes", "No"))
        {
            return false;
        }

        SetupSchematic(prefab, assetPath);
        return true;
    }

    internal static void OpenSchematicForPrefab(GameObject prefab)
    {
        var assetPath = AssetDatabase.GetAssetPath(prefab);

        if (!SchematicAssetManager.ObjectFolderExists(prefab)) return;

        else
        {
            string prefabDir = Path.GetDirectoryName(assetPath);
            string folderName = $"_${prefab.name}";
            string schematicFolder = Path.Combine(prefabDir, folderName);
            string schematicPath = Path.Combine(schematicFolder, $"{prefab.name}_Schematic.asset");

            var schematicGraph = AssetDatabase.LoadAssetAtPath<SchematicGraph>(schematicPath);
            GraphAssetHandler.OnOpenGraph(schematicGraph);
        }
    }

    /// <summary>
    /// Sets up a schematic for the given prefab by creating a schematic folder, asset, 
    /// graph, and controller, then links them together.
    /// </summary>
    /// <param name="prefab">The prefab to associate with the schematic.</param>
    /// <param name="assetPath">The path in the project where the prefab asset is located.</param>
    private static void SetupSchematic(GameObject prefab, string assetPath)
    {
        var schematicGraph = SchematicAssetManager.Create<SchematicGraph>(prefab, "", "", "SchematicGraph");

        //schematicGraph = AssetDatabase.LoadAssetAtPath<SchematicGraph>(schematicPath);

        PrefabUtility.SaveAsPrefabAsset(prefab, assetPath);
        var loadedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

        var controller = loadedPrefab.GetComponent<SchematicInstanceController>() ?? loadedPrefab.AddComponent<SchematicInstanceController>();

        controller.SchematicGraph = controller.SchematicGraph = schematicGraph;

        controller.Assign(schematicGraph);
        controller.SchematicGraph = schematicGraph;
        schematicGraph.Prefab = loadedPrefab;

/*        var schematicScope = SchematicAssetManager.Create<SchematicScope>(prefab, "", "", "SchematicScope");
        schematicScope.Graph = schematicGraph;
        schematicGraph.Scope = schematicScope;*/

        var editor = (SchematicGraphEditor)GraphAssetHandler.OnOpenGraph(schematicGraph);
        /*editor.SchematicScope = schematicScope;*/

        EditorApplication.RepaintHierarchyWindow();
    }
}