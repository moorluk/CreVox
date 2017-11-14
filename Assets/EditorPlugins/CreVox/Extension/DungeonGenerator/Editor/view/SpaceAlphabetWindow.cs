using UnityEngine;
using System.Collections;
using UnityEditor;
using MissionGrammarSystem;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;

namespace CrevoxExtend {
	public class SpaceAlphabetWindow : EditorWindow {
		private Vector2 scrollPosition = new Vector2(0, 0);
		private List<string> Alphabets { get; set; }
		// Togglers of the replacement alphabet.
		private List<bool> IsSelected { get; set; }

		void Initialize() {
			// Load files and set Alphabets.
			SpaceAlphabet.Load();
			// Copy a new alphabet.
			Alphabets = SpaceAlphabet.ReplacementAlphabet;
			// Toggler of the connections.
			IsSelected = new List<bool>(new bool[Alphabets.Count]);
			UpdatePaletteWindow();
		}
		void Awake() {
			Initialize();
		}
		void OnFocus() {
			if(SpaceAlphabet.alphabetUpdate(MissionGrammarSystem.Alphabet.Connections.Select(c => c.Name).ToList())) {
				UpdatePaletteWindow();
			}
		}
		void OnGUI() {
			if (GUILayout.Button("Export")) {
				string path = EditorUtility.SaveFilePanel("Export xml", "", "SpaceAlphabet.xml", "xml");
				if (path != string.Empty) {
					SpaceAlphabetXML.Serialize.SerializeToXml(path);
				}
			}
			if (GUILayout.Button("Import")) {
				string path = EditorUtility.OpenFilePanel("Import xml", "", "xml");
				if (path != string.Empty) {
					SpaceAlphabetXML.Unserialize.UnserializeFromXml(path);
					Initialize();
				}
			}
			// Aphabets list.//
			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
			for (int i = 0; i < Alphabets.Count; i++) {
				// Check if this alphabet is Saved or not.
				string currentName = Alphabets[i];


				EditorGUILayout.BeginHorizontal();
				currentName = EditorGUILayout.TextField(currentName, GUILayout.Width(Screen.width * 0.51f), GUILayout.Height(18));
				// If current name of alphabet symbol is change, update the name.
				if (currentName != Alphabets[i] && ! Alphabets.Exists(a => a == currentName)) {
					Alphabets[i] = currentName;
				}

				// If alphabet not Saved, disable setting button.
				if (GUILayout.Button("Setting", GUILayout.Width(Screen.width * 0.4f), GUILayout.Height(18))) {
					// Switch setting between 0/1.
					IsSelected[i] = ! IsSelected[i];
				}

				EditorGUILayout.EndHorizontal();
				// If this alphabet is exist in Last Saved version, then can check if setting is clicked.
				// If setting button is switched to true. Open the UI.


				if (IsSelected[i]) {
					var vDataList = SpaceAlphabet.ReplacementDictionary[Alphabets[i]];
					// Load the vData from the folder.
					if (GUILayout.Button("Open Folder", GUILayout.Width(150), GUILayout.Height(17))) {
						// Open folder.
						string path = EditorUtility.OpenFolderPanel("Load Folder", "", "");
						if (path != string.Empty) {
							// First clear all origin volumeDatas.
							vDataList.Clear();
							// Get the files.
							foreach (var file in Directory.GetFiles(path)) {
								if (Regex.IsMatch(file, @".*[\\\/].*_vData\.asset$")) {
									vDataList.Add(CrevoxOperation.GetVolumeData(file.Replace(Environment.CurrentDirectory.Replace('\\', '/') + "/", "")));
								}
							}
						}
					}
					// Button of adding vData.
					if (GUILayout.Button("Add New vData", GUILayout.Width(150), GUILayout.Height(20))){
						vDataList.Add(null);
					}
					// Buttons.
					for (int j = 0; j < vDataList.Count; j++) {
						EditorGUILayout.BeginHorizontal();
						// Object field.
						vDataList[j] = (CreVox.VolumeData)EditorGUILayout.ObjectField(vDataList[j], typeof(CreVox.VolumeData), false, GUILayout.Height(17));
						// Button of deleting vData.
						if (GUILayout.Button("Delete vData", GUILayout.Height(17))) {
							vDataList.RemoveAt(j);
						}
						EditorGUILayout.EndHorizontal();
					}
				}
			}
			EditorGUILayout.EndScrollView();
		}
		private void UpdatePaletteWindow() {
			CreVox.PaletteWindow window = EditorWindow.GetWindow<CreVox.PaletteWindow>();
			window.InitialPaletteWindow();
		}
	}
}
