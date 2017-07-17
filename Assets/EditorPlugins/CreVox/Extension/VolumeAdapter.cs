using UnityEngine;
using System;
using System.Collections.Generic;

public class VolumeAdapter {

	public static void AfterVolumeInit(GameObject volume)
    {
		//event system
		Type eventDriver = Type.GetType ("RoomDriver");
		if (eventDriver != null && volume.GetComponent(eventDriver) == null)
			volume.AddComponent(eventDriver);
		//SECTR
		Type sectr = Type.GetType ("SECTR_Sector");
		GameObject root = (volume.transform.FindChild ("DecorationRoot")).gameObject;
		if (sectr != null) {
			if (root.GetComponent(sectr) == null) {
				root.AddComponent(sectr);
			}
			SECTR_Sector ss = root.GetComponent(sectr) as SECTR_Sector;
			ss.BoundsUpdateMode = SECTR_Member.BoundsUpdateModes.Static;
//			ss.ChildCulling = SECTR_Member.ChildCullModes.Individual;
			ss.ForceUpdate (true);
		}

    }

	public static void UpdatePortals(GameObject root)
	{
		UpdatePortalsByDis (root);
//		UpdatePortalsByInfo (root);
	}

	private static void UpdatePortalsByDis (GameObject root)
	{
		List<SECTR_Portal> _portals = new List<SECTR_Portal> ();
		Dictionary<GameObject,GameObject> _rooms = new Dictionary<GameObject, GameObject> ();
		root.GetComponentsInChildren (false, _portals);
		for (int i = 0; i < _portals.Count; i++) {
			//find all connection's volume.
			GameObject _p = _portals [i].gameObject;
			GameObject _room = _p.transform.parent.parent.parent.gameObject;
			_rooms.Add (_p, _room);
			_portals [i].FrontSector = _room.GetComponentInChildren<SECTR_Sector> ();
		}
		string log1 = "<b>Linked Sectr_Portal:</b>\n";
		string log2 = "<b>Diasbled Sectr_Portal:</b>\n";
		for (int i = 0; i < _portals.Count; i++) {
			float _near = float.PositiveInfinity;
			GameObject _target = null;
			int _targetId = 0;
			Vector3 _start = _portals [i].transform.parent.position;
			//find nearst connection
			for (int j = 0; j < _portals.Count; j++) {
				Vector3 _end = _portals [j].transform.parent.position;
				float _dis = Vector3.Distance (_start, _end);
				if (i != j && _dis < _near && _dis < 3) {
					_near = _dis;
					_target = _portals [j].gameObject;
					_targetId = j;
				}
			}
			if (_target != null) {
				_portals [i].BackSector = _rooms [_target].GetComponentInChildren<SECTR_Sector> ();
				log1 += _rooms [_portals [i].gameObject].name + "<size=8>." + _portals [i].transform.parent.name
					+ "</size> <b><size=16>→</size></b> "
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
	}
}
