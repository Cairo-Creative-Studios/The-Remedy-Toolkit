using Remedy.Framework;
using Remedy.Schematics;
using System;
using System.Collections.Generic;

namespace Remedy.Schematics
{
    public class GraphRegistry : SingletonScriptableObject<GraphRegistry>
    {
        private IReadOnlyList<SchematicGraph> _graphs = new List<SchematicGraph>().AsReadOnly();
        public static IReadOnlyList<SchematicGraph> Graphs => Asset._graphs;

        public static Action<int> OnGraphRemoved;

        public static void AddGraph(SchematicGraph graph)
        {

        }

        public static void RemoveGraph(SchematicGraph graph)
        {

        }
    }
}