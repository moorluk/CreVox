using System.Collections.Generic;
using UnityEngine;
using CreVox;
using UnityEditor;
using System.Linq;

namespace CrevoxExtend {

	public class CrevoxOperation {
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
			return volume;
		}

		// Initial the resultVolumeData and create the VolumeManager.
		public static Volume InitialVolume(VolumeData vdata) {
			doorInfoVdataTable = new Dictionary<VolumeData, List<DoorInfo>>();
			GameObject volumeMangerObject = new GameObject() { name = "resultVolumeManger" };
			resultVolumeManager = volumeMangerObject.AddComponent<VolumeManager>();
			Volume NowNode = CreateVolumeObject(vdata);
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
					continue;
				}
				foreach (var connection1 in connections1) {
					if (connection1.used) {
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
					if (!IsCollider(volume2)) {
						connection1.used = true;
						connection2.used = true;
						Debug.Log("Combine finish.");
						return true;
					}

				}
			}
			Debug.Log("No door can combine.");
			return false;
		}
		// Get volumedata via path as string.
		public static VolumeData GetVolumeData(string path) {
			VolumeData vdata = (VolumeData) AssetDatabase.LoadAssetAtPath(path, typeof(VolumeData));
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

		#region 重疊判定
		// The set about Size of BlockAir object.
		private static Dictionary<string, Vector3> BlockAirScale = new Dictionary<string, Vector3>() {
			{ "Door", new Vector3(1, 3, 1) },
			{ "Wall", new Vector3(1, 3, 1) },
			{ "Gnd.in.bottom", new Vector3(1, 1, 1) },
			{ "Stair", new Vector3(1, 1, 1) },
			{ "Fence", new Vector3(1, 1, 1) }
		};
		private static Vector3 MINIMUM_SIZE = new Vector3(1.5f, 1.0f, 1.5f);
		private static Vector3 OFFSET_SIZE = new Vector3(0.2f, 0.2f, 0.2f);
		const float CHUNK_DISTANCE_MAXIMUM = 41.5692f; // Vector3.Magnitude(new Vector3(24, 24, 24))
		
		// Collision
		private static bool IsCollider(Volume volume) {
			foreach (var chunkdata in volume.vd.chunkDatas) {
				foreach (var compareVolume in resultVolumeManager.GetComponentsInChildren<Volume>()) {
					if (compareVolume == volume) {
						continue;
					}
					Vector3 chunkPosition = volume.transform.position + chunkdata.ChunkPos.ToRealPosition() - RotationOffset[(int) volume.transform.eulerAngles.y / 90].ToRealPosition();
					foreach (var compareChunkData in compareVolume.vd.chunkDatas) {
						Vector3 compareChunkPosition = compareVolume.transform.position + compareChunkData.ChunkPos.ToRealPosition() - RotationOffset[(int) compareVolume.transform.eulerAngles.y / 90].ToRealPosition();
						// Calculate both distance. If it is out of maximum distance of interact then ignore it. 
						if (Vector3.Distance(chunkPosition, compareChunkPosition) > CHUNK_DISTANCE_MAXIMUM) {
							continue;
						}
						// Chunk interact.
						if (ChunkInteract(chunkdata, compareChunkData, chunkPosition, compareChunkPosition)) {
							return true;
						}
					}
				}	
			}
			return false;
		}
		// Chunk interact.
		private static bool ChunkInteract(ChunkData chunkData, ChunkData compareChunkData, Vector3 chunkPosition, Vector3 compareChunkPosition) {
			// Get all of Blocks.
			foreach (var block in chunkData.blocks) {
				Vector3 blockPosition = chunkPosition + block.BlockPos.ToRealPosition();
				// Transform Block into Bounds.
				Bounds bounds = new Bounds(blockPosition + MINIMUM_SIZE, MINIMUM_SIZE * 2 - OFFSET_SIZE);
				// Bounds interact.
				if (BoundsInteract(bounds, compareChunkData, compareChunkPosition)) {
					return true;
				}
			}
			// Get all of BlockAirs.
			foreach (var blockAir in chunkData.blockAirs) {
				Vector3 blockPosition = chunkPosition + blockAir.BlockPos.ToRealPosition();
				// Through pass all of pieceNames to get maximum of item size.
				Vector3 maximumScale = new Vector3(1, 1, 1);
				foreach (var names in blockAir.pieceNames) {
					if (names == "") { continue; }
					try{
					if (maximumScale.x < BlockAirScale[names].x) { maximumScale.x = BlockAirScale[names].x; }
					if (maximumScale.y < BlockAirScale[names].y) { maximumScale.y = BlockAirScale[names].y; }
					if (maximumScale.z < BlockAirScale[names].z) { maximumScale.z = BlockAirScale[names].z; }
					}catch{ Debug.Log("Missing name: " + names); }
				}
				// Transform relative size into absolute size.
				maximumScale = Vector3.Scale(MINIMUM_SIZE, maximumScale);
				// Transform BlockAirs into Bounds.
				Bounds bounds = new Bounds(blockPosition + maximumScale, maximumScale * 2 - OFFSET_SIZE);
				// Bounds interact.
				if (BoundsInteract(bounds, compareChunkData, compareChunkPosition)) {
					return true;
				}
			}
			// No interact then return false.
			return false;
		}
		// Bounds Interact.
		private static bool BoundsInteract(Bounds bounds, ChunkData compareChunkData, Vector3 compareChunkPosition) {
			// Get all of Blocks.
			foreach (var compareBlock in compareChunkData.blocks) {
				Vector3 compareBlockPosition = compareChunkPosition + compareBlock.BlockPos.ToRealPosition();
				// Transform into Bounds.
				Bounds compareBounds = new Bounds(compareBlockPosition + MINIMUM_SIZE, MINIMUM_SIZE * 2);
				// Both bounds interact.
				if (bounds.Intersects(compareBounds)) {
					return true;
				}
			}
			// Get all of BlockAirs.
			foreach (var compareBlockAir in compareChunkData.blockAirs) {
				Vector3 compareBlockPosition = compareChunkPosition + compareBlockAir.BlockPos.ToRealPosition();
				// Through pass all of pieceNames to get maximum of item size.
				Vector3 maximumScale = new Vector3(1, 1, 1);
				foreach (var names in compareBlockAir.pieceNames) {
					if (names == "") { continue; }
					try {
						if (maximumScale.x < BlockAirScale[names].x) { maximumScale.x = BlockAirScale[names].x; }
						if (maximumScale.y < BlockAirScale[names].y) { maximumScale.y = BlockAirScale[names].y; }
						if (maximumScale.z < BlockAirScale[names].z) { maximumScale.z = BlockAirScale[names].z; }
					} catch { Debug.Log("Missing name: " + names); }
				}
				// Transform relative size into absolute size.
				maximumScale = Vector3.Scale(MINIMUM_SIZE, maximumScale);
				// Transform BlockAirs into Bounds.
				Bounds compareBounds = new Bounds(compareBlockPosition + maximumScale, maximumScale * 2);
				if (bounds.Intersects(compareBounds)) {
					// Both bounds interact.
					return true;
				}
			}
			return false;
		}
		#endregion
	}
}