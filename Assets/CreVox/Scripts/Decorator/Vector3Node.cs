using UnityEngine;
using UnityEditor;
using System.Collections;
using NodeEditorFramework;
using System;

[Node(false, "CreVox/Vector3 Node")]
public class Vector3Node : Node
{
    public const string ID = "Vector3Node";
    public override string GetID { get { return ID; } }

    Vector3 v;

    public override Node Create(Vector2 pos)
    {
        Vector3Node node = ScriptableObject.CreateInstance<Vector3Node>();

        node.name = "Vector3 Node";
        node.rect = new Rect(pos.x, pos.y, 150, 50);

        node.CreateOutput("Vector3 out:", "Vector3", NodeSide.Right, 10);

        return node;
    }

    protected override void NodeGUI()
    {
        GUILayout.BeginVertical();
        v = EditorGUILayout.Vector3Field("", v);
        GUILayout.EndVertical();
    }


}

public class Vector3Type : IConnectionTypeDeclaration
{
    public string Identifier { get { return "Vector3"; } }
    public Type Type { get { return typeof(Vector3); } }
    public Color Color { get { return Color.blue; } }
    public string InKnobTex { get { return "Textures/In_Knob.png"; } }
    public string OutKnobTex { get { return "Textures/Out_Knob.png"; } }
}