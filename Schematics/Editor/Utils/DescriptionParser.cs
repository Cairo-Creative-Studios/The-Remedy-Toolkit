using Remedy.Schematics;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public static class DescriptionParser
{
    static readonly Regex TokenRegex = new(@"\{([^{}]+)\}");

    public enum DocTokenType
    {
        PortIndex,
        Variable,
        ComponentProperty,
        None
    }

    public struct DocToken
    {
        public DocTokenType Type;
        public string Name;
        public string Raw;
        public int Index;
        public string[] Path;
    }

    public enum DescriptionPartType
    {
        Text,
        Token
    }

    public struct DescriptionPart
    {
        public DescriptionPartType Type;
        public string Text;      // Used if Type == Text
        public DocToken Token;   // Used if Type == Token
    }

    private static DocToken GetTokenType(string token, bool belongsToComponent)
    {
        // {0} 
        if (int.TryParse(token, out int index))
        {
            return new DocToken
            {
                Type = DocTokenType.PortIndex,
                Index = index,
                Raw = token
            };
        }

        if (token.Contains('.'))
        {
            var path = token.Split('.');

            // var.varName (Recieved from Schematic Graph
            if (path[0] == "var")
            {
                return new DocToken
                {
                    Type = DocTokenType.Variable,
                    Name = path[1],
                    Path = path,
                    Raw = token
                };
            }
            // prop.propertyFieldName (Only works for Component Nodes)
            if (path[0] == "prop" && belongsToComponent)
            {
                return new DocToken
                {
                    Type = DocTokenType.ComponentProperty,
                    Name = path[1],
                    Path = path,
                    Raw = token
                };
            }
        }

        return new DocToken
        {
            Type = DocTokenType.None,
            Raw = token
        };
    }

    private static List<DescriptionPart> GetDescriptionParts(string input, Object component)
    {
        var parts = new List<DescriptionPart>();
        int lastIndex = 0;

        foreach (Match match in TokenRegex.Matches(input))
        {
            if (match.Index > lastIndex)
            {
                parts.Add(new DescriptionPart
                {
                    Type = DescriptionPartType.Text,
                    Text = input.Substring(lastIndex, match.Index - lastIndex)
                });
            }

            string tokenText = match.Groups[1].Value;
            var token = GetTokenType(tokenText, component);

            parts.Add(new DescriptionPart
            {
                Type = DescriptionPartType.Token,
                Token = token
            });

            lastIndex = match.Index + match.Length;
        }

        if (lastIndex < input.Length)
        {
            parts.Add(new DescriptionPart
            {
                Type = DescriptionPartType.Text,
                Text = input.Substring(lastIndex)
            });
        }

        return parts;
    }

    private static string ResolveToken(DocToken token, SchematicGraph graph, Object component)
    {
        switch(token.Type)
        {
            case DocTokenType.None:
                return "token failure: " + token.Raw;
            case DocTokenType.Variable:

                break;
            case DocTokenType.ComponentProperty:
                break;
            case DocTokenType.PortIndex:
                break;
        }
        return string.Empty;
    }

    /// <summary>
    /// Resolves the given Description for the 
    /// </summary>
    /// <param name="parts"></param>
    /// <param name="component"></param>
    /// <returns></returns>
    public static string ResolveDescription(IEnumerable<DescriptionPart> parts, SchematicGraph graph, Object component)
    {
        var sb = new System.Text.StringBuilder();

        foreach (var part in parts)
        {
            if (part.Type == DescriptionPartType.Text)
            {
                sb.Append(part.Text);
            }
            else
            {
                var value = ResolveToken(part.Token, graph, component);
                sb.Append(value?.ToString() ?? "—");
            }
        }

        return sb.ToString();
    }

}