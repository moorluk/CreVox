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
using System.Xml.Linq;

namespace CrevoxExtend {
	class CrevoxGenerationWindow : EditorWindow {
		private static Dictionary<GraphGrammarNode, List<VolumeData>> RefrenceTable { get; set; }
		// [NEW] New Dictionary
		private static Dictionary<GraphGrammarNode, List<VDataAndMax>> ReferenceTableVMax { get; set;}
		private static string regex = @".*[\\\/](\w+)_.+_vData\.asset$";
		private static Vector2 scrollPosition = new Vector2(0, 0);
		private static bool specificRandomSeed = false;
		private static int randomSeed = 0;
		private static int stageID = 1;
		private VGlobal vg;
		private static string vDatasPath;

		void Initialize() {
			// Create a new one or clear it.
			if (RefrenceTable == null) {
				RefrenceTable = new Dictionary<GraphGrammarNode, List<VolumeData>>();
			} else {
				RefrenceTable.Clear();
			}

			vg = VGlobal.GetSetting();
			foreach (var node in Alphabet.Nodes.Where(n => (n != Alphabet.AnyNode && n.Terminal != NodeTerminalType.NonTerminal))) {
				RefrenceTable.Add(node, new List<VolumeData>());
			}

			// [NEW] Create new instance of dictionary
			if (ReferenceTableVMax == null) {
				ReferenceTableVMax = new Dictionary<GraphGrammarNode, List<VDataAndMax>> ();
			} else {
				ReferenceTableVMax.Clear();
			}
			foreach (var node in Alphabet.Nodes.Where(n => (n != Alphabet.AnyNode && n.Terminal != NodeTerminalType.NonTerminal))) {
				ReferenceTableVMax.Add(node, new List<VDataAndMax>());
			}
		}
		void Awake() {
			Initialize();
		}
		// On focus on the window.
		void OnFocus() {
			if (RefrenceTable == null) {
				Initialize();
			} else if (IsChanged()) {
				Initialize();
			}

			// [NEW] 
			if (ReferenceTableVMax == null) {
				Initialize();
			} else if(IsChanged()) {
				Initialize();
			}
		}
		// If the alphabet changed, update the list of nodes.
		bool IsChanged() {
			var currentNodes = RefrenceTable.Keys.Select(n => n.Name).ToList();
			var latestNodes  = Alphabet.Nodes.Where(n => (n != Alphabet.AnyNode && n.Terminal != NodeTerminalType.NonTerminal)).Select(n => n.Name).ToList();
			var firstNotSecond = currentNodes.Except(latestNodes).ToList();
			var secondNotFirst = latestNodes.Except(currentNodes).ToList();
			//return firstNotSecond.Any() || secondNotFirst.Any();

			// [NEW] 
			var _currentNodes = ReferenceTableVMax.Keys.Select(n => n.Name).ToList();
			// Select nodes where are not nonterminal and then sort by the name
			var _latestNodes = Alphabet.Nodes.Where(n => (n != Alphabet.AnyNode && n.Terminal != NodeTerminalType.NonTerminal)).Select(n => n.Name).ToList();
			var _firstNotSecond = currentNodes.Except(_latestNodes).ToList();
			var _secondNotFirst = latestNodes.Except(_currentNodes).ToList();
			// return changed == true if there is any different from the dictionary and alphabet's nodes

			return (firstNotSecond.Any() || secondNotFirst.Any()) || (_firstNotSecond.Any() || _secondNotFirst.Any());
		}

