using UnityEngine;
using System;
//using CreVox;

public class VolumeAdapter {

	public static void AfterVolumeInit(GameObject volume)
    {
		Type eventDriver = Type.GetType ("EventDriver");
		if (eventDriver != null)
			volume.AddComponent(eventDriver);
    }
}
