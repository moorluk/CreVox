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
	[MenuItem("CrevoxExtend/SpaceAlphabet", false, 1)]
	public static void ShowSpaceAlphabet() {
		_window = EditorWindow.GetWindow<CrevoxExtend.SpaceAlphabetWindow>("Space Alphabet", true);
		_window.position = new Rect(35, 35, 300, 50);
	}
	[MenuItem("CrevoxExtend/Volume Replacement", false, 1)]
	public static void ShowVolumeReplacement() {
		_window = EditorWindow.GetWindow<CrevoxExtend.VolumeReplacementWindow>("Volume Replacement", true);
		_window.position = new Rect(35, 35, 300, 50);
	}
}
