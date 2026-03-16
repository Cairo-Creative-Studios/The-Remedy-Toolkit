using System;

/// <summary>
/// Describes how the Field given this Attribute is documented in the Schematic Editor <br></br>
///  <br></br>  
/// Interpolation:  <br></br>
/// {0}, {1} for interpolating Input Port Documentation (recieved from the other Node's Output Value or <see cref="SchematicDocumentationAttribute"/>)  <br></br>
/// {var.*name*} for interpolating graph variables by name  <br></br>
/// {prop.*name} for interpolating Component Properties (only works for nodes belonging to a Component/MonoBehavior
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class SchematicDocumentationAttribute : CustomFieldRendererAttribute
{
    public SchematicDocumentationAttribute(string text)
    {

    }
}