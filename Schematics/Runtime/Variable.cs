using Remedy.Framework;
using Remedy.Schematics;
using System;

public class SignalBase
{
    public string _name;
    protected SignalData _data;
    protected bool _failed = false;

    public void TryGetData(UnityEngine.Object component)
    {
        if (_failed) return;

        var controller = component.GetCachedComponentInParents<SchematicInstanceController>();
        _data = controller.SchematicGraph.SignalCache[_name];
        if (_data == null)
            _failed = true;
    }

    public void Unsubscribe(UnityEngine.Object component)
    {
        TryGetData(component);

        if (_failed) return;

        _data.Unsubscribe(component);
    }
}
public class Signal : SignalBase
{
    public Signal(string name)
    {
        _name = name;
    }
    public void Subscribe(UnityEngine.Object component, Action method)
    {
        TryGetData(component);

        if (_failed) return;

        _data.Subscribe(component, method);
    }
    public void Invoke(UnityEngine.Object component)
    {
        TryGetData(component);

        if (_failed) return;

        _data.Invoke();
    }
}
public class Signal<T> : SignalBase
{
    public Signal(string name)
    {
        _name = name;
    }
    public void Subscribe(UnityEngine.Object component, Action<T> method)
    {
        TryGetData(component);

        if (_failed) return;

        _data.Subscribe(component, method);
    }
    public void Invoke(UnityEngine.Object component, T val)
    {
        TryGetData(component);

        if (_failed) return;

        _data.Invoke(val);
    }
}
public class Signal<T1, T2> : SignalBase
{
    public Signal(string name)
    {
        _name = name;
    }
    public void Subscribe(UnityEngine.Object component, Action<T1, T2> method)
    {
        TryGetData(component);

        if (_failed) return;

        _data.Subscribe(component, method);
    }
    public void Invoke(UnityEngine.Object component, T1 val1, T2 val2)
    {
        TryGetData(component);

        if (_failed) return;

        _data.Invoke(val1, val2);
    }
}
public class Signal<T1, T2, T3> : SignalBase
{
    public Signal(string name)
    {
        _name = name;
    }
    public void Subscribe(UnityEngine.Object component, Action<T1, T2, T3> method)
    {
        TryGetData(component);

        if (_failed) return;

        _data.Subscribe(component, method);
    }
    public void Invoke(UnityEngine.Object component, T1 val1, T2 val2, T3 val3)
    {
        TryGetData(component);

        if (_failed) return;

        _data.Invoke(val1, val2, val3);
    }
}
public class Signal<T1, T2, T3, T4> : SignalBase
{
    public Signal(string name)
    {
        _name = name;
    }
    public void Subscribe(UnityEngine.Object component, Action<T1, T2, T3, T4> method)
    {
        TryGetData(component);

        if (_failed) return;

        _data.Subscribe(component, method);
    }
    public void Invoke(UnityEngine.Object component, T1 val1, T2 val2, T3 val3, T4 val4)
    {
        TryGetData(component);

        if (_failed) return;

        _data.Invoke(val1, val2, val3, val4);
    }
}
public class Signal<T1, T2, T3, T4, T5> : SignalBase
{
    public Signal(string name)
    {
        _name = name;
    }
    public void Subscribe(UnityEngine.Object component, Action<T1, T2, T3, T4, T5> method)
    {
        TryGetData(component);

        if (_failed) return;

        _data.Subscribe(component, method);
    }
    public void Invoke(UnityEngine.Object component, T1 val1, T2 val2, T3 val3, T4 val4, T5 val5)
    {
        TryGetData(component);

        if (_failed) return;

        _data.Invoke(val1, val2, val3, val4, val5);
    }
}
public class Signal<T1, T2, T3, T4, T5, T6> : SignalBase
{
    public Signal(string name)
    {
        _name = name;
    }
    public void Subscribe(UnityEngine.Object component, Action<T1, T2, T3, T4, T5, T6> method)
    {
        TryGetData(component);

        if (_failed) return;

        _data.Subscribe(component, method);
    }
    public void Invoke(UnityEngine.Object component, T1 val1, T2 val2, T3 val3, T4 val4, T5 val5, T6 val6)
    {
        TryGetData(component);

        if (_failed) return;

        _data.Invoke(val1, val2, val3, val4, val5, val6);
    }
}
public class Signal<T1, T2, T3, T4, T5, T6, T7> : SignalBase
{
    public Signal(string name)
    {
        _name = name;
    }
    public void Subscribe(UnityEngine.Object component, Action<T1, T2, T3, T4, T5, T6, T7> method)
    {
        TryGetData(component);

        if (_failed) return;

        _data.Subscribe(component, method);
    }
    public void Invoke(UnityEngine.Object component, T1 val1, T2 val2, T3 val3, T4 val4, T5 val5, T6 val6, T7 val7)
    {
        TryGetData(component);

        if (_failed) return;

        _data.Invoke(val1, val2, val3, val4, val5, val6, val7);
    }
}
public class Signal<T1, T2, T3, T4, T5, T6, T7, T8> : SignalBase
{
    public Signal(string name)
    {
        _name = name;
    }
    public void Subscribe(UnityEngine.Object component, Action<T1, T2, T3, T4, T5, T6, T7, T8> method)
    {
        TryGetData(component);

        if (_failed) return;

        _data.Subscribe(component, method);
    }
    public void Invoke(UnityEngine.Object component, T1 val1, T2 val2, T3 val3, T4 val4, T5 val5, T6 val6, T7 val7, T8 val8)
    {
        TryGetData(component);

        if (_failed) return;

        _data.Invoke(val1, val2, val3, val4, val5, val6, val7, val8);
    }
}
public class Signal<T1, T2, T3, T4, T5, T6, T7, T8, T9> : SignalBase
{
    public Signal(string name)
    {
        _name = name;
    }
    public void Subscribe(UnityEngine.Object component, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> method)
    {
        TryGetData(component);

        if (_failed) return;

        _data.Subscribe(component, method);
    }
    public void Invoke(UnityEngine.Object component, T1 val1, T2 val2, T3 val3, T4 val4, T5 val5, T6 val6, T7 val7, T8 val8, T9 val9)
    {
        TryGetData(component);

        if (_failed) return;

        _data.Invoke(val1, val2, val3, val4, val5, val6, val7, val8, val9);
    }
}
public class Signal<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : SignalBase
{
    public Signal(string name)
    {
        _name = name;
    }
    public void Subscribe(UnityEngine.Object component, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> method)
    {
        TryGetData(component);

        if (_failed) return;

        _data.Subscribe(component, method);
    }
    public void Invoke(UnityEngine.Object component, T1 val1, T2 val2, T3 val3, T4 val4, T5 val5, T6 val6, T7 val7, T8 val8, T9 val9, T10 val10)
    {
        TryGetData(component);

        if (_failed) return;

        _data.Invoke(val1, val2, val3, val4, val5, val6, val7, val8, val9, val10);
    }
}