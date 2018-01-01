﻿using UnityEngine;
using System.Collections;
using System.Xml.Linq;
using System.Linq;
using System;

using System.Collections.Generic;

namespace CrevoxExtend {
	public static class SpaceAlphabetXML {
		#if UNITY_EDITOR
		public static class Serialize {
			// Static method for other class calling.
			public static void SerializeToXml(string path) {
				XDocument xmlDocument = new XDocument();
				xmlDocument.Add(SerializeSpaceAlphabet());
				xmlDocument.Save(path);
			}
			// Serialize SpaceAlphabet
			private static XElement SerializeSpaceAlphabet() {
				XElement elementSpaceAlphabet = new XElement("SpaceAlphabet");
				elementSpaceAlphabet.Add(SerializeConnections(SpaceAlphabet.ReplacementAlphabet));

				return elementSpaceAlphabet;
			}
			// Serialize Instructions
			private static XElement SerializeConnections(List<string> alphabets) {
				XElement element = new XElement("Connections");
				foreach (var connectionType in alphabets) {
					XElement elementConnection = new XElement("Connection", new XAttribute("Type", connectionType));
					elementConnection.Add(new XElement("Instructions"));

					XElement elementInstruction = elementConnection.Element("Instructions");
					foreach (var vData in SpaceAlphabet.ReplacementDictionary[connectionType]) {
						elementInstruction.Add(new XElement("vData", UnityEditor.AssetDatabase.GetAssetPath(vData)));
					}
					element.Add(elementConnection);
				}
				return element;
			}
		}
		#endif

		public static class Unserialize {
			// Static method for other class calling.
			public static void UnserializeFromXml(string path) {
				TextAsset xmlData = Resources.Load(path.Replace(".xml", "")) as TextAsset;
				XDocument xmlDocument = (xmlData == null) ? XDocument.Load(path) : XDocument.Parse(xmlData.text);
				UnserializeSpaceAlphabet(xmlDocument);
			}
			// Unserialize SpaceAlphabet
			private static void UnserializeSpaceAlphabet(XDocument xmlDocument) {
				XElement elementSpaceAlphabet = xmlDocument.Element("SpaceAlphabet");
				UnserializeConnections(elementSpaceAlphabet);
			}
			// Unserialize Instructions
			private static void UnserializeConnections(XElement elementSpaceAlphabet) {
				XElement elementConnections = elementSpaceAlphabet.Element("Connections");
				SpaceAlphabet.ReplacementDictionary = UnserializeInstruction(elementConnections);
			}
			// Unserialize nodes
			private static Dictionary<string,List<CreVox.VolumeData>> UnserializeInstruction(XElement element) {
				Dictionary<string, List<CreVox.VolumeData>> instructions = new Dictionary<string, List<CreVox.VolumeData>>();

				foreach (var connection in element.Elements("Connection")) {
					string connectionType = connection.Attribute("Type").Value;
					List<CreVox.VolumeData> vDatas = new List<CreVox.VolumeData>();
					XElement elementInstrucitons = connection.Element("Instructions");
					foreach (var vData in elementInstrucitons.Elements("vData")) {
						if(vData.Value == "") {
							vDatas.Add(null);
						} else {
							vDatas.Add(CrevoxOperation.GetVolumeData(vData.Value));
						}
						// if(Regex.IsMatch(vData.Value, regex)) {
						// 	vDatas.Add(CrevoxOperation.GetVolumeData(vData.Value));
						// 	//Debug.Log(vDatas.ToArray()[vDatas.ToArray().Length - 1].name);
						// } else {
						// 	vDatas.Add(null);
						// }
					}
					instructions.Add(connectionType, vDatas);
				}
				// Update spaceAlphabetWindow
				#if UNITY_EDITOR
				List<string> newAlphabet = element.Elements("Connection").Attributes().Select(e => e.Value).ToList();
				SpaceAlphabet.alphabetUpdate(newAlphabet);
				#endif
				return instructions;
			}
		}
	}
}

