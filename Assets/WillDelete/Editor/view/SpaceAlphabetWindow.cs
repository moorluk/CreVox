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
		private string regex = @".*[\\\/].*_vData\.asset$";
		private List<string> alphabets;
		private Vector2 scrollPosition = new Vector2(0, 0);
		private bool alphabetIsChanged;
		void Initialize() {
			// Load files and set alphabets.
			SpaceAlphabet.Load();
			alphabets = new List<string>(SpaceAlphabet.Alphabets);
			alphabetIsChanged = false;
		}
		void Awake() {
			Initialize();
		}
		public void UpdatePaletteWindow() {
			PaletteWindow window = EditorWindow.GetWindow<PaletteWindow>();
			window.InitialPaletteWindow();
		}
		void OnFocus() {
			SpaceAlphabet.SetPath(PaletteWindow.GetLevelPiecePath());
		}
		void OnGUI() {
			if (SpaceAlphabet._isChanged) {
				SpaceAlphabet._isChanged = false;
				Initialize();
			}
			if (GUILayout.Button("Export")) {
				string path = EditorUtility.SaveFilePanel("Export xml", "", "SpaceAlphabet.xml", "xml");
				if (path != "") {
					SpaceAlphabetXML.Serialize.SerializeToXml(path);
				}
			}
			if (GUILayout.Button("Import")) {
				string path = EditorUtility.OpenFilePanel("Import xml", "", "xml");
				if (path != "") {
					SpaceAlphabetXML.Unserialize.UnserializeFromXml(path);
				}
				UpdatePaletteWindow();
			}
			// Aphabets list.//
			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(Screen.width), GUILayout.Height(Screen.height - 150));
			for (int i = 0; i < alphabets.Count; i++) {
				// Check if this alphabet is Saved or not.
				string currentName = alphabets[i];
				bool isExistInAlphabet = SpaceAlphabet.Alphabets.Exists(a => (a == alphabets[i]));
				EditorGUILayout.BeginHorizontal();
				currentName = EditorGUILayout.TextField(currentName, GUILayout.Width(Screen.width * 0.47f), GUILayout.Height(17));
				// [WARNING] name cannot be the same.
				if (currentName != alphabets[i] && !alphabets.Exists(a => a == currentName)) {
					alphabets[i] = currentName;
				}

				// If alphabet not Saved, disable setting button.
				EditorGUI.BeginDisabledGroup(!isExistInAlphabet);
				if (GUILayout.Button("Setting", GUILayout.Width(Screen.width * 0.27f), GUILayout.Height(17))) {
					// Switch setting between 0/1.
					SpaceAlphabet.isSelected[i] = !SpaceAlphabet.isSelected[i];
				}
				EditorGUI.EndDisabledGroup();

				// delete button.
				if (GUILayout.Button("Delete_Alphabet", GUILayout.Width(Screen.width * 0.23f), GUILayout.Height(17))) {
					alphabets.RemoveAt(i);
					isExistInAlphabet = false;
				}
				EditorGUILayout.EndHorizontal();
				// If this alphabet is exist in Last Saved version, then can check if setting is clicked.
				if (isExistInAlphabet) {
					// If setting button is switched to true. Open the UI.
					if (SpaceAlphabet.isSelected[i]) {
						EditorGUILayout.BeginVertical();
						// [open window]
						if (GUILayout.Button("Open Folder", GUILayout.Width(150), GUILayout.Height(17))) {
							// Open folder.
							string path = EditorUtility.OpenFolderPanel("Load Folder", "", "");
							if (path != "") {
								// First clear all origin volumeDatas.
								SpaceAlphabet.replaceDictionary[alphabets[i]].Clear();
								// Get the files.
								string[] files = Directory.GetFiles(path);
								for (int j = 0; j < files.Length; j++) {
									if (Regex.IsMatch(files[j], regex)) {
										SpaceAlphabet.replaceDictionary[alphabets[i]].Add(CrevoxOperation.GetVolumeData(files[j].Replace(Environment.CurrentDirectory.Replace('\\', '/') + "/", "")));
									}
								}
								// if not find match vData, default null.
								if (SpaceAlphabet.replaceDictionary[alphabets[i]].Count == 0) {
									SpaceAlphabet.replaceDictionary[alphabets[i]].Add(null);
								}
							}
						}
						// Buttons.
						for (int j = 0; j < SpaceAlphabet.replaceDictionary[alphabets[i]].Count; j++) {
							EditorGUILayout.BeginHorizontal();
							// Object field.
							SpaceAlphabet.replaceDictionary[alphabets[i]][j] = (VolumeData)EditorGUILayout.ObjectField(SpaceAlphabet.replaceDictionary[alphabets[i]][j], typeof(VolumeData), false, GUILayout.Width(300), GUILayout.Height(17));
							// Delete button.
							if (GUILayout.Button("Delete_vData", GUILayout.Width(130), GUILayout.Height(17))) {
								SpaceAlphabet.replaceDictionary[alphabets[i]].RemoveAt(j);
							}
							EditorGUILayout.EndHorizontal();
						}
						// Add button.
						if (GUILayout.Button("Add New vData", GUILayout.Width(150), GUILayout.Height(20))) {
							SpaceAlphabet.replaceDictionary[alphabets[i]].Add(null);
						}
						EditorGUILayout.EndVertical();
					}
				}
			}
			EditorGUILayout.EndScrollView();
			string _msg;
			if (!alphabetIsChanged) {
				alphabetIsChanged = SpaceAlphabet.Alphabets.Exists(a => (!alphabets.Exists(newA => (newA == a)))) || alphabets.Exists(newA => (!SpaceAlphabet.Alphabets.Exists(a => (newA == a))));
			}
			_msg = alphabetIsChanged ? "Space alphabet has changed press 'Save' to save your changes." : "none";

			GUILayout.BeginArea(new Rect(0, Screen.height - 100, Screen.width, 30));
			EditorGUILayout.HelpBox(_msg, MessageType.Info);
			GUILayout.EndArea();
			// Alphabet add button.
			GUILayout.BeginArea(new Rect(0, Screen.height - 70, Screen.width, 60));
			if (GUILayout.Button("Add New")) {
				// Name Cannot be the same.
				int Count = alphabets.Count;
				while (alphabets.Exists(a => (a == ("NewConnection" + Count)))) {
					Count++;
				}
				alphabets.Add("NewConnection" + Count);
			}
			// Saving this alphabet's "types".
			if (GUILayout.Button("Save")) {
				alphabetIsChanged = false;
				// Update alphabet types.//
				SpaceAlphabet.alphabetUpdate(alphabets);
				UpdatePaletteWindow();
			}
			GUILayout.EndArea();
		}
	}
}