		void OnGUI() {
			if (GUILayout.Button("Open Folder")) {
				// At first, clear all volume list.
				RefrenceTable.Values.ToList().ForEach(vd => vd.Clear());
				// [NEW] First, clear all VDataAndMaxV in the dictionary
				ReferenceTableVMax.Values.ToList().ForEach(vd => vd.Clear());

				// Open folder.
				string path = EditorUtility.OpenFolderPanel("Load Folder", 
					Application.dataPath + PathCollect.resourcesPath.Substring (6) + PathCollect.save, "");
				if (path != string.Empty) {
					// Save the global path for loading XML
					vDatasPath = path;
					// Get the files.
					string[] files = Directory.GetFiles(path);
					foreach (var file in files) {
						if (! Regex.IsMatch(file, regex)) {
							continue;
						}
						Debug.Log("Files: " + file);
						foreach (var node in RefrenceTable.Keys) {
							if (node.Name.ToLower() != Regex.Match(file, regex).Groups[1].Value.ToLower()) {
								continue;
							}
							RefrenceTable[node].Add(CrevoxOperation.GetVolumeData(file.Replace(Environment.CurrentDirectory.Replace('\\', '/') + "/", "")));
						}
						// [NEW]
						foreach (var node in ReferenceTableVMax.Keys) {
							if (node.Name.ToLower() != Regex.Match(file, regex).Groups[1].Value.ToLower()) {
								continue;
							}
							ReferenceTableVMax[node].Add(new VDataAndMax (CrevoxOperation.GetVolumeData(file.Replace(Environment.CurrentDirectory.Replace ('\\', '/') + "/", "")), 0));
						}
					}

					// if not find match vData, default null.
					foreach (var volumeList in RefrenceTable.Values) {
						if (volumeList.Count == 0) {
							volumeList.Clear();
						}
					}
					// [NEW] if not find match vData, default null.
					foreach (var vDataAndMaxList in ReferenceTableVMax.Values) {
						if (vDataAndMaxList.Count == 0) {
							vDataAndMaxList.Clear();
						}
					}
				}
			}
			if (GUILayout.Button("Save")) {
				SaveToXML();
			}
			if (GUILayout.Button("Load")) {
				LoadFromXML();
			}

			// Node list.
			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(Screen.width), GUILayout.Height(Screen.height-270f));
			/*foreach (var node in RefrenceTable.Keys) {
				// Label field, show the ExpressName of node.
				EditorGUILayout.LabelField(node.ExpressName);
				// Show volumeData object each node.
				foreach (var volume in RefrenceTable[node].ToList()) {
					EditorGUILayout.BeginHorizontal();
					// Field about input the volume data.
					RefrenceTable[node][RefrenceTable[node].IndexOf(volume)] = (VolumeData) EditorGUILayout.ObjectField(volume, typeof(VolumeData), false, GUILayout.Width(Screen.width / 2 - 10));
					// Delete function.
					if (GUILayout.Button("Delete")) {
						RefrenceTable[node].Remove(volume);
					}
					EditorGUIUtility.labelWidth = 100;
					EditorGUILayout.IntField("Max", 0, GUILayout.Width(Screen.width/4));
					EditorGUILayout.EndHorizontal();
				}
				// Create function.
				GUILayout.Space(5);
				if (GUILayout.Button("Add New vData")) {
					RefrenceTable[node].Add(null);
				}
			}*/

			// [NEW] Layout for Nodes and their VolumeDatas in Window
			GUILayout.Label("New Dictionary", EditorStyles.boldLabel);
			float originalLabelWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 50;
			foreach (var node in ReferenceTableVMax.Keys) {
				EditorGUILayout.LabelField(node.ExpressName);
				// List of VolumeData from each node
				foreach (var vDataAndMax in ReferenceTableVMax[node].ToList()) {
					EditorGUILayout.BeginHorizontal();
					// Dictionary[node][index of vDataAndMax]
					ReferenceTableVMax[node][ReferenceTableVMax[node].IndexOf(vDataAndMax)].vData = (VolumeData) EditorGUILayout.ObjectField(vDataAndMax.vData, typeof(VolumeData), false, GUILayout.Width(Screen.width / 2 - 10));
					if (GUILayout.Button("Delete")) {
						ReferenceTableVMax[node].Remove(vDataAndMax);
					}
					ReferenceTableVMax[node][ReferenceTableVMax[node].IndexOf(vDataAndMax)].maxVData = (int)EditorGUILayout.IntField("Max", vDataAndMax.maxVData, GUILayout.Width(Screen.width/4));
					EditorGUILayout.EndHorizontal();
				}
				if (GUILayout.Button("Add New vData")) {
					ReferenceTableVMax[node].Add(new VDataAndMax(null, 0)); // or Add(null) ?
				}
			}
			EditorGUIUtility.labelWidth = originalLabelWidth;
			EditorGUILayout.EndScrollView();

			using (var v = new EditorGUILayout.VerticalScope (EditorStyles.helpBox)) {
				stageID = EditorGUILayout.IntSlider ("Stage", stageID, 1, vg.StageList.Count);
				CrevoxGeneration.stage = vg.GetStageSetting(stageID);
				EditorGUILayout.LabelField ("Level", CrevoxGeneration.stage.number.ToString());
				EditorGUILayout.LabelField ("Xml Path", CrevoxGeneration.stage.XmlPath);
				EditorGUILayout.LabelField ("VData Path", CrevoxGeneration.stage.vDataPath);
				EditorGUILayout.LabelField ("ArtPack", CrevoxGeneration.stage.artPack);
			}
			// If symbol has none of vData, user cannot press Generate.
			// Add null prevent.
			EditorGUI.BeginDisabledGroup(RefrenceTable.Values.ToList().Exists(vs => vs.Count == 0 || vs.Exists(v => v == null))); 
			// [NEW] Change with this later 
			EditorGUI.BeginDisabledGroup(ReferenceTableVMax.Values.ToList().Exists(vs => vs.Count == 0 || vs.Exists(v => v.vData == null)));

			CrevoxGeneration.generateVolume = EditorGUILayout.Toggle("Generate Volume", CrevoxGeneration.generateVolume);
			EditorGUILayout.BeginHorizontal();
			// Random seed and its toggler.
			specificRandomSeed = GUILayout.Toggle(specificRandomSeed, "Set Random Seed");
			EditorGUI.BeginDisabledGroup(! specificRandomSeed);
			randomSeed = EditorGUILayout.IntField(randomSeed, GUILayout.MaxWidth(Screen.width));
			EditorGUI.EndDisabledGroup();
			EditorGUILayout.EndHorizontal();
			// Generate level.
			if (GUILayout.Button("Generate")) {
				// Pass the RefrenceTable to CrevoxGeneration.
				CrevoxGeneration.RefrenceTable.Clear();
				foreach (var node in RefrenceTable.Keys) {
					CrevoxGeneration.RefrenceTable.Add(node.AlphabetID, RefrenceTable[node]);
				}
				// [NEW] Must edit the CrevoxGeneration.cs 
				CrevoxGeneration.ReferenceTableVMax.Clear();
				foreach (var node in ReferenceTableVMax.Keys) {
					CrevoxGeneration.ReferenceTableVMax.Add(node.AlphabetID, ReferenceTableVMax[node]);
				}
				// Set the random seed.
				if (! specificRandomSeed) { randomSeed = UnityEngine.Random.Range(0, int.MaxValue); }
				CrevoxGeneration.InitialTable(randomSeed);
				CrevoxGeneration.Generate(CrevoxGeneration.stage);
			}
			// [EDIT LATER] [Must modify the operations in "Generate" Button later (adding the max usage)]. In CrevoxGeneration.cs
			// Replace connections.
			if (GUILayout.Button("ReplaceConnection")) {
				CrevoxGeneration.ReplaceConnection(CrevoxGeneration.stage);
			}
			EditorGUI.EndDisabledGroup();
		}

		// Save to and Load from XML
		private static void SaveToXML(){
			string path = EditorUtility.SaveFilePanel("Save to XML", Application.dataPath + PathCollect.resourcesPath.Substring (6) + PathCollect.save,
				"VolumeGeneration.xml", "xml");
			if (path != string.Empty) {
				SerializeToXML(path);
			}
		}
		private static void LoadFromXML(){
			string path = EditorUtility.OpenFilePanel("Load from XML", 
				Application.dataPath + PathCollect.resourcesPath.Substring (6) + PathCollect.save, "xml");
			if (path != string.Empty) {
				DeserializeFromXML(path);
			}
		}

		// Serialize
		private static void SerializeToXML(string path){
			XDocument xmlDocument = new XDocument();
			xmlDocument.Add(SerializeVolumeGeneration(vDatasPath));
			xmlDocument.Save(path);
		}
		private static XElement SerializeVolumeGeneration(string vdataspath){
			XElement elementVolumeGeneration = new XElement("VolumeGeneration");
			elementVolumeGeneration.Add(SerializeSymbols(), new XElement("VDatasPath", vdataspath));
			return elementVolumeGeneration;
		}
		private static XElement SerializeSymbols(){
			XElement elementSymbols = new XElement("Symbols"); 
			foreach (var node in ReferenceTableVMax.Keys) {
				XElement elementSymbol = new XElement("Symbol");
				elementSymbol.Add(new XElement("Name", node.Name));
				//elementSymbol.Add(new XElement("AlphabetID", node.AlphabetID));
				elementSymbol.Add(new XElement("VolumeDatas"));
				// Add the detail of VolumeData in VolumeDatas
				XElement elementVolumeDatas = elementSymbol.Element("VolumeDatas");
				foreach (var vdataAndmaxv in ReferenceTableVMax[node].ToList()) {
					elementVolumeDatas.Add(SerializeVData("VolumeData", vdataAndmaxv));
				}
				elementSymbols.Add (elementSymbol);
			}
			return elementSymbols;
		}
		private static XElement SerializeVData(string name, VDataAndMax vdataAndmaxv){
			XElement elementVData = new XElement(name);
			elementVData.Add(new XElement("VDataName", vdataAndmaxv.vData.name), new XElement("MaxVData", vdataAndmaxv.maxVData));
			return elementVData;
		}

		// Deserialize
		private static void DeserializeFromXML(string path){
			TextAsset xmlData = Resources.Load(path.Replace(".xml", "")) as TextAsset;
			XDocument xmlDocument = (xmlData == null) ? XDocument.Load(path) : XDocument.Parse(xmlData.text);
			DeserializeVolumeGeneration(xmlDocument);
		}
		private static void DeserializeVolumeGeneration(XDocument xmlDocument){
			XElement elementVolumeGeneration = xmlDocument.Element("VolumeGeneration");
			// [EDIT LATER] Must Update the UI using the this new Dictionary. 
			Dictionary<GraphGrammarNode, List<VDataAndMax>> RefTabVMax = new Dictionary<GraphGrammarNode, List<VDataAndMax>>();
			RefTabVMax = DeserializeSymbols(elementVolumeGeneration);
			ReferenceTableVMax.Clear();
			ReferenceTableVMax = RefTabVMax;
		}
		private static Dictionary<GraphGrammarNode, List<VDataAndMax>> DeserializeSymbols(XElement element){
			Dictionary<GraphGrammarNode, List<VDataAndMax>> RefTableVMax = new Dictionary<GraphGrammarNode, List<VDataAndMax>>();
			XElement elementSymbols = element.Element("Symbols");
			XElement elementVDatasPath = element.Element("VDatasPath");
			// This was a bug before
			List<VolumeData> VDatas = GetVolumeDatasFromDir(elementVDatasPath.Value.ToString());

			foreach (var elementSymbol in elementSymbols.Elements("Symbol")) {
				// Find node in Alphabet to be added to dictionary later
				GraphGrammarNode node = new GraphGrammarNode(); 
				node = Alphabet.Nodes.Find(n => n.Name == elementSymbol.Element("Name").Value.ToString());
				//Debug.Log("Node Name: "+node.Name+", Element Symbol: "+elementSymbol.Element("Name").Value + 
				//	". Same? "+ (node.Name == elementSymbol.Element("Name").Value.ToString() ? true : false));

				List<VDataAndMax> VDataAndMaxVs = new List<VDataAndMax>();

				XElement elementVolumeDatas = elementSymbol.Element("VolumeDatas");
				// In here volumeData means VolumeDataAndMaxV
				foreach (var volumeData in elementVolumeDatas.Elements("VolumeData")) { 

					XElement elementVDataName = volumeData.Element("VDataName");
					XElement elementMaxVData = volumeData.Element("MaxVData");
					// [BUG]
					/* VDataAndMax vdataAndMax = new VDataAndMax(null, 0);
					vdataAndMax.vData = VDatas.Find(v => v.name == elementVDataName.Value);
					vdataAndMax.maxVData = Int32.Parse(elementMaxVData.Value);*/	 
					VDataAndMax vdataAndMax = new VDataAndMax(VDatas.Find(v => v.name == elementVDataName.Value), Int32.Parse(elementMaxVData.Value));
					// Add the current volumeData (based on Name and maxV) to the List.
					VDataAndMaxVs.Add(vdataAndMax); 	
				}
				// Add node and vdataAndmaxv to dictionary
				RefTableVMax.Add(node, VDataAndMaxVs);
			}
			return RefTableVMax;
		}

		// Get All VolumeDatas from the directory
		// [EDIT LATER] Change to GetVolumeDatasFromDir(string path)
		private static List<VolumeData> GetVolumeDatasFromDir(string path){
			Debug.Log("Load VDatas from: " + path);
			// [EDIT LATER] [BUG] Change the directory according to the XML. The performance is also slow. 
			//String path = Application.dataPath + PathCollect.resourcesPath.Substring(6) + PathCollect.save + "/IsaacNew";
			string[] files = Directory.GetFiles(path);
			List<VolumeData> VDatas = new List<VolumeData>();

			foreach (var file in files) {
				// Debug.Log("Files: " + file);
				if (!Regex.IsMatch (file, regex)) {
					continue;
				}
				foreach (var node in ReferenceTableVMax.Keys) {
					if (node.Name.ToLower() != Regex.Match(file, regex).Groups[1].Value.ToLower()) {
						continue;
					}
					// Just to make sure the path is correct
					VDatas.Add(CrevoxOperation.GetVolumeData(file.Replace(Environment.CurrentDirectory.Replace('\\', '/') + "/", "")));
					// Debug.Log("" + VDatas.Last ().name);
				}
			}
			return VDatas;
		}
	}
}