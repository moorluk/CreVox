using CrevoxExtend;
using MissionGrammarSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CreVox;

public class TestRealTimeRun : MonoBehaviour {

	public string XmlPath = @"Issac_Flat.xml";
	public string ResourcePath = @"Assets\Resources\CreVox\VolumeData\Isaac";
	// Use this for initialization
	public bool testGenerateLevel = false;

	public VGlobal vg;
	public int stageLevel = 1;
	public int randomSeed = 0;
	public bool testGenerateStage = false;

	void Awake()
	{
		if (vg == null)
			vg = VGlobal.GetSetting ();
	}

	void Update() {
		if (testGenerateLevel) {
			if (XmlPath.Length > 0) {
				VGlobal.Stage _s = new VGlobal.Stage ();
				_s.artPack = "B02";
				_s.XmlPath = XmlPath;
				_s.vDataPath = ResourcePath;
				CreVoxNode root = CreVoxAttach.GenerateMissionGraph (_s.XmlPath, 538064);
				CrevoxGeneration.GenerateLevel (root, _s, 0);
			}
			testGenerateLevel = false;
		}
		if (testGenerateStage) {
			randomSeed = UnityEngine.Random.Range (0, int.MaxValue);
			vg.GenerateStage (stageLevel, randomSeed);
			testGenerateStage = false;
		}
	}
}
