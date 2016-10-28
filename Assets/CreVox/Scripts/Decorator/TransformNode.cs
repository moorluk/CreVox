﻿using UnityEngine;
using UnityEditor;
using System.Collections;
using NodeEditorFramework;
using System;

[Node (false, "CreVox/Transform Node")]
public class TransformNode : Node
{
	public const string ID = "TransformNode";

	public override string GetID { get { return ID; } }

	Vector3 p = Vector3.zero;
	Vector3 r = Vector3.zero;
	Vector3 s = Vector3.one;

	#region Visual Design
	float nWidth = 150;
	float nHeight = 300;
	float cSize = 100;
	#endregion

	public override Node Create (Vector2 pos)
	{
		TransformNode node = ScriptableObject.CreateInstance<TransformNode> ();

		node.name = "Transform Node";
		node.rect = new Rect (pos.x, pos.y, 150, 80);

//		node.CreateOutput ("Vector3 out:", "Vector3", NodeSide.Right, 10);
		node.CreateOutput("Offset", "Vector3", NodeSide.Right, 30);
		node.CreateOutput("Rotate", "Vector3", NodeSide.Right, 50);
		node.CreateOutput("Scale", "Vector3", NodeSide.Right, 70);

		return node;
	}

	protected override void NodeGUI ()
	{
		EditorGUIUtility.wideMode = true;
		using (var h = new GUILayout.HorizontalScope ()) {
			using (var v = new GUILayout.VerticalScope ()) {
				EditorGUIUtility.labelWidth = 25;
				p = EditorGUILayout.Vector3Field ("Pos"/*ition"*/, p, GUILayout.Width (nWidth - 10));
				r = EditorGUILayout.Vector3Field ("Rot"/*ation"*/, r, GUILayout.Width (nWidth - 10));
				s = EditorGUILayout.Vector3Field ("Sca"/*le"*/, s, GUILayout.Width (nWidth - 10));
				EditorGUIUtility.labelWidth = 60;
			}
			using (var v = new GUILayout.VerticalScope ()) {
				for (int i = 1; i < Inputs.Count; i++) {
					Outputs [i].DisplayLayout ();
				}
			}
		}
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