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
		private static int _randomCount;

		private static List<List<VolumeData>> volumeDatas = new List<List<VolumeData>>();

		void Initialize() {
			alphabets.Clear();
			volumeDatas.Clear();
			foreach (var node in Alphabet.Nodes) {
				if (node == Alphabet.AnyNode || node.Terminal == NodeTerminalType.NonTerminal) {
					continue;
				}
				alphabets.Add(node);
				volumeDatas.Add(new List<VolumeData>());
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
				// First clear all origin volumeDatas.
				for (int j = 0; j < alphabets.Count; j++) {
					volumeDatas[j].Clear();
				}
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
							if (fileName.Length < alphabets[j].Name.Length) { continue; }
							else if (alphabets[j].Name.ToLower() == fileName.Substring(0, alphabets[j].Name.Length).ToLower()) {
								volumeDatas[j].Add(CrevoxOperation.GetVolumeData(files[i].Replace(Environment.CurrentDirectory.Replace('\\', '/') + "/", "")));
							}
						}
					}
					// if not find match vData, default null.
					for (int j = 0; j < alphabets.Count; j++) {
						if (volumeDatas[j].Count < 1) {
							volumeDatas[j].Add(null);
						}
					}
				}
			}
			// Node list.
			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(Screen.width), GUILayout.Height(Screen.height * 0.75f));
			for (int i = 0; i < alphabets.Count; i++) {
				EditorGUILayout.LabelField(alphabets[i].ExpressName);
				for (int j = 0; j < volumeDatas[i].Count; j++) {
					EditorGUILayout.BeginHorizontal();
					volumeDatas[i][j] = (VolumeData)EditorGUILayout.ObjectField(volumeDatas[i][j], typeof(VolumeData), false, GUILayout.Width(Screen.width / 2 - 10));
					if (GUILayout.Button("Delete")) {
						volumeDatas[i].RemoveAt(j);
					}
					EditorGUILayout.EndHorizontal();
				}
				if (GUILayout.Button("Add New vData")) {
					volumeDatas[i].Add(null);
				}
			}
			EditorGUILayout.EndScrollView();
			// Generate button.
			if (GUILayout.Button("Generate")) {
				VolumeDataTransform.AlphabetIDs = alphabets.Select(x => x.AlphabetID).ToList();
				VolumeDataTransform.SameVolumeDatas = volumeDatas;
				VolumeDataTransform.InitialTable();
				VolumeDataTransform.Generate();
			}
			// [TEST] Will delete.
			// Random generate button.
			if (GUILayout.Button("Random Generate")) {
				VolumeDataTransform.AlphabetIDs = alphabets.Select(x => x.AlphabetID).ToList();
				VolumeDataTransform.SameVolumeDatas = volumeDatas;
				VolumeDataTransform.InitialTable();
				VolumeDataTransform.RandomGenerate(_randomCount);
			}
			_randomCount = EditorGUILayout.IntField("Random generate count", _randomCount);
		}
	}
}