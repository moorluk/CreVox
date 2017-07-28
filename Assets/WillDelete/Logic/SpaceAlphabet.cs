using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using CreVox;

namespace CrevoxExtend {
	public static class SpaceAlphabet {
		private static readonly string _connectionRegex = @"Connection_(\w+)$";
		// .
		private static readonly string _systemPath = PathCollect.pieces + "/2_System/";
		// Connection names.
		public static List<string> ReplacementAlphabet { get; private set; }
		// 
		public static Dictionary<string, List<VolumeData>> ReplacementDictionary;

		// Constructor.
		static SpaceAlphabet() {
			Debug.Log(_systemPath);
			ReplacementAlphabet = new List<string>();
			ReplacementDictionary = new Dictionary<string, List<VolumeData>>() {
				{ "Default", new List<VolumeData>() }
			};
		}

		// Load connectionType from folder
		public static void Load() {
			// Clear the connection name list.
			ReplacementAlphabet.Clear();
			// Get the file list from the artpack folder.
			var files = Resources.LoadAll(_systemPath, typeof(GameObject));
			// Add the connections from the specific path.
			foreach (var file in files) {
				var match = Regex.Match(file.name, _connectionRegex);
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
//			foreach (string s in newAlphabet) {
//				if (!ReplacementAlphabet.Exists(e => (e == s))) {
//					NewPrefab("Connection_" + s);
//				}
//			}
//
//			for (int i = ReplacementAlphabet.Count - 1; i >= 0; i--) {
//				if (!newAlphabet.Exists(e => (e == ReplacementAlphabet[i]))) {
//					DeletePrefab("Connection_" + ReplacementAlphabet[i]);
//				}
//			}
			Load();
		}
		// File IO
//		private static void NewPrefab(string fileName) {
//			#if UNITY_EDITOR
//			UnityEditor.AssetDatabase.CopyAsset(@"Assets\Resources\SpaceAlphabet_DefaultConnection\Connection_Default.prefab", @"Assets\Resources\" + _systemPath + fileName + ".prefab");
//			//File.Copy(ArtPackPath + "Connection_Default.prefab", ArtPackPath + fileName + ".prefab");
//			//AssetDatabase.ImportAsset(@"Assets\Resources\CreVox\VolumeArtPack\LevelPieces\2_System/" + fileName + ".prefab");
//			PaletteItem pt = GetPrefab(fileName).GetComponent<PaletteItem>();
//			pt.itemName = fileName;
//			#endif
//		}
//		private static void DeletePrefab(string fileName) {
//			#if UNITY_EDITOR
//			UnityEditor.AssetDatabase.DeleteAsset(@"Assets\Resources\" + _systemPath + fileName + ".prefab");
//			#endif
//		}
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
			return (UnityEditor.AssetDatabase.LoadAssetAtPath(@"Assets\Resources\" + _systemPath + name + ".prefab" ,typeof(GameObject))) as GameObject;
		}

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
