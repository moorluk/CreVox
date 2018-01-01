﻿using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CreVox
{
	enum CamZoneType
	{
		none,
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
		//none = 0,
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
		
		public Volume volume;
		private VGlobal vg;
		public WorldPos curPos;
		private WorldPos oldPos, localPos;
		public int oldID = 4;
		private Transform target;
		private GameObject camNode;
		private CamSys camsys;

		[SerializeField] GameObject[] camZonePreset;
		[SerializeField] GameObject[] camZones = new GameObject[3 * 3];

		public int[] obsLayer = new int[3 * 3];
		public int[] sclLayer = new int[3 * 3];
		public int[] adjLayer = new int[3 * 3];
		public int[] dirLayer = new int[3 * 3];
		public int[] oldIDLayer = new int[3 * 3];
		public int[] idLayer = new int[3 * 3];

		void Start ()
		{
			target = GameObject.FindGameObjectWithTag ("Player").transform;
			volume = GetVolume (target.position);
			vg = VGlobal.GetSetting ();
			curPos = EditTerrain.GetBlockPos (target.position);
			camsys = GameObject.FindObjectOfType<CamSys> ();
			LoadPreset ();
			InitCamZones ();
			for (int i = 0; i < sclLayer.Length; i++) sclLayer [i] = (int)mainDir;
			ResetAdjecentLayer (mainDir);	
			for (int i = 0; i < idLayer.Length; i++) idLayer [i] = i;
			CalcCamDir ();
		}

		void LoadPreset ()
		{
			camZonePreset = new GameObject[System.Enum.GetValues (typeof(CamZoneType)).Length];

			camZonePreset [(int)CamZoneType.none] = Resources.Load (PathCollect.camSetting + "/AutoCamZone_N") as GameObject;
			camZonePreset [(int)CamZoneType.front] = Resources.Load (PathCollect.camSetting + "/AutoCamZone_F") as GameObject;
			camZonePreset [(int)CamZoneType.left] = Resources.Load (PathCollect.camSetting + "/AutoCamZone_L") as GameObject;
			camZonePreset [(int)CamZoneType.right] = Resources.Load (PathCollect.camSetting + "/AutoCamZone_R") as GameObject;
			camZonePreset [(int)CamZoneType.left_right] = Resources.Load (PathCollect.camSetting + "/AutoCamZone_LR") as GameObject;
			camZonePreset [(int)CamZoneType.up] = Resources.Load (PathCollect.camSetting + "/AutoCamZone_U") as GameObject;
			camZonePreset [(int)CamZoneType.down] = Resources.Load (PathCollect.camSetting + "/AutoCamZone_D") as GameObject;
			camZonePreset [(int)CamZoneType.back] = Resources.Load (PathCollect.camSetting + "/AutoCamZone_B") as GameObject;
		}

		void InitCamZones ()
		{
			camNode = new GameObject ();
			camNode.name = "CameraZones";
			camNode.transform.parent = this.transform;
			for (int i = 0; i < camZones.Length; i++) {
				float x = (i % 3 - 1) * vg.w;
				float y = 0f;
				float z = ((int)(i / 3) - 1) * vg.d;
				GameObject co = GameObject.Instantiate (camZonePreset [(int)CamZoneType.front], Vector3.zero, Quaternion.identity) as GameObject;
				co.name = "zone" + i.ToString ();
				co.transform.localPosition = new Vector3 (x, y, z);
				co.transform.parent = camNode.transform;
				camZones [i] = co;
			}
		}

		void Update ()
		{
			curPos = EditTerrain.GetBlockPos (target.position);
			localPos = EditTerrain.GetBlockPos (target.position, volume.transform);
			int offsetX = Mathf.Clamp (curPos.x - oldPos.x, -1, 1);
//			int offsetY = Mathf.Clamp (curPos.y - oldPos.y, -1, 1);
			int offsetZ = Mathf.Clamp (curPos.z - oldPos.z, -1, 1);
			if (offsetX != 0 || offsetZ != 0) {
				oldID = 4 - (offsetX + 3 * offsetZ);
				volume = GetVolume (target.position);
//				camNode.transform.localRotation = volume.transform.localRotation;
				if (dirLayer [8 - oldID] > 0)
					mainDir = (CamDir)Mathf.Clamp ((dirLayer [8 - oldID] & ((1 << 4) - 1)), 1, 1 << 3);
			}
			
			UpdateObstacleLayer ();
			UpdateScrollLayer (offsetX, offsetZ);

			UpdateAdjecentLayer ();
			CalcCamDir ();

			if (offsetX != 0 || offsetZ != 0) {
				Debug.Log (
					volume.gameObject.name
					+ " (" + curPos.x + "," + curPos.y + "," + curPos.z + "),"
					+ " (" + localPos.x + "," + localPos.y + "," + localPos.z + ")\n"
					+ "[" + oldID + "]:" + (dirLayer [oldID] % (1 << 4)) + "→[4]:" + (dirLayer [4] % (1 << 4))
				);
			}
			
			UpdateIDLayer (offsetX, offsetZ);
			UpdateCamZones ();
			oldPos = curPos;
		}

		Volume GetVolume(Vector3 _pos)
		{
			RaycastHit hit;
			LayerMask _mask = 1 << LayerMask.NameToLayer("Floor");
			bool isHit = Physics.Raycast (
				             /*origin: */_pos, 
				             /*direction: */Vector3.down,
				             /*hitInfo: */out hit,
				             /*maxDistance: */3f,
				             /*layerMask: */_mask.value);

			if (isHit) {
				return hit.collider.transform.GetComponentInParent<Volume> ();
			} else {
				return volume;
			}
		}

		void UpdateObstacleLayer ()
		{
			for (int i = 0; i < obsLayer.Length; i++) {
				int dx = localPos.x + i % 3 - 1;
				int dy = localPos.y;
				int dz = localPos.z + (int)(i / 3) - 1;
				Volume v = volume;
				WorldPos n = new WorldPos (dx, dy, dz);

				// chkOutsideChunk
//				if (dx > 15 || dx < 0 || dy > 15 || dy < 0 || dz > 15 || dz < 0) {
//					Vector3 pos = target.position + new Vector3 ((i % 3 - 1) * vg.w, 0, ((int)(i / 3) - 1) * vg.d);
//					v = GetVolume (pos);
//					n = EditTerrain.GetBlockPos (pos,v.transform);
//				}

				BlockAir b = v.GetBlock (n.x, n.y, n.z) as BlockAir;
				if(b == null || IsOutside(n))
					obsLayer [i] = 0;
				else
					obsLayer [i] = 1;
			}
		}

		void UpdateScrollLayer (int _offsetX, int _offsetZ)
		{
			for (int i = 0; i < sclLayer.Length; i++) {
				int x = (i % 3) + _offsetX;
				int z = (int)(i / 3) + _offsetZ;

				if (x >= 0 && z >= 0 && x < 3 && z < 3) {
					sclLayer [i] = dirLayer [x + z * 3];
				}
				if (i != 4 ) {
					if (i != oldID)
						sclLayer [i] = -1;
					else if (camsys.isBlending == false)
						sclLayer [i] = -1;
				}

			}
		}
			
		void UpdateAdjecentLayer ()
		{
			ResetAdjecentLayer (mainDir);
			if (IsVisible (4, 1) == false)
				adjLayer [4] |= (int)CamDir.to_wall;

			//↑
			if (IsVisible (4, 7) == true) {
				
				// ↖
				if (IsVisible (7, 3) == true) {
					if (IsVisible (6, 1) == false) {
						adjLayer [Turn (7)] |= (int)CamDir.turn_left;
						if (IsVisible (6, 7) == false)
							adjLayer [Turn (6)] = (int)Turn (CamDir.left);
						else
							adjLayer [Turn (6)] |= (int)CamDir.to_wall;
					}
				} else {
					if (IsVisible (6, 3) == false && IsVisible (4, 3) == false)
						adjLayer [Turn (6)] = (int)Turn (CamDir.back);
				}

				// ↗
				if (IsVisible (7, 5) == true) {
					if (IsVisible (8, 1) == false) {
						adjLayer [Turn (7)] |= (int)CamDir.turn_right;
						if (IsVisible (8, 7) == false)
							adjLayer [Turn (8)] = (int)Turn (CamDir.right);
						else
							adjLayer [Turn (8)] |= (int)CamDir.to_wall;
					}
				} else {
					if (IsVisible (8, 5) == false && IsVisible (4, 5) == false)
						adjLayer [Turn (8)] = (int)Turn (CamDir.back);
				}
			} else { 
				// ↖
				if (IsVisible (3, 7) == false) {
//					if (IsVisible (6, 7) == false)
//						adjLayer [Turn (6)] = (int)Turn (CamDir.right);
//					else
//						adjLayer [Turn (6)] |= (int)CamDir.to_wall;
				} else if (IsVisible (6, 5) == true) {
					adjLayer [Turn (6)] |= (int)CamDir.turn_right;
					if (IsVisible (6, 3) == true)
						adjLayer [Turn (6)] |= (int)CamDir.turn_left;
//					if (IsVisible (7, 7) == false)
//						adjLayer [Turn (7)] = (int)Turn (CamDir.right);
//					else
//						adjLayer [Turn (7)] |= (int)CamDir.to_wall;
				}
				// ↗
				if (IsVisible (5, 7) == false) {
//					if (IsVisible (8, 7) == false)
//						adjLayer [Turn (8)] = (int)Turn (CamDir.left);
//					else
//						adjLayer [Turn (8)] |= (int)CamDir.to_wall;
				} else if (IsVisible (8, 3) == true) {
					adjLayer [Turn (8)] |= (int)CamDir.turn_left;
					if (IsVisible (8, 5) == true)
						adjLayer [Turn (8)] |= (int)CamDir.turn_right;
//					if (IsVisible (7, 7) == false)
//						adjLayer [Turn (7)] = (int)Turn (CamDir.left);
//					else
//						adjLayer [Turn (7)] |= (int)CamDir.to_wall;
				}
			}

			// ←
			if (IsVisible (4, 3) == true) {
				if (IsVisible (3, 1) == false) {
					if (IsVisible (3, 7) == false)
						adjLayer [Turn (3)] = (int)Turn (CamDir.left);
					else
						adjLayer [Turn (3)] |= (int)CamDir.to_wall;
				}
//				if (IsVisible (1, 3) == false && IsVisible (7, 3) == false)
//					adjLayer [Turn (3)] = (int)Turn (CamDir.left) + (int)CamDir.turn_left + (int)CamDir.turn_right;
//			} else if (IsVisible (4, 7) == true && IsVisible (7, 3) == true) {
//				if (IsVisible (3, 3) == false)
//					adjLayer [Turn (3)] = (int)Turn (CamDir.back);
//				else
//					adjLayer [Turn (3)] = (int)Turn (CamDir.left) + (int)CamDir.to_wall;
			}

			// →
			if (IsVisible (4, 5) == true) {
				if (IsVisible (5, 1) == false) {
					if (IsVisible (5, 7) == false)
						adjLayer [Turn (5)] = (int)Turn (CamDir.right);
					else
						adjLayer [Turn (5)] |= (int)CamDir.to_wall;
				}
//				if (IsVisible (1, 5) == false && IsVisible (7, 5) == false)
//					adjLayer [Turn (5)] = (int)Turn (CamDir.right) + (int)CamDir.turn_left + (int)CamDir.turn_right;
//			} else if (IsVisible (4, 7) == true && IsVisible (7, 5) == true) {
//				if (IsVisible (5, 5) == false)
//					adjLayer [Turn (5)] = (int)Turn (CamDir.back);
//				else
//					adjLayer [Turn (5)] = (int)Turn (CamDir.right) + (int)CamDir.to_wall;
			}

			// ↓
			if (IsVisible (4, 1) == true) {
				if (IsVisible (1, 1) == false)
					adjLayer [Turn (1)] |= (int)CamDir.to_wall;
				
				// ↙
				if (IsVisible (1, 3) == true) {
					//後向判斷
					if (IsVisible (0, 1) == false) {
						if (IsVisible (0, 7) == false) {
							adjLayer [Turn (0)] = (int)Turn (CamDir.left);
							if (IsVisible (1, 1) == true)
								adjLayer [Turn (1)] |= (int)CamDir.turn_left;
						} else {
							adjLayer [Turn (0)] |= (int)CamDir.to_wall;
						}
					}
					//側向判斷
					if (IsVisible (4, 3) == false) {
						WorldPos out0_1 = GetNeighbor (GetNeighbor (localPos, Turn(0)), Turn(1));
						if (IsVisible (out0_1, 5) == false) {
							adjLayer [Turn (0)] = (int)Turn (CamDir.left);
							if (IsVisible (0, 7) == true && IsVisible (3, 3) == false)
								adjLayer [Turn (0)] |= (int)CamDir.turn_right;
							if (IsVisible (0, 1) == true && IsVisible (out0_1, 3) == false) {
								adjLayer [Turn (0)] |= (int)CamDir.turn_left;
							}
						}
//						if (IsVisible (1, 1) == true)
//							adjLayer [Turn (1)] |= (int)CamDir.turn_left;
					}
				} else {
					//側向判斷
//					if (IsVisible (0, 3) == true)
//						adjLayer [Turn (0)] = (int)Turn (CamDir.left) + (int)CamDir.to_wall;
//					else if (IsVisible (4, 3) == true)
					if (IsVisible (0, 3) == false && IsVisible (4, 3) == true)
						adjLayer [Turn (0)] = (int)Turn (CamDir.back);
				}

				// ↘
				if (IsVisible (1, 5) == true) {
					//後向判斷
					if (IsVisible (2, 1) == false) {
						if (IsVisible (2, 7) == false) {
							adjLayer [Turn (2)] = (int)Turn (CamDir.right);
							if (IsVisible (1, 1) == true)
								adjLayer [Turn (1)] |= (int)CamDir.turn_right;
						} else {
							adjLayer [Turn (2)] |= (int)CamDir.to_wall;
						}
					}
					//側向判斷
					if (IsVisible (4, 5) == false) {
						WorldPos out2_1 = GetNeighbor (GetNeighbor (localPos, Turn(2)), Turn(1));
						if (IsVisible (out2_1, 3) == false) {
							adjLayer [Turn (2)] = (int)Turn (CamDir.right);
							if (IsVisible (2, 7) == true && IsVisible (5, 5) == false)
								adjLayer [Turn (2)] |= (int)CamDir.turn_left;
							if (IsVisible (2, 1) == true && IsVisible (out2_1, 5) == false) {
								adjLayer [Turn (2)] |= (int)CamDir.turn_right;
							}
						}
//						if (IsVisible (1, 1) == true)
//							adjLayer [Turn (1)] |= (int)CamDir.turn_right;
					}
				} else {
					//側向判斷
//					if (IsVisible (2, 5) == true)
//						adjLayer [Turn (2)] = (int)Turn (CamDir.right) + (int)CamDir.to_wall;
//					else if (IsVisible (4, 5) == true)
					if (IsVisible (2, 5) == false && IsVisible (4, 5) == true)
						adjLayer [Turn (2)] = (int)Turn (CamDir.back);
				}
			}
			// ↙
			if (IsVisible (0, 7) == true) {
				if (IsVisible (0, 1) == false)
					adjLayer [Turn (0)] |= (int)CamDir.to_wall;
				if (IsVisible (1, 3) == true && IsVisible (1, 1) == false && IsVisible (1, 7) == false) {
					if (IsVisible (0, 1) == true)
						adjLayer [Turn (0)] |= (int)CamDir.turn_right;
					adjLayer [Turn (1)] = (int)Turn (CamDir.right);
					sclLayer [Turn (1)] = -1;
					if (IsVisible (2, 3) == true && IsVisible (2, 1) == false && IsVisible (2, 7) == false) {
						adjLayer [Turn (2)] = (int)Turn (CamDir.right);
						sclLayer [Turn (2)] = -1;
					}
				}
			}
			// ↘
			if (IsVisible (2, 7) == true) {
				if (IsVisible (2, 1) == false)
					adjLayer [Turn (2)] |= (int)CamDir.to_wall;
				if (IsVisible (1, 5) == true && IsVisible (1, 1) == false && IsVisible (1, 7) == false) {
					if (IsVisible (2, 1) == true)
						adjLayer [Turn (2)] |= (int)CamDir.turn_left;
					adjLayer [Turn (1)] = (int)Turn (CamDir.left);
					sclLayer [Turn (1)] = -1;
					if (IsVisible (0, 5) == true && IsVisible (0, 1) == false && IsVisible (0, 7) == false) {
						adjLayer [Turn (0)] = (int)Turn (CamDir.left);
						sclLayer [Turn (0)] = -1;
					}
				}
			}
		}

		void ResetAdjecentLayer (CamDir _dir)
		{
			int v = (int)_dir;
			for (int i = 0; i < dirLayer.Length; i++) {
				adjLayer [i] = v;
			}
		}

		bool IsOutside(WorldPos _pos)
		{
			BlockAir centerB = volume.GetBlock (_pos.x, _pos.y, _pos.z) as BlockAir;	
			BlockAir downB = volume.GetBlock (_pos.x, _pos.y - 1, _pos.z) as BlockAir;	
			if (downB != null && downB.pieceNames == null) {
				if (centerB != null) {
//					if (centerB.pieceNames == null)
//						return true;
//					else if (centerB.pieceNames.Length < 1)
						return true;
				}
			}
			return false;
		}
		bool IsVisible(int _index, int _lookDirIndex)
		{
			WorldPos _pos = GetNeighbor (localPos, Turn (_index));
			return IsVisible (_pos,Turn (_lookDirIndex));
		}
		bool IsVisible (WorldPos _pos, int _lookDirIndex)
		{
			BlockAir centerB = volume.GetBlock (_pos.x, _pos.y, _pos.z) as BlockAir;
			WorldPos n = GetNeighbor (_pos, _lookDirIndex);

			if (centerB == null)
				return false;

			if (IsOutside (n))
				return false;

			if (_lookDirIndex == 7) {
				if (centerB.IsSolid (Direction.north)
				    || volume.GetBlock (n.x, n.y, n.z).IsSolid (Direction.south))
					return false;
			}
			if (_lookDirIndex == 3) {
				if (centerB.IsSolid (Direction.west)
				    || volume.GetBlock (n.x, n.y, n.z).IsSolid (Direction.east))
					return false;
			}
			if (_lookDirIndex == 1) {
				if (centerB.IsSolid (Direction.south)
				    || volume.GetBlock (n.x, n.y, n.z).IsSolid (Direction.north))
					return false;
			}
			if (_lookDirIndex == 5) {
				if (centerB.IsSolid (Direction.east)
				    || volume.GetBlock (n.x, n.y, n.z).IsSolid (Direction.west))
					return false;
			}
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
//			camNode.transform.position = new Vector3 (localPos.x * vg.w, localPos.y * vg.h, localPos.z * vg.d);
			for (int i = 0; i < idLayer.Length; i++) {
				int camID = idLayer [i];
				int dir = dirLayer [i];

//				if (sclLayer [i] == -1) {
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
						CopyCameraZoneData (camID, CamZoneType.none);
					}
					camZones [camID].transform.rotation = Quaternion.Euler (0f, GetAngle (dir), 0f);
					camZones [camID].transform.position = new Vector3 ((curPos.x + x - 1) * vg.w, curPos.y * vg.h, (curPos.z + z - 1) * vg.d);
//				}
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
			tDCam.minSafeFrameVertical = fDCam.minSafeFrameVertical;
			tDCam.maxSafeFrameVertical = fDCam.maxSafeFrameVertical;

			//camera
			//fov
			Camera fCam = fDCamGO.GetComponent<Camera> ();
			Camera tCam = tDCamGO.GetComponent<Camera> ();
			tCam.fieldOfView = fCam.fieldOfView;
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
			CamDir[] srcDir= new CamDir[4];

			switch (mainDir) {
			case CamDir.front:
				srcDir = new CamDir[] {
					CamDir.front,
					CamDir.left,
					CamDir.back,
					CamDir.right
				};
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
		public Direction Turn (Direction _dir, CamDir _baseDir)
		{
			Direction[] srcDir = new Direction[] {
				Direction.north,
				Direction.west,
				Direction.south,
				Direction.east
			};
			Direction[] dstDir = new Direction[4];

			if (_baseDir == CamDir.turn_none)
				_baseDir = mainDir;
			
			switch (_baseDir) {
			case CamDir.front:
				dstDir = srcDir;
				break;

			case CamDir.left:
				dstDir = new Direction[] {
					Direction.west,
					Direction.south,
					Direction.east,
					Direction.north
				};
				break;

			case CamDir.back:
				dstDir = new Direction[] {
					Direction.south,
					Direction.east,
					Direction.north,
					Direction.west
				};
				break;

			case CamDir.right:
				dstDir = new Direction[] {
					Direction.east,
					Direction.north,
					Direction.west,
					Direction.south
				};
				break;
			}

			Direction result = _dir;

			for (int i = 0; i < 4; i++) {
				if (_dir == srcDir [i])
					result = dstDir [i];
			}

			return result;
		}

		public WorldPos GetNeighbor (WorldPos _base, int _index)
		{
			int _x = (_index % 3) - 1;
			int _z = (int)(_index / 3) - 1;

			WorldPos result;
			result.x = _base.x + _x;
			result.y = _base.y;
			result.z = _base.z + _z;
			return result;
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
		#if UNITY_EDITOR
		void OnDrawGizmos ()
		{
			Color oldColor = Gizmos.color;
			for (int i = 0; i < dirLayer.Length; i++) {
				if (EditorApplication.isPlaying)
					Gizmos.color = camZones [idLayer [i]].name.Contains ("_F") ? Color.green : Color.red;
				WorldPos wPos;
				wPos.x = curPos.x + i % 3 - 1;
				wPos.y = curPos.y;
				wPos.z = curPos.z + (int)(i / 3) - 1;
				if (obsLayer [i] != 0)
					DrawCamDir (wPos, dirLayer [i]);
			}
			Gizmos.color = oldColor;
		}
		void DrawCamDir (WorldPos _pos, int _dir)
		{
			Vector3 v = new Vector3 (_pos.x * vg.w, _pos.y * vg.h, _pos.z * vg.d);
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
		#endif
	}
}