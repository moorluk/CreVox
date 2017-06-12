using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using CreVox;

public enum ATBT_EVN_PCE
{
    EVENTType,
}
public enum EventGroup
{
	Blue,
	Green,
	Magenta,
	Red,
	Yellow,
	Cyan
}
public class EventPiece : LevelPiece {

    private EventGroup eventGrp;
    private Color pieceColor;

	public override void SetupPiece (BlockItem item) {
        if(item.attributes[(int)ATBT_EVN_PCE.EVENTType] == "")
        {
            item.attributes[(int)ATBT_EVN_PCE.EVENTType] = EventGroup.Red.ToString();
        }

        eventGrp = (EventGroup)Enum.Parse(typeof(EventGroup), item.attributes[(int)ATBT_EVN_PCE.EVENTType]);
        pieceColor = GetColor(eventGrp);

		TriggerEvent[] tes = GetComponentsInChildren<TriggerEvent> ();
		foreach (TriggerEvent te in tes) {
			List<string> msgs = te.m_keyStrings;
			for (int i = 0; i < msgs.Count; i++) {
				//if(msgs[i].Contains("global"))
					te.m_keyStrings[i] += " " + eventGrp.ToString ();
			}
		}

        EventActor[] acs = GetComponentsInChildren<EventActor>();
        foreach (EventActor a in acs)
        {
            //string msg = a.m_keyString;
            //if (msg.Contains ("global")) {
            a.m_keyString += " " + eventGrp.ToString();
            //}

            if (a.m_keyString != null)
            {
#if UNITY_STANDALONE_WIN
                Debug.Log("RegisterActor" + GetInstanceID());
                a.SendMessageUpwards("RegisterActor", a);
#endif
            }
        }
    }

    void Start()
    {

    }

	void OnDrawGizmos() {
        //Debug.Log("Draw Piece!" + this.GetInstanceID().ToString());
        MeshFilter[] ms = GetComponentsInChildren<MeshFilter> ();
        Color oldColor = Gizmos.color;
        Gizmos.color = pieceColor;
        //Debug.Log("OnDrawGizmos piece color: " + pieceColor.ToString());

        foreach (MeshFilter m in ms) {
			Transform t = m.gameObject.transform;
            Gizmos.DrawMesh(m.sharedMesh, t.position, t.rotation, t.localScale * 1.02f);
		}

        Gizmos.color = oldColor;
    }

    Color GetColor(EventGroup grp) {
		switch (grp) {
		case EventGroup.Blue:
			return Color.blue;

		case EventGroup.Green:
			return Color.green;

		case EventGroup.Magenta:
			return Color.magenta;

		case EventGroup.Red:
			return Color.red;

		case EventGroup.Yellow:
			return Color.yellow;
		}

		return Color.cyan;
	}
}
