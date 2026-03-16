using Remedy.Framework;
using Remedy.Schematics;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GlobalSchematicsManager : Singleton<GlobalSchematicsManager>
{
    private Dictionary<SchematicGraph, List<SchematicInstanceController>> SchematicInstances = new();
    private List<SchematicInstanceController> _activeInstances = new();
    [SerializeField]
    private List<SingletonGraph> _runningSingletons = new();

    public static event UnityAction OnUpdate;

    [RuntimeInitializeOnLoadMethod]
    public static void Init()
    {
        // Adds logic to the Object Manager's Prefab Instantiation to determine whether to add the Instantiated Object as a Schematic Instance.
        ObjectManager.OnPrefabInstantiated += (GameObject prefab, GameObject instance) =>
        {
            if (GlobalSchematicManagerData.SchematicPrefabs.ContainsKey(prefab))
            {
                var schemInst = instance.GetComponent<SchematicInstanceController>();
                Instance.SchematicInstances[GlobalSchematicManagerData.SchematicPrefabs[prefab]].Add(schemInst);
                Instance.SetupSchematicInstance(schemInst);
            }
            if(GlobalSchematicManagerData.PrefabIOBases.ContainsKey(prefab))
            {
                foreach (var id in GlobalSchematicManagerData.PrefabIOBases[prefab])
                {
                    //ScriptableSignal.IOBase.OnIOBaseInstantiated(id, ScriptableEventsData.IOBases[id].IOEvents);
                }
            }
        };

        Instance.InitializeSingletons();
    }

    // Okay, now for the good shit
    // ... Except, returning to this months later I have no idea what I was doing.
    // Must have been pretty cool, though, whatever it was. 
    private void SetupSchematicInstance(SchematicInstanceController instance)
    {

    }

    private void InitializeSingletons()
    {
        foreach(var graph in GlobalSchematicManagerData.SingletonGraphs)
        {
            _runningSingletons.Add(graph);
            graph.TriggerEvent<OnStart>(null);

            OnUpdate += () => graph.TriggerEvent<OnUpdate>(null);
        }
    }

    private void Update()
    {
        OnUpdate?.Invoke();
    }
}
