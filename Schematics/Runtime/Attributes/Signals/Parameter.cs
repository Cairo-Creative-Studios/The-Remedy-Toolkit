
using System;
using System.IO;

/// <summary>
/// Decorate a Signal with this Attribute to tell the Schematic Editor that the signal needs this Parameter
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class ParameterAttribute : Attribute
{
    public string Name;
    public Type Type; 

    public ParameterAttribute(string name, Type type)
    {
        Name = name;
        Type = type;
    }
}