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
		private static Dictionary<Guid, List<VolumeData>> _refrenceTable = new Dictionary<Guid, List<VolumeData>>();

		private static List<List<VolumeData>> sameVolumeDatas = new List<List<VolumeData>>();
		public static List<Guid> AlphabetIDs {
			get { return _alphabetIDs; }
			set { _alphabetIDs = value; }
		}
		public static Dictionary<Guid, List<VolumeData>> RefrenceTable {
			get { return _refrenceTable; }
			set { _refrenceTable = value; }
		}
		public static List<List<VolumeData>> SameVolumeDatas {
			get { return sameVolumeDatas; }
			set { sameVolumeDatas = value; }
		}
		public static void InitialTable() {
			_refrenceTable = new Dictionary<Guid, List<VolumeData>>();
			for (int i = 0; i < _alphabetIDs.Count; i++) {
				_refrenceTable[_alphabetIDs[i]] = sameVolumeDatas[i];
			}
		}
		// Generate the volume data that refer graph grammar.
		public static void Generate() {
			// Get root.
			CreVoxNode root = CreVoxAttach.RootNode;
			// Initial root.
			Volume volume = CrevoxOperation.InitialVolume(SelectData(_refrenceTable[root.AlphabetID]));
			GenerateRecursion(root, volume);
			// Update volume manager and scene.
			CrevoxOperation.RefreshVolume();
		}
		// Dfs generate.
		private static void GenerateRecursion(CreVoxNode node, Volume volumeOrigin) {
			foreach (var child in node.Children) {
				Volume volume = CrevoxOperation.CreateVolumeObject(SelectData(_refrenceTable[child.AlphabetID]));
				if (CrevoxOperation.CombineVolumeObject(volumeOrigin, volume)) {
						GenerateRecursion(child, volume);
				}else {
					MonoBehaviour.DestroyImmediate(volume.gameObject);
					Debug.Log("Error");
				}
			}
		}
		// [TEST] Will delete.
		public static void RandomGenerate(int count) {
			List<Volume> vols = new List<Volume>();
			Volume volume = CrevoxOperation.InitialVolume(SelectData(sameVolumeDatas[UnityEngine.Random.Range(0, sameVolumeDatas.Count)]));
			vols.Add(volume);
			while (--count > 0) {
				volume = CrevoxOperation.AddAndCombineVolume(volume, SelectData(sameVolumeDatas[UnityEngine.Random.Range(0, sameVolumeDatas.Count)]));
				if (volume != null) {
					vols.Add(volume);
				} else {
					volume = vols[UnityEngine.Random.Range(0, vols.Count)];
				}
			}
			CrevoxOperation.RefreshVolume();
		}
		// Random select from multi vDatas.
		private static VolumeData SelectData(List<VolumeData> sameRoomData) {
				return sameRoomData[(int)UnityEngine.Random.Range(0, sameRoomData.Count)];
		}

	}
}
