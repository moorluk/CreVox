using UnityEngine;
using System;
using System.Collections.Generic;
using CreVox;

public static class VolumeAdapter
{
    static Type sectrSector;
    static Type sectrPortal;

    public static void AfterVolumeInit (GameObject volume)
    {
        //event system
        Type eventDriver = Type.GetType ("RoomDriver");
        if (eventDriver != null && volume.GetComponent (eventDriver) == null)
            volume.AddComponent (eventDriver);
        //SECTR
        sectrSector = Type.GetType ("SECTR_Sector");
        GameObject root = (volume.transform.Find ("DecorationRoot")).gameObject;
        if (sectrSector != null) {
            var s = root.GetComponent(sectrSector) ?? root.AddComponent(sectrSector);
            ((Behaviour)s).enabled = false;
            //SECTR_Member.BoundsUpdateModes
            sectrSector.GetField("BoundsUpdateMode").SetValue(s, 3);
            //SECTR_Member.ChildCulling
            sectrSector.GetField("ChildCulling").SetValue(s, 1);
        }
    }
    public static void AfterLoadComplete()
    {
        if (VGlobal.GetSetting().setting.debugLog) Debug.Log("<color=teal>Load volumes complete</color> time: " + Time.realtimeSinceStartup);

        Type levelAgent = Type.GetType("LevelAgent");
        if (levelAgent != null)
        {
            var levelAgentInstance = levelAgent.GetProperty("Instance").GetGetMethod().Invoke(null, null) as MonoBehaviour;
            levelAgentInstance.SendMessage("LoadCompleted");
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

    static void UpdatePortalsByDis(GameObject root)
    {
        string log1 = "<b>Linked Sectr_Portal:</b>\n";
        string log2 = "<b>Diasbled Sectr_Portal:</b>\n";

        sectrPortal = Type.GetType("SECTR_Portal");
        if (sectrPortal != null)
        {
            var _portals = root.GetComponentsInChildren(sectrPortal);
            var _rooms = new Dictionary<GameObject, GameObject>();
            for (int i = 0; i < _portals.Length; i++)
                _rooms.Add(_portals[i].gameObject, _portals[i].transform.parent.parent.parent.gameObject);

            for (int i = 0; i < _portals.Length; i++)
            {
                float _nearDist = float.PositiveInfinity;
                GameObject _target = null;
                Vector3 _start = _portals[i].transform.parent.position;
                //find nearst connection.
                for (int j = 0; j < _portals.Length; j++)
                {
                    if (i == j) continue;
                    Vector3 _end = _portals[j].transform.parent.position;
                    float _TargetDist = Vector3.Distance(_start, _end);
                    if (_TargetDist < _nearDist && _TargetDist < 3)
                    {
                        _nearDist = _TargetDist;
                        _target = _portals[j].gameObject;
                    }
                }
                //if find legal target update portal, else disable portal.
                if (_target != null)
                {
                    //var f = ((SECTR_Portal)_portals[i]).BackSector;
                    var fs = _rooms[_portals[i].gameObject].GetComponentInChildren(sectrSector);
                    sectrPortal.GetProperty("FrontSector").SetValue(_portals[i], fs, null);
                    var bs = _rooms[_target].GetComponentInChildren(sectrSector);
                    sectrPortal.GetProperty("BackSector").SetValue(_portals[i], bs, null);

                    log1 += string.Format(
                        "{0}.<size=8>{1}</size>\t<b><size=16>→</size></b>\t{2}.<size=8>{3}</size>\t{4}\n",
                        _rooms[_portals[i].gameObject].name, _portals[i].transform.parent.name,
                        _rooms[_target].name, _target.transform.parent.name, _nearDist
                    );
                }
                else
                {
                    var pp = _portals[i].GetComponentInParent<PropertyPiece>();
                    log2 += string.Format("{0}<size=8>.{1}</size> ", _rooms[_portals[i].gameObject].name, _portals[i].transform.parent.name);
                    log2 += _rooms[_portals[i].gameObject].GetComponent<VolumeMaker>().FixDoor(pp);
                }
            }
            if (VGlobal.GetSetting().setting.debugLog) Debug.LogFormat("<color=teal>Update portal finish</color>\n{0}\n{1}", log1, log2);
        }
    }

    static void UpdatePortalsByInfo (GameObject root)
    {
        List<SECTR_Portal> _portals = new List<SECTR_Portal> ();
        root.GetComponentsInChildren (false, _portals);
        Dictionary<SECTR_Portal,Volume> _rooms = new Dictionary<SECTR_Portal, Volume> ();

        foreach (var _p in _portals) {
            //find all connection's volume.
            Volume _room = _p.transform.parent.parent.parent.gameObject.GetComponent<Volume> ();
            _rooms.Add (_p, _room);
        }

        string log = "<b>Linked Sectr_Portal:</b>\n";

        for (int i = 0; i < _portals.Count; i++) {
            Vector3 _start = _portals [i].transform.parent.position;
            Volume _vol = _rooms [_portals [i]];
            if (_vol.ConnectionInfos == null)
                continue;
            for (int c = 0; c < _vol.ConnectionInfos.Count; c++) {
                if (object.Equals(_vol.ConnectionInfos[c].position, _start)) {
                    _portals[i].FrontSector = _vol.gameObject.GetComponentInChildren<SECTR_Sector>();
                    _portals[i].BackSector = _vol.ConnectionInfos[c].connectedGameObject.GetComponentInChildren<SECTR_Sector>();
                }
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
