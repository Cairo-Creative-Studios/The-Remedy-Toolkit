
using System;
using System.IO;

[AttributeUsage(AttributeTargets.Field)]
public class CustomFieldRendererAttribute : Attribute
{
    public CustomFieldRendererAttribute()
    {
    }
}