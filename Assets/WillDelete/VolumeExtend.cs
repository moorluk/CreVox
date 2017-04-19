using CreVox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Test {
	// Idk why the top and bottom is reverse.
	public enum DirectionOfBlock {
		LeftBottom,
		Bottom,/*???*/
		RightBottom,
		Left,
		Center,
		Right,
		LeftTop,
		Top,/*???*/
		RightTop
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
			return this.position.Compare(obj.position) && this.direction == obj.direction;
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
