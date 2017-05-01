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
		private static List<CreVoxNode> _usedNode;
		// Generate the volume data that refer graph grammar.
		public static void Generate() {
			_usedNode = new List<CreVoxNode>();
			// Get root.
			CreVoxNode root = CreVoxAttach.RootNode;
			// Initial root.
			Volume volume = CrevoxOperation.InitialVolume(SelectData(_refrenceTable[root.AlphabetID]));
			_usedNode.Add(root);
			if (GenerateRecursion(root, volume)) {
				Debug.Log("Successful.");
				// Update volume manager and scene.
				CrevoxOperation.RefreshVolume();
			} else {
				Debug.Log("Error");
			}
		}
		// Dfs generate.
		private static bool GenerateRecursion(CreVoxNode node, Volume originalVolume) {
			foreach (var child in node.Children) {
				if (_usedNode.Exists(n => n.SymbolID == child.SymbolID)) {
					continue;
				}
				Volume newVolume = null;
				// Find the suitable vdata by random ordering.
				foreach (var vdata in _refrenceTable[child.AlphabetID].OrderBy(x => UnityEngine.Random.value)) {
					newVolume = CrevoxOperation.CreateVolumeObject(vdata);
					if (newVolume.GetComponent<VolumeExtend>().ConnectionInfos.Count - 1 >= child.Children.Count) {
						//Debug.Log("Find the count is " + newVolume.GetComponent<VolumeExtend>().ConnectionInfos.Count);
						break;
					} else {
						// Cannot connect, so delete it.
						MonoBehaviour.DestroyImmediate(newVolume.gameObject);
						newVolume = null;
					}
				}
				// No vdata have enough connection.
				if (newVolume == null) {
					Debug.Log("There is no vdata that have enough connection in " + _refrenceTable[child.AlphabetID][0].name + ". It means this graph  doesn't match with vdata.");
					return false;
				}
				// Combine.
				VolumeExtend originalVolumeExtend = originalVolume.GetComponent<VolumeExtend>();
				VolumeExtend newVolumeExtend = newVolume.GetComponent<VolumeExtend>();
				ConnectionInfo[] originalConnectionList = originalVolumeExtend.ConnectionInfos.OrderBy(x => UnityEngine.Random.value).ToArray();
				ConnectionInfo[] newConnectionList = newVolumeExtend.ConnectionInfos.OrderBy(x => UnityEngine.Random.value).ToArray();
				bool success = false;
				foreach (var newConnection in newConnectionList) {
					if (newConnection.used || newConnection.type != ConnectionInfoType.StartingNode) {
						continue;
					}
					foreach (var originalConnection in originalConnectionList) {
						if (originalConnection.used || originalConnection.type == ConnectionInfoType.StartingNode) {
							continue;
						}
						Debug.Log(originalVolume.name + " + " + newVolume.name);
						if (CrevoxOperation.CombineVolumeObject(originalVolume, newVolume, originalConnection, newConnection)) {
							_usedNode.Add(child);
							originalConnection.used = true;
							newConnection.used = true;
							if (GenerateRecursion(child, newVolume)) {
								success = true;
								break;
							} else {
								_usedNode.Remove(child);
								originalConnection.used = false;
								newConnection.used = false;
							}
						}
					}
					if (success) {
						break;
					}
				}
				if (!success) {
					Debug.Log("Destroy " + newVolume.name);
					MonoBehaviour.DestroyImmediate(newVolume.gameObject);
					return false;
				}
			}
			return true;
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
