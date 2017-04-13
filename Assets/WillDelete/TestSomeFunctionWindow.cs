using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(TestSomeFunction))]
public class TestSomeFunctionWindow : Editor {
	public override void OnInspectorGUI() {
		DrawDefaultInspector();

		TestSomeFunction myScript = (TestSomeFunction) target;
		if (GUILayout.Button("Create Volume Objects")) {
			myScript.CreateVolumeObjects();
		}
	}
}