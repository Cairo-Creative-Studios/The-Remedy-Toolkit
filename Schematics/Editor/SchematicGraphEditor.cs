using BlueGraph.Editor;
using UnityEditor;
using System.IO;
using UnityEngine;

namespace Remedy.Schematics
{
    [CustomEditor(typeof(SchematicGraph), true)]
    public class SchematicGraphEditor : GraphEditor
    {
        SchematicGraphEditorWindow editorWindow;
        /*public SchematicScope SchematicScope;*/

        public override GraphEditorWindow CreateEditorWindow()
        {
            SchematicGraph graph = target as SchematicGraph;

            editorWindow = CreateInstance<SchematicGraphEditorWindow>();
            /*            if(SchematicScope == null) SchematicScope = EnsureSchematicScopeExists(graph);
                        editorWindow.SchematicScope = SchematicScope;*/
            editorWindow.SchematicGraph = graph;
            editorWindow.Show();

            editorWindow.Load(graph);

            return editorWindow;
        }
        /*
         * private static SchematicScope EnsureSchematicScopeExists(SchematicGraph graph)
        {
            SchematicScope schematic;
            string graphPath = AssetDatabase.GetAssetPath(graph);
            string graphFolder = Path.GetDirectoryName(graphPath);

            string scopeAssetPath = Path.Combine(graphFolder, "SchematicScope.asset").Replace("\\", "/");
            SchematicScope existingScope = AssetDatabase.LoadAssetAtPath<SchematicScope>(scopeAssetPath);

            if (existingScope == null)
            {
                SchematicScope newScope = ScriptableObject.CreateInstance<SchematicScope>();
                AssetDatabase.CreateAsset(newScope, scopeAssetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                schematic = newScope;
            }
            else
                schematic = existingScope;

            schematic.Graph = graph;

            return schematic;
        }
        */

    }
}
