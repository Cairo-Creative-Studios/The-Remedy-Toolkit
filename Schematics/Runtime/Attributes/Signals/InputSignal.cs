
using System;
using System.IO;

/// <summary>
/// Decorating a Signal with this attribute tells the Schematic System that it is used as an Input by the MonoBehaviour
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class InputSignalAttribute : Attribute
{
    public InputSignalAttribute()
    {
    }
}