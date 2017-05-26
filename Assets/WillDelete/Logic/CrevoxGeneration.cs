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
		// Private members.
		private static List<Guid> _alphabetIDs = new List<Guid>();
		private static Dictionary<Guid, List<VolumeData>> _refrenceTable = new Dictionary<Guid, List<VolumeData>>();
		private static List<List<VolumeData>> sameVolumeDatas = new List<List<VolumeData>>();
		// Public members.
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
		// Initial.
		public static void InitialTable(int seed) {
			UnityEngine.Random.InitState(seed);
			_refrenceTable = new Dictionary<Guid, List<VolumeData>>();
			for (int i = 0; i < _alphabetIDs.Count; i++) {
				_refrenceTable[_alphabetIDs[i]] = sameVolumeDatas[i];
			}
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
		public static string artPack = "";
		public static bool generateVolume;
		// Generate
		public static void Generate() {
			// Record.
			testStopWatch = System.Diagnostics.Stopwatch.StartNew();
			// Initialize connection table.
			CrevoxState.ConnectionInfoVdataTable = new Dictionary<VolumeData, List<ConnectionInfo>>();
			// Initialize sequence of edges.
			edgeList = new List<Edge>();
			// Get sequence of edges.
			RecursionGetSequence(CreVoxAttach.RootNode);
			// Initialize state.
			nowState = null;
			// Get mapping vdata from root node.
			foreach (var rootVdata in _refrenceTable[CreVoxAttach.RootNode.AlphabetID].OrderBy(x => UnityEngine.Random.value)) {
				// Initialize a state.
				CrevoxState state = new CrevoxState(rootVdata);
				// Set the root vdata.
				state.VolumeDatasByID[CreVoxAttach.RootNode.SymbolID] = state.ResultVolumeDatas[0];
				if(Recursion(state, 0)) {
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
				CrevoxOperation.TransformStateIntoObject (nowState, artPack, generateVolume);
			}else {
				// Keep null means failed.
				Debug.Log("Failed.");
			}
			Debug.Log(testStopWatch.ElapsedMilliseconds + " ms");
			testStopWatch.Stop();
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
			foreach (var vdata in _refrenceTable[edge.end.AlphabetID]) {
				CrevoxState.VolumeDataEx newVolumeEx = new CrevoxState.VolumeDataEx(vdata);
				if (newVolumeEx.ConnectionInfos.Count - 1 >= edge.end.Children.Count) {
					feasibleVdata.Add(vdata);
				}
			}
			// No vdata have enough connection. Return false.
			if (feasibleVdata.Count == 0) {
				Debug.Log("There is no vdata that have enough connection in " + _refrenceTable[edge.end.AlphabetID][0].name + ". It means this graph  doesn't match with vdata.");
				return false;
			}
			// Find mapping vdata in table that order by random.
			foreach (var mappingVdata in feasibleVdata.OrderBy(x => UnityEngine.Random.value)) {
				// Set the end node.
				state.VolumeDatasByID[edge.end.SymbolID] = new CrevoxState.VolumeDataEx(mappingVdata);
				// Get startingNode from the end node.
				ConnectionInfo startingNode = state.VolumeDatasByID[edge.end.SymbolID].ConnectionInfos.Find(x => !x.used && x.type == ConnectionInfoType.StartingNode);
				// Get connection from the start node.
				foreach (var connection in state.VolumeDatasByID[edge.start.SymbolID].ConnectionInfos.OrderBy(x => UnityEngine.Random.value)) {
					// Ignore used or type-error connection.
					if (connection.used || connection.type != ConnectionInfoType.Connection) { continue; }
					// Combine.
					if (state.CombineVolumeObject(state.VolumeDatasByID[edge.start.SymbolID], state.VolumeDatasByID[edge.end.SymbolID], connection, startingNode)) {
						// Success then add this vdata to state.
						state.ResultVolumeDatas.Add(state.VolumeDatasByID[edge.end.SymbolID]);
						// Recursion next level.
						if(Recursion(state, edgeIndex + 1)) {
							return true;
						}else {
							// If next level has problem then remove the vdata that added before.
							state.ResultVolumeDatas.Remove(state.VolumeDatasByID[edge.end.SymbolID]);
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
			CrevoxOperation.TransformStateIntoObject (nowState, artPack, generateVolume);
		}
		// Realtime level generation II
		public static void GenerateLevel(CreVoxNode root, string volumeDataPath, int seed, String artPack, bool generateVolume) {
			CrevoxGeneration.artPack = artPack;
			CrevoxGeneration.generateVolume = generateVolume;
			List<GraphGrammarNode> alphabets = new List<GraphGrammarNode>();
			List<List<VolumeData>> volumeDatas = new List<List<VolumeData>>();
			foreach (var node in Alphabet.Nodes) {
				if (node == Alphabet.AnyNode || node.Terminal == NodeTerminalType.NonTerminal) {
					continue;
				}
				alphabets.Add(node);
				volumeDatas.Add(new List<VolumeData>());
			}
			if (volumeDataPath != "") {
				// Get the files.
				string[] files = Directory.GetFiles(volumeDataPath);
				const string regex = @".*[\\\/](\w+)_.+_vData\.asset$";
				for (int i = 0; i < files.Length; i++) {
					if (Regex.IsMatch(files[i], regex)) {
						for (int j = 0; j < alphabets.Count; j++) {
							if (alphabets[j].Name.ToLower() == Regex.Match(files[i], regex).Groups[1].Value.ToLower()) {
								volumeDatas[j].Add(CrevoxOperation.GetVolumeData(files[i]));
							}
						}
					}

				}
				// if not find match vData, default null.
				for (int j = 0; j < alphabets.Count; j++) {
					if (volumeDatas[j].Count < 1) {
						volumeDatas[j].Add(null);
					}
				}
			}
			AlphabetIDs = alphabets.Select(x => x.AlphabetID).ToList();
			SameVolumeDatas = volumeDatas;
			InitialTable(seed);
			Generate();
		}
	}
}
