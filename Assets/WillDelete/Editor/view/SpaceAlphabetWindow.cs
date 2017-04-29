using UnityEngine;
using System.Collections;
using UnityEditor;
using CreVox;
using MissionGrammarSystem;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;

namespace CrevoxExtend {
	public class SpaceAlphabetWindow : EditorWindow {
		private List<string> alphabets;
		private Vector2 scrollPosition = new Vector2(0, 0);

		void Initialize() {
			SpaceAlphabet.Load();
			alphabets = SpaceAlphabet.Alphabets;
		}
		void Awake() {
			Initialize();
		}
		void OnGUI() {
			// Aphabets list.
			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(Screen.width), GUILayout.Height(Screen.height * 0.75f));
			for (int i = 0; i < alphabets.Count; i++) {
				EditorGUILayout.BeginHorizontal();
				alphabets[i] = EditorGUILayout.TextField(alphabets[i]);
				if (i > 0) {
					if (GUILayout.Button("Delete")) {
						alphabets.RemoveAt(i);
					}
				}
				EditorGUILayout.EndHorizontal();
			}
			if (GUILayout.Button("Add New")) {
				alphabets.Add("NewConnection"+alphabets.Count);
			}
			if (GUILayout.Button("Save")) {
				SpaceAlphabet.alphabetUpdate(alphabets);
				SpaceAlphabet.Changed = true;
			}
			EditorGUILayout.EndScrollView();
		}
	}
}

