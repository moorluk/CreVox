using UnityEngine;
using System.Collections;
using UnityEditor;
namespace Test {
	class TestSomeFunctionWindow : EditorWindow {
		private int _count = 10;
		void OnGUI() {
			GUILayout.Space(10f);
			_count = EditorGUILayout.IntField("Count",_count);
			GUILayout.Space(10f);
			if (GUILayout.Button("Generate volume")) {
				AddOn.Initial(AddOn.VolumeDataResource[Random.Range(0, 5)]);
				for (int i = 0; i < _count - 1; i++) {
					AddOn.AddAndCombineVolume(AddOn.VolumeDataResource[Random.Range(0, 5)]);
				}
			}
		}
	}
}