using UnityEngine;
using System;

public class VolumeAdapter {

	public static void AfterVolumeInit(GameObject volume)
    {
		//event system
		Type eventDriver = Type.GetType ("RoomDriver");
		if (eventDriver != null && volume.GetComponent(eventDriver) == null)
			volume.AddComponent(eventDriver);
		//SECTR
		Type sectr = Type.GetType ("SECTR_Sector");
		if (sectr != null && volume.GetComponent (sectr) == null) {
			volume.AddComponent (sectr);
//			SECTR_Sector ss = volume.GetComponent (sectr) as SECTR_Sector;
//			ss.ChildCulling = SECTR_Member.ChildCullModes.Individual;
		}

    }
}
