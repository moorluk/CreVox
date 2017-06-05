using CrevoxExtend;
using MissionGrammarSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CreVox;
using UnityEngine.UI;

public class TestRealTimeRun : MonoBehaviour {

	[Header("Test generate stage by properties")]
	[Tooltip("我是按鈕")]
	public bool testGenerateLevel = false;
	public string XmlPath = @"Issac.xml";
	public string ResourcePath = @"IsaacNew";

	[Header("Test generate stage from global setting")]
	[Tooltip("我是按鈕")]
	public bool testGenerateStage = false;
	public VGlobal vg;
	public int stageLevel = 1;
	public int randomSeed = 0;

	void Awake()
	{
		if (vg == null)
			vg = VGlobal.GetSetting ();
	}

	void Update() {
		if (testGenerateLevel) {
			if (XmlPath.Length > 0) {
				bool succeed = false;
				VGlobal.Stage _s = new VGlobal.Stage ();
				_s.artPack = "B02";
				_s.XmlPath = XmlPath;
				_s.vDataPath = ResourcePath;
				int testTime = 0;
				while (!succeed && testTime < 20) {
					randomSeed = UnityEngine.Random.Range (0, int.MaxValue);
					Debug.Log ("[" + testTime +"]Random Seed : " + randomSeed);
					CreVoxNode root = CreVoxAttach.GenerateMissionGraph (PathCollect.gram + "/" + _s.XmlPath, randomSeed);
					succeed = CrevoxGeneration.GenerateLevel (root, _s, randomSeed);
					testTime++;
				}
			}
			testGenerateLevel = false;
		}
		if (testGenerateStage) {
			bool succeed = false;
			int testTime = 0;
			while (!succeed && testTime < 20) {
				randomSeed = UnityEngine.Random.Range (0, int.MaxValue);
				Debug.Log ("<color=teal>[" + testTime + "]Random Seed : " + randomSeed +"</color>");
//				succeed = vg.GenerateStage (stageLevel, randomSeed);
				VGlobal.CreateStage n = new VGlobal.CreateStage(vg.GenerateStage);
				succeed = n.Invoke (stageLevel, randomSeed);
				testTime++;
			}
			testGenerateStage = false;
		}
	}
//=======
//	void Start() {
//		if (XmlPath.Length > 0) {
//			bool succeed = false;
//			while (!succeed) {
//				randomSeed = UnityEngine.Random.Range (0, int.MaxValue);
//				VGlobal.Stage _s = new VGlobal.Stage ();
//				_s.artPack = "B02";
//				_s.XmlPath = XmlPath;
//				_s.vDataPath = ResourcePath;
//				CreVoxNode root = CreVoxAttach.GenerateMissionGraph(_s.XmlPath, randomSeed);
//				succeed = CrevoxGeneration.GenerateLevel(root, _s,randomSeed);
//			}
//
//
//			// [Test] Camera.
//			List<GameObject> gameList = new List<GameObject>();
//			for(int i=0;i< CrevoxOperation.resultVolumeManager.transform.childCount; i++) {
//				gameList.Add(CrevoxOperation.resultVolumeManager.transform.GetChild(i).gameObject);
//			}
//			// 讓Camera照中心點 方便觀察
//			Vector3 allCenter = FindCenterPoint(gameList.ToArray());
//			Camera.main.transform.position = new Vector3(allCenter.x, 400.0f, allCenter.z);
//			Camera.main.transform.eulerAngles = new Vector3(90, 0, 0);
//>>>>>>> Move_VolumeExtend_into_Volume
//		}
//	}
	Vector3 FindCenterPoint(GameObject[] gos) {
		if (gos.Length == 0)
			return Vector3.zero;
		if (gos.Length == 1)
			return gos [0].transform.position;
		var bounds = new Bounds (gos [0].transform.position, Vector3.zero);
		for (var i = 1; i < gos.Length; i++)
			bounds.Encapsulate (gos [i].transform.position); 
		return bounds.center;
	}
}
