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
	class CrevoxGenerationWindow : EditorWindow {
		private static List<GraphGrammarNode> alphabets = new List<GraphGrammarNode>();
		private static List<List<VolumeData>> volumeDatas = new List<List<VolumeData>>();
		private static string regex = @".*[\\\/](\w+)_.+_vData\.asset$";

		private static Vector2 scrollPosition = new Vector2(0, 0);
		private static bool specificRandomSeed = false;
		private static int randomSeed = 0;

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
						if (Regex.IsMatch(files[i], regex)){
							for (int j = 0; j < alphabets.Count; j++) {
								if (alphabets[j].Name.ToLower() == Regex.Match(files[i], regex).Groups[1].Value.ToLower()) {
									volumeDatas[j].Add(CrevoxOperation.GetVolumeData(files[i].Replace(Environment.CurrentDirectory.Replace('\\', '/') + "/", "")));
								}
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
			// If symbol has none vData, user cannot press Generate.
			// Add null prevent.
			EditorGUI.BeginDisabledGroup(volumeDatas.Exists(vs => vs.Count == 0||vs.Exists(v => v == null)));
			// Generate button.
			if (GUILayout.Button("Generate")) {
				CrevoxGeneration.AlphabetIDs = alphabets.Select(x => x.AlphabetID).ToList();
				CrevoxGeneration.SameVolumeDatas = volumeDatas;
				if (!specificRandomSeed) { randomSeed = UnityEngine.Random.Range(0, int.MaxValue); }
				CrevoxGeneration.InitialTable(randomSeed);
				CrevoxGeneration.Generate();
			}
			if (GUILayout.Button("ReplaceConnection")) {
				CrevoxGeneration.ReplaceConnection();
			}
			EditorGUILayout.BeginHorizontal();
			specificRandomSeed = GUILayout.Toggle(specificRandomSeed, "Set Random Seed");
			EditorGUI.BeginDisabledGroup(!specificRandomSeed);
			randomSeed = EditorGUILayout.IntField(randomSeed, GUILayout.MaxWidth(Screen.width));
			EditorGUI.EndDisabledGroup();
			EditorGUILayout.EndHorizontal();
			// [TEST] Will delete.
			EditorGUI.EndDisabledGroup();
		}
	}
}
