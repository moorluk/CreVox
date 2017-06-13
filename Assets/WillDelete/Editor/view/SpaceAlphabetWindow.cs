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
		private static bool AlphabetIsChanged { get; set; }
		private static string ArtPackPath { get; set; }
		void Initialize() {
			// Load files and set Alphabets.
			SpaceAlphabet.Load();
			Alphabets = new List<string>(SpaceAlphabet.Alphabets);
			AlphabetIsChanged = false;
		}
		void Awake() {
			Initialize();
		}
		void OnFocus() {
			// Update the art pack path and initialize the window when the path is changed.
			if (ArtPackPath != CreVox.PaletteWindow.GetLevelPiecePath()) {
				ArtPackPath = CreVox.PaletteWindow.GetLevelPiecePath();
				SpaceAlphabet.SetPath(ArtPackPath);
				Initialize();
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
				// Refresh another window.
				UpdatePaletteWindow();
			}
			// Aphabets list.//
			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(Screen.width), GUILayout.Height(Screen.height - 150));
			for (int i = 0; i < Alphabets.Count; i++) {
				// Check if this alphabet is Saved or not.
				string currentName = Alphabets[i];
				bool isExistInAlphabet = SpaceAlphabet.Alphabets.Exists(a => (a == Alphabets[i]));

				EditorGUILayout.BeginHorizontal();
				currentName = EditorGUILayout.TextField(currentName, GUILayout.Width(Screen.width * 0.47f), GUILayout.Height(17));
				// If current name of alphabet symbol is change, update the name.
				if (currentName != Alphabets[i] && ! Alphabets.Exists(a => a == currentName)) {
					Alphabets[i] = currentName;
				}

				// If alphabet not Saved, disable setting button.
				EditorGUI.BeginDisabledGroup(! isExistInAlphabet);
				if (GUILayout.Button("Setting", GUILayout.Width(Screen.width * 0.27f), GUILayout.Height(17))) {
					// Switch setting between 0/1.
					SpaceAlphabet.isSelected[i] = ! SpaceAlphabet.isSelected[i];
				}
				EditorGUI.EndDisabledGroup();

				// delete button.
				if (GUILayout.Button("Delete Alphabet", GUILayout.Width(Screen.width * 0.23f), GUILayout.Height(17))) {
					Alphabets.RemoveAt(i);
					isExistInAlphabet = false;
				}
				EditorGUILayout.EndHorizontal();
				// If this alphabet is exist in Last Saved version, then can check if setting is clicked.
				if (isExistInAlphabet) {
					// If setting button is switched to true. Open the UI.
					if (SpaceAlphabet.isSelected[i]) {
						var vDataList = SpaceAlphabet.replaceDictionary[Alphabets[i]];
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
						// Buttons.
						for (int j = 0; j < vDataList.Count; j++) {
							EditorGUILayout.BeginHorizontal();
							// Object field.
							vDataList[j] = (CreVox.VolumeData) EditorGUILayout.ObjectField(vDataList[j], typeof(CreVox.VolumeData), false, GUILayout.Height(17));
							// Button of deleting vData.
							if (GUILayout.Button("Delete vData", GUILayout.Height(17))) {
								vDataList.RemoveAt(j);
							}
							EditorGUILayout.EndHorizontal();
						}
						// Button of adding vData.
						if (GUILayout.Button("Add New vData", GUILayout.Width(150), GUILayout.Height(20))) {
							vDataList.Add(null);
						}
					}
				}
			}
			EditorGUILayout.EndScrollView();
			string _msg;
			if (!AlphabetIsChanged) {
				AlphabetIsChanged = SpaceAlphabet.Alphabets.Exists(a => (!Alphabets.Exists(newA => (newA == a)))) || Alphabets.Exists(newA => (!SpaceAlphabet.Alphabets.Exists(a => (newA == a))));
			}
			_msg = AlphabetIsChanged ? "Space alphabet has changed press 'Save' to save your changes." : "none";

			GUILayout.BeginArea(new Rect(0, Screen.height - 100, Screen.width, 30));
			EditorGUILayout.HelpBox(_msg, MessageType.Info);
			GUILayout.EndArea();
			// Alphabet add button.
			GUILayout.BeginArea(new Rect(0, Screen.height - 70, Screen.width, 60));
			if (GUILayout.Button("Add New")) {
				// Name Cannot be the same.
				int Count = Alphabets.Count;
				while (Alphabets.Exists(a => (a == ("NewConnection" + Count)))) {
					Count++;
				}
				Alphabets.Add("NewConnection" + Count);
			}
			// Saving this alphabet's "types".
			if (GUILayout.Button("Save")) {
				AlphabetIsChanged = false;
				// Update alphabet types.//
				SpaceAlphabet.alphabetUpdate(Alphabets);
				UpdatePaletteWindow();
			}
			GUILayout.EndArea();
		}
		private void UpdatePaletteWindow() {
			CreVox.PaletteWindow window = EditorWindow.GetWindow<CreVox.PaletteWindow>();
			window.InitialPaletteWindow();
		}
	}
}
