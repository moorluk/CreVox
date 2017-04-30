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
		private static int Counter = 0;
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
				EditorGUILayout.BeginHorizontal();
				alphabets[i] = EditorGUILayout.TextField(alphabets[i]);
				if (GUILayout.Button("Setting")) {
					// [open window]
					EditorWindow.GetWindow<CrevoxExtend.SpacevDataWindow>(alphabets[i], true);
					/*Somthing here*/
				}
				if (i > 0) {
					if (GUILayout.Button("Delete")) {
						alphabets.RemoveAt(i);
					}
				}
				EditorGUILayout.EndHorizontal();
			}
			if (GUILayout.Button("Add New")) {
				alphabets.Add("NewConnection"+ (Counter++));
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
			foreach(var key in SpaceAlphabet.replaceDictionary.Keys) {
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
	public class SpacevDataWindow : EditorWindow {
		private string regex = @".*[\\\/].*_vData\.asset$";
		void OnGUI() {
			EditorGUILayout.PrefixLabel(this.titleContent.text);
			if (GUILayout.Button("Open Folder")) {
				// First clear all origin volumeDatas.
				SpaceAlphabet.replaceDictionary[this.titleContent.text].Clear();
				// Open folder.
				string path = EditorUtility.OpenFolderPanel("Load Folder", "", "");
				if (path != "") {
					// Get the files.
					string[] files = Directory.GetFiles(path);
					for (int i = 0; i < files.Length; i++) {
						if (Regex.IsMatch(files[i], regex)) {
							SpaceAlphabet.replaceDictionary[this.titleContent.text].Add(CrevoxOperation.GetVolumeData(files[i].Replace(Environment.CurrentDirectory.Replace('\\', '/') + "/", "")));
						}
					}
					// if not find match vData, default null.
					if (SpaceAlphabet.replaceDictionary[this.titleContent.text].Count == 0) {
						SpaceAlphabet.replaceDictionary[this.titleContent.text].Add(null);
					}
				}
			}
			for (int i = 0;i < SpaceAlphabet.replaceDictionary[this.titleContent.text].Count;i++) { 
				EditorGUILayout.BeginHorizontal();
				SpaceAlphabet.replaceDictionary[this.titleContent.text][i] = (VolumeData)EditorGUILayout.ObjectField(SpaceAlphabet.replaceDictionary[this.titleContent.text][i], typeof(VolumeData), false, GUILayout.Width(Screen.width / 2 - 10));
				if (GUILayout.Button("Delete")) {
					SpaceAlphabet.replaceDictionary[this.titleContent.text].RemoveAt(i);
				}
				EditorGUILayout.EndHorizontal();
			}
			if (GUILayout.Button("Add New vData")) {
				SpaceAlphabet.replaceDictionary[this.titleContent.text].Add(null);
			}
		}
	}
}

