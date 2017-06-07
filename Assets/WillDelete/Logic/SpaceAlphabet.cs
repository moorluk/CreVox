using UnityEngine;
using CreVox;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CrevoxExtend {
	public class SpaceAlphabet {
		public static Dictionary<string, List<VolumeData>> replaceDictionary = new Dictionary<string, List<VolumeData>>() { { "Default", new List<VolumeData>() } };
		private static string _path = Environment.CurrentDirectory + @"\Assets\Resources\CreVox\VolumeArtPack\LevelPieces\2_System/";
		private static string prefabRegex = @".*[\\\/]Connection_(\w+)\.prefab$";
		private static List<string> alphabets = new List<string>();
		public static List<bool> isSelected = new List<bool>() { false };

#if UNITY_EDITOR
		public static void Load() {
			isSelected = new List<bool>() { false };
			alphabets = new List<string>() { "Default" };
			string[] files = Directory.GetFiles(_path);
			string matchFile;
			for (int i = 0; i < files.Length; i++) {
				if (Regex.IsMatch(files[i], prefabRegex)) {
					matchFile = Regex.Match(files[i], prefabRegex).Groups[1].Value;
					if (!alphabets.Exists(a => (a == matchFile))) {
						alphabets.Add(matchFile);
						isSelected.Add(false);
					}
				}
			}
			dictionaryUpdate();
		}
		public static void alphabetUpdate(List<string> newAlphabet) {
			//Load();
			foreach (string s in newAlphabet) {
				if (!alphabets.Exists(e => (e == s))) {
					NewPrefab("Connection_" + s);
				}
			}

			for(int i = alphabets.Count-1;i >= 0; i--) {
				if (!newAlphabet.Exists(e => (e == alphabets[i]))) {
					DeletePrefab("Connection_" + alphabets[i]);
				}
			}
			Load();
		}
		// Update dictionary.
		public static void dictionaryUpdate() {
			foreach (var connectionType in alphabets) {
				if (!replaceDictionary.ContainsKey(connectionType)) {
					replaceDictionary.Add(connectionType, new List<VolumeData>());
				}
			}
			for (int i = replaceDictionary.Keys.Count - 1; i >= 0 ; i--) {
				if (!alphabets.Exists(s => (s == replaceDictionary.Keys.ToArray()[i]))) {
					replaceDictionary.Remove(replaceDictionary.Keys.ToArray()[i]);
				}
			}
		}
		// File IO
		public static void NewPrefab(string fileName) {
			if(!File.Exists(_path + fileName + ".prefab")) {
				File.Copy(_path + "Connection_Default.prefab", _path + fileName + ".prefab");
				AssetDatabase.ImportAsset(@"Assets\Resources\CreVox\VolumeArtPack\LevelPieces\4_Sign\ConnectionTypes/" + fileName + ".prefab");
				GetPrefab(fileName).GetComponent<PaletteItem>().itemName = fileName;
			}
		}
		public static void DeletePrefab(string fileName) {
			AssetDatabase.DeleteAsset(@"Assets\Resources\CreVox\VolumeArtPack\LevelPieces\4_Sign\ConnectionTypes/" + fileName + ".prefab");
		}
		public static List<string> Alphabets {
			get { return alphabets; }
		}
		private static GameObject GetPrefab(string name) {
			return (Resources.Load(@"CreVox/VolumeArtPack/LevelPieces/4_Sign/ConnectionTypes/"+name) as GameObject);
		}
#endif
	}
}
