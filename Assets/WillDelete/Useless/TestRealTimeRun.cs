using CrevoxExtend;
using MissionGrammarSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestRealTimeRun : MonoBehaviour {

	public string XmlPath = @"Issac_Flat.xml";
	public string ResourcePath = @"Assets\Resources\CreVox\VolumeData\IsaacNew";
	// Use this for initialization
	void Start() {
		if (XmlPath.Length > 0) {
			CreVoxNode root = CreVoxAttach.GenerateMissionGraph(XmlPath, 538064);
			CrevoxGeneration.GenerateLevel(root, ResourcePath, 0, "", true);
		}
	}
}
