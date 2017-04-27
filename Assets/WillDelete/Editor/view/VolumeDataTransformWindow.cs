using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.IO;
using System;
// Import CreVox and DungeonGenerator.
using CreVox;
using MissionGrammarSystem;

namespace CrevoxExtend {
	class VolumeDataTransformWindow : EditorWindow {
		private static Vector2 scrollPosition = new Vector2(0, 0);
		private static int _randomCount;

		private static Dictionary<GraphGrammarNode, List<VolumeData>> _volumeList = new Dictionary<GraphGrammarNode, List<VolumeData>>();

		void Initialize() {
			_volumeList.Clear();
			foreach (var node in Alphabet.Nodes) {
				if (node == Alphabet.AnyNode || node.Terminal == NodeTerminalType.NonTerminal) {
					continue;
				}
				_volumeList.Add(node, new List<VolumeData>());
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
			// If count is different than before (reduce one because 'any node').
			if (Alphabet.Nodes.Count - 1 > _volumeList.Values.Sum(vs => vs.Count)) { return true; }
			// Each node.
			foreach (var node in Alphabet.Nodes) {
				if (node == Alphabet.AnyNode || node.Terminal == NodeTerminalType.NonTerminal) {
					continue;
				}
				if (! _volumeList.Keys.ToList().Exists(n => n.AlphabetID == node.AlphabetID)) {
					return true;
				}
			}
			return false;
		}
		void OnGUI() {
			// Instruction. Import the prefabs.
			if (GUILayout.Button("Open Folder")) {
				ImportPrefabsAutomatically();
			}
			// Node list.
			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(Screen.width), GUILayout.Height(Screen.height * 0.75f));
			foreach (var volumePair in _volumeList) {
				var volumes = volumePair.Value;
				EditorGUILayout.LabelField(volumePair.Key.ExpressName);
				foreach (VolumeData volume in volumes.ToList()) {
					EditorGUILayout.BeginHorizontal();
					// Input field.
					volumes[volumes.IndexOf(volume)] = (VolumeData) EditorGUILayout.ObjectField(volume, typeof (VolumeData), false, GUILayout.Width(Screen.width / 2 - 10));
					// Delete.
					if (GUILayout.Button("Delete")) { volumes.Remove(volume); }
					EditorGUILayout.EndHorizontal();
				}
				// Append.
				if (GUILayout.Button("Add New vData")) { volumes.Add(null); }
			}
			EditorGUILayout.EndScrollView();
			// If symbol has none vData, user cannot press Generate.
			EditorGUI.BeginDisabledGroup(_volumeList.Values.ToList().Exists(vs => vs.Count == 0 || vs.Exists(v => v == null)));
			// Generate button.
			if (GUILayout.Button("Generate")) {
				VolumeDataTransform.AlphabetIDs = _volumeList.Keys.Select(n => n.AlphabetID).ToList();
				VolumeDataTransform.SameVolumeDatas = _volumeList.Values.ToList();
				VolumeDataTransform.InitialTable();
				VolumeDataTransform.Generate();
			}
			// Random generate button.
			if (GUILayout.Button("Random Generate")) {
				VolumeDataTransform.AlphabetIDs = _volumeList.Keys.Select(n => n.AlphabetID).ToList();
				VolumeDataTransform.SameVolumeDatas = _volumeList.Values.ToList();
				VolumeDataTransform.InitialTable();
				VolumeDataTransform.RandomGenerate(_randomCount);
			}
			_randomCount = EditorGUILayout.IntField("Random generate count", _randomCount);
			EditorGUI.EndDisabledGroup();
		}

		// Import prefabs automatically via path of folder.
		void ImportPrefabsAutomatically() {
			string path;
			// First clear all volumeData in volumeList.
			foreach (var volumeData in _volumeList.Values.ToList()) { volumeData.Clear(); }
			// Open folder.
			path = EditorUtility.OpenFolderPanel("Load Folder", "", "");
			if (path != string.Empty) {
				// All prefabs in the path.
				foreach (string file in Directory.GetFiles(path)) {
					// Exactly, only once or no.
					foreach (Match m in Regex.Matches(file, @".*[\\\/](\w+)_.+_vData\.asset$")) {
						var matchedVolumes =
							from pair in _volumeList
							where pair.Key.Name.ToLower() == m.Groups[1].Value.ToLower()
							select pair;

						foreach (var volume in matchedVolumes) {
							_volumeList[volume.Key].Add(CrevoxOperation.GetVolumeData(file.Replace(Environment.CurrentDirectory.Replace('\\', '/') + "/", "")));
						}
					}
				}
				// If not find matched vData, default null.
				_volumeList.Values.Where(vs => vs.Count == 0).ToList().ForEach(vs => vs.Add(null));
			}
		}
	}
}