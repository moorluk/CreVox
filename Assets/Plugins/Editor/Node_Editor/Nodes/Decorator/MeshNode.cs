using UnityEngine;
using UnityEditor;
using System.Collections;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;
using System;

[Node (false, "CreVox/Mesh Node")]
public class MeshNode : Node
{
	GameObject go = null;
	Vector3 pos = Vector3.zero;
	Vector3 rot = Vector3.zero;
	Vector3 sca = Vector3.one;
	float probability;
	bool consume;

	#region Visual Design
	float nWidth = 150;
	float nHeight = 300;
	float cSize = 100;
	Texture2D preview;
//	bool showOffset = false;
	#endregion

	public const string ID = "MeshNode";

	public override string GetID { get { return ID; } }

	public override Node Create (Vector2 pos)
	{
		MeshNode node = ScriptableObject.CreateInstance<MeshNode> ();

		node.name = "Mesh Node";
		node.rect = new Rect (pos.x, pos.y, nWidth, nHeight);

		node.CreateInput ("Prev Flow:", "Flow", NodeSide.Top, nWidth/2);
	        node.CreateInput("Offset", "Vector3", NodeSide.Left, 30);
	        node.CreateInput("Rotate", "Vector3", NodeSide.Left, 50);
	        node.CreateInput("Scale", "Vector3", NodeSide.Left, 70);
//		node.CreateInput ("Affinity", "Float", NodeSide.Left, 90);

		node.CreateOutput ("Next Flow:", "Flow", NodeSide.Bottom, nWidth/2);
		node.CreateOutput("Offset", "Vector3", NodeSide.Right, 30);
		node.CreateOutput("Rotate", "Vector3", NodeSide.Right, 50);
		node.CreateOutput("Scale", "Vector3", NodeSide.Right, 70);

		return node;
	}

	protected internal override void NodeGUI ()
	{
		EditorGUIUtility.wideMode = true;

//		using (var v = new GUILayout.VerticalScope ()) {
//			for (int i = 1; i < Inputs.Count; i++) {
//				Inputs [i].DisplayLayout ();
//			}
//		}
		using (var v = new GUILayout.VerticalScope ()) {
			EditorGUIUtility.labelWidth = 25;
			pos = EditorGUILayout.Vector3Field ("Pos"/*ition"*/, pos, GUILayout.Width (nWidth - 10));
			rot = EditorGUILayout.Vector3Field ("Rot"/*ation"*/, rot, GUILayout.Width (nWidth - 10));
			sca = EditorGUILayout.Vector3Field ("Sca"/*le"*/, sca, GUILayout.Width (nWidth - 10));
			EditorGUIUtility.labelWidth = 60;
		}
		using (var v = new GUILayout.VerticalScope (EditorStyles.helpBox)) {
			go = RTEditorGUI.ObjectField<GameObject> (go, false);
			preview = AssetPreview.GetAssetPreview (go);
			//			RTEditorGUI.DrawTexture ((Texture)preview, (int)cSize, EditorStyles.objectFieldThumb);
			GUILayout.Box (preview, EditorStyles.objectFieldThumb, new GUILayoutOption[] {
				GUILayout.Width (cSize)
				, GUILayout.Height (cSize)
			});
		}
		EditorGUILayout.Space ();
		consume = EditorGUILayout.Toggle ("Consume", consume, GUILayout.Width (cSize));

		if (GUI.changed)
			NodeEditor.RecalculateFrom (this);
	}

	public override bool Calculate ()
	{
//		if (Inputs [1].connection != null)
//			pos = Inputs [1].connection.GetValue<Vector3> ();
//		if (Inputs [2].connection != null)
//			rot = Inputs [2].connection.GetValue<Vector3> ();
//		if (Inputs [3].connection != null)
//			sca = Inputs [3].connection.GetValue<Vector3> ();
//		return true;
		if (Inputs [1].connection != null)
			Outputs [1].SetValue<Vector3> (Inputs [1].connection.GetValue<Vector3> () + pos);
		if (Inputs [2].connection != null)
			Outputs [2].SetValue<Vector3> (Inputs [2].connection.GetValue<Vector3> () + rot);
		if (Inputs [3].connection != null)
			Outputs [3].SetValue<Vector3> (Inputs [3].connection.GetValue<Vector3> () + sca);
		return true;
	}

}
