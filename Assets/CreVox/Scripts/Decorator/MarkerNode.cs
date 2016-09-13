using UnityEngine;
using System.Collections;
using NodeEditorFramework;
using System;

[Node(false, "CreVox/Marker Node")]
public class MarkerNode : Node {
    public const string ID = "MarkerNode";
    public override string GetID { get {return ID;}}

    string markerName = "Marker Name";

    public override Node Create(Vector2 pos)
    {
        MarkerNode node = ScriptableObject.CreateInstance<MarkerNode>();

        node.name = "Marker Node";
        node.rect = new Rect(pos.x, pos.y, 100, 50);

        node.CreateOutput("Next Flow:", "Flow", NodeSide.Right, 10);

        return node;
    }

    protected override void NodeGUI()
    {
        GUILayout.BeginVertical();
        markerName = GUILayout.TextField(markerName);
        GUILayout.EndVertical();
    }


}
