using System.Collections.Generic;
using UnityEngine;
using CreVox;
using UnityEditor;
using System.Linq;

namespace Test {
	public class AddOn {
		// Idk why the top and bottom is reverse.
		public enum DirectionOfBlock {
			LeftBottom,
			Bottom,
			RightBottom,
			Left,
			Center,
			Right,
			LeftTop,
			Top,
			RightTop
		}
		public struct DoorInfo {
			public WorldPos position;
			public DirectionOfBlock direction;
			public DoorInfo(WorldPos position, DirectionOfBlock direction) {
				this.position = position;
				this.direction = direction;
			}
			public bool Compare(DoorInfo obj) {
				return this.position.Compare(obj.position) && this.direction == obj.direction;
			}
		}
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

		private static VolumeManager resultVolumeManager;

		// Create Volume object and return it.
		private static Volume CreateVolumeObject(VolumeData vdata) {
			GameObject volumeObject = new GameObject() { name = vdata.name };
			Volume volume = volumeObject.AddComponent<Volume>();
			volume.vd = vdata;

			volumeObject.transform.parent = resultVolumeManager.transform;
			volume.Init(volume.chunkX, volume.chunkY, volume.chunkZ);
			Debug.Log("Add " + vdata.name);
			return volume;
		}

		// Initial the resultVolumeData and create the VolumeManager.
		public static Volume Initial(VolumeData vdata) {
			GameObject volumeMangerObject = new GameObject() { name = "resultVolumeManger" };
			resultVolumeManager = volumeMangerObject.AddComponent<VolumeManager>();
			Volume NowNode = CreateVolumeObject(vdata);

			RefreshVolume();
			Debug.Log("Initial finish.");
			return NowNode;
		}
		public static void RefreshVolume() {
			resultVolumeManager.UpdateDungeon();
			SceneView.RepaintAll();
		}
		// Add node.
		public static Volume AddAndCombineVolume(Volume nowNode, VolumeData vdata) {
			Volume volume = CreateVolumeObject(vdata);
			if (CombineVolumeObject(nowNode, volume)) {
				RefreshVolume();
				return volume;
			}
			Object.DestroyImmediate(volume.gameObject);
			return null;
		}
		// Combine both volumeData.
		public static bool CombineVolumeObject(Volume volume1, Volume volume2) {
			// Get the connection.
			List<DoorInfo> connections_1 = GetDoorPosition(volume1.vd);
			List<DoorInfo> connections_2 = GetDoorPosition(volume2.vd);

			WorldPos relativePosition = new WorldPos();
			// Compare door connection.
			
			foreach (var connection_2 in connections_2.OrderBy(x => Random.value)) {
				foreach (var connection_1 in connections_1) {
					int combineArg = ((int)connection_1.direction) + ((int)connection_2.direction);
					if (combineArg == 8) {
						relativePosition = connection_1.position - connection_2.position;
						relativePosition += DirectionTrans[(int)connection_1.direction];
						Debug.Log(connection_1.direction.ToString());
						volume2.transform.localPosition = volume1.transform.position + relativePosition.ToVector3() * 3;
						if (!isCollider(volume2)) {
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
		// Get the list that contains postisions and directions of door(conntection).
		private static List<DoorInfo> GetDoorPosition() {
			List<DoorInfo> doors = new List<DoorInfo>();
			// All vdata.
			foreach (var volume in resultVolumeManager.transform.GetComponentsInChildren<Volume>()) {
				VolumeData vdata = volume.vd;
				// All chunk.
				foreach (var chunk in vdata.chunkDatas) {
					// All blockAir.
					foreach (var blockAir in chunk.blockAirs) {
						// All direction.
						for (int i = 0; i < blockAir.pieceNames.Length; i++) {
							// If get door then add.
							if (blockAir.pieceNames[i] == "Door") {
								// Real position = (gameObject.position / 3) + chunk position + block position.
								WorldPos realPos = new WorldPos(volume.transform.position / 3) + chunk.ChunkPos + blockAir.BlockPos;
								DoorInfo doorInfo = new DoorInfo(realPos, (DirectionOfBlock)i);
								doors.Add(doorInfo);
							}
						}
					}
				}
			}
			return doors;
		}
		private static bool isCollider(Volume volume) {
			foreach (var chunkdata in volume.vd.chunkDatas) {
				foreach (var block in chunkdata.blocks) {
					Vector3 realPosition = volume.transform.position + (chunkdata.ChunkPos + block.BlockPos).ToVector3() * 3;
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
						Vector3 comparePosition = compareVolume.transform.position + (compareChunkdata.ChunkPos + compareBlock.BlockPos).ToVector3() * 3;
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