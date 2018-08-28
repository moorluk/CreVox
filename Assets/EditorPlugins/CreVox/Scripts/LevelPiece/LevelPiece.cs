using UnityEngine;
using System;
using System.Collections.Generic;

namespace CreVox
{

    public class LevelPiece : MonoBehaviour
    {
        [Serializable]
        public struct PProperty
        {
            public bool tActive;
            public FocalComponent tComponent;
            public UnityEngine.Object tObject;
            public EventRange tRange;
        }

        public enum PivotType
        {
            Vertex,
            Edge,
            Center,
            Grid
        }

        public enum EventRange
        {
            Free,
            Marker,
            Room,
            Global
        }

        [Serializable]
        public class Hold
        {
            public WorldPos offset;
            public bool isSolid;
        }

        public Block block;
        public PivotType pivot;
        public bool[] isSolid = new bool[6];

        public EventRange eventRange = EventRange.Free;
        public PProperty[] PProperties = new PProperty[5];
        [HideInInspector]public static string[] breakChar = { "," };
        [HideInInspector]public static string[] breakCharSub = { "_" };

        public virtual void SetupPiece (BlockItem item)
        {
            //解析從blockitem的attritube,進行相應的動作.
            string[] _code = UpdateValue (ref item, 0);
            if (_code.Length != 0) {
                string[] t = _code [0].Split (breakChar, StringSplitOptions.None);
                switch (t.Length) {
                case 1:
                    t = new [] { "true", t [0], PProperties [0].tRange.ToString () };
                    break;

                case 2:
                    t = new [] { "true", t [0], t [1] };
                    break;
                }
                if (t [0] == "true") {
                    PProperties [0].tActive = true;
                    if (t [1] == "DefaultEventRange") {
                        eventRange = (LevelPiece.EventRange)Enum.Parse (typeof(LevelPiece.EventRange), t [2]);
                    }
                } else {
                    PProperties [0].tActive = false;
                }
            }
            SendActorUpward ();
        }

        public bool IsSolid (Direction direction)
        {
            int angle = (int)(gameObject.transform.localEulerAngles.y + 360) % 360;
            if (direction == Direction.north) {
                if (isSolid [(int)Direction.north] && angle == 0)
                    return true;
                if (isSolid [(int)Direction.east] && angle == 270)
                    return true;
                if (isSolid [(int)Direction.west] && angle == 90)
                    return true;
                if (isSolid [(int)Direction.south] && angle == 180)
                    return true;
            }
            if (direction == Direction.east) {
                if (isSolid [(int)Direction.north] && angle == 90)
                    return true;
                if (isSolid [(int)Direction.east] && angle == 0)
                    return true;
                if (isSolid [(int)Direction.west] && angle == 180)
                    return true;
                if (isSolid [(int)Direction.south] && angle == 270)
                    return true;
            }
            if (direction == Direction.west) {
                if (isSolid [(int)Direction.north] && angle == 270)
                    return true;
                if (isSolid [(int)Direction.east] && angle == 180)
                    return true;
                if (isSolid [(int)Direction.west] && angle == 0)
                    return true;
                if (isSolid [(int)Direction.south] && angle == 90)
                    return true;
            }
            if (direction == Direction.south) {
                if (isSolid [(int)Direction.north] && angle == 180)
                    return true;
                if (isSolid [(int)Direction.east] && angle == 90)
                    return true;
                if (isSolid [(int)Direction.west] && angle == 270)
                    return true;
                if (isSolid [(int)Direction.south] && angle == 0)
                    return true;
            }
            if (direction == Direction.up) {
                if (isSolid [(int)Direction.up])
                    return true;
            }
            if (direction == Direction.down) {
                if (isSolid [(int)Direction.down])
                    return true;
            }
            return false;
        }

        public virtual void SendActorUpward (EventGroup e = EventGroup.Default)
        {
            EventActor[] acs = GetComponentsInChildren<EventActor> ();
            foreach (EventActor a in acs) {
                Debug.Log ("<b>" + name + "</b> send <b> " + a.name + "</b> to <b>range(" + eventRange + ")</b>\n");
                SendActorUpward (a, eventRange);
            }
        }

        public static void SendActorUpward (EventActor a, EventRange range = EventRange.Free)
        {
            Transform edt = a.transform;
            EventDriver ed = GetDriver (edt, range);
            while (ed == null) {
                edt = edt.parent;
                ed = GetDriver (edt, range);
                if (Transform.Equals (edt, edt.root))
                    break;
            }

            if (a.m_keyString != null && ed != null) {
                ed.RegisterActor (a);
            }
        }

        public static string[] UpdateValue (ref BlockItem item, int id)
        {
            if (string.IsNullOrEmpty (item.attributes [id]))
                return new string[0];
            return item.attributes [id].Split (new []{ ";" }, StringSplitOptions.None);
        }

        static EventDriver GetDriver (Transform t, EventRange range)
        {
            EventDriver ed;
            switch (range) {
            case EventRange.Marker:
                ed = t.GetComponent (typeof(MarkerDriver)) as EventDriver;
                break;

            case EventRange.Room:
                ed = t.GetComponent (typeof(RoomDriver)) as EventDriver;
                break;

            case EventRange.Global:
                ed = t.GetComponent (typeof(GlobalDriver)) as EventDriver;
                break;

            case EventRange.Free:
            default:
                ed = t.GetComponent<EventDriver> ();
                break;
            }
            return ed;
        }
    }
}