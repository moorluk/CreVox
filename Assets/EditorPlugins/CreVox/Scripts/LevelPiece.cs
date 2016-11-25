using UnityEngine;
using System.Collections;
using System;

namespace CreVox
{

	public class LevelPiece : MonoBehaviour
	{

		public enum PivotType
		{
			Vertex,
			Edge,
			Center,
			Grid,
		}

		public PivotType pivot;
		public bool isStair = false;
		public bool[] isSolid = new bool[6];

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
	}
}