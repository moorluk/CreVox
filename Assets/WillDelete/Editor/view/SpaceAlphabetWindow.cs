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

		void Initialize() {
			SpaceAlphabet.Load();
			alphabets = SpaceAlphabet.Alphabets;
		}
		void Awake() {
			Initialize();
		}
		void OnGUI() {
			// Aphabets list.
			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(Screen.width), GUILayout.Height(Screen.height * 0.75f));
			for (int i = 0; i < alphabets.Count; i++) {
				bool isExistInAlphabet = SpaceAlphabet.Alphabets.Exists(a => (a == alphabets[i]));
				EditorGUILayout.BeginHorizontal();
				alphabets[i] = EditorGUILayout.TextField(alphabets[i], GUILayout.Width(300), GUILayout.Height(17));
				EditorGUI.BeginDisabledGroup(!isExistInAlphabet);
				if (GUILayout.Button("Setting", GUILayout.Width(200), GUILayout.Height(17))) {
					SpaceAlphabet.isSelected[i] = !SpaceAlphabet.isSelected[i];
				}
				EditorGUI.EndDisabledGroup();

				if (i > 0) {
					if (GUILayout.Button("Delete", GUILayout.Width(180), GUILayout.Height(17))) {
						alphabets.RemoveAt(i);
						isExistInAlphabet = false;
					}
				}
				EditorGUILayout.EndHorizontal();
				if (isExistInAlphabet) {
					if (SpaceAlphabet.isSelected[i]) {
						EditorGUILayout.BeginVertical();
						// [open window]
						if (GUILayout.Button("Open Folder", GUILayout.Width(150), GUILayout.Height(17))) {
							// First clear all origin volumeDatas.
							SpaceAlphabet.replaceDictionary[alphabets[i]].Clear();
							// Open folder.
							string path = EditorUtility.OpenFolderPanel("Load Folder", "", "");
							if (path != "") {
								// Get the files.
								string[] files = Directory.GetFiles(path);
								for (int j = 0; i < files.Length; j++) {
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
						for (int j = 0; j < SpaceAlphabet.replaceDictionary[alphabets[i]].Count; j++) {
							EditorGUILayout.BeginHorizontal();
							SpaceAlphabet.replaceDictionary[alphabets[i]][j] = (VolumeData)EditorGUILayout.ObjectField(SpaceAlphabet.replaceDictionary[alphabets[i]][j], typeof(VolumeData), false, GUILayout.Width(300), GUILayout.Height(17));
							if (GUILayout.Button("Delete", GUILayout.Width(150), GUILayout.Height(17))) {
								SpaceAlphabet.replaceDictionary[alphabets[i]].RemoveAt(j);
							}
							EditorGUILayout.EndHorizontal();
						}
						if (GUILayout.Button("Add New vData", GUILayout.Width(150), GUILayout.Height(17))) {
							SpaceAlphabet.replaceDictionary[alphabets[i]].Add(null);
						}
						EditorGUILayout.EndVertical();
					}
				}
			}
			if (GUILayout.Button("Add New")) {
				alphabets.Add("NewConnection" + alphabets.Count);
			}
			if (GUILayout.Button("Save")) {
				SpaceAlphabet.alphabetUpdate(alphabets);
				SpaceAlphabet.Changed = true;
			}
			EditorGUILayout.EndScrollView();
		}
		public void InitializeV() {
			SpaceAlphabet.Load();
			foreach (var connectionType in alphabets) {
				if (!SpaceAlphabet.replaceDictionary.ContainsKey(connectionType)) {
					SpaceAlphabet.replaceDictionary.Add(connectionType, new List<VolumeData>());
				}
			}
			foreach (var key in SpaceAlphabet.replaceDictionary.Keys) {
				if (!alphabets.Exists(s => (s == key))) {
					SpaceAlphabet.replaceDictionary.Remove(key);
				}
			}
		}
		void OnFocus() {
			if (SpaceAlphabet.Changed) {
				SpaceAlphabet.Changed = false;
				InitializeV();
			}
		}
		public static VolumeData GetReplaceVData(string fileName) {
			return SpaceAlphabet.replaceDictionary[fileName][UnityEngine.Random.Range(0, SpaceAlphabet.replaceDictionary[fileName].Count)];
		}
	}
}