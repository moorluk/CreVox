using System.Collections.Generic;
using UnityEngine;
using CreVox;
using UnityEditor;
using System.Linq;

namespace Test {
	
	public class AddOn {
		public static WorldPos[] DirectionTrans = new WorldPos[] {
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
		private static List<VolumeData> DEFAULT_VOLUME_DATA = new List<VolumeData>() {
			GetVolumeData("Assets/WillDelete/VolumeData/Stair_vdata.asset")
		};

		private static VolumeManager resultVolumeManager;
		private static Dictionary<VolumeData, List<DoorInfo>> doorInfoVdataTable;

		private static int[] _orderByDirection = new int[] { 0,1,2,3,4,5,6,7,8 };

		// Create Volume object and return it.
		private static Volume CreateVolumeObject(VolumeData vdata) {
			GameObject volumeObject = new GameObject() { name = vdata.name };
			Volume volume = volumeObject.AddComponent<Volume>();
			volume.vd = vdata;
			VolumeExtend volumeExtend = volumeObject.AddComponent<VolumeExtend>();
			if(! doorInfoVdataTable.ContainsKey(vdata)) {
				doorInfoVdataTable[vdata] = GetDoorPosition(vdata);
			}
			volumeExtend.DoorInfos = new List<DoorInfo>(doorInfoVdataTable[vdata].Select( x => x.Clone() ).ToArray());

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
			for(int i = 0; i < 9; i++) {
				_orderByDirection[i] = number[i] - '0';
			}
		}
		public static void SetPriority(int LT, int B, int RT, int L, int CENTER, int R, int LB, int T, int RB) {
			int[] set = new int[] { LT, B, RT, L, CENTER, R, LB, T, RB };
			for (int i = 0; i < 9; i++) {
				_orderByDirection[i] = set[i];
			}
		}
		// Combine both volumeData.
		public static bool CombineVolumeObject(Volume volume1, Volume volume2) {
			VolumeExtend volumeExtend_1 = volume1.GetComponent<VolumeExtend>();
			VolumeExtend volumeExtend_2 = volume2.GetComponent<VolumeExtend>();

			WorldPos relativePosition = new WorldPos();
			// Compare door connection.
			//DoorInfo[] connections_2 = volumeExtend_2.DoorInfos.OrderBy(x => -_orderByDirection[(int)x.direction]).ToArray();
			DoorInfo[] connections_2 = volumeExtend_2.DoorInfos.OrderBy(x => Random.value).ToArray();
			DoorInfo[] connections_1 = volumeExtend_1.DoorInfos.ToArray();
			foreach (var connection_2 in connections_2) {
				if (connection_2.used) {
					Debug.Log("2Used");
					continue;
				}
				foreach (var connection_1 in connections_1) {
					if (connection_1.used) {
						Debug.Log("1Used");
						continue;
					}
					int combineArg = ((int)connection_1.direction) + ((int)connection_2.direction);
					if (combineArg == 8) {
						relativePosition = connection_1.position - connection_2.position;
						relativePosition += DirectionTrans[(int)connection_1.direction];
						// Real position.
						volume2.transform.localPosition = volume1.transform.position + Vector3.Scale(relativePosition.ToVector3(),new Vector3(3, 2, 3));
						if (!isCollider(volume2)) {
							connection_1.used = true;
							connection_2.used = true;
							Debug.Log("Combine finish.");
							return true;
						} else {
							Debug.Log("Next");
						}
					}
				}
			}
			Debug.Log("No door can combine.");
			return false;
		}
		// Get volumedata via path as string.
		public static VolumeData GetVolumeData(string path) {
			VolumeData vdata = (VolumeData)AssetDatabase.LoadAssetAtPath(path, typeof(VolumeData));
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
							doors.Add(new DoorInfo(realPos, (DirectionOfBlock)i));
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
					Vector3 realPosition = volume.transform.position + (chunkdata.ChunkPos + block.BlockPos).ToVector3();
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
						Vector3 comparePosition = compareVolume.transform.position + Vector3.Scale((compareChunkdata.ChunkPos + compareBlock.BlockPos).ToVector3(), new Vector3(3, 2, 3));
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