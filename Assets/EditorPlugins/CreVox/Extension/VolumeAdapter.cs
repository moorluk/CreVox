using UnityEngine;
using System;

public class VolumeAdapter {

	public static void AfterVolumeInit(GameObject volume)
    {
		Type eventDriver = Type.GetType ("RoomDriver");
		if (eventDriver != null && volume.GetComponent(eventDriver) == null)
			volume.AddComponent(eventDriver);
    }
}
