using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class test : MonoBehaviour {
	public void CreateButton() {
		CreVox.VolumeData entrance = AddOn.GetVolumeData("Assets/WillDelete/VolumeData/Entrance_vdata.asset");
		CreVox.VolumeData explore = AddOn.GetVolumeData("Assets/WillDelete/VolumeData/Explore_vdata.asset");
		CreVox.VolumeData none = AddOn.GetVolumeData("Assets/WillDelete/VolumeData/None_vdata.asset");
		AddOn.Initial(entrance);
		AddOn.CombineVolumeData(explore);
		AddOn.CreateObject();
	}
}
