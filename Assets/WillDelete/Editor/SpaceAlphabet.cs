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
	public class SpaceAlphabet {
		public static Dictionary<string, List<VolumeData>> replaceDictionary = new Dictionary<string, List<VolumeData>>();
		private static string _path = Environment.CurrentDirectory + @"\Assets\Resources\CreVox\VolumeArtPack\LevelPieces\4_Sign\ConnectionTypes/";
		private static string prefabRegex = @".*[\\\/]Connection_(\w+)\.prefab$";
		private static List<string> alphabets = new List<string>();
		private static bool changed = true;

		public static void Load() {
			alphabets = new List<string>();
			string[] files = Directory.GetFiles(_path);
			string matchFile;
			for (int i = 0; i < files.Length; i++) {
				if (Regex.IsMatch(files[i], prefabRegex)) {
					matchFile = Regex.Match(files[i], prefabRegex).Groups[1].Value;
					if (!alphabets.Exists(a => (a == matchFile))) {
						alphabets.Add(matchFile);
					}
				}
			}
		}
		public static void alphabetUpdate(List<string> newAlphabet) {
			Load();
			foreach (string s in newAlphabet) {
				if (!alphabets.Exists(e => (e == s))) {
					NewPrefab("Connection_" + s);
				}
			}

			for(int i = alphabets.Count-1;i >= 0; i--) {
				if (!newAlphabet.Exists(e => (e == alphabets[i]))) {
					DeletePrefab("Connection_"+alphabets[i]);
					alphabets.RemoveAt(i);
				}
			}
			PaletteWindow window = EditorWindow.GetWindow<PaletteWindow>();
			window.InitialPaletteWindow();
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
		public static bool Changed {
			get { return changed; }
			set { changed = value; }
		}
	}
}
