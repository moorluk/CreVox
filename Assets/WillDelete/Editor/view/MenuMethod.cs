using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MenuMethod : EditorWindow {
	private static EditorWindow _window;
	[MenuItem("CrevoxExtend/Volume generation", false, 1)]
	public static void ShowWindow() {
		_window = EditorWindow.GetWindow<CrevoxExtend.VolumeDataTransformWindow>("Volume generation", true);
		_window.position = new Rect(35, 35, 300, 50);
	}
}
