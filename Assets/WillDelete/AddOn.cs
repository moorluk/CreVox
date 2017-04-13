using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CreVox;
using UnityEditor;

public class AddOn {
	public enum DirectionOfBlock {
		LeftTop,
		Top,
		RightTop,
		Left,
		Center,
		Right,
		LeftBottom,
		Bottom,
		RightBottom
	}
	public struct DoorInfo {
		public WorldPos position;
		public DirectionOfBlock direction;
		public DoorInfo(WorldPos position, DirectionOfBlock direction) {
			this.position = position;
			this.direction = direction;
		}
	}
	public static WorldPos[] DirectionTrans = new WorldPos[] {
		new WorldPos(-1, 0, 1),
		new WorldPos(0, 0, 1),
		new WorldPos(1, 0, 1),
		new WorldPos(-1, 0, 0),
		new WorldPos(0, 0, 0),
		new WorldPos(1, 0, 0),
		new WorldPos(-1, 0, -1),
		new WorldPos(0, 0, -1),
		new WorldPos(1, 0, -1)
	};

	private static VolumeData resultVolumeData;
	private static List<DoorInfo> volumeDataConnections;

	// Initial the resultVolumeData.
	public static void Initial(VolumeData vdataClone) {
		VolumeData vdata = new VolumeData(vdataClone);
		AssetDatabase.CreateAsset(vdata, "Assets/WillDelete/VolumeData/Result_vdata.asset");
		resultVolumeData = (VolumeData) AssetDatabase.LoadAssetAtPath("Assets/WillDelete/VolumeData/Result_vdata.asset", typeof(VolumeData));
		volumeDataConnections = GetDoorPosition(resultVolumeData);
		Debug.Log("Initial finish.");
	}
	// Create volumeMananger and volumeData gameObject.
	public static void CreateObject() {
		GameObject volumeObject = new GameObject() { name = "Result" };
		Volume volume = volumeObject.AddComponent<Volume>();
		volume.vd = resultVolumeData;
		
		volume._useBytes = false;
		volume.BuildVolume(new Save(), volume.vd);
		volume.tempPath = "";
		
		GameObject volumeMangerObject = new GameObject() { name = "VolumeManger" };
		volumeMangerObject.AddComponent<VolumeManager>();

		volumeObject.transform.parent = volumeMangerObject.transform;

		volumeMangerObject.GetComponent<VolumeManager>().UpdateDungeon();
		SceneView.RepaintAll();
		
		Debug.Log("Create finish.");
	}
	// Combine both volumeData.
	public static void CombineVolumeData(VolumeData vdataAdd) {
		// Get the connection.
		List<DoorInfo> connectionsAdd = GetDoorPosition(vdataAdd);
		WorldPos relativePosition = new WorldPos();
		//
		bool canCombine = false;
		foreach (var connectionAdd in connectionsAdd) {
			foreach (var connection in volumeDataConnections) {
				int combineArg = ( (int) connection.direction ) + ( (int) connectionAdd.direction );
				if(combineArg == 8) {
					relativePosition = connection.position - connectionAdd.position;
					relativePosition += DirectionTrans[(int)connection.direction];
					canCombine = true;
					break;
				}
			}
			if (canCombine)
				break;
		}
		if(! canCombine) {
			Debug.Log("No door can combine.");
			return;
		}
		if(relativePosition.x < 0) {
			foreach (var chunkData in resultVolumeData.chunkDatas) {
				chunkData.ChunkPos.x = chunkData.ChunkPos.x - relativePosition.x;
				// Get minimum of vdata range.
				resultVolumeData.chunkX = (int)Mathf.Ceil(Mathf.Max((float)resultVolumeData.chunkX, (float)chunkData.ChunkPos.x / 9.0f + 1));
				resultVolumeData.chunkY = (int)Mathf.Ceil(Mathf.Max((float)resultVolumeData.chunkY, (float)chunkData.ChunkPos.y / 9.0f + 1));
				resultVolumeData.chunkZ = (int)Mathf.Ceil(Mathf.Max((float)resultVolumeData.chunkZ, (float)chunkData.ChunkPos.z / 9.0f + 1));
			}
			relativePosition.x = 0; 
		}
		if (relativePosition.y < 0) {
			foreach (var chunkData in resultVolumeData.chunkDatas) {
				chunkData.ChunkPos.y = chunkData.ChunkPos.y - relativePosition.y;
				// Get minimum of vdata range.
				resultVolumeData.chunkX = (int)Mathf.Ceil(Mathf.Max((float)resultVolumeData.chunkX, (float)chunkData.ChunkPos.x / 9.0f + 1));
				resultVolumeData.chunkY = (int)Mathf.Ceil(Mathf.Max((float)resultVolumeData.chunkY, (float)chunkData.ChunkPos.y / 9.0f + 1));
				resultVolumeData.chunkZ = (int)Mathf.Ceil(Mathf.Max((float)resultVolumeData.chunkZ, (float)chunkData.ChunkPos.z / 9.0f + 1));
			}
			relativePosition.y = 0;
		}
		if (relativePosition.z < 0) {
			foreach (var chunkData in resultVolumeData.chunkDatas) {
				chunkData.ChunkPos.z = chunkData.ChunkPos.z - relativePosition.z;
				// Get minimum of vdata range.
				resultVolumeData.chunkX = (int)Mathf.Ceil(Mathf.Max((float)resultVolumeData.chunkX, (float)chunkData.ChunkPos.x / 9.0f + 1));
				resultVolumeData.chunkY = (int)Mathf.Ceil(Mathf.Max((float)resultVolumeData.chunkY, (float)chunkData.ChunkPos.y / 9.0f + 1));
				resultVolumeData.chunkZ = (int)Mathf.Ceil(Mathf.Max((float)resultVolumeData.chunkZ, (float)chunkData.ChunkPos.z / 9.0f + 1));
			}
			relativePosition.z = 0;
		}
		foreach (var chunkData in vdataAdd.chunkDatas) {
			// Deep copy setting.
			ChunkData newChunkData = new ChunkData(chunkData);
			newChunkData.ChunkPos = chunkData.ChunkPos + relativePosition;
			resultVolumeData.chunkDatas.Add(newChunkData);
			// Get minimum of vdata range.
			resultVolumeData.chunkX = (int) Mathf.Ceil(Mathf.Max((float) resultVolumeData.chunkX, (float) newChunkData.ChunkPos.x / 9.0f + 1));
			resultVolumeData.chunkY = (int) Mathf.Ceil(Mathf.Max((float) resultVolumeData.chunkY, (float) newChunkData.ChunkPos.y / 9.0f + 1));
			resultVolumeData.chunkZ = (int) Mathf.Ceil(Mathf.Max((float) resultVolumeData.chunkZ, (float) newChunkData.ChunkPos.z / 9.0f + 1));
		}
		Debug.Log("Combine finish.");
		volumeDataConnections = GetDoorPosition(resultVolumeData);
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
				for(int i = 0; i < blockAir.pieceNames.Length; i++) {
					// If get door then add.
					if(blockAir.pieceNames[i] == "Door") {
						doors.Add(new DoorInfo(blockAir.BlockPos, (DirectionOfBlock) i));
					}
				}
			}
		}
		return doors;
	}
}
