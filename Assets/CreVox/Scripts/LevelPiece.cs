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

		public bool IsSolid (Block.Direction direction)
		{
			int angle = (int)(gameObject.transform.localEulerAngles.y + 360) % 360;
			if (direction == Block.Direction.north) {
				if (isSolid [(int)Block.Direction.north] && angle == 0)
					return true;
				if (isSolid [(int)Block.Direction.east] && angle == 270)
					return true;
				if (isSolid [(int)Block.Direction.west] && angle == 90)
					return true;
				if (isSolid [(int)Block.Direction.south] && angle == 180)
					return true;
			}
			if (direction == Block.Direction.east) {
				if (isSolid [(int)Block.Direction.north] && angle == 90)
					return true;
				if (isSolid [(int)Block.Direction.east] && angle == 0)
					return true;
				if (isSolid [(int)Block.Direction.west] && angle == 180)
					return true;
				if (isSolid [(int)Block.Direction.south] && angle == 270)
					return true;
			}
			if (direction == Block.Direction.west) {
				if (isSolid [(int)Block.Direction.north] && angle == 270)
					return true;
				if (isSolid [(int)Block.Direction.east] && angle == 180)
					return true;
				if (isSolid [(int)Block.Direction.west] && angle == 0)
					return true;
				if (isSolid [(int)Block.Direction.south] && angle == 90)
					return true;
			}
			if (direction == Block.Direction.south) {
				if (isSolid [(int)Block.Direction.north] && angle == 180)
					return true;
				if (isSolid [(int)Block.Direction.east] && angle == 90)
					return true;
				if (isSolid [(int)Block.Direction.west] && angle == 270)
					return true;
				if (isSolid [(int)Block.Direction.south] && angle == 0)
					return true;
			}
			if (direction == Block.Direction.up) {
				if (isSolid [(int)Block.Direction.up])
					return true;
			}
			if (direction == Block.Direction.down) {
				if (isSolid [(int)Block.Direction.down])
					return true;
			}
			return false;
		}
	}
}