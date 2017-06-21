#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using CreVox;

namespace CrevoxExtend {
	public static class SpaceAlphabet {
		private static readonly string _prefabRegex = @".*[\\\/]Connection_(\w+)\.prefab$";
		// .
		private static string ArtPackPath { get; set; }
		// Connection names.
		public static List<string> ReplacementAlphabet { get; private set; }
		// 
		public static Dictionary<string, List<VolumeData>> ReplacementDictionary;

		// Constructor.
		static SpaceAlphabet() {
			ReplacementAlphabet = new List<string>();
			ReplacementDictionary = new Dictionary<string, List<VolumeData>>() {
				{ "Default", new List<VolumeData>() }
			};
		}

#if UNITY_EDITOR
		public static void SetPath(string path) {
			if (ArtPackPath != path) {
				ArtPackPath = path;
				Debug.Log("Now: " + ArtPackPath);
			}
		}
		// 
		public static void Load() {
			// Clear the connection name list.
			ReplacementAlphabet.Clear();
			// Get the file list from the artpack folder.
			var files = Directory.GetFiles(Environment.CurrentDirectory + "/" + ArtPackPath + "/2_System/");
			// Add the connections from the specific path.
			foreach (var file in files) {
				var match = Regex.Match(file, _prefabRegex);
				if (match.Success) {
					ReplacementAlphabet.Add(match.Groups[1].Value);
				}
			}
			// Append the new ones and removed the old ones.
			// If already exist in the dictionary, nothing happend.
			DictionaryUpdate();
		}
		public static void alphabetUpdate(List<string> newAlphabet) {
			//Load();
			foreach (string s in newAlphabet) {
				if (!ReplacementAlphabet.Exists(e => (e == s))) {
					NewPrefab("Connection_" + s);
				}
			}

			for (int i = ReplacementAlphabet.Count - 1; i >= 0; i--) {
				if (!newAlphabet.Exists(e => (e == ReplacementAlphabet[i]))) {
					DeletePrefab("Connection_" + ReplacementAlphabet[i]);
				}
			}
			Load();
		}
		// File IO
		private static void NewPrefab(string fileName) {
			AssetDatabase.CopyAsset(@"Assets\Resources\SpaceAlphabet_DefaultConnection\Connection_Default.prefab", ArtPackPath + "/2_System/" + fileName + ".prefab");
			//File.Copy(ArtPackPath + "Connection_Default.prefab", ArtPackPath + fileName + ".prefab");
			//AssetDatabase.ImportAsset(@"Assets\Resources\CreVox\VolumeArtPack\LevelPieces\2_System/" + fileName + ".prefab");
			PaletteItem pt = GetPrefab(fileName).GetComponent<PaletteItem>();
			pt.itemName = fileName;
		}
		private static void DeletePrefab(string fileName) {
			AssetDatabase.DeleteAsset(ArtPackPath + "/2_System/" + fileName + ".prefab");
		}
		// Update dictionary.
		private static void DictionaryUpdate() {
			// 
			foreach (var connectionType in ReplacementAlphabet) {
				if (! ReplacementDictionary.ContainsKey(connectionType)) {
					ReplacementDictionary.Add(connectionType, new List<VolumeData>());
				}
			}
			// 
			foreach (var connection in ReplacementDictionary.Keys.ToList()) {
				if (! ReplacementAlphabet.Exists(c => c == connection)) {
					ReplacementDictionary.Remove(connection);
				}
			}

			// for (int i = ReplacementDictionary.Keys.Count - 1; i >= 0; i--) {
			// 	if (!ReplacementAlphabet.Exists(s => (s == ReplacementDictionary.Keys.ToArray()[i]))) {
			// 		ReplacementDictionary.Remove(ReplacementDictionary.Keys.ToArray()[i]);
			// 	}
			// }
		}
		private static GameObject GetPrefab(string name) {
			Debug.Log(ArtPackPath + "/2_System/" + name + ".prefab");
			return (AssetDatabase.LoadAssetAtPath(ArtPackPath + "/2_System/" + name + ".prefab" ,typeof(GameObject))) as GameObject;
		}
#endif

		// Load space alphabet from 'xmlPath'.
		public static void RuntimeGenerate(string xmlPath) {
			SpaceAlphabetXML.Unserialize.UnserializeFromXml(xmlPath);
		}
		// Return a random volumData from chosen replacement.
		public static VolumeData GetReplaceVData(string fileName) {
			return ReplacementDictionary[fileName][UnityEngine.Random.Range(0, ReplacementDictionary[fileName].Count)];
		}
	}
}
