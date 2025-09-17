﻿// project armada

#pragma warning disable 0414

using BlueGraph;
using Remedy.Schematics;
using UnityEngine;

[Node(Path = "Events"), Tags("Runtime")]
public class OnLevelStart : SchematicEventNode
{
    [Output]
    public string LevelName;
}