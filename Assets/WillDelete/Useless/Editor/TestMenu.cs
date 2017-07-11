using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TestMenu : EditorWindow {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	[MenuItem("測試用/跑跑VM", false, 1)]
	public static void Test() {
		GameObject vm = GameObject.Find("VolumeManager(Generated)");
		foreach (var item in vm.GetComponentsInChildren<CreVox.Volume>()) {
			foreach (var connection in item.ConnectionInfos) {
				if (connection.connectedGameObject != null) {
					Debug.Log(item.name + "->" + connection.connectionName + "->" + connection.connectedGameObject.name);
				}
			}
		}
	}
	[MenuItem("測試用/清除VM", false, 2)]
	public static void Clear() {
		GameObject vm = GameObject.Find("VolumeManager(Generated)");
		while (vm != null) {
			DestroyImmediate(vm);
			vm = GameObject.Find("VolumeManager(Generated)");
		}
	}
}
