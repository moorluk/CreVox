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
			bool succeed = false;
			while (!succeed) {
				CreVoxNode root = CreVoxAttach.GenerateMissionGraph(XmlPath, Random.Range(int.MinValue, int.MaxValue));
				succeed = CrevoxGeneration.GenerateLevel(root, ResourcePath, Random.Range(int.MinValue, int.MaxValue), "", true);
			}


			// [Test] Camera.
			List<GameObject> gameList = new List<GameObject>();
			for(int i=0;i< CrevoxOperation.resultVolumeManager.transform.childCount; i++) {
				gameList.Add(CrevoxOperation.resultVolumeManager.transform.GetChild(i).gameObject);
			}
			// 讓Camera照中心點 方便觀察
			Vector3 allCenter = FindCenterPoint(gameList.ToArray());
			Camera.main.transform.position = new Vector3(allCenter.x, 400.0f, allCenter.z);
			Camera.main.transform.eulerAngles = new Vector3(90, 0, 0);
		}
	}
	Vector3 FindCenterPoint(GameObject[] gos) {
     if (gos.Length == 0)
         return Vector3.zero;
     if (gos.Length == 1)
         return gos[0].transform.position;
     var bounds = new Bounds(gos[0].transform.position, Vector3.zero);
     for (var i = 1; i<gos.Length; i++)
         bounds.Encapsulate(gos[i].transform.position); 
     return bounds.center;
 }
}
