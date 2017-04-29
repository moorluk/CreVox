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
	public class VolumeReplacementWindow : EditorWindow {
		private static Dictionary<string, List<VolumeData>> replaceDictionary = new Dictionary<string, List<VolumeData>>();
		private static List<string> alphabets = new List<string>();

		private static Vector2 scrollPosition = new Vector2(0, 0);



		private string regex = @".*[\\\/](\w+)_.+_vData\.asset$";

		public void Initialize() {
			SpaceAlphabet.Load();
			alphabets = SpaceAlphabet.Alphabets;
			replaceDictionary.Clear();
			foreach(var connectionType in alphabets) {
				replaceDictionary.Add(connectionType, new List<VolumeData>());
			}
		}
		void Awake() {
			Initialize();
		}
		void OnFocus() {
			if (SpaceAlphabet.Changed) {
				SpaceAlphabet.Changed = false;
				Initialize();
			}
		}
		void OnGUI() {
			// Node list.
			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(Screen.width), GUILayout.Height(Screen.height * 0.75f));
			foreach (string key in replaceDictionary.Keys) {
				EditorGUILayout.PrefixLabel(key);

				for (int i = 0; i < replaceDictionary[key].Count; i++) {
					EditorGUILayout.BeginHorizontal();
					replaceDictionary[key][i] = (VolumeData)EditorGUILayout.ObjectField(replaceDictionary[key][i], typeof(VolumeData), false, GUILayout.Width(Screen.width / 2 - 10));
					if (GUILayout.Button("Delete")) {
						replaceDictionary[key].RemoveAt(i);
					}
					EditorGUILayout.EndHorizontal();
				}
				
				if (GUILayout.Button("Add New vData")) {
					replaceDictionary[key].Add(null);
				}
			}
			EditorGUILayout.EndScrollView();
		}

		public static VolumeData GetReplaceVData(string fileName) {
			return replaceDictionary[fileName][UnityEngine.Random.Range(0, replaceDictionary[fileName].Count)];
		}
	}
}

