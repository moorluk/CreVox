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
		private static Dictionary<GraphGrammarNode, List<VolumeData>> RefrenceTable { get; set; }
		private static string regex = @".*[\\\/](\w+)_.+_vData\.asset$";
		private static Vector2 scrollPosition = new Vector2(0, 0);
		private static bool specificRandomSeed = false;
		private static int randomSeed = 0;
		private static int stageID = 1;
		private VGlobal vg;

		void Initialize() {
			// Create a new one or clear it.
			if (RefrenceTable == null) {
				RefrenceTable = new Dictionary<GraphGrammarNode, List<VolumeData>>();
			} else {
				RefrenceTable.Clear();
			}

			vg = VGlobal.GetSetting();
			foreach (var node in Alphabet.Nodes.Where(n => (n != Alphabet.AnyNode && n.Terminal != NodeTerminalType.NonTerminal))) {
				RefrenceTable.Add(node, new List<VolumeData>());
			}
		}
		void Awake() {
			Initialize();
		}
		// On focus on the window.
		void OnFocus() {
			if (RefrenceTable == null) {
				Initialize();
			} else if (IsChanged()) {
				Initialize();
			}
		}
		// If the alphabet changed, update the list of nodes.
		bool IsChanged() {
			var currentNodes = RefrenceTable.Keys.Select(n => n.Name).ToList();
			var latestNodes  = Alphabet.Nodes.Where(n => (n != Alphabet.AnyNode && n.Terminal != NodeTerminalType.NonTerminal)).Select(n => n.Name).ToList();

			var firstNotSecond = currentNodes.Except(latestNodes).ToList();
			var secondNotFirst = latestNodes.Except(currentNodes).ToList();

			return firstNotSecond.Any() || secondNotFirst.Any();
		}

		void OnGUI() {
			if (GUILayout.Button("Open Folder")) {
				// At first, clear all volume list.
				RefrenceTable.Values.ToList().ForEach(vd => vd.Clear());
				// Open folder.
				string path = EditorUtility.OpenFolderPanel("Load Folder", 
					Application.dataPath + PathCollect.resourcesPath.Substring (6) + PathCollect.save, "");
				if (path != string.Empty) {
					// Get the files.
					string[] files = Directory.GetFiles(path);
					foreach (var file in files) {
						if (! Regex.IsMatch(file, regex)) {
							continue;
						}
						foreach (var node in RefrenceTable.Keys) {
							if (node.Name.ToLower() != Regex.Match(file, regex).Groups[1].Value.ToLower()) {
								continue;
							}
							RefrenceTable[node].Add(CrevoxOperation.GetVolumeData(file.Replace(Environment.CurrentDirectory.Replace('\\', '/') + "/", "")));
						}
					}
					// if not find match vData, default null.
					foreach (var volumeList in RefrenceTable.Values) {
						if (volumeList.Count == 0) {
							volumeList.Clear();
						}
					}
				}
			}
			// Node list.
			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(Screen.width), GUILayout.Height(Screen.height-230f));
			foreach (var node in RefrenceTable.Keys) {
				// Label field, show the ExpressName of node.
				EditorGUILayout.LabelField(node.ExpressName);
				// Show volumeData object each node.
				foreach (var volume in RefrenceTable[node].ToList()) {
					EditorGUILayout.BeginHorizontal();
					// Field about input the volume data.
					RefrenceTable[node][RefrenceTable[node].IndexOf(volume)] = (VolumeData) EditorGUILayout.ObjectField(volume, typeof(VolumeData), false, GUILayout.Width(Screen.width / 2 - 10));
					// Delete function.
					if (GUILayout.Button("Delete")) {
						RefrenceTable[node].Remove(volume);
					}
					EditorGUILayout.EndHorizontal();
				}
				// Create function.
				if (GUILayout.Button("Add New vData")) {
					RefrenceTable[node].Add(null);
				}
			}
			EditorGUILayout.EndScrollView();
			using (var v = new EditorGUILayout.VerticalScope (EditorStyles.helpBox)) {
				stageID = EditorGUILayout.IntSlider ("Stage", stageID, 1, vg.StageList.Count);
				CrevoxGeneration.stage = vg.GetStageSetting(stageID);
				EditorGUILayout.LabelField ("Level", CrevoxGeneration.stage.number.ToString());
				EditorGUILayout.LabelField ("Xml Path", CrevoxGeneration.stage.XmlPath);
				EditorGUILayout.LabelField ("VData Path", CrevoxGeneration.stage.vDataPath);
				EditorGUILayout.LabelField ("ArtPack", CrevoxGeneration.stage.artPack);
			}
			// If symbol has none vData, user cannot press Generate.
			// Add null prevent.
			EditorGUI.BeginDisabledGroup(RefrenceTable.Values.ToList().Exists(vs => vs.Count == 0 || vs.Exists(v => v == null)));
			CrevoxGeneration.generateVolume = EditorGUILayout.Toggle("Generate Volume", CrevoxGeneration.generateVolume);
			EditorGUILayout.BeginHorizontal();
			// Random seed and its toggler.
			specificRandomSeed = GUILayout.Toggle(specificRandomSeed, "Set Random Seed");
			EditorGUI.BeginDisabledGroup(! specificRandomSeed);
			randomSeed = EditorGUILayout.IntField(randomSeed, GUILayout.MaxWidth(Screen.width));
			EditorGUI.EndDisabledGroup();
			EditorGUILayout.EndHorizontal();
			// Generate level.
			if (GUILayout.Button("Generate")) {
				// Pass the RefrenceTable to CrevoxGeneration.
				CrevoxGeneration.RefrenceTable.Clear();
				foreach (var node in RefrenceTable.Keys) {
					CrevoxGeneration.RefrenceTable.Add(node.AlphabetID, RefrenceTable[node]);
				}
				// Set the random seed.
				if (! specificRandomSeed) { randomSeed = UnityEngine.Random.Range(0, int.MaxValue); }
				CrevoxGeneration.InitialTable(randomSeed);
				CrevoxGeneration.Generate(CrevoxGeneration.stage);
			}
			// Replace connections.
			if (GUILayout.Button("ReplaceConnection")) {
				CrevoxGeneration.ReplaceConnection(CrevoxGeneration.stage);
			}
			EditorGUI.EndDisabledGroup();
		}
	}
}
