using UnityEngine;
using System;
using System.Collections.Generic;

public static class VolumeAdapter
{

    public static void AfterVolumeInit (GameObject volume)
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

    static int useSetupDungeon = -1;

    public static bool CheckSetupDungeon ()
    {
        if (useSetupDungeon < 0) {
            Type t = Type.GetType ("SetupDungeon");
            UnityEngine.Object gg = null;
            if (t != null)
                gg = UnityEngine.Object.FindObjectOfType (t);
            useSetupDungeon = (gg != null && ((Component)gg).gameObject.activeSelf) ? 1 : 0;
        }

        return (useSetupDungeon > 0);
    }

    public static bool CheckActiveComponent (String _type)
    {
        Type t = Type.GetType (_type);
        UnityEngine.Object gg = null;
        if (t != null)
            gg = UnityEngine.Object.FindObjectOfType (t);
        return (gg != null && ((Component)gg).gameObject.activeSelf);
    }

    public static void UpdatePortals (GameObject root)
    {
//		if (CreVox.VGlobal.GetSetting ().Generation && Application.isPlaying)
//			UpdatePortalsByInfo (root);
//		else
        UpdatePortalsByDis (root);
    }

    static void UpdatePortalsByDis (GameObject root)
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
                log1 += string.Format (
                    "{0}.<size=8>{1}</size>  <b><size=16>→</size></b> {2}.<size=8>{3}</size>\n", 
                    _rooms [_portals [i].gameObject].name, 
                    _portals [i].transform.parent.name, 
                    _rooms [_target].name, 
                    _target.transform.parent.name
                );
            } else {
                _portals [i].enabled = false;
                log2 += string.Format ("{0}<size=8>.{1}</size>\n", _rooms [_portals [i].gameObject].name, _portals [i].transform.parent.name);
            }
        }

        Debug.Log (log1 + "\n" + log2);
    }

    static void UpdatePortalsByInfo (GameObject root)
    {
        List<SECTR_Portal> _portals = new List<SECTR_Portal> ();
        root.GetComponentsInChildren (false, _portals);
        Dictionary<SECTR_Portal,CreVox.Volume> _rooms = new Dictionary<SECTR_Portal, CreVox.Volume> ();

        foreach (var _p in _portals) {
            //find all connection's volume.
            CreVox.Volume _room = _p.transform.parent.parent.parent.gameObject.GetComponent<CreVox.Volume> ();
            _rooms.Add (_p, _room);
        }

        string log = "<b>Linked Sectr_Portal:</b>\n";

        for (int i = 0; i < _portals.Count; i++) {
            Vector3 _start = _portals [i].transform.parent.position;
            CreVox.Volume _vol = _rooms [_portals [i]];
            if (_vol.ConnectionInfos == null)
                continue;
            for (int c = 0; c < _vol.ConnectionInfos.Count; c++) {
                if (object.Equals (_vol.ConnectionInfos [c].position, _start))
                    _portals [i].FrontSector = _vol.gameObject.GetComponentInChildren<SECTR_Sector> ();
                _portals [i].BackSector = _vol.ConnectionInfos [c].connectedGameObject.GetComponentInChildren<SECTR_Sector> ();
                log += string.Format (
                    "{0}<size=8>.{1}  <b><size=16>→</size></b> {2}.{3}</size>\n", 
                    _vol.gameObject.name, 
                    _vol.transform.parent.name, 
                    _vol.ConnectionInfos [c].connectedGameObject.name, 
                    _vol.ConnectionInfos [c].connectedGameObject.transform.parent.name
                );
            }
        }
        Debug.Log (log);
    }
}
