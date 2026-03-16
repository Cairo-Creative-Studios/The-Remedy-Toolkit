using Remedy.Schematics;
using UnityEngine;

/// <summary>
/// Extension of MonoBehavior that provides easy functions to interop with the Schematics System
/// </summary>
public class SchematicMonoBehavior : MonoBehaviour
{
    private SchematicInstanceController _schematicController;
    private int _uid;
    private SchematicGraph _mainGraph;
}