using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CreVox;
using MissionGrammarSystem;
using System;
using System.Linq;

namespace Test {
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
		public static Dictionary<Guid,VolumeData> RefrenceTable {
			get { return _refrenceTable; }
			set { _refrenceTable = value; }
		}
		public static void InitialTable() {
			_refrenceTable = new Dictionary<Guid, VolumeData>();
			for (int i = 0; i < _alphabetIDs.Count; i++) {
				_refrenceTable[_alphabetIDs[i]] = _volumeDatas[i];
			}
		}
		public static void Generate() {
			RewriteSystem.CreVoxNode root = RewriteSystem.CreVoxAttach.RootNode;
			Volume volume = AddOn.Initial(_refrenceTable[root.AlphabetID]);
			Recursion(root, volume);
			AddOn.RefreshVolume();
		}
		private static void Recursion(RewriteSystem.CreVoxNode node, Volume volumeOrigin) {
			foreach (var child in node.Children) {
				Volume volume = AddOn.AddAndCombineVolume(volumeOrigin, _refrenceTable[child.AlphabetID]);
				if (volume !=null) {
					Recursion(child, volume);
				} else {
					Debug.Log("Error.");
				}
			}
		}
		public static void RandomGenerate(int count) {
			List<Volume> vols = new List<Volume>();
			Volume volume = AddOn.Initial(_volumeDatas[UnityEngine.Random.Range(0, _volumeDatas.Count)]);
			vols.Add(volume);
			while (--count > 0) {
				volume = AddOn.AddAndCombineVolume(volume, _volumeDatas[UnityEngine.Random.Range(0, _volumeDatas.Count)]);
				if(volume != null) {
					vols.Add(volume);
				}else {
					volume = vols[UnityEngine.Random.Range(0, vols.Count)];
				}
			}
			AddOn.RefreshVolume();
		}

	}
}
