using System.Collections.Generic;
using UnityEngine;
using CreVox;
using UnityEditor;
using System.Linq;

namespace Test {

	public class AddOn {
		// Constant parameter array.
		public static WorldPos[] DirectionOffset = new WorldPos[] {
			new WorldPos(-1, 0, -1),
			new WorldPos(0, 0, -1),
			new WorldPos(1, 0, -1),
			new WorldPos(-1, 0, 0),
			new WorldPos(0, 0, 0),
			new WorldPos(1, 0, 0),
			new WorldPos(-1, 0, 1),
			new WorldPos(0, 0, 1),
			new WorldPos(1, 0, 1)
		};
		// Offset about 0, 90, 180, 270 degrees.
		private static WorldPos[] RotationOffset = new WorldPos[] {
			new WorldPos(0, 0, 0),
			new WorldPos(0, 0, 8),
			new WorldPos(8, 0, 8),
			new WorldPos(8, 0, 0)
		};
		// Volume Manager object.
		private static VolumeManager resultVolumeManager;
		private static Dictionary<VolumeData, List<DoorInfo>> doorInfoVdataTable;

		private static int[] _orderByDirection = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 };

		// Create Volume object and return it.
		private static Volume CreateVolumeObject(VolumeData vdata) {
			GameObject volumeObject = new GameObject() { name = vdata.name };
			Volume volume = volumeObject.AddComponent<Volume>();
			volume.vd = vdata;
			VolumeExtend volumeExtend = volumeObject.AddComponent<VolumeExtend>();
			if (!doorInfoVdataTable.ContainsKey(vdata)) {
				doorInfoVdataTable[vdata] = GetDoorPosition(vdata);
			}
			volumeExtend.DoorInfos = new List<DoorInfo>(doorInfoVdataTable[vdata].Select(x => x.Clone()).ToArray());

			volumeObject.transform.parent = resultVolumeManager.transform;
			volume.Init(volume.chunkX, volume.chunkY, volume.chunkZ);
			Debug.Log("Add " + vdata.name);
			return volume;
		}

		// Initial the resultVolumeData and create the VolumeManager.
		public static Volume Initial(VolumeData vdata) {
			doorInfoVdataTable = new Dictionary<VolumeData, List<DoorInfo>>();
			GameObject volumeMangerObject = new GameObject() { name = "resultVolumeManger" };
			resultVolumeManager = volumeMangerObject.AddComponent<VolumeManager>();
			Volume NowNode = CreateVolumeObject(vdata);

			RefreshVolume();
			Debug.Log("Initial finish.");
			return NowNode;
		}
		// Update and repaint.
		public static void RefreshVolume() {
			resultVolumeManager.UpdateDungeon();
			SceneView.RepaintAll();
		}
		// Add volume data.
		public static Volume AddAndCombineVolume(Volume nowNode, VolumeData vdata) {
			Volume volume = CreateVolumeObject(vdata);
			if (CombineVolumeObject(nowNode, volume)) {
				return volume;
			}
			Object.DestroyImmediate(volume.gameObject);
			return null;
		}
		public static void SetPriority(string number) {
			for (int i = 0; i < 9; i++) {
				_orderByDirection[i] = number[i] - '0';
			}
		}
		public static void SetPriority(int LB, int B, int RB, int L, int CENTER, int R, int LT, int T, int RT) {
			int[] set = new int[] { LB, B, RB, L, CENTER, R, LT, T, RT };
			for (int i = 0; i < 9; i++) {
				_orderByDirection[i] = set[i];
			}
		}
		// Combine both volumeData.
		public static bool CombineVolumeObject(Volume volume1, Volume volume2) {
			VolumeExtend volumeExtend1 = volume1.GetComponent<VolumeExtend>();
			VolumeExtend volumeExtend2 = volume2.GetComponent<VolumeExtend>();

			WorldPos relativePosition = new WorldPos();
			int rotationOfVolume1 = (int) volume1.transform.eulerAngles.y;
			int rotationOfVolume2 = (int) volume2.transform.eulerAngles.y;
			// Compare door connection.
			//DoorInfo[] connections_2 = volumeExtend_2.DoorInfos.OrderBy(x => -_orderByDirection[(int)x.direction]).ToArray();
			DoorInfo[] connections2 = volumeExtend2.DoorInfos.OrderBy(x => Random.value).ToArray();
			DoorInfo[] connections1 = volumeExtend1.DoorInfos.ToArray();
			foreach (var connection2 in connections2) {
				if (connection2.used) {
					Debug.Log("2 Used");
					continue;
				}
				foreach (var connection1 in connections1) {
					if (connection1.used) {
						Debug.Log("1 Used");
						continue;
					}
					// Added vdata need to rotate for matching  origin vdata.
					int rotateAngle = (((( connection1.direction.angle + (int)volume1.transform.eulerAngles.y ) + 180) % 360) - connection2.direction.angle);
					if(rotateAngle < 0) {
						rotateAngle += 360;
					}
					// Relative position between connections.
					relativePosition = connection1.AbsolutePosition(rotationOfVolume1) - connection2.AbsolutePosition(rotateAngle);
					relativePosition += DirectionOffset[connection1.AbsoluteDirection(rotationOfVolume1).ToIndex()];
					// Rotation
					volume2.transform.eulerAngles = new Vector3(0, rotateAngle , 0);
					// Real position.
					volume2.transform.localPosition = volume1.transform.position - RotationOffset[rotationOfVolume1 / 90].ToRealPosition() + relativePosition.ToRealPosition() + RotationOffset[rotateAngle / 90].ToRealPosition();
					if (!isCollider(volume2)) {
						connection1.used = true;
						connection2.used = true;
						Debug.Log("Combine finish.");
						return true;
					} else {
						Debug.Log("Next");
					}

				}
			}
			Debug.Log("No door can combine.");
			return false;
		}
		// Get volumedata via path as string.
		public static VolumeData GetVolumeData(string path) {
			VolumeData vdata = (VolumeData) AssetDatabase.LoadAssetAtPath(path, typeof(VolumeData));
			Debug.Log("Get vdata : " + vdata.name);
			return vdata;
		}
		// Get the list that contains postisions and directions of door(conntection) via volumedata.
		private static List<DoorInfo> GetDoorPosition(VolumeData vdata) {
			List<DoorInfo> doors = new List<DoorInfo>();
			// All chunk.
			foreach (var chunk in vdata.chunkDatas) {
				// All blockAir.
				foreach (var blockAir in chunk.blockAirs) {
					// All direction.
					for (int i = 0; i < blockAir.pieceNames.Length; i++) {
						// If get door then add.
						if (blockAir.pieceNames[i] == "Door") {
							// Real position = chunk position + block position.
							WorldPos realPos = chunk.ChunkPos + blockAir.BlockPos;
							doors.Add(new DoorInfo(realPos, new DirectionOfBlock(i)));
						}
					}
				}
			}
			return doors;
		}


		// Collision
		private static bool isCollider(Volume volume) {
			foreach (var chunkdata in volume.vd.chunkDatas) {
				foreach (var block in chunkdata.blocks) {
					Vector3 realPosition = volume.transform.position + ( chunkdata.ChunkPos + block.BlockPos ).ToRealPosition() - RotationOffset[(int)volume.transform.eulerAngles.y / 90].ToRealPosition();
					if (interact(realPosition, volume)) {
						return true;
					}
				}
			}
			return false;
		}
		private static bool interact(Vector3 position, Volume volume) {
			foreach (var compareVolume in resultVolumeManager.GetComponentsInChildren<Volume>()) {
				if (compareVolume == volume) {
					continue;
				}
				foreach (var compareChunkdata in compareVolume.vd.chunkDatas) {
					foreach (var compareBlock in compareChunkdata.blocks) {
						Vector3 comparePosition = compareVolume.transform.position + ( compareChunkdata.ChunkPos + compareBlock.BlockPos ).ToRealPosition() - RotationOffset[(int) compareVolume.transform.eulerAngles.y / 90].ToRealPosition();
						if (position == comparePosition) {
							return true;
						}
					}
				}
			}
			return false;
		}
	}
}