using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CreVox;
using MissionGrammarSystem;
using System;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;

namespace CrevoxExtend {
	public class CrevoxGeneration {
		private const int TIMEOUT_MILLISECOND = 5000;
		// Members.
		private static Dictionary<Guid, List<VolumeData>> _refrenceTable;
		public static Dictionary<Guid, List<VolumeData>> RefrenceTable {
			get {
				// 
				if (_refrenceTable == null) {
					_refrenceTable = new Dictionary<Guid, List<VolumeData>>();
				}
				return _refrenceTable;
			}
			set {
				_refrenceTable = value;
			}
		}
		// Initial.
		public static void InitialTable(int seed) {
			UnityEngine.Random.InitState(seed);
		}
		// State.
		private static CrevoxState nowState;
		// Edge contain two node.
		private struct Edge {
			public CreVoxNode start;
			public CreVoxNode end;
			public Edge(CreVoxNode s, CreVoxNode e) {
				this.start = s;
				this.end = e;
			}
		}
		// All edges from graph.
		private static List<Edge> edgeList;
		// Record time.
		private static System.Diagnostics.Stopwatch testStopWatch;
		public static VGlobal.Stage stage;
		public static bool generateVolume;
		// Generate
		public static bool Generate(VGlobal.Stage _stage, CreVoxNode root = null) {
			// Check the root of mission graph.
			root = (root != null) ? root : CreVoxAttach.RootNode;
			if (root == null) {
				throw new System.Exception("Root of mission graph is NULL, please check the imported mission graph.");
			}
			// Record.
			testStopWatch = System.Diagnostics.Stopwatch.StartNew();
			// Initialize connection table.
			CrevoxState.ConnectionInfoVdataTable = new Dictionary<VolumeData, List<ConnectionInfo>>();
			// Initialize sequence of edges.
			edgeList = new List<Edge>();
			// Get sequence of edges.
			RecursionGetSequence(root);
			// Initialize state.
			nowState = null;
			// Get mapping vdata from root node.
			foreach (var rootVdata in RefrenceTable[CreVoxAttach.RootNode.AlphabetID].OrderBy(x => UnityEngine.Random.value)) {
				// Initialize a state.
				CrevoxState state = new CrevoxState(rootVdata);
				// Set the root vdata.
				state.VolumeDatasByID[CreVoxAttach.RootNode.SymbolID] = state.ResultVolumeDatas[0];
				if (Recursion(state, 0)) {
					// If time's up then nowState keeps null.
					if (testStopWatch.ElapsedMilliseconds < TIMEOUT_MILLISECOND) {
						nowState = state;
					}
					break;
				}
				// If Recursion return false then it means this root(vdata) cannot generate. 
			}
			if (nowState != null) {
				Debug.Log("Completed.");
				// Transform state into gameobject.
				CrevoxOperation.TransformStateIntoObject(nowState, _stage.artPack, generateVolume);
			} else {
				// Keep null means failed.
				Debug.Log("Failed.");
			}
			Debug.Log(testStopWatch.ElapsedMilliseconds + " ms");
			testStopWatch.Stop();
			// Return boolean.
			return nowState != null;
		}
		// Dfs the sequence.
		private static bool Recursion(CrevoxState state, int edgeIndex) {
			// If time over then return true. (Cuz true can break the recursion early.) 
			if (testStopWatch.ElapsedMilliseconds > TIMEOUT_MILLISECOND) { return true; }
			// If it is end of sequence then return true.
			if (edgeIndex >= edgeList.Count) { return true; }
			// Get the edge in this recursion.
			Edge edge = edgeList[edgeIndex];
			// If the end node is used.
			if (state.VolumeDatasByID.ContainsKey(edge.end.SymbolID)) {
				// Ignore.
				if (Recursion(state, edgeIndex + 1)) {
					return true;
				}
			}
			List<VolumeData> feasibleVdata = new List<VolumeData>();
			// Find the suitable vdata.
			foreach (var vdata in RefrenceTable[edge.end.AlphabetID]) {
				CrevoxState.VolumeDataEx newVolumeEx = new CrevoxState.VolumeDataEx(vdata);
				if (newVolumeEx.ConnectionInfos.Count - 1 >= edge.end.Children.Count) {
					feasibleVdata.Add(vdata);
				}
			}
			// No vdata have enough connection. Return false.
			if (feasibleVdata.Count == 0) {
				Debug.Log("There is no vdata that have enough connection in " + RefrenceTable[edge.end.AlphabetID][0].name + ". It means this graph  doesn't match with vdata.");
				return false;
			}
			// Find mapping vdata in table that order by random.
			foreach (var mappingVdata in feasibleVdata.OrderBy(x => UnityEngine.Random.value)) {
				// Set the end node.
				state.VolumeDatasByID[edge.end.SymbolID] = new CrevoxState.VolumeDataEx(mappingVdata);
				// Get startingNode from the end node.
				ConnectionInfo startingNode = state.VolumeDatasByID[edge.end.SymbolID].ConnectionInfos.Find(x => !x.used && x.type == ConnectionInfoType.StartingNode);
				List<ConnectionInfo> newConnections = new List<ConnectionInfo>();
				if (startingNode != null) {
					newConnections.Add(startingNode);
					Debug.Log(startingNode.connectionName);
				} else {
					// No starting node then find connections.
					newConnections = state.VolumeDatasByID[edge.end.SymbolID].ConnectionInfos.FindAll(x => !x.used && x.type == ConnectionInfoType.Connection);
					Debug.Log(newConnections.Count);
				}
				foreach (var newConnection in newConnections) {
					// Get connection from the start node.
					foreach (var connection in state.VolumeDatasByID[edge.start.SymbolID].ConnectionInfos.OrderBy(x => UnityEngine.Random.value)) {
						// Ignore used or type-error connection.
						if (connection.used || connection.type != ConnectionInfoType.Connection) { continue; }
						// Combine.
						if (state.CombineVolumeObject(state.VolumeDatasByID[edge.start.SymbolID], state.VolumeDatasByID[edge.end.SymbolID], connection, newConnection)) {
							// Success then add this vdata to state.
							state.ResultVolumeDatas.Add(state.VolumeDatasByID[edge.end.SymbolID]);
							// Recursion next level.
							if(Recursion(state, edgeIndex + 1)) {
								return true;
							} else {
								// If next level has problem then remove the vdata that added before.
								state.ResultVolumeDatas.Remove(state.VolumeDatasByID[edge.end.SymbolID]);
							}
						}
					}
				}
			}
			// If no one success then restore state.
			state.VolumeDatasByID.Remove(edge.end.SymbolID);
			return false;
		}
		// Dfs get sequence.
		private static void RecursionGetSequence(CreVoxNode node) {
			foreach (var child in node.Children.OrderBy(x=> UnityEngine.Random.value)) {
				// The edge is exist then ignore.
				if(edgeList.Exists(x=>x.start == node && x.end == child)) {
					continue;
				}
				edgeList.Add(new Edge(node, child));
				RecursionGetSequence(child);
			}
		}
		// Replace remaining connection.
		public static void ReplaceConnection(VGlobal.Stage _stage) {
			var stopWatch = System.Diagnostics.Stopwatch.StartNew();
			int counter = 1;
			List<CrevoxState.VolumeDataEx> volumeList = nowState.ResultVolumeDatas;
			// Find all volume.
			for (int i = 0; i < volumeList.Count && stopWatch.ElapsedMilliseconds < TIMEOUT_MILLISECOND; i++) {
				// Find all connections that haven't used.
				foreach (var connection in volumeList[i].ConnectionInfos.FindAll(c => !c.used && c.type == ConnectionInfoType.Connection)) {
					bool success = false;
					// Find all vdata replaced order by random.
					foreach (var vdata in SpaceAlphabet.ReplacementDictionary[connection.connectionName].OrderBy(x => UnityEngine.Random.value)) {
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
			CrevoxOperation.TransformStateIntoObject (nowState, _stage.artPack, generateVolume);
		}
		// Realtime level generation II. Return succeed or failed.
		public static bool GenerateLevel(CreVoxNode root, VGlobal.Stage _stage, int seed) {
			UnityEngine.Object[] vDatas;

			// If vDataPath is empty, then throw error.
			if (_stage.vDataPath == string.Empty) {
				throw new System.Exception("vDataPath in stage cannot be empty.");
			}

			// Create the keys of reference table.
			RefrenceTable = new Dictionary<Guid, List<VolumeData>>();
			foreach (var node in Alphabet.Nodes.Where(n => (n != Alphabet.AnyNode && n.Terminal != NodeTerminalType.NonTerminal))) {
				RefrenceTable.Add(node.AlphabetID, new List<VolumeData>());
			}

			// Get the files.
			vDatas = Resources.LoadAll(PathCollect.save + "/" + _stage.vDataPath, typeof(VolumeData));
			foreach (VolumeData vData in vDatas) {
				foreach (var node in Alphabet.Nodes.Where(n => (n != Alphabet.AnyNode && n.Terminal != NodeTerminalType.NonTerminal))) {
					if (node.Name.ToLower() == Regex.Match(vData.name, @"(\w+)_.+_vData$").Groups[1].Value.ToLower()) {
						RefrenceTable[node.AlphabetID].Add(vData);
					}
				}
			}

			// If not find match vData, then throw error.
			foreach (var volumeList in RefrenceTable.Values) {
				if (volumeList.Count == 0) {
					throw new System.Exception("Every nodes in alphabet must map at least one vData.");
				}
			}

			InitialTable(seed);
			return Generate (_stage, root);
		}
	}
}
