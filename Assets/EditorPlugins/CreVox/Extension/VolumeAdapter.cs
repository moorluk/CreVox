using UnityEngine;
using System;
using System.Collections.Generic;

public class VolumeAdapter {

	public static void AfterVolumeInit(GameObject volume)
    {
        //event system
        Type eventDriver = Type.GetType ("RoomDriver");
        if (eventDriver != null && volume.GetComponent (eventDriver) == null)
            volume.AddComponent (eventDriver);
        //SECTR
        Type sectr = Type.GetType ("SECTR_Sector");
        GameObject root = (volume.transform.FindChild ("DecorationRoot")).gameObject;
        if (sectr != null) {
            if (root.GetComponent (sectr) == null) {
                root.AddComponent (sectr);
            }
            SECTR_Sector ss = root.GetComponent (sectr) as SECTR_Sector;
            ss.BoundsUpdateMode = SECTR_Member.BoundsUpdateModes.Static;
//            ss.ChildCulling = SECTR_Member.ChildCullModes.Individual;
            ss.ForceUpdate (true);
        }
    }

    private static int useSetupDungeon = -1;
    public static bool CheckSetupDungeon ()
    {
        if (useSetupDungeon < 0) {
            Type t = Type.GetType ("SetupDungeon");
            UnityEngine.Object gg = null;
            if (t != null) {
                gg =  UnityEngine.Object.FindObjectOfType(t);
            }
            useSetupDungeon = (t != null && gg != null) ? 1 : 0;
        }

        return (useSetupDungeon > 0) ? true : false;
    }

	public static void UpdatePortals(GameObject root)
	{
//		if (CreVox.VGlobal.GetSetting ().Generation && Application.isPlaying)
//			UpdatePortalsByInfo (root);
//		else
			UpdatePortalsByDis (root);
	}

	private static void UpdatePortalsByDis (GameObject root)
	{
		string log1 = "<b>Linked Sectr_Portal:</b>\n";
		string log2 = "<b>Diasbled Sectr_Portal:</b>\n";

		List<SECTR_Portal> _portals = new List<SECTR_Portal> ();
		root.GetComponentsInChildren (false, _portals);

		Dictionary<GameObject,GameObject> _rooms = new Dictionary<GameObject, GameObject> ();
		for (int i = 0; i < _portals.Count; i++) 
			_rooms.Add (_portals [i].gameObject, _portals [i].transform.parent.parent.parent.gameObject);
		
		for (int i = 0; i < _portals.Count; i++) {
			float _nearDist = float.PositiveInfinity;
			GameObject _target = null;
			Vector3 _start = _portals [i].transform.parent.position;
			//find nearst connection.
			for (int j = 0; j < _portals.Count; j++) {
				Vector3 _end = _portals [j].transform.parent.position;
				float _TargetDist = Vector3.Distance (_start, _end);
				if (i != j && _TargetDist < _nearDist && _TargetDist < 3) {
					_nearDist = _TargetDist;
					_target = _portals [j].gameObject;
				}
			}
			//if find legal target update portal, else disable portal.
			if (_target != null) {
				_portals [i].FrontSector = _rooms [_portals [i].gameObject].GetComponentInChildren<SECTR_Sector> ();
				_portals [i].BackSector = _rooms [_target].GetComponentInChildren<SECTR_Sector> ();
				log1 += _rooms [_portals [i].gameObject].name + "<size=8>." + _portals [i].transform.parent.name + "</size>" 
					+ "  <b><size=16>→</size></b> "
					+ _rooms [_target].name + "<size=8>." + _target.transform.parent.name + "</size>\n";
			} else {
				_portals [i].enabled = false;
				log2 +=_rooms [_portals [i].gameObject].name + "<size=8>." + _portals [i].transform.parent.name + "</size>\n";
			}
		}

		Debug.Log (log1 + "\n" + log2);
	}

	private static void UpdatePortalsByInfo (GameObject root)
	{
		List<SECTR_Portal> _portals = new List<SECTR_Portal> ();
		root.GetComponentsInChildren (false, _portals);
		Dictionary<SECTR_Portal,CreVox.Volume> _rooms = new Dictionary<SECTR_Portal, CreVox.Volume> ();

		for (int i = 0; i < _portals.Count; i++) {
			//find all connection's volume.
			SECTR_Portal _p = _portals [i];
			CreVox.Volume _room = _p.transform.parent.parent.parent.gameObject.GetComponent<CreVox.Volume>();
			_rooms.Add (_p, _room);
		}

		string log1 = "<b>Linked Sectr_Portal:</b>\n";
		string log2 = "<b>Diasbled Sectr_Portal:</b>\n";

		for (int i = 0; i < _portals.Count; i++) {
			Vector3 _start = _portals [i].transform.parent.position;
			CreVox.Volume _vol = _rooms[_portals[i]];
			if (_vol.ConnectionInfos == null)
				continue;
			for (int c = 0; c < _vol.ConnectionInfos.Count; c++) {
				if (Vector3.Equals (_vol.ConnectionInfos [c].position, _start))
					_portals [i].FrontSector = _vol.gameObject.GetComponentInChildren<SECTR_Sector> ();
					_portals [i].BackSector = _vol.ConnectionInfos [c].connectedGameObject.GetComponentInChildren<SECTR_Sector> ();
			}
		}
		Debug.Log (log1 + "\n" + log2);
	}
}
