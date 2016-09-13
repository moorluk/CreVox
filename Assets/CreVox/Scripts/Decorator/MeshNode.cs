using UnityEngine;
using UnityEditor;
using System.Collections;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;
using System;

[Node(false, "CreVox/Mesh Node")]
public class MeshNode : Node
{
    GameObject go = null;
    Vector3 p;
    Vector3 r;
    Vector3 s;
    float chance;
    bool consume;
    Texture2D preview;

    public const string ID = "MeshNode";

    public override string GetID { get { return ID; } }

    public override Node Create(Vector2 pos)
    {
        MeshNode node = ScriptableObject.CreateInstance<MeshNode>();

        node.name = "Mesh Node";
        node.rect = new Rect(pos.x, pos.y, 120, 140);

        node.CreateInput("Prev Flow:", "Flow", NodeSide.Left, 10);
        node.CreateInput("Offset", "Vector3", NodeSide.Left, 30);
        node.CreateInput("Rotate", "Vector3", NodeSide.Left, 50);
        node.CreateInput("Scale", "Vector3", NodeSide.Left, 70);
        node.CreateInput("Affinity", "Float", NodeSide.Left, 90);

        node.CreateOutput("Next Flow:", "Flow", NodeSide.Right, 10);

        return node;
    }

    protected override void NodeGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        GameObject newGO = null;
        newGO = RTEditorGUI.ObjectField<GameObject>(go, false);
        consume = GUILayout.Toggle(consume, "Consume");
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical();
        Inputs[1].DisplayLayout();
        Inputs[2].DisplayLayout();
        Inputs[3].DisplayLayout();
        Inputs[4].DisplayLayout();
        GUILayout.EndVertical();

        GUILayout.BeginVertical(GUILayout.Width(32));
       
        if (newGO != go )
        {
            preview = AssetPreview.GetAssetPreview(newGO);
            go = newGO;
        }

        if (preview != null)
        {
            GUILayout.Box(preview, GUIStyle.none, new GUILayoutOption[] { GUILayout.Width(64), GUILayout.Height(64) });
        }
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();

        if (GUI.changed)
            NodeEditor.RecalculateFrom(this);
    }

    public override bool Calculate()
    {
        if (Inputs[1].connection != null)
            p = Inputs[1].connection.GetValue<Vector3>();
        if (Inputs[2].connection != null)
            r = Inputs[2].connection.GetValue<Vector3>();
        if (Inputs[3].connection != null)
            s = Inputs[3].connection.GetValue<Vector3>();
        return true;
    }
}
