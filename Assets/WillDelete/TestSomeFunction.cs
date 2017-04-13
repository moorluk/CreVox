using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TestSomeFunction : MonoBehaviour {
	public int RandomCreateCount = 10;
	public void CreateVolumeObjects() {
		CreVox.VolumeData entrance = AddOn.GetVolumeData("Assets/WillDelete/VolumeData/Entrance_vdata.asset");
		CreVox.VolumeData explore = AddOn.GetVolumeData("Assets/WillDelete/VolumeData/Explore_vdata.asset");
		CreVox.VolumeData none = AddOn.GetVolumeData("Assets/WillDelete/VolumeData/None_vdata.asset");
		CreVox.VolumeData[] array = new CreVox.VolumeData[] { entrance, explore, none };
		AddOn.Initial(array[Random.Range(0, 3)]);
		for (int i = 0; i < RandomCreateCount; i++) {
			AddOn.AddAndCombineVolume(array[Random.Range(0,3)]);
		}

	}
}
