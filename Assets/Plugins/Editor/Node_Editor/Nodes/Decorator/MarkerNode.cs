using UnityEngine;
using System.Collections;
using NodeEditorFramework;
using System;

[Node (false, "CreVox/Marker Node")]
public class MarkerNode : Node
{
	public const string ID = "MarkerNode";

	public override string GetID { get { return ID; } }

	string markerName = "Marker Name";

	#region Visual Design
	float nWidth = 150;
	float nHeight = 45;
	#endregion

	public override Node Create (Vector2 pos)
	{
		MarkerNode node = ScriptableObject.CreateInstance<MarkerNode> ();

		node.name = "Marker Node";
		node.rect = new Rect (pos.x, pos.y, nWidth, nHeight);

		node.CreateOutput ("Next Flow:", "Flow", NodeSide.Bottom, nWidth/2);

		return node;
	}

	protected internal override void NodeGUI ()
	{
		using (var v = new GUILayout.VerticalScope ()) {
			markerName = GUILayout.TextField (markerName);
		}
	}


}
