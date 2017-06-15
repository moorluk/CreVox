using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace CreVox
{

	public class LevelPiece : MonoBehaviour
	{
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
			Global,
		}

		[Serializable]
		public class Hold
		{
			public WorldPos offset;
			public bool isSolid;
		}

		public PivotType pivot;
		public bool[] isSolid = new bool[6];
		public bool isHold = false;
		public List<Hold> holdBlocks;
		public int maxX, minX, maxY, minY, maxZ, minZ;

		public EventRange[] eventRange = new EventRange[6]
		{
			EventRange.Free,
			EventRange.Free,
			EventRange.Free,
			EventRange.Free,
			EventRange.Free,
			EventRange.Free
		};

        public virtual void SetupPiece(BlockItem item)
		{
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
	
		public void SendActorUpward (EventGroup e = EventGroup.Default)
		{
			EventActor[] acs = GetComponentsInChildren<EventActor>();
			foreach (EventActor a in acs) {
				if(e != EventGroup.Default)
					a.m_keyString += "." + e.ToString ();
				SendActorUpward (a,eventRange[5]);
			}
		}

		public static void SendActorUpward (EventActor a, EventRange range = EventRange.Free)
		{
			Transform edt = a.transform;
			EventDriver ed = GetDriver(edt,range);
			while (ed == null) {
				edt = edt.parent;
				ed = GetDriver(edt,range);
				if (Transform.Equals (edt, edt.root))
					break;
			}

			if (a.m_keyString != null && ed != null) {
				ed.RegisterActor (a);
			}
		}

		private static EventDriver GetDriver(Transform t, EventRange range)
		{
			EventDriver ed;
			switch (range){
			case EventRange.Marker:
				ed = t.GetComponent (typeof(MarkerDriver)) as EventDriver;
				break;

			case EventRange.Room:
				ed = t.GetComponent(typeof(RoomDriver)) as EventDriver;
				break;

			case EventRange.Global:
				ed = t.GetComponent(typeof(GlobalDriver)) as EventDriver;
				break;
				
			default:
			case EventRange.Free:
				ed = t.GetComponent<EventDriver> ();
				break;
			}
			return ed;
		}
	}
}