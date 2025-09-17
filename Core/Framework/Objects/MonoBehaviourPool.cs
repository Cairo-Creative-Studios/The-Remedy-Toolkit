using Remedy.Framework;
using PlasticPipe.PlasticProtocol.Messages;
using System.Collections.Generic;

public class MonoBehaviourPool : SingletonData<MonoBehaviourPool>
{
    private Dictionary<string, object> _lookup;
    public static Dictionary<string, object> Lookup = Instance._lookup ??= new();
}