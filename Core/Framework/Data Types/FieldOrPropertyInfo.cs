using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class FieldOrPropertyInfo
{
    private FieldInfo field = null;
    private PropertyInfo property = null;

    public FieldOrPropertyInfo(FieldInfo field) => this.field = field;
    public FieldOrPropertyInfo(PropertyInfo property) => this.property = property;

    public static implicit operator FieldOrPropertyInfo(FieldInfo field) => new FieldOrPropertyInfo(field);
    public static implicit operator FieldOrPropertyInfo(PropertyInfo property) => new FieldOrPropertyInfo(property);

    public Type MemberType => field != null ? field.FieldType : property.PropertyType;
    public IEnumerable<CustomAttributeData> Attributes => field != null ? field.CustomAttributes : property.CustomAttributes;
    public string Name => field != null ? field.Name : property.Name;

    public object GetValue(object obj)
    {
        try
        {
            return field != null ? field.GetValue(obj) : property.GetValue(obj);
        }
        catch
        {
            return null;
        }
    }

    public void SetValue(object obj, object value)
    {
        if (field != null) field.SetValue(obj, value);
        else property.SetValue(obj, value);
    }
    public IEnumerable<Attribute> GetCustomAttributes()
    {
        return field != null ? field.GetCustomAttributes() : property.GetCustomAttributes();
    }
}