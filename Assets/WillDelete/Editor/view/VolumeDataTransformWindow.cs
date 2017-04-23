using UnityEngine;
using System.Collections;
using UnityEditor;
using CreVox;
using MissionGrammarSystem;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;

namespace CrevoxExtend {
	class VolumeDataTransformWindow : EditorWindow {
		private static Vector2 scrollPosition = new Vector2(0, 0);
		private static List<GraphGrammarNode> alphabets = new List<GraphGrammarNode>();
		private static List<VolumeData> vdatas = new List<VolumeData>();
		private static int _randomCount;

		void Initialize() {
			alphabets.Clear();
			vdatas.Clear();
			foreach (var node in Alphabet.Nodes) {
				if (node == Alphabet.AnyNode || node.Terminal == NodeTerminalType.NonTerminal) {
					continue;
				}
				alphabets.Add(node);
				vdatas.Add(null);
			}
		}
		void Awake() {
			Initialize();
		}
		void OnFocus() {
			if (isChanged()) {
				Initialize();
			}
		}
		bool isChanged() {
			for (int i = 0, index = 0; i < Alphabet.Nodes.Count; i++) {
				if (Alphabet.Nodes[i] == Alphabet.AnyNode || Alphabet.Nodes[i].Terminal == NodeTerminalType.NonTerminal) {
					continue;
				}
				if (index >= alphabets.Count || Alphabet.Nodes[i].AlphabetID != alphabets[index].AlphabetID) {
					return true;
				}
				index++;
			}
			return false;
		}
		void OnGUI() {
			if (GUILayout.Button("Open Folder")) {
				// Open folder.
				string path = EditorUtility.OpenFolderPanel("Load Folder", "", "");
				if (path != "") {
					// Get the files.
					string[] files = Directory.GetFiles(path);
					for (int i = 0; i < files.Length; i++) {
						// Get the file name.
						string fileName = files[i].Split('\\').Last();
						// If the file name is illegal then continue.
						if (fileName.Length <= 12 || fileName.Substring(fileName.Length - 12, 12).ToLower() != "_vdata.asset") {
							continue;
						}
						// Get the keyword of file name.
						fileName = fileName.Remove(fileName.Length - 12, 12);
						// Keyword compare.
						for (int j = 0; j < alphabets.Count; j++) {
							if (alphabets[j].Name.ToLower() == fileName.ToLower()) {
								vdatas[j] = CrevoxOperation.GetVolumeData(files[i].Replace(Environment.CurrentDirectory.Replace('\\', '/') + "/", ""));
							}
						}
					}
				}
			}
			// Node list.
			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(Screen.width), GUILayout.Height(Screen.height * 0.75f));
			for (int i = 0; i < alphabets.Count; i++) {
				EditorGUILayout.BeginHorizontal();
				GUILayout.Label(alphabets[i].ExpressName, GUILayout.Width(Screen.width / 2));
				vdatas[i] = (VolumeData) EditorGUILayout.ObjectField(vdatas[i], typeof(VolumeData), false, GUILayout.Width(Screen.width / 2 - 10));
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.EndScrollView();
			// Generate button.
			if (GUILayout.Button("Generate")) {
				VolumeDataTransform.AlphabetIDs = alphabets.Select(x => x.AlphabetID).ToList();
				VolumeDataTransform.VolumeDatas = vdatas;
				VolumeDataTransform.InitialTable();
				VolumeDataTransform.Generate();
			}
			// [TEST] Will delete.
			// Random generate button.
			if (GUILayout.Button("Random Generate")) {
				VolumeDataTransform.AlphabetIDs = alphabets.Select(x => x.AlphabetID).ToList();
				VolumeDataTransform.VolumeDatas = vdatas;
				VolumeDataTransform.InitialTable();
				VolumeDataTransform.RandomGenerate(_randomCount);
			}
			_randomCount = EditorGUILayout.IntField("Random generate count", _randomCount);
		}
	}
}