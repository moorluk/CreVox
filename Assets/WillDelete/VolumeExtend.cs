using CreVox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace CrevoxExtend {
	public struct DirectionOfBlock {
		public int angle;
		public DirectionOfBlock(float degree) {
			this.angle = (int) degree;
		}
		public DirectionOfBlock(int index) {
			this.angle = Index2Angle[index];
		}
		public int ToIndex() {
			if(angle == -1) {
				return 4;
			}
			return Angle2Index[angle / 45];
		}
		// Constant array.
		private static int[] Index2Angle = new int[] {
			135, 90, 45,
			180, -1, 0,
			225, 270, 315
		};
		private static int[] Angle2Index = new int[] { 5, 2, 1, 0, 3, 6, 7, 8 };

	}
	public class DoorInfo {
		public WorldPos position;
		public DirectionOfBlock direction;
		public bool used;
		public DoorInfo(WorldPos position, DirectionOfBlock direction) {
			this.position = position;
			this.direction = direction;
			used = false;
		}
		public DoorInfo(DoorInfo clone) {
			this.position = clone.position;
			this.direction = clone.direction;
			this.used = clone.used;
		}
		public DoorInfo Clone() {
			return new DoorInfo(this);
		}
		public bool Compare(DoorInfo obj) {
			return this.position.Compare(obj.position) && this.direction.angle == obj.direction.angle;
		}
		// Compute the position after rotated.
		public WorldPos AbsolutePosition(float degree) {
			Vector2 aPoint = new Vector2(position.x, position.z);
			// Set 4, 4 to be center point.
			aPoint -= new Vector2(4, 4);
			float rad = degree * Mathf.Deg2Rad;
			float sin = Mathf.Sin(rad);
			float cos = Mathf.Cos(rad);
			return new WorldPos((int) Mathf.Round( aPoint.x * cos + aPoint.y * sin + 4 ),
				position.y,
				(int) Mathf.Round( aPoint.y * cos - aPoint.x * sin + 4 ) );
		}
		// 
		public DirectionOfBlock AbsoluteDirection(float degree) {
			return new DirectionOfBlock(( degree + this.direction.angle ) % 360);
		}
	}
	public class VolumeExtend : MonoBehaviour {
		private List<DoorInfo> _doorInfos;
		public List<DoorInfo> DoorInfos {
			get { return _doorInfos; }
			set { _doorInfos = value; }
		}
	}
}
