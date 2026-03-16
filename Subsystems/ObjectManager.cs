using Remedy.Framework;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using Cysharp.Threading.Tasks;
using System.Reflection;

public class ObjectManager : Singleton<ObjectManager>
{
    [Tooltip("Boolean Events that determine whether the physics system should be paused. This affects all physics objects, of course.")]
    public Action<bool> PhysicsPause;

    [Tooltip("Time between last instantiation and the next is subtracted from this to determine the amount of time to wait before detroying an instance.")]
    public int MaxTimeToDestroy = 1000;

    public UnityEvent<GameObject> OnInstantiate;
    public UnityEvent<GameObject> OnDestroy;

    private static Dictionary<GameObject, (Pool pool, List<GameObject> activeInstances)> _pools = new();

    /// <summary>
    /// Ivoked whenever a Prefab is Instantiated by the Pooling System. Add functionality here for extra control over Object Spawning.
    /// </summary>
    public static Action<GameObject, GameObject> OnPrefabInstantiated;

    private void OnEnable()
    {
        PhysicsPause += SetPhysicsState;
    }

    private void SetPhysicsState(bool isPaused)
    {
        if (isPaused)
            Physics.simulationMode = SimulationMode.Script;
        else
            Physics.simulationMode = SimulationMode.FixedUpdate;
    }

    /// <summary>
    /// Instantiate a Prefab using the Pooling system.
    /// </summary>
    /// <param name="prefab"></param>
    /// <returns></returns>
    public static GameObject Instantiate(GameObject prefab, int timeToDestroy = -1)
    {
        if (!_pools.ContainsKey(prefab))
            _pools.Add(prefab, (new(prefab), new()));

        var inst = _pools[prefab].pool.GetInstance(timeToDestroy);

        Instance.OnInstantiate?.Invoke(inst);
        OnPrefabInstantiated?.Invoke(prefab, inst);

        return inst;
    }

    public static GameObject Instantiate(GameObject prefab, Vector3 position, Quaternion rotation, int timeToDestroy = -1)
    {
        var inst = Instantiate(prefab, timeToDestroy);
        inst.transform.position = position;
        inst.transform.rotation = rotation;
        return inst;
    }

    /// <summary>
    /// Destroy an Instance of a prefab using the Pooling System
    /// </summary>
    /// <param name="instance"></param>
    public static void Destroy(GameObject instance)
    {
        Instance.OnDestroy?.Invoke(instance);

        foreach(var kvp in _pools)
        {
            if (kvp.Value.activeInstances.Contains(instance))
            {
                kvp.Value.pool.Recycle(instance);
                return;
            }
        }
    }

    public static void Destroy(GameObject instance, float time)
    {
        _ = Instance.DestroyInstanceDelayed(instance, time);
    }

    private async UniTaskVoid DestroyInstanceDelayed(GameObject instance, float time)
    {
        await UniTask.Delay((int)(time * 1000));
        Destroy(instance);
    }

    [Serializable]
    public class Pool
    {
        public GameObject Prefab;
        public int CustomTimeToDestroy = -1;

        private List<GameObject> _active => ObjectManager._pools[Prefab].activeInstances;
        private Queue<GameObject> _inactive = new();
        private Dictionary<GameObject, Dictionary<MonoBehaviour, (MethodInfo StartMethod, MethodInfo DestroyMethod)>> _cachedMethods = new();

        private int _deleteTime;

        public Pool(GameObject prefab)
        {
            Prefab = prefab;
        }

        /// <summary>
        /// Gets an Instance from the <see cref="_inactive"/> Queue or creates one if no inactive instances exist and adds it to the <see cref="_active"/> List.
        /// </summary>
        /// <returns></returns>
        public GameObject GetInstance(int timeToDestroy = -1)
        {
            GameObject inst;

            if (_inactive.Count > 0)
                inst = _inactive.Dequeue();
            else
                inst = GameObject.Instantiate(Prefab);

            // Cache lifecycle methods and call Start for monobehaviours attached to recycled instances
            if (_cachedMethods.ContainsKey(inst))
            {
                foreach (var cache in _cachedMethods[inst])
                {
                    if (cache.Key.didStart)
                    {
                        cache.Value.StartMethod?.Invoke(cache.Key, null);
                    }
                }
            }
            else
            {
                _cachedMethods.Add(inst, new());

                var monoBehaviours = inst.GetComponents<MonoBehaviour>();
                foreach (var behaviour in monoBehaviours)
                {
                    var type = behaviour.GetType();
                    var startMethod = type.GetMethod("Start");
                    var destroyMethod = type.GetMethod("Destroy");

                    if (behaviour.didStart)
                    {
                        startMethod?.Invoke(behaviour, null);
                        _cachedMethods[inst].Add(behaviour, (startMethod, destroyMethod));
                    }
                }
            }

            _active.Add(inst);

            _deleteTime = (int)Time.realtimeSinceStartup;

            inst.SetActive(true);
            return inst;
        }

        /// <summary>
        /// "destroys" the Instance, moving it from <see cref="_active"/> to <see cref="_inactive"/>.
        /// </summary>
        /// <param name="instance"></param>
        public void Recycle(GameObject instance)
        {
            if (_active.Remove(instance))
            {
                _inactive.Enqueue(instance);
                _ = ScheduleDeletion();
                instance.transform.parent = null;
                instance.SetActive(false);

                // Lifecycle methods should have been cached when creating the instance originally,
                // we call the destruction lifecycle even when recycling an instance
                if (_cachedMethods.ContainsKey(instance))
                {
                    foreach (var cache in _cachedMethods[instance])
                    {
                        if (cache.Key.didStart)
                        {
                            cache.Value.DestroyMethod?.Invoke(cache.Key, null);
                        }
                    }
                }
            }
        }

        public async UniTaskVoid ScheduleDeletion()
        {
            int timeBetween = ((int)Time.realtimeSinceStartup) - _deleteTime;
            await UniTask.Delay(Mathf.Max(0, Instance.MaxTimeToDestroy - timeBetween));

            if (_inactive.Count > 0)
            {
                GameObject.Destroy(_inactive.Dequeue());

                if (_inactive.Count == 0 && _active.Count == 0)
                    _pools.Remove(Prefab);

                _deleteTime = (int)Time.realtimeSinceStartup * 1000;

                _ = ScheduleDeletion();
            }
        }
    }
}
