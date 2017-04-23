using CreVox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace CrevoxExtend {
	public struct DirectionOfBlock {
		public int Angle;
		public DirectionOfBlock(float degree) {
			this.Angle = (int) degree;
		}
		public DirectionOfBlock(int index) {
			this.Angle = Index2Angle[index];
		}
		public WorldPos RelativePosition {
			get { return DirectionOffset[this.Angle / 90]; }
		}
		// Constant array.
		private static int[] Index2Angle = new int[] { 90, 180, 0, 270 };
		public static WorldPos[] DirectionOffset = new WorldPos[] {
			new WorldPos(1, 0, 0),
			new WorldPos(0, 0, -1),
			new WorldPos(-1, 0, 0),
			new WorldPos(0, 0, 1)
		};

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
			return this.position.Compare(obj.position) && this.direction.Angle == obj.direction.Angle;
		}
		// 
		public DirectionOfBlock AbsoluteDirection(float degree) {
			return new DirectionOfBlock(( degree + this.direction.Angle ) % 360);
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
