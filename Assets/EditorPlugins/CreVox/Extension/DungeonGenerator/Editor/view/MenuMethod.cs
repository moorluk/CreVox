using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MenuMethod : EditorWindow {
	private static EditorWindow _window;
	[MenuItem("CrevoxExtend/Volume generation", false, 1)]
	public static void ShowWindow() {
		_window = EditorWindow.GetWindow<CrevoxExtend.CrevoxGenerationWindow>("Volume generation", true);
		_window.position = new Rect(35, 35, 300, 50);
	}
	[MenuItem("CrevoxExtend/SpaceAlphabet", false, 1)]
	public static void ShowSpaceAlphabet() {
		_window = EditorWindow.GetWindow<CrevoxExtend.SpaceAlphabetWindow>("Space Alphabet", true);
		_window.minSize = new Vector2(35, 130);
		_window.position = new Rect(35, 35, 300, 50);
	}
}
