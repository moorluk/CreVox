using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CreVox;
using MissionGrammarSystem;
using System;
using System.Linq;

namespace CrevoxExtend {
	public class VolumeDataTransform {
		private const int TIMEOUT_MILLISECOND = 5000;

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
		// [TEST BackTracking]
		struct Edge {
			public CreVoxNode start;
			public CreVoxNode end;
			public Edge(CreVoxNode s, CreVoxNode e) {
				this.start = s;
				this.end = e;
			}
		}
		private static List<Edge> edgeList;
		private static System.Diagnostics.Stopwatch testStopWatch;
		public static void GenerateTest() {
			CrevoxState.ConnectionInfoVdataTable = new Dictionary<VolumeData, List<ConnectionInfo>>();
			testStopWatch = System.Diagnostics.Stopwatch.StartNew();
			// Get root.
			CreVoxNode rootNode = CreVoxAttach.RootNode;
			edgeList = new List<Edge>();
			RecursionGetSeriesTest(rootNode);
			nowState = null;
			// Get mapping vdata.
			foreach (var rootVdata in _refrenceTable[rootNode.AlphabetID].OrderBy(x => UnityEngine.Random.value)) {
				CrevoxState state = new CrevoxState(rootVdata);
				state.VolumeDatasByID[rootNode.SymbolID] = state.ResultVolumeDatas[0];
				if(RecursionTest(state, 0)) {
					if (testStopWatch.ElapsedMilliseconds < TIMEOUT_MILLISECOND) {
						nowState = state;
					}
					break;
				}
			}
			if (nowState != null) {
				Debug.Log("Completed.");
				// Update volume manager and scene.
				CrevoxOperation.InitialVolume(nowState.ResultVolumeDatas);
			}else {
				Debug.Log("Failed.");
			}
			Debug.Log(testStopWatch.ElapsedMilliseconds + " ms");
			testStopWatch.Stop();
		}
		private static bool RecursionTest(CrevoxState state, int edgeIndex) {
			if (testStopWatch.ElapsedMilliseconds > TIMEOUT_MILLISECOND) { return true; }
			if (edgeIndex >= edgeList.Count) { return true; }
			Edge edge = edgeList[edgeIndex];
			// If end node is used.
			if (state.VolumeDatasByID.ContainsKey(edge.end.SymbolID)) {
				// Ignore.
				if (RecursionTest(state, edgeIndex + 1)) {
					return true;
				}
			}
			List<VolumeData> suitableVdata = new List<VolumeData>();
			// Find the suitable vdata by random ordering.
			foreach (var vdata in _refrenceTable[edge.end.AlphabetID].OrderBy(x => UnityEngine.Random.value)) {
				CrevoxState.VolumeDataEx newVolumeEx = new CrevoxState.VolumeDataEx(vdata);
				if (newVolumeEx.ConnectionInfos.Count - 1 >= edge.end.Children.Count) {
					suitableVdata.Add(vdata);
				}
			}
			// No vdata have enough connection.
			if (suitableVdata.Count == 0) {
				Debug.Log("There is no vdata that have enough connection in " + _refrenceTable[edge.end.AlphabetID][0].name + ". It means this graph  doesn't match with vdata.");
				return false;
			}
			// Find mapping vdata in table.
			foreach (var mappingVdata in suitableVdata) {
				state.VolumeDatasByID[edge.end.SymbolID] = new CrevoxState.VolumeDataEx(mappingVdata);
				// Get startingNode from end node.
				ConnectionInfo startingNode = state.VolumeDatasByID[edge.end.SymbolID].ConnectionInfos.Find(x => !x.used && x.type == ConnectionInfoType.StartingNode);
				// Get connection from start node.
				foreach (var connection in state.VolumeDatasByID[edge.start.SymbolID].ConnectionInfos.OrderBy(x => UnityEngine.Random.value)) {
					if (connection.used || connection.type != ConnectionInfoType.Connection) { continue; }
					if (state.CombineVolumeObject(state.VolumeDatasByID[edge.start.SymbolID], state.VolumeDatasByID[edge.end.SymbolID], connection, startingNode)) {
						state.ResultVolumeDatas.Add(state.VolumeDatasByID[edge.end.SymbolID]);
						if(RecursionTest(state, edgeIndex + 1)) {
							return true;
						}else {
							state.ResultVolumeDatas.Remove(state.VolumeDatasByID[edge.end.SymbolID]);
						}
					}
				}
			}
			state.VolumeDatasByID.Remove(edge.end.SymbolID);
			return false;
		}
		// Dfs generate.
		private static void RecursionGetSeriesTest(CreVoxNode node) {
			foreach (var child in node.Children.OrderBy(x=> UnityEngine.Random.value)) {
				if(edgeList.Exists(x=>x.start == node && x.end == child)) {
					continue;
				}
				//Debug.Log(node.SymbolID + " + " + child.SymbolID);
				edgeList.Add(new Edge(node, child));
				RecursionGetSeriesTest(child);
			}
		}
		private static List<CreVoxNode> _usedNode;
		private static CrevoxState nowState;
		// Generate the volume data that refer graph grammar.
		public static void Generate() {
			CrevoxState.ConnectionInfoVdataTable = new Dictionary<VolumeData, List<ConnectionInfo>>();
			var stopWatch = System.Diagnostics.Stopwatch.StartNew();
			int counter = 1;
			// Wait 5 sec.
			while (stopWatch.ElapsedMilliseconds <= TIMEOUT_MILLISECOND) {
				_usedNode = new List<CreVoxNode>();
				// Get root.
				CreVoxNode root = CreVoxAttach.RootNode;
				// Initial root.
				nowState = new CrevoxState(SelectData(_refrenceTable[root.AlphabetID]));
				_usedNode.Add(root);
				if (GenerateRecursion(root, nowState.ResultVolumeDatas[0])) {
					Debug.Log("Completed.");
					// Update volume manager and scene.
					CrevoxOperation.InitialVolume(nowState.ResultVolumeDatas);
					CrevoxOperation.RefreshVolume();
					break;
				} else {
					// Faild then destroy.
					Debug.Log("Failed.");
				}
				counter++;
			}
			Debug.Log("Try " + counter + " times.");
			Debug.Log(stopWatch.ElapsedMilliseconds + " ms");
			stopWatch.Stop();
		}
		// Dfs generate.
		private static bool GenerateRecursion(CreVoxNode node, CrevoxState.VolumeDataEx originalVolumeEx) {
			foreach (var child in node.Children) {
				if (_usedNode.Exists(n => n.SymbolID == child.SymbolID)) {
					continue;
				}
				List<VolumeData> suitableVdata = new List<VolumeData>();
				CrevoxState.VolumeDataEx newVolumeEx = null;
				// Find the suitable vdata by random ordering.
				foreach (var vdata in _refrenceTable[child.AlphabetID].OrderBy(x => UnityEngine.Random.value)) {
					newVolumeEx = new CrevoxState.VolumeDataEx(vdata);
					if (newVolumeEx.ConnectionInfos.Count - 1 >= child.Children.Count) {
						suitableVdata.Add(vdata);
					}
				}
				// No vdata have enough connection.
				if (suitableVdata.Count == 0) {
					Debug.Log("There is no vdata that have enough connection in " + _refrenceTable[child.AlphabetID][0].name + ". It means this graph  doesn't match with vdata.");
					return false;
				}
				bool canCombine = false;
				foreach (var vdata in suitableVdata) {
					newVolumeEx = new CrevoxState.VolumeDataEx(vdata);
					// Combine.
					ConnectionInfo[] originalConnectionList = originalVolumeEx.ConnectionInfos.OrderBy(x => UnityEngine.Random.value).ToArray();
					ConnectionInfo[] newConnectionList = newVolumeEx.ConnectionInfos.ToArray();
					// Get starting node.
					ConnectionInfo newStartingNode = newConnectionList.FirstOrDefault(x => !x.used && x.type == ConnectionInfoType.StartingNode);
					if (newStartingNode != null) {
						// Get connection.
						foreach (var originalConnection in originalConnectionList) {
							if (originalConnection.used || originalConnection.type == ConnectionInfoType.StartingNode) {
								continue;
							}
							//Debug.Log(originalVolumeEx.volumeData.name + " + " + newVolumeEx.volumeData.name);
							// Combine.
							if (nowState.CombineVolumeObject(originalVolumeEx, newVolumeEx, originalConnection, newStartingNode)) {
								nowState.ResultVolumeDatas.Add(newVolumeEx);
								_usedNode.Add(child);
								originalConnection.used = true;
								newStartingNode.used = true;
								if (GenerateRecursion(child, newVolumeEx)) {
									canCombine = true;
									break;
								} else {
									nowState.ResultVolumeDatas.Remove(newVolumeEx);
									_usedNode.Remove(child);
									originalConnection.used = false;
									newStartingNode.used = false;
								}
							}
						}
					}
					if (canCombine) {
						break;
					}
				}
				if (!canCombine) {
					return false;
				}
			}
			return true;
		}
		// Replace remaining connection.
		public static void ReplaceConnection() {
			var stopWatch = System.Diagnostics.Stopwatch.StartNew();
			int counter = 1;
			List<CrevoxState.VolumeDataEx> volumeList = nowState.ResultVolumeDatas;
			// Find all volume.
			for (int i = 0; i < volumeList.Count && stopWatch.ElapsedMilliseconds < TIMEOUT_MILLISECOND; i++) {
				// Find all connections that haven't used.
				foreach (var connection in volumeList[i].ConnectionInfos.FindAll(c => !c.used && c.type == ConnectionInfoType.Connection)) {
					bool success = false;
					// Find all vdata replaced order by random.
					foreach (var vdata in SpaceAlphabet.replaceDictionary[connection.connectionName].OrderBy(x => UnityEngine.Random.value)) {
						CrevoxState.VolumeDataEx replaceVol = new CrevoxState.VolumeDataEx(vdata);
						ConnectionInfo replaceStartingNode = replaceVol.ConnectionInfos.Find(x => x.type == ConnectionInfoType.StartingNode);
						// Combine.
						if (nowState.CombineVolumeObject(volumeList[i], replaceVol, connection, replaceStartingNode)) {
							Debug.Log(connection.connectionName + " is replaced by " + vdata.name);
							nowState.ResultVolumeDatas.Add(replaceVol);
							connection.used = true;
							replaceStartingNode.used = true;
							volumeList.Add(replaceVol);
							success = true;
							counter++;
							break;
						}
					}
					// No one can combine then alert.
					if (!success) {
						Debug.Log(volumeList[i].volumeData.name + ":" + connection.connectionName + " replace failed.");
					}
				}
			}
			// Record.
			Debug.Log("Replace " + counter + " connections.");
			Debug.Log(stopWatch.ElapsedMilliseconds + " ms");
			stopWatch.Stop();
			CrevoxOperation.InitialVolume(nowState.ResultVolumeDatas);
		}
		// [TEST] Will delete.
		public static void RandomGenerate(int count) {
			/*List<Volume> vols = new List<Volume>();
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
			CrevoxOperation.RefreshVolume();*/
		}
		// Random select from multi vDatas.
		private static VolumeData SelectData(List<VolumeData> sameRoomData) {
			return sameRoomData[(int) UnityEngine.Random.Range(0, sameRoomData.Count)];
		}

	}
}
