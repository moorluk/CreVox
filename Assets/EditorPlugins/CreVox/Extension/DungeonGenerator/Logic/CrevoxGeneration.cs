using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CreVox;
using MissionGrammarSystem;
using System;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace CrevoxExtend {
	public class CrevoxGeneration {
		private const int TIMEOUT_MILLISECOND = 5000;
		// Members.
		private static Dictionary<Guid, List<VDataAndMaxV>> _referenceTableVMax;
		public static Dictionary<Guid, List<VDataAndMaxV>> ReferenceTableVMax {
			get {
				// 
				if (_referenceTableVMax == null) {
					_referenceTableVMax = new Dictionary<Guid, List<VDataAndMaxV>>();
				}
				return _referenceTableVMax;
			}
			set {
				_referenceTableVMax = value;
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
			// Mapping VData from root node
			foreach (var rootVDataAndMaxV in ReferenceTableVMax[CreVoxAttach.RootNode.AlphabetID].OrderBy(x => UnityEngine.Random.value)){
				// Initialize a state.
				CrevoxState state = new CrevoxState(rootVDataAndMaxV.vData);
				// Set the root of VData.
				state.VolumeDatasByID[CreVoxAttach.RootNode.SymbolID] = state.ResultVolumeDatas[0];
				if (Recursion(state, 0)) {
					// If time's up then nowState still null. 
					if (testStopWatch.ElapsedMilliseconds < TIMEOUT_MILLISECOND) {
						nowState = state;
					}
					break;
				}
				// Recursion return false means this root(VData) can't be generated. 
			}

			if (nowState != null) {
				Debug.Log("<color=green>Completed.</color> (" + testStopWatch.ElapsedMilliseconds + " ms)");
				// Transform state into gameobject.
				CrevoxOperation.TransformStateIntoObject(nowState, _stage.artPack, generateVolume);
			} else {
				// Keep null means failed.
				Debug.Log("<color=red>Failed.</color> (" + testStopWatch.ElapsedMilliseconds + " ms)");
			}
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
				
			List<VDataAndMaxV> feasibleVDataAndMaxVs = new List<VDataAndMaxV>();
			// Find the suitable vdata via the connection amount and the children of node in graph.
			foreach (var vdataAndmaxv in ReferenceTableVMax[edge.end.AlphabetID]) {
				CrevoxState.VolumeDataEx newVolumeEx = new CrevoxState.VolumeDataEx(vdataAndmaxv.vData);
				if (newVolumeEx.ConnectionInfos.Count - 1 >= edge.end.Children.Count) {
					// Get clone.
					feasibleVDataAndMaxVs.Add(vdataAndmaxv);
					//Debug.Log ("Feasible VData: " + vdataAndmaxv.vData.name + ", MaxV: " + vdataAndmaxv.maxVData);
				}
			}
			// Reduce the maxV of vData. Use tolist to modify the enumerating list
			foreach (VDataAndMaxV vdataAndmaxv in feasibleVDataAndMaxVs.ToArray()) {
				// Count.
				int vdataCount = state.ResultVolumeDatas.Count(v => v.volumeData.name == vdataAndmaxv.vData.name);
				if(vdataAndmaxv.maxVData != -1 && vdataCount >= vdataAndmaxv.maxVData) {
					feasibleVDataAndMaxVs.Remove(vdataAndmaxv);
					//Debug.Log(vdataAndmaxv.vData.name + " Over");
				}
			}
			// If none of VData have enough connection, return false. 
			if (feasibleVDataAndMaxVs.Count == 0) {
				Debug.Log ("There is no vdata that have enough connection in <color=red>" + ReferenceTableVMax[edge.end.AlphabetID][0].vData.name + "</color>. It means this graph doesn't match with vdata.");
				return false;
			}
				
			// Find mapping VDataAndMaxV in table that ordered randomly. 
			foreach (var mappingvdataAndmaxv in feasibleVDataAndMaxVs.OrderBy(x => UnityEngine.Random.value)) {
				// Debug.Log("Mapping vData : " + mappingvdataAndmaxv.vData.name + " has " + mappingvdataAndmaxv.maxVData + "vData.");
				// Set the end node.
				if (state.VolumeDatasByID.ContainsKey(edge.end.SymbolID)) {
					state.VolumeDatasByID[edge.end.SymbolID] = new CrevoxState.VolumeDataEx(mappingvdataAndmaxv.vData);
				} else {
					state.VolumeDatasByID.Add(edge.end.SymbolID, new CrevoxState.VolumeDataEx(mappingvdataAndmaxv.vData));
				}
				// Get starting node from the end node.
				ConnectionInfo startingNode = state.VolumeDatasByID[edge.end.SymbolID].ConnectionInfos.Find(x => !x.used && x.type == ConnectionInfoType.StartingNode);
				List<ConnectionInfo> newConnections = new List<ConnectionInfo>();
				if (startingNode != null) {
					newConnections.Add (startingNode);
					// Debug.Log (startingNode.connectionName);
				} else {
					// If there is no starting node then find connections. 
					newConnections = state.VolumeDatasByID[edge.end.SymbolID].ConnectionInfos.FindAll(x => !x.used && x.type == ConnectionInfoType.Connection);
				}
				foreach (var newConnection in newConnections.OrderBy(c => UnityEngine.Random.value)) {
					// Get connection from the start node
					foreach (var connection in state.VolumeDatasByID[edge.start.SymbolID].ConnectionInfos.OrderBy(x => UnityEngine.Random.value)) {
						// Ignore used or type-error connection. 
						if (connection.used || connection.type != ConnectionInfoType.Connection) { continue; }
						if (RewriteSystem.ResultGraph.GetConnectionByNodeID(edge.start.SymbolID, edge.end.SymbolID).Name.ToLower() != connection.connectionName.ToLower()) {
							Debug.Log(RewriteSystem.ResultGraph.GetConnectionByNodeID(edge.start.SymbolID, edge.end.SymbolID).Name + " : " + connection.connectionName);
							continue;
						} else{Debug.Log("success : " + connection.connectionName);}
						// Combine.
						if (state.CombineVolumeObject(state.VolumeDatasByID[edge.start.SymbolID], state.VolumeDatasByID[edge.end.SymbolID], connection, newConnection)) {
							// If Success, add this VData to the state
							state.ResultVolumeDatas.Add(state.VolumeDatasByID[edge.end.SymbolID]);
							// Recursion next level. 
							if (Recursion (state, edgeIndex + 1)) {
								// Save connection info.
								state.VolumeDatasByID[edge.start.SymbolID].ConnectionInfos.Find(x => x.Compare(connection)).connectedObjectGuid = edge.end.SymbolID;
								state.VolumeDatasByID[edge.end.SymbolID].ConnectionInfos.Find(x => x.Compare(newConnection)).connectedObjectGuid = edge.start.SymbolID;
								// Success then return.
								return true;
							} else {
								// If next level has problem then remove the VData that has been added before
								state.ResultVolumeDatas.Remove(state.VolumeDatasByID[edge.end.SymbolID]);
							}
						}
					}
				}
			}
			// If none is success then restore the state.
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
		// Realtime Level Generation II.
		public static bool GenerateRealLevel(CreVoxNode root, VGlobal.Stage _stage, int seed){
			if (_stage.VGXmlPath == string.Empty){
				throw new System.Exception("VGXmlPath in stage cannot be empty.");
			}

			ReferenceTableVMax = LoadFromXML(PathCollect.save + "/" + _stage.VGXmlPath);
			// ReferenceTableVMax = LoadFromXML(_stage.VGXmlPath);

			// Check if there is null VData
			foreach (var volumeList in ReferenceTableVMax.Values){
				if (volumeList.Count == 0) {
					throw new System.Exception("Every nodes in alphabet must map at least one vData.");
				}
			}				
			InitialTable(seed);
			return Generate(_stage, root);
		}

		// Load Volume Generation XML
		// Call this function before generate actual level, VGXMLpath = stage.vgpath.
		private static Dictionary<Guid, List<VDataAndMaxV>> LoadFromXML(string VGXMLPath){
			if (VGXMLPath != string.Empty) {
				return DeserializeFromXML (VGXMLPath);
			}
			throw new System.Exception("XML path not found!");
		}
		// Deserialize
		private static Dictionary<Guid, List<VDataAndMaxV>> DeserializeFromXML(string path){
			TextAsset xmlData = Resources.Load(path.Replace(".xml", "")) as TextAsset;
			XDocument xmlDocument = (xmlData == null) ? XDocument.Load(path) : XDocument.Parse(xmlData.text);
			return DeserializeVolumeGeneration(xmlDocument);
		}
		private static Dictionary<Guid, List<VDataAndMaxV>> DeserializeVolumeGeneration(XDocument xmlDocument){
			XElement elementVolumeGeneration = xmlDocument.Element("VolumeGeneration");
			return DeserializeSymbols(elementVolumeGeneration);
		}
		private static Dictionary<Guid, List<VDataAndMaxV>> DeserializeSymbols(XElement element){
			Dictionary<Guid, List<VDataAndMaxV>> newRefTableMax = new Dictionary<Guid, List<VDataAndMaxV>>();
			XElement elementSymbols = element.Element("Symbols");
			XElement elementVDatasPath = element.Element("VDatasPath");
			List<VolumeData> VDatas = GetVolumeDatasFromDir(PathCollect.save + "/" + elementVDatasPath.Value.ToString());
			// or like this-> GetVolumeDatasFromDir(PathCollect.save + "/" + stage.vDataPath); 
			// vDataPath is still empty because of the order of function call

			foreach (var elementSymbol in elementSymbols.Elements("Symbol")) {
				// Find node in Alphabet to be added to dictionary later.
				GraphGrammarNode node = new GraphGrammarNode(); 
				node = Alphabet.Nodes.Find(n => n.Name == elementSymbol.Element("Name").Value.ToString());
				List<VDataAndMaxV> VDataAndMaxVs = new List<VDataAndMaxV>();
				XElement elementVolumeDatas = elementSymbol.Element("VolumeDatas");

				// In here volumeData means VolumeDataAndMaxV.
				foreach (var volumeData in elementVolumeDatas.Elements("VolumeData")) { 
					XElement elementVDataName = volumeData.Element("VDataName");
					XElement elementMaxVData = volumeData.Element("MaxVData");
					VDataAndMaxV vdataAndMax = new VDataAndMaxV(VDatas.Find(v => v.name == elementVDataName.Value), Int32.Parse(elementMaxVData.Value));
					// Add the current volumeData (based on Name and maxV) to the List.
					VDataAndMaxVs.Add(vdataAndMax);
				}
				// Add node and vdataAndmaxv to dictionary.
				newRefTableMax.Add(node.AlphabetID, VDataAndMaxVs);
			}
			// Debug.Log("VolumeData from " + elementVDatasPath.Value + " are mapped to " + newRefTableMax.Count + " symbols");
			return newRefTableMax;
		}
		// Get All VolumeDatas from the directory that match the Nodes.
		private static List<VolumeData> GetVolumeDatasFromDir(string path){
			List<VolumeData> vDatas;
			vDatas = Resources.LoadAll(path, typeof(VolumeData)).Cast<VolumeData>().ToList();
			return vDatas;
		}
	}
	// New Type for handling Max Usage.
	public class VDataAndMaxV {
		public VolumeData vData;
		public int maxVData;
		public VDataAndMaxV (VolumeData v, int maxV){
			this.vData = v;
			this.maxVData = maxV;
		}
		// Copy constructor.
		public VDataAndMaxV(VDataAndMaxV clone) {
			this.vData = clone.vData;
			this.maxVData = clone.maxVData;
		}
		public VDataAndMaxV Clone() {
			return new VDataAndMaxV(this);
		}
	}
}
