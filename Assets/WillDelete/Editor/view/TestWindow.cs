using UnityEngine;
using System.Collections;
using UnityEditor;
using CreVox;
using MissionGrammarSystem;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;

namespace Test {
	class TestWindow : EditorWindow {
		private static Vector2 scrollPosition = new Vector2(0, 0);
		private static List<GraphGrammarNode> alphabets = new List<GraphGrammarNode>();
		private static List<VolumeData> vdatas = new List<VolumeData>();
		public static int Count;

		void Initial() {
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
			Initial();
		}
		void OnFocus() {
			if (isChanged()) {
				Initial();
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
			if(GUILayout.Button("Open Folder")) {
				string path = EditorUtility.OpenFolderPanel("Load Folder", "", "");
				if(path != "") {	
					string[] files = Directory.GetFiles(path);
					Debug.Log("Folder:");
					Debug.Log(path);
					for (int i = 0; i < files.Length; i++) {
						string fileName = files[i].Split('\\').Last();
						if(fileName.Length <= 12 || fileName.Substring(fileName.Length - 12, 12) != "_vData.asset" ) {
							continue;
						}
						fileName = fileName.Remove(fileName.Length - 12, 12);
						for (int j = 0; j < alphabets.Count; j++) {
							if(alphabets[j].Name.ToLower() == fileName.ToLower()) {
								Debug.Log(files[i]);
								vdatas[j] = AddOn.GetVolumeData(files[i].Replace(Environment.CurrentDirectory.Replace('\\', '/') + "/", ""));
							}
						}
					}
				}
			}
			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(Screen.width), GUILayout.Height(Screen.height * 0.75f));
			for(int i = 0; i < alphabets.Count; i++) {
				EditorGUILayout.BeginHorizontal();
				GUILayout.Label(alphabets[i].ExpressName, GUILayout.Width(Screen.width / 2));
				vdatas[i] = (VolumeData) EditorGUILayout.ObjectField(vdatas[i], typeof(VolumeData), false, GUILayout.Width(Screen.width / 2 - 10));
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.EndScrollView();
			if (GUILayout.Button("Generate")) {
				VolumeDataTransform.AlphabetIDs = alphabets.Select(x => x.AlphabetID).ToList();
				VolumeDataTransform.VolumeDatas = vdatas;
				VolumeDataTransform.InitialTable();
				VolumeDataTransform.Generate();
			}
			if(GUILayout.Button("Random Generate")) {
				VolumeDataTransform.AlphabetIDs = alphabets.Select(x => x.AlphabetID).ToList();
				VolumeDataTransform.VolumeDatas = vdatas;
				VolumeDataTransform.InitialTable();
				VolumeDataTransform.RandomGenerate(Count);
			}
			Count = EditorGUILayout.IntField("Random generate count", Count);
		}
	}
}