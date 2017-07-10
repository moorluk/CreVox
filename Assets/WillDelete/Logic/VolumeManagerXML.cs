using UnityEngine;
using System.Collections;
using System.Xml.Linq;
using System.Linq;
using System;

using System.Collections.Generic;

namespace CrevoxExtend {
	public static class VolumeManagerXML {
		public static class Serialize {
			private static Dictionary<Guid, XElement> _volumeDataDicionary;
			private static string _path = "";
			// Init.
			public static void Init(string path) {
				_path = path;
				_volumeDataDicionary = new Dictionary<Guid, XElement>();
			}
			// Add xml data.
			public static void AddToDictionary(CrevoxState state, MissionGrammarSystem.CreVoxNode startNode, string connectionType, MissionGrammarSystem.CreVoxNode endNode) {
				string startVdataName = state.VolumeDatasByID[startNode.SymbolID].volumeData.name;
				string endVdataName = state.VolumeDatasByID[endNode.SymbolID].volumeData.name;
				// Set startNode -> endNode.
				if (! _volumeDataDicionary.ContainsKey(startNode.SymbolID)) {
					_volumeDataDicionary.Add(startNode.SymbolID, new XElement("VolumeData", new XAttribute("Type", startNode.AlphabetID), new XElement("Instructions", new XElement("vdata", startVdataName))));
					_volumeDataDicionary[startNode.SymbolID].Add(new XElement("Connections"));
				}
				XElement startNodeElementConnections = _volumeDataDicionary[startNode.SymbolID].Element("Connections");
				startNodeElementConnections.Add(new XElement("Connection", new XAttribute("Type", connectionType), new XElement("Instructions", new XElement("vdata", endVdataName))));

				// Set endNode -> startNode
				if (!_volumeDataDicionary.ContainsKey(endNode.SymbolID)) {
					_volumeDataDicionary.Add(endNode.SymbolID, new XElement("VolumeData", new XAttribute("Type", endNode.AlphabetID), new XElement("Instructions", new XElement("vdata", endVdataName))));
					_volumeDataDicionary[endNode.SymbolID].Add(new XElement("Connections"));
				}
				XElement endNodeElementConnections = _volumeDataDicionary[endNode.SymbolID].Element("Connections");
				endNodeElementConnections.Add(new XElement("Connection", new XAttribute("Type", connectionType), new XElement("Instructions", new XElement("vdata", startVdataName))));
			}
			// Serialize to xml.
			public static void SerializeToXml() {
				XDocument xmlDocument = new XDocument();
				XElement volumeManagerElement = new XElement("VolumeManager");
				XElement vdatasElement = new XElement("VolumeDatas");
				// Add volume datas.
				foreach (var elementKey in _volumeDataDicionary.Keys) {
					vdatasElement.Add(_volumeDataDicionary[elementKey]);
				}
				volumeManagerElement.Add(vdatasElement);
				xmlDocument.Add(volumeManagerElement);
				// Save to path.
				xmlDocument.Save(_path);
			}
			
		}

		public static class Unserialize {
			// Static method for other class calling.
			public static void UnserializeFromXml(string path) {
				TextAsset xmlData = Resources.Load(path.Replace(".xml", "")) as TextAsset;
				XDocument xmlDocument = (xmlData == null) ? XDocument.Load(path) : XDocument.Parse(xmlData.text);

			}
		}
	}
}

