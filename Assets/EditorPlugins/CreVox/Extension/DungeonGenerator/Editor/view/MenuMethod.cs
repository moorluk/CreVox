using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MenuMethod : EditorWindow {
	private static EditorWindow _window;
	[MenuItem("Dungeon/Generation/Volume generation", false, 32)]
	public static void ShowWindow() {
		_window = GetWindow<CrevoxExtend.CrevoxGenerationWindow>("Volume generation", true);
		_window.position = new Rect(35, 35, 300, 50);
	}
}
