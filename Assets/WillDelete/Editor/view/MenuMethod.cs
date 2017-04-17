using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MenuMethod : EditorWindow {
	private static EditorWindow _window;
	[MenuItem("Test/Generate", false, 1)]
	public static void ShowWindow() {
		_window = EditorWindow.GetWindow<Test.TestSomeFunctionWindow>("Test", true);
		_window.position = new Rect(35, 35, 300, 50);
	}
}
