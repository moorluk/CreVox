using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.IO;
using System;
using CreVox;

namespace CrevoxExtend {
	public class SpaceAlphabetWindow : EditorWindow {
		private Vector2 scrollPosition = new Vector2(0, 0);
		private List<string> Alphabets { get; set; }
		private bool AlphabetIsChanged { get; set; }
		// Togglers of the replacement alphabet.
		private List<bool> IsSelected { get; set; }

		void Initialize() {
			// Load files and set Alphabets.
			SpaceAlphabet.Load();
			// Copy a new alphabet.
			Alphabets = new List<string>(SpaceAlphabet.ReplacementAlphabet);
			AlphabetIsChanged = false;
			// Toggler of the connections.
			IsSelected = new List<bool>(new bool[Alphabets.Count]);
		}
		void Awake() {
			Initialize();
		}
		void OnGUI() {
			if (GUILayout.Button("Initialize")) {
				Initialize();
			}
			if (GUILayout.Button("Export")) {
				string path = EditorUtility.SaveFilePanel("Export xml", "", "SpaceAlphabet.xml", "xml");
				if (path != string.Empty) {
					SpaceAlphabetXML.Serialize.SerializeToXml(path);
				}
			}
			if (GUILayout.Button("Import")) {
				string path = EditorUtility.OpenFilePanel("Import xml", "", "xml");
				if (path != string.Empty) {
					Initialize();
					SpaceAlphabetXML.Unserialize.UnserializeFromXml(path);
				}
				// Refresh another window.
				UpdatePaletteWindow();
			}
			// Aphabets list.//
			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(Screen.width), GUILayout.Height(Screen.height - 150));
			Color def = GUI.color;
			for (int i = 0; i < Alphabets.Count; i++) {
				// Check if this alphabet is Saved or not.
				string currentName = Alphabets [i];
				bool isExistInAlphabet = SpaceAlphabet.ReplacementDictionary.ContainsKey (Alphabets [i]);

				GUI.color = Color.gray;
				using (var h = new EditorGUILayout.HorizontalScope (EditorStyles.helpBox, GUILayout.Width (Screen.width - 12f))) {
					GUI.color = def;

					// If alphabet not Saved, disable setting button.
					EditorGUI.BeginDisabledGroup (!isExistInAlphabet);
					IsSelected [i] = EditorGUILayout.Foldout (isExistInAlphabet ? IsSelected [i] : false, "");
					EditorGUI.EndDisabledGroup ();
					currentName = EditorGUILayout.TextField (currentName, GUILayout.Width (Screen.width - 221f));
					// If current name of alphabet symbol is change, update the name.
					if (currentName != Alphabets [i] && !Alphabets.Exists (a => a == currentName)) {
						Alphabets [i] = currentName;
					}

					// delete button.
					if (GUILayout.Button ("Delete Alphabet", GUILayout.Width (110f))) {
						Alphabets.RemoveAt (i);
						IsSelected.RemoveAt (i);
						isExistInAlphabet = false;
					}
				}
				// If this alphabet is exist in Last Saved version, then can check if setting is clicked.
				// If setting button is switched to true. Open the UI.

				if (isExistInAlphabet && IsSelected [i]) {
					var vDataList = SpaceAlphabet.ReplacementDictionary [Alphabets [i]];
					// Load the vData from the folder.
					using (var h = new EditorGUILayout.HorizontalScope ()) {
						EditorGUILayout.LabelField ("", GUILayout.Width (11f));
						if (GUILayout.Button ("Open Folder", GUILayout.Width (Screen.width - 42f))) {
							// Open folder.
							string path = EditorUtility.OpenFolderPanel ("Load Folder", "", "");
							if (path != string.Empty) {
								// First clear all origin volumeDatas.
								vDataList.Clear ();
								// Get the files.
								foreach (var file in Directory.GetFiles(path)) {
									if (Regex.IsMatch (file, @".*[\\\/].*_vData\.asset$")) {
										vDataList.Add (CrevoxOperation.GetVolumeData (file.Replace (Environment.CurrentDirectory.Replace ('\\', '/') + "/", "")));
									}
								}
							}
						}
					}
					EditorGUI.indentLevel++;
					// Buttons.
					for (int j = 0; j < vDataList.Count; j++) {
						using (var h = new EditorGUILayout.HorizontalScope ()) {
							// Object field.
							vDataList [j] = (VolumeData)EditorGUILayout.ObjectField (vDataList [j], typeof(VolumeData), false, GUILayout.Width (Screen.width - 141f), GUILayout.Height (17));
							// Button of deleting vData.
							if (GUILayout.Button ("Delete vData", GUILayout.Width (110f), GUILayout.Height (17))) {
								vDataList.RemoveAt (j);
							}
						}
					}
					using (var h = new EditorGUILayout.HorizontalScope ()) {
						// Button of adding vData.
						EditorGUILayout.LabelField ("-------->", EditorStyles.objectField, GUILayout.Width (Screen.width - 141f), GUILayout.Height (17));
						if (GUILayout.Button ("Add New vData", GUILayout.Width (110f), GUILayout.Height (17))) {
							vDataList.Add (null);
						}
					}
					GUILayout.Space (4.0f);
					EditorGUI.indentLevel--;
				}
			}
			EditorGUILayout.EndScrollView();
			string _msg;
			if (!AlphabetIsChanged) {
				AlphabetIsChanged = SpaceAlphabet.ReplacementAlphabet.Exists(a => (!Alphabets.Exists(newA => (newA == a)))) || Alphabets.Exists(newA => (!SpaceAlphabet.ReplacementAlphabet.Exists(a => (newA == a))));
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
				IsSelected.Add(false);
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
			PaletteWindow window = EditorWindow.GetWindow<PaletteWindow>();
			window.InitialPaletteWindow();
		}
	}
}
