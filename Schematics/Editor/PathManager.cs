using System.Reflection;

public static class PathManager
{
    public static string GetArrayPath(string originalPath, FieldOrPropertyInfo arrayField, int index)
    {
        string elementPath = originalPath;
        
        if (string.IsNullOrEmpty(elementPath))
            elementPath = $"{arrayField.Name}.Array.data[{index}]";
        else
            elementPath = $"{originalPath}.{arrayField.Name}.Array.data[{index}]";

        return elementPath;
    }
}