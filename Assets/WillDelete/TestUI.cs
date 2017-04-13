using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(test))]
public class TestUI : Editor {
	public override void OnInspectorGUI() {
		DrawDefaultInspector();

		test myScript = (test)target;
		if (GUILayout.Button("Build Object")) {
			myScript.CreateButton();
		}
	}
}