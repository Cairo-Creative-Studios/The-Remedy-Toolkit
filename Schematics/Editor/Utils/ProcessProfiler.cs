using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class ProcessProfiler
{
    private Dictionary<string, Stopwatch> _processes = new();

    public void StartTracking(string processName)
    {
        if (!SchematicEditorData.Profile)
            return;

        _processes[processName] = new Stopwatch();
        _processes[processName].Start();
    }

    public void StopTracking(string processName)
    {
        if (!SchematicEditorData.Profile)
            return;

        if(_processes.TryGetValue(processName, out Stopwatch stopwatch))
        {
            stopwatch.Stop();
            UnityEngine.Debug.Log($"[PROFILE] {processName}: {stopwatch.ElapsedMilliseconds}ms");
        }
    }
}