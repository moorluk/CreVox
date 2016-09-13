using UnityEngine;
using System.Collections;
using UnityEditor;

namespace CreVox
{
	public enum CamZoneType
	{
		front,
		left,
		right,
		left_right,
		up,
		down,
		back
	}

	public enum CamDir
	{
		//none,
		front = 1 << 0,
		left = 1 << 1,
		back = 1 << 2,
		right = 1 << 3,
		turn_left = 1 << 4,
		turn_right = 1 << 5,
		turn_up = 1 << 6,
		turn_down = 1 << 7,
		turn_none = 1 << 8,
		to_wall = 1 << 9
	}

	public class AutoCamManager : MonoBehaviour
	{
		public CamDir mainDir = CamDir.front;

		[SerializeField]
		private GameObject[] camZonePreset;
		public GameObject[] camZones = new GameObject[3 * 3];

		public int[] obsLayer = new int[3 * 3];
		public int[] sclLayer = new int[3 * 3];
		public int[] adjLayer = new int[3 * 3];
		public int[] dirLayer = new int[3 * 3];
		public int[] oldIDLayer = new int[3 * 3];
		public int[] idLayer = new int[3 * 3];

		public Transform target;
		private GameObject camNode;
		private CreVox.WorldPos oldPos;
		public CreVox.WorldPos curPos;
		public CreVox.World world;

		void Start ()
		{
			world = GetComponent<CreVox.World> ();
			target = GameObject.FindGameObjectWithTag ("Player").transform;
			curPos = CreVox.EditTerrain.GetBlockPos (target.position);
			oldPos = curPos;

			dirLayer [4] = (int)mainDir;

//			UpdateObstacleLayer ();
			//UpdateScrollLayer (99, 99);
			UpdateAdjecentLayer ();				
			for (int i = 0; i < sclLayer.Length; i++)
				sclLayer [i] = -1;
			for (int i = 0; i < idLayer.Length; i++)
				idLayer [i] = i;
			//CalcCamDir ();
			LoadPreset ();
			InitCamZones ();
//			UpdateCamZones ();
		}

		void LoadPreset ()
		{
			camZonePreset = new GameObject[System.Enum.GetValues (typeof(CamZoneType)).Length];

			camZonePreset [(int)CamZoneType.front] = Resources.Load (PathCollect.camSettingPath + "/AutoCamZone_F") as GameObject;
			camZonePreset [(int)CamZoneType.left] = Resources.Load (PathCollect.camSettingPath + "/AutoCamZone_L") as GameObject;
			camZonePreset [(int)CamZoneType.right] = Resources.Load (PathCollect.camSettingPath + "/AutoCamZone_R") as GameObject;
			camZonePreset [(int)CamZoneType.left_right] = Resources.Load (PathCollect.camSettingPath + "/AutoCamZone_LR") as GameObject;
			camZonePreset [(int)CamZoneType.up] = Resources.Load (PathCollect.camSettingPath + "/AutoCamZone_U") as GameObject;
			camZonePreset [(int)CamZoneType.down] = Resources.Load (PathCollect.camSettingPath + "/AutoCamZone_D") as GameObject;
			camZonePreset [(int)CamZoneType.back] = Resources.Load (PathCollect.camSettingPath + "/AutoCamZone_B") as GameObject;
		}

		void InitCamZones ()
		{
			camNode = new GameObject ();
			camNode.name = "CameraZones";
			camNode.transform.parent = this.transform;
			for (int i = 0; i < camZones.Length; i++) {
				float x = (i % 3 - 1) * Block.w;
				float y = 0f;
				float z = ((int)(i / 3) - 1) * Block.d;
				GameObject co = GameObject.Instantiate (camZonePreset [(int)CamZoneType.front], Vector3.zero, Quaternion.identity) as GameObject;
				co.name = "zone" + i.ToString ();
				co.transform.localPosition = new Vector3 (x, y, z);
				co.transform.parent = camNode.transform;
				camZones [i] = co;
			}
		}

		void Update ()
		{
			curPos = CreVox.EditTerrain.GetBlockPos (target.position);
			int offsetX = curPos.x - oldPos.x;
			int offsetY = curPos.y - oldPos.y;
			int offsetZ = curPos.z - oldPos.z;

			// /*270907n : 更新周邊 Voxel 阻擋資訊*/
			UpdateObstacleLayer ();

			// /*270907n : 確認位移*/
			if (offsetY != 0) {
				for (int i = 0; i < sclLayer.Length; i++)
					sclLayer [i] = -1;
			} else if (offsetX != 0 || offsetZ != 0)
				UpdateScrollLayer (offsetX, offsetZ);

			// /*270907n : 計算面向*/
			mainDir = (CamDir)(dirLayer [4] & ((1 << 4) - 1));
			UpdateAdjecentLayer ();

			CalcCamDir ();

			// /*270907n : 更新ID*/
			if (offsetX != 0 || offsetZ != 0)
				UpdateIDLayer (offsetX, offsetZ);
						
			// /*270907n : 更新鏡頭*/
			UpdateCamZones ();
			oldPos = curPos;
		}

		void UpdateObstacleLayer ()
		{
			for (int i = 0; i < obsLayer.Length; i++) {
				int dx = curPos.x + i % 3 - 1;
				int dz = curPos.z + (int)(i / 3) - 1;
				CreVox.BlockAir b = world.GetBlock (dx, curPos.y, dz) as CreVox.BlockAir;
				if (b != null)
					obsLayer [i] = 1;
				else
					obsLayer [i] = 0;
			}
		}

		void UpdateScrollLayer (int _offsetX, int _offsetZ)
		{
			Debug.Log ("offset: " + _offsetX.ToString () + "," + _offsetZ.ToString ());
			for (int i = 0; i < sclLayer.Length; i++) {
				int x = (i % 3) + _offsetX;
				int z = (int)(i / 3) + _offsetZ;

				if (x >= 0 && z >= 0 && x < 3 && z < 3) {
					sclLayer [i] = dirLayer [x + z * 3];
				} else
					sclLayer [i] = -1;
			}
		}

		public int Turn (int _index)
		{
			var srcIndex = new int[] {
				0, 1, 2,
				3, 4, 5,
				6, 7, 8
			};

			int[] dstIndex = srcIndex;

			switch (mainDir) {
			case CamDir.front:
				dstIndex = srcIndex;
				break;

			case CamDir.left:
				dstIndex = new int[] {
					2, 5, 8,
					1, 4, 7,
					0, 3, 6
				};
				break;

			case CamDir.right:
				dstIndex = new int[] {
					6, 3, 0,
					7, 4, 1,
					8, 5, 2
				};
				break;

			case CamDir.back:
				dstIndex = new int[] {
					8, 7, 6,
					5, 4, 3,
					2, 1, 0
				};
				break;
			}

			int result = dstIndex [srcIndex [_index]];
			return result;
		}
		public CamDir Turn (CamDir _dir)
		{
			var srcDir = new CamDir[] {
				CamDir.front,
				CamDir.left,
				CamDir.back,
				CamDir.right
			};

			switch (mainDir) {
			case CamDir.front:
				break;

			case CamDir.left:
				srcDir = new CamDir[] {
					CamDir.left,
					CamDir.back,
					CamDir.right,
					CamDir.front
				};
				break;

			case CamDir.back:
				srcDir = new CamDir[] {
					CamDir.back,
					CamDir.right,
					CamDir.front,
					CamDir.left
				};
				break;

			case CamDir.right:
				srcDir = new CamDir[] {
					CamDir.right,
					CamDir.front,
					CamDir.left,
					CamDir.back
				};
				break;
			}

			CamDir result = _dir ;

			for (int i = 0; i < 4; i++) {
				if ((int)_dir == 1 << i) {
					result = srcDir [i];
				}
			}

			return result;
		}
		public Block.Direction Turn (Block.Direction _dir)
		{
			var srcDir = new Block.Direction[] {
				Block.Direction.north,
				Block.Direction.west,
				Block.Direction.south,
				Block.Direction.east
			};

			switch (mainDir) {
			case CamDir.front:
				break;

			case CamDir.left:
				srcDir = new Block.Direction[] {
					Block.Direction.west,
					Block.Direction.south,
					Block.Direction.east,
					Block.Direction.north
				};
				break;

			case CamDir.back:
				srcDir = new Block.Direction[] {
					Block.Direction.south,
					Block.Direction.east,
					Block.Direction.north,
					Block.Direction.west
				};
				break;

			case CamDir.right:
				srcDir = new Block.Direction[] {
					Block.Direction.east,
					Block.Direction.north,
					Block.Direction.west,
					Block.Direction.south
				};
				break;
			}

			Block.Direction result = _dir ;

			for (int i = 0; i < 4; i++) {
				if ((int)_dir == 1 << i) {
					result = srcDir [i];
				}
			}

			return result;
		}
		WorldPos GetNeighbor (WorldPos _base, int _index)
		{
			int _x = (_index % 3) - 1;
			int _z = (int)(_index / 3) - 1;

			WorldPos result;
			result.x = _base.x + _x;
			result.y = _base.y;
			result.z = _base.z + _z;

			return result;
		}
			
		void UpdateAdjecentLayer ()
		{
//			if (((int)mainDir & (int)CamDir.front) != 0)
//				UpdateFront ();
//			if (((int)mainDir & (int)CamDir.left) != 0)
//				UpdateLeft ();
//			if (((int)mainDir & (int)CamDir.back) != 0)
//				UpdateBack ();
//			if (((int)mainDir & (int)CamDir.right) != 0)
//				UpdateRight ();
//		}
//		
//		void UpdateFront ()
//		{
//
//			Debug.LogWarning ("Front:6 -> " + Turn (6));
//			Debug.LogWarning ("Front:front -> " + Turn (CamDir.front));
			ResetAdjecentLayer (mainDir);

			// ↖
			if (!IsVisible (GetNeighbor(curPos,Turn (6)), Turn (CamDir.left))) {
				if (obsLayer [Turn (6)] != 0) {
					adjLayer [Turn (6)] = (int)Turn (CamDir.left);
					if (obsLayer [Turn (7)] != 0)
						adjLayer [Turn (7)] |= (int)CamDir.turn_left;
				}
			}

			// ↗
			if (!IsVisible (GetNeighbor(curPos,Turn (8)), Turn (CamDir.right))) {
				if (obsLayer [Turn (8)] != 0) {
					adjLayer [Turn (8)] = (int)Turn (CamDir.right);
					if (obsLayer [Turn (7)] != 0)
						adjLayer [Turn (7)] |= (int)CamDir.turn_right;
				}
			}

			// ↑
			if (!IsVisible (GetNeighbor(curPos,Turn (7)), Turn (CamDir.front))) {
				if (obsLayer [Turn (7)] != 0) {
//					if (obsLayer [Turn (6)] == 0 && obsLayer [Turn (8)] == 0)
						adjLayer [Turn (7)] = (int)Turn (CamDir.back) + (int)CamDir.to_wall;
				}
			}

			// ←
			if (!IsVisible (GetNeighbor(curPos,Turn (3)), Turn (CamDir.left))) { 
				if (obsLayer [Turn (3)] != 0) {
					adjLayer [Turn (3)] = (int)Turn (CamDir.left);
					adjLayer [Turn (4)] |= (int)CamDir.turn_left;
				}
			}

			// →
			if (!IsVisible (GetNeighbor(curPos,Turn (5)), Turn (CamDir.right))) { 
				if (obsLayer [Turn (5)] != 0) {
					adjLayer [Turn (5)] = (int)Turn (CamDir.right);
					adjLayer [Turn (4)] |= (int)CamDir.turn_right;
				}
			}

			// ↙
			if (!IsVisible (GetNeighbor(curPos,Turn (0)), Turn (CamDir.left))) { 
				if (obsLayer [Turn (0)] != 0) {
					adjLayer [Turn (0)] = (int)Turn (CamDir.left);
					if (obsLayer [Turn (1)] != 0)
						adjLayer [Turn (1)] |= (int)CamDir.turn_left;
				}
			}
			// ↘
			if (!IsVisible (GetNeighbor(curPos,Turn (2)), Turn (CamDir.right))) { 
				if (obsLayer [Turn (2)] != 0) {
					adjLayer [Turn (2)] = (int)Turn (CamDir.right);
					if (obsLayer [Turn (1)] != 0)
						adjLayer [Turn (1)] |= (int)CamDir.turn_right;
				}
			}

			// ↓
			if (!IsVisible (GetNeighbor(curPos,Turn (1)), Turn(CamDir.front))) {
				if (obsLayer [Turn (1)] != 0)
					adjLayer [Turn (1)] = (int)Turn (CamDir.front) + (int)CamDir.to_wall;
			}
		}
/*
		void UpdateBack ()
		{
			Debug.LogWarning ("Back:6 -> " + Turn (6));
			Debug.LogWarning ("Back:front -> " + Turn (CamDir.front));
			ResetAdjecentLayer (mainDir);
			//CreVox.BlockAir b = null;
			//b = world.GetBlock(curPos.x - 1, curPos.y, curPos.z) as CreVox.BlockAir;
			//if (b == null)
			if (!IsVisible (curPos.x - 1, curPos.y, curPos.z - 1, CamDir.right)) {
				if (obsLayer [0 + 0 * 3] != 0) {
					adjLayer [0 + 0 * 3] = (int)CamDir.left;
					if (obsLayer [1 + 0 * 3] != 0)
						adjLayer [1 + 0 * 3] |= (int)CamDir.turn_right;
				}
			}
			//b = world.GetBlock(curPos.x + 1, curPos.y, curPos.z) as CreVox.BlockAir;
			//if (b == null)
			if (!IsVisible (curPos.x + 1, curPos.y, curPos.z - 1, CamDir.left)) {
				if (obsLayer [2 + 0 * 3] != 0) {
					adjLayer [2 + 0 * 3] = (int)CamDir.right;
					if (obsLayer [1 + 0 * 3] != 0)
						adjLayer [1 + 0 * 3] |= (int)CamDir.turn_left;
				}
			}

			//b = world.GetBlock(curPos.x - 1, curPos.y, curPos.z + 1) as CreVox.BlockAir;
			//if (b == null)
			if (!IsVisible (curPos.x - 1, curPos.y, curPos.z, CamDir.right)) {
				if (obsLayer [0 + 1 * 3] != 0) {
					adjLayer [0 + 1 * 3] = (int)CamDir.left;
					if (obsLayer [1 + 1 * 3] != 0)
						adjLayer [1 + 1 * 3] |= (int)CamDir.turn_right;
				}
			}
			//b = world.GetBlock(curPos.x + 1, curPos.y, curPos.z + 1) as CreVox.BlockAir;
			//if (b == null)
			if (!IsVisible (curPos.x + 1, curPos.y, curPos.z, CamDir.left)) {
				if (obsLayer [2 + 1 * 3] != 0) {
					adjLayer [2 + 1 * 3] = (int)CamDir.right;
					if (obsLayer [1 + 1 * 3] != 0)
						adjLayer [1 + 1 * 3] |= (int)CamDir.turn_left;
				}
			}

			//b = world.GetBlock(curPos.x - 1, curPos.y, curPos.z + 2) as CreVox.BlockAir;
			//if (b == null)
			if (!IsVisible (curPos.x - 1, curPos.y, curPos.z + 1, CamDir.right)) {
				if (obsLayer [0 + 2 * 3] != 0) {
					adjLayer [0 + 2 * 3] = (int)CamDir.left;
					if (obsLayer [1 + 2 * 3] != 0)
						adjLayer [1 + 2 * 3] |= (int)CamDir.turn_right;
				}
			}
			//b = world.GetBlock(curPos.x + 1, curPos.y, curPos.z + 2) as CreVox.BlockAir;
			//if (b == null)
			if (!IsVisible (curPos.x + 1, curPos.y, curPos.z + 1, CamDir.left)) {
				if (obsLayer [2 + 2 * 3] != 0) {
					adjLayer [2 + 2 * 3] = (int)CamDir.right;
					if (obsLayer [1 + 2 * 3] != 0)
						adjLayer [1 + 2 * 3] |= (int)CamDir.turn_left;
				}
			}

			//b = world.GetBlock(curPos.x, curPos.y, curPos.z + 2) as CreVox.BlockAir;
			//if (b == null)
			if (!IsVisible (curPos.x, curPos.y, curPos.z + 1)) {
				if (obsLayer [1 + 2 * 3] != 0)
					adjLayer [1 + 2 * 3] = ((int)CamDir.turn_none) + ((int)CamDir.back);
			}
		}

		void UpdateRight ()
		{
			Debug.LogWarning ("Right:6 -> " + Turn (6));
			Debug.LogWarning ("Right:front -> " + Turn (CamDir.front));
			ResetAdjecentLayer (mainDir);
			//CreVox.BlockAir b = null;
			//b = world.GetBlock(curPos.x, curPos.y, curPos.z + 1) as CreVox.BlockAir;
			//if (b == null)
			if (!IsVisible (curPos.x + 1, curPos.y, curPos.z + 1, CamDir.left)) {
				if (obsLayer [2 + 2 * 3] != 0) {
					adjLayer [2 + 2 * 3] = (int)CamDir.front;
					if (obsLayer [2 + 1 * 3] != 0)
						adjLayer [2 + 1 * 3] |= (int)CamDir.turn_left;
				}
			}
			//b = world.GetBlock(curPos.x, curPos.y, curPos.z - 1) as CreVox.BlockAir;
			//if (b == null)
			if (!IsVisible (curPos.x + 1, curPos.y, curPos.z - 1, CamDir.right)) {
				if (obsLayer [2 + 0 * 3] != 0) {
					adjLayer [2 + 0 * 3] = (int)CamDir.back;
					if (obsLayer [2 + 1 * 3] != 0)
						adjLayer [2 + 1 * 3] |= (int)CamDir.turn_right;
				}
			}

			//b = world.GetBlock(curPos.x - 1, curPos.y, curPos.z + 1) as CreVox.BlockAir;
			//if (b == null)
			if (!IsVisible (curPos.x, curPos.y, curPos.z + 1, CamDir.left)) {
				if (obsLayer [1 + 2 * 3] != 0) {
					adjLayer [1 + 2 * 3] = (int)CamDir.front;
					if (obsLayer [1 + 1 * 3] != 0)
						adjLayer [1 + 1 * 3] |= (int)CamDir.turn_left;
				}
			}
			//b = world.GetBlock(curPos.x - 1, curPos.y, curPos.z - 1) as CreVox.BlockAir;
			//if (b == null)
			if (!IsVisible (curPos.x, curPos.y, curPos.z - 1, CamDir.right)) {
				if (obsLayer [1 + 0 * 3] != 0) {
					adjLayer [1 + 0 * 3] = (int)CamDir.back;
					if (obsLayer [1 + 1 * 3] != 0)
						adjLayer [1 + 1 * 3] |= (int)CamDir.turn_right;
				}
			}

			//b = world.GetBlock(curPos.x - 2, curPos.y, curPos.z + 1) as CreVox.BlockAir;
			//if (b == null)
			if (!IsVisible (curPos.x - 1, curPos.y, curPos.z + 1, CamDir.left)) {
				if (obsLayer [0 + 2 * 3] != 0) {
					adjLayer [0 + 2 * 3] = (int)CamDir.front;
					if (obsLayer [0 + 1 * 3] != 0)
						adjLayer [0 + 1 * 3] |= (int)CamDir.turn_left;
				}
			}
			//b = world.GetBlock(curPos.x - 2, curPos.y, curPos.z - 1) as CreVox.BlockAir;
			//if (b == null)
			if (!IsVisible (curPos.x - 1, curPos.y, curPos.z - 1, CamDir.right)) {
				if (obsLayer [0 + 0 * 3] != 0) {
					adjLayer [0 + 0 * 3] = (int)CamDir.back;
					if (obsLayer [0 + 1 * 3] != 0)
						adjLayer [0 + 1 * 3] |= (int)CamDir.turn_right;
				}
			}

			//b = world.GetBlock(curPos.x - 2, curPos.y, curPos.z) as CreVox.BlockAir;
			//if (b == null)
			if (!IsVisible (curPos.x - 1, curPos.y, curPos.z)) {
				if (obsLayer [0 + 1 * 3] != 0)
					adjLayer [0 + 1 * 3] = ((int)CamDir.turn_none) + ((int)CamDir.right);
			}
		}

		void UpdateLeft ()
		{
			Debug.LogWarning ("Left:6 -> " + Turn (6));
			Debug.LogWarning ("Left:front -> " + Turn (CamDir.front));
			ResetAdjecentLayer (mainDir);
			//CreVox.BlockAir b = null;
			//b = world.GetBlock(curPos.x, curPos.y, curPos.z + 1) as CreVox.BlockAir;
			//if (b == null)
			if (!IsVisible (curPos.x - 1, curPos.y, curPos.z + 1, CamDir.right)) {
				if (obsLayer [0 + 2 * 3] != 0) {
					adjLayer [0 + 2 * 3] = (int)CamDir.front;
					if (obsLayer [0 + 1 * 3] != 0)
						adjLayer [0 + 1 * 3] |= (int)CamDir.turn_right;
				}
			}
			//b = world.GetBlock(curPos.x, curPos.y, curPos.z - 1) as CreVox.BlockAir;
			//if (b == null)
			if (!IsVisible (curPos.x - 1, curPos.y, curPos.z - 1, CamDir.left)) {
				if (obsLayer [0 + 0 * 3] != 0) {
					adjLayer [0 + 0 * 3] = (int)CamDir.back;
					if (obsLayer [0 + 1 * 3] != 0)
						adjLayer [0 + 1 * 3] |= (int)CamDir.turn_left;
				}
			}

			//b = world.GetBlock(curPos.x + 1, curPos.y, curPos.z + 1) as CreVox.BlockAir;
			//if (b == null)
			if (!IsVisible (curPos.x, curPos.y, curPos.z + 1, CamDir.right)) {
				if (obsLayer [1 + 2 * 3] != 0) {
					adjLayer [1 + 2 * 3] = (int)CamDir.front;
					if (obsLayer [1 + 1 * 3] != 0)
						adjLayer [1 + 1 * 3] |= (int)CamDir.turn_right;
				}
			}
			//b = world.GetBlock(curPos.x + 1, curPos.y, curPos.z - 1) as CreVox.BlockAir;
			//if (b == null)
			if (!IsVisible (curPos.x, curPos.y, curPos.z - 1, CamDir.left)) {
				if (obsLayer [1 + 0 * 3] != 0) {
					adjLayer [1 + 0 * 3] = (int)CamDir.back;
					if (obsLayer [1 + 1 * 3] != 0)
						adjLayer [1 + 1 * 3] |= (int)CamDir.turn_left;
				}
			}

			//b = world.GetBlock(curPos.x + 2, curPos.y, curPos.z + 1) as CreVox.BlockAir;
			//if (b == null)
			if (!IsVisible (curPos.x + 1, curPos.y, curPos.z + 1, CamDir.right)) {
				if (obsLayer [2 + 2 * 3] != 0) {
					adjLayer [2 + 2 * 3] = (int)CamDir.front;
					if (obsLayer [2 + 1 * 3] != 0)
						adjLayer [2 + 1 * 3] |= (int)CamDir.turn_right;
				}
			}
			//b = world.GetBlock(curPos.x + 2, curPos.y, curPos.z - 1) as CreVox.BlockAir;
			//if (b == null)
			if (!IsVisible (curPos.x + 1, curPos.y, curPos.z - 1, CamDir.left)) {
				if (obsLayer [2 + 0 * 3] != 0) {
					adjLayer [2 + 0 * 3] = (int)CamDir.back;
					if (obsLayer [2 + 1 * 3] != 0)
						adjLayer [2 + 1 * 3] |= (int)CamDir.turn_left;
				}
			}

			//b = world.GetBlock(curPos.x + 2, curPos.y, curPos.z) as CreVox.BlockAir;
			//if (b == null)
			if (!IsVisible (curPos.x + 1, curPos.y, curPos.z)) {
				if (obsLayer [2 + 1 * 3] != 0)
					adjLayer [2 + 1 * 3] = ((int)CamDir.turn_none) + ((int)CamDir.left);
			}
		}
*/
		void ResetAdjecentLayer (CamDir _dir)
		{
			int v = (int)_dir;
			for (int i = 0; i < dirLayer.Length; i++) {
				adjLayer [i] = v;
			}
		}

		private bool IsVisible (WorldPos _pos, CamDir _lookDir)
		{
			CreVox.BlockAir centerB = world.GetBlock (_pos.x, _pos.y, _pos.z) as CreVox.BlockAir;

			WorldPos backB_pos = GetNeighbor(_pos,Turn (1));
			CreVox.BlockAir backB = world.GetBlock (backB_pos.x, backB_pos.y, backB_pos.z) as CreVox.BlockAir;

//			if (mainDir == CamDir.front) {
			if (centerB == null || centerB.IsSolid (Turn (Block.Direction.south)))
				return false;
			if (backB == null)
				return false;
			if (backB.IsSolid (Block.Direction.north))
				return false;
			if (_lookDir == Turn (CamDir.left) && backB.IsSolid (Turn (Block.Direction.west)))
				return false;
			if (_lookDir == Turn (CamDir.right) && backB.IsSolid (Turn (Block.Direction.east)))
				return false;
//			}
//
//			if (mainDir == CamDir.back) {
//				if (centerB == null || centerB.IsSolid (Block.Direction.north))
//					return false;
//				if (backB == null)
//					return false;
//				if (backB.IsSolid (Block.Direction.south))
//					return false;
//				if (_lookDir == CamDir.left && backB.IsSolid (Block.Direction.west))
//					return false;
//				if (_lookDir == CamDir.right && backB.IsSolid (Block.Direction.east))
//					return false;
//			}
//
//			if (mainDir == CamDir.left) {
//				if (centerB == null || centerB.IsSolid (Block.Direction.east))
//					return false;
//				if (backB == null)
//					return false;
//				if (backB.IsSolid (Block.Direction.west))
//					return false;
//				if (_lookDir == CamDir.left && backB.IsSolid (Block.Direction.north))
//					return false;
//				if (_lookDir == CamDir.right && backB.IsSolid (Block.Direction.south))
//					return false;
//			}
//
//			if (mainDir == CamDir.right) {
//				if (centerB == null || centerB.IsSolid (Block.Direction.west))
//					return false;
//				if (backB == null)
//					return false;
//				if (backB.IsSolid (Block.Direction.east))
//					return false;
//				if (_lookDir == CamDir.left && backB.IsSolid (Block.Direction.south))
//					return false;
//				if (_lookDir == CamDir.right && backB.IsSolid (Block.Direction.north))
//					return false;
//			}

			return true;
		}

		void CalcCamDir ()
		{
			for (int i = 0; i < dirLayer.Length; i++) {
				if (obsLayer [i] == 0)
					dirLayer [i] = -1;
				else
					dirLayer [i] = (sclLayer [i] == -1) ? adjLayer [i] : sclLayer [i];
			}
		}

		void UpdateIDLayer (int _offsetX, int _offsetZ)
		{
			for (int i = 0; i < idLayer.Length; i++)
				oldIDLayer [i] = idLayer [i];

			for (int i = 0; i < sclLayer.Length; i++) {
				int x = (i % 3);
				int z = (int)(i / 3);

				x = (x + _offsetX + 3) % 3;
				z = (z + _offsetZ + 3) % 3;

				idLayer [i] = oldIDLayer [x + z * 3];
			}
		}

		void UpdateCamZones ()
		{
//			camNode.transform.position = new Vector3 (curPos.x * Block.w, curPos.y * Block.h, curPos.z * Block.d);
			for (int i = 0; i < idLayer.Length; i++) {
				int camID = idLayer [i];
				int dir = dirLayer [i];

				if (sclLayer [i] == -1) {
					int l = (int)CamDir.turn_left;
					int r = (int)CamDir.turn_right;
					int x = i % 3;
					int z = (int)(i / 3);


					if (((dir & l) != 0) && ((dir & r) != 0)) {
						CopyCameraZoneData (camID, CamZoneType.left_right);
					} else if ((dir & l) != 0) {
						CopyCameraZoneData (camID, CamZoneType.left);
					} else if ((dir & r) != 0) {
						CopyCameraZoneData (camID, CamZoneType.right);
					} else if ((dir & (int)CamDir.to_wall) != 0) {
						CopyCameraZoneData (camID, CamZoneType.back);
					} else {
						CopyCameraZoneData (camID, CamZoneType.front);
					}
					if (dir == -1) {
						CopyCameraZoneData (camID, CamZoneType.back);
					}
					camZones [camID].transform.localRotation = Quaternion.Euler (0f, GetAngle (dir), 0f);
					camZones [camID].transform.localPosition = new Vector3 ((curPos.x + x - 1) * Block.w, curPos.y * Block.h, (curPos.z + z - 1) * Block.d);
				}
			}
		}

		void CopyCameraZoneData (int _id, CamZoneType _type)
		{

			//camerazone
			GameObject fGO = camZonePreset [(int)_type];
			GameObject tGO = camZones [_id];
			DynamicCameraZone fZone = fGO.GetComponent<DynamicCameraZone> ();
			DynamicCameraZone tZone = tGO.GetComponent<DynamicCameraZone> ();
			tZone.name = fZone.name;
			tZone.curve = fZone.curve;
			tZone.blendTime = fZone.blendTime;

			//collider
			BoxCollider fBCol = fGO.GetComponent<BoxCollider> ();
			BoxCollider tBCol = tGO.GetComponent<BoxCollider> ();
			tBCol.size = fBCol.size;
			tBCol.center = fBCol.center;
			
			//dynamiccamera
			GameObject fDCamGO = fGO.transform.GetChild (0).gameObject;
			GameObject tDCamGO = tGO.transform.GetChild (0).gameObject;
			tDCamGO.transform.localPosition = fDCamGO.transform.localPosition;
			tDCamGO.transform.localRotation = fDCamGO.transform.localRotation;

			//cam pos
			DynamicCamera fDCam = fDCamGO.GetComponent<DynamicCamera> ();
			DynamicCamera tDCam = tDCamGO.GetComponent<DynamicCamera> ();

			//cam angle Y/P
			tDCam.minAngleP = fDCam.minAngleP;
			tDCam.maxAngleP = fDCam.maxAngleP;
			tDCam.minAngleY = fDCam.minAngleY;
			tDCam.maxAngleY = fDCam.maxAngleY;

			//dist min/max
			tDCam.minDist = fDCam.minDist;
			tDCam.maxDist = fDCam.maxDist;

			//safe frame
			tDCam.minSafeFrameHorizon = fDCam.minSafeFrameHorizon;
			tDCam.maxSafeFrameHorizon = fDCam.maxSafeFrameHorizon;
			tDCam.minSafeFrameVertical = fDCam.minSafeFrameHorizon;
			tDCam.maxSafeFrameVertical = fDCam.maxSafeFrameVertical;

			//camera
			//fov
			Camera fCam = fDCamGO.GetComponent<Camera> ();
			Camera tCam = tDCamGO.GetComponent<Camera> ();
			tCam.fieldOfView = fCam.fieldOfView;
		}

		float GetAngle (int _dir)
		{
			if ((_dir & (int)CamDir.front) != 0) {
				return 0f;
			}
			if ((_dir & (int)CamDir.back) != 0) {
				return 180f;
			}
			if ((_dir & (int)CamDir.right) != 0) {
				return 90f;
			}
			if ((_dir & (int)CamDir.left) != 0) {
				return 270f;
			}

			return 0f;
		}

		void OnDrawGizmos ()
		{
			Vector3 pos = new Vector3 (curPos.x * Block.w, curPos.y * Block.h, curPos.z * Block.d);
			Color oldColor = Gizmos.color;
			Gizmos.DrawWireCube (pos, new Vector3 (Block.w, Block.h, Block.d));
			for (int i = 0; i < dirLayer.Length; i++) {
				if (EditorApplication.isPlaying)
					Gizmos.color = camZones [idLayer [i]].name.Contains ("_F") ? Color.green : Color.red;
				CreVox.WorldPos wPos;
				wPos.x = curPos.x + i % 3 - 1;
				wPos.y = curPos.y;
				wPos.z = curPos.z + (int)(i / 3) - 1;
				DrawCamDir (wPos, dirLayer [i]);
			}
			Gizmos.color = oldColor;
		}

		void DrawCamDir (CreVox.WorldPos _pos, int _dir)
		{
			Vector3 v = new Vector3 (_pos.x * Block.w, _pos.y * Block.h, _pos.z * Block.d);
			Gizmos.DrawCube (v, Vector3.one * 0.1f);
			if ((_dir & (int)CamDir.front) != 0) {
				Gizmos.DrawLine (v, v + Vector3.forward * 1f);
			}
			if ((_dir & (int)CamDir.back) != 0) {
				Gizmos.DrawLine (v, v - Vector3.forward * 1f);
			}
			if ((_dir & (int)CamDir.right) != 0) {
				Gizmos.DrawLine (v, v + Vector3.right * 1f);
			}
			if ((_dir & (int)CamDir.left) != 0) {
				Gizmos.DrawLine (v, v - Vector3.right * 1f);
			}
		}
	}
}