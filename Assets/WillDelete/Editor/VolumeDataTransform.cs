using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CreVox;
using MissionGrammarSystem;
using System;
using System.Linq;

namespace CrevoxExtend {
	public class VolumeDataTransform {
		private static List<Guid> _alphabetIDs = new List<Guid>();
		private static List<VolumeData> _volumeDatas = new List<VolumeData>();
		private static Dictionary<Guid, VolumeData> _refrenceTable = new Dictionary<Guid, VolumeData>();
		public static List<Guid> AlphabetIDs {
			get { return _alphabetIDs; }
			set { _alphabetIDs = value; }
		}
		public static List<VolumeData> VolumeDatas {
			get { return _volumeDatas; }
			set { _volumeDatas = value; }
		}
		public static Dictionary<Guid, VolumeData> RefrenceTable {
			get { return _refrenceTable; }
			set { _refrenceTable = value; }
		}
		public static void InitialTable() {
			_refrenceTable = new Dictionary<Guid, VolumeData>();
			for (int i = 0; i < _alphabetIDs.Count; i++) {
				_refrenceTable[_alphabetIDs[i]] = _volumeDatas[i];
			}
		}
		// Generate the volume data that refer graph grammar.
		public static void Generate() {
			// Get root.
			RewriteSystem.CreVoxNode root = RewriteSystem.CreVoxAttach.RootNode;
			// Initial root.
			Volume volume = CrevoxOperation.InitialVolume(_refrenceTable[root.AlphabetID]);
			GenerateRecursion(root, volume);
			// Update volume manager and scene.
			CrevoxOperation.RefreshVolume();
		}
		// Dfs generate.
		private static bool GenerateRecursion(RewriteSystem.CreVoxNode node, Volume volumeOrigin) {
			foreach (var child in node.Children) {
				Volume volume = CrevoxOperation.CreateVolumeObject(_refrenceTable[child.AlphabetID]);
				do {
					if (CrevoxOperation.CombineVolumeObject(volumeOrigin, volume)) {
						return false;
					}
				} while (!GenerateRecursion(child, volume));
			}
			return true;
		}
		// [TEST] Will delete.
		public static void RandomGenerate(int count) {
			List<Volume> vols = new List<Volume>();
			Volume volume = CrevoxOperation.InitialVolume(_volumeDatas[UnityEngine.Random.Range(0, _volumeDatas.Count)]);
			vols.Add(volume);
			while (--count > 0) {
				volume = CrevoxOperation.AddAndCombineVolume(volume, _volumeDatas[UnityEngine.Random.Range(0, _volumeDatas.Count)]);
				if (volume != null) {
					vols.Add(volume);
				} else {
					volume = vols[UnityEngine.Random.Range(0, vols.Count)];
				}
			}
			CrevoxOperation.RefreshVolume();
		}

	}
}
