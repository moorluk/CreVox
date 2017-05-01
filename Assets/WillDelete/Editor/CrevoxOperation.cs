using System.Collections.Generic;
using UnityEngine;
using CreVox;
using UnityEditor;
using System.Linq;
using System.Text.RegularExpressions;

namespace CrevoxExtend {

	public class CrevoxOperation {
		// Compute the position after rotated.
		public static WorldPos AbsolutePosition(WorldPos position, float degree) {
			Vector2 aPoint = new Vector2(position.x, position.z);
			// Set 4, 4 to be center point.
			float rad = degree * Mathf.Deg2Rad;
			float sin = Mathf.Sin(rad);
			float cos = Mathf.Cos(rad);
			return new WorldPos((int) Mathf.Round(aPoint.x * cos + aPoint.y * sin),
				position.y,
				(int) Mathf.Round(aPoint.y * cos - aPoint.x * sin));
		}
		public static Vector3 AbsolutePosition(Vector3 position, float degree) {
			Vector2 aPoint = new Vector2(position.x, position.z);
			// Set 4, 4 to be center point.
			float rad = degree * Mathf.Deg2Rad;
			float sin = Mathf.Sin(rad);
			float cos = Mathf.Cos(rad);
			return new Vector3(Mathf.Round(aPoint.x * cos + aPoint.y * sin),
				position.y,
				Mathf.Round(aPoint.y * cos - aPoint.x * sin));
		}
		// Volume Manager object.
		private static VolumeManager resultVolumeManager;
		private static Dictionary<VolumeData, List<ConnectionInfo>> doorInfoVdataTable;

		// Create Volume object and return it.
		public static Volume CreateVolumeObject(VolumeData vdata) {
			GameObject volumeObject = new GameObject() { name = vdata.name };
			Volume volume = volumeObject.AddComponent<Volume>();
			volume.vd = vdata;
			VolumeExtend volumeExtend = volumeObject.AddComponent<VolumeExtend>();
			if (!doorInfoVdataTable.ContainsKey(vdata)) {
				doorInfoVdataTable[vdata] = GetDoorPosition(vdata);
			}
			volumeExtend.ConnectionInfos = new List<ConnectionInfo>(doorInfoVdataTable[vdata].Select(x => x.Clone()).ToArray());

			volumeObject.transform.parent = resultVolumeManager.transform;
			volume.Init(volume.chunkX, volume.chunkY, volume.chunkZ);
			return volume;
		}

		// Initial the resultVolumeData and create the VolumeManager.
		public static Volume InitialVolume(VolumeData vdata) {
			doorInfoVdataTable = new Dictionary<VolumeData, List<ConnectionInfo>>();
			GameObject volumeMangerObject = new GameObject() { name = "VolumeManger(Generated)" };
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
			Quaternion rotationOfVolume1 = volume1.transform.rotation;
			Quaternion rotationOfVolume2 = volume2.transform.rotation;
			// Compare door connection.
			ConnectionInfo[] connections2 = volumeExtend2.ConnectionInfos.OrderBy(x => Random.value).ToArray();
			ConnectionInfo[] connections1 = volumeExtend1.ConnectionInfos.OrderBy(x => Random.value).ToArray();
			foreach (var connection2 in connections2) {
				if (connection2.used) {
					continue;
				}
				foreach (var connection1 in connections1) {
					if (connection1.used) {
						continue;
					}
					if(connection1.type == connection2.type) {
						continue;
					}
					// Added vdata need to rotate for matching  origin vdata.
					int rotateAngle = ( ( (int) (connection1.rotation.eulerAngles + rotationOfVolume1.eulerAngles).y % 360 ) - (int) connection2.rotation.eulerAngles.y );
					if (rotateAngle < 0) {
						rotateAngle += 360;
					}
					// Relative position between connections.
					relativePosition = AbsolutePosition(connection1.position, rotationOfVolume1.eulerAngles.y) - AbsolutePosition(connection2.position, rotateAngle);
					relativePosition += connection1.RelativePosition((rotationOfVolume1.eulerAngles.y));
					
					// Rotation
					volume2.transform.eulerAngles = new Vector3(0, rotateAngle, 0);
					// Absolute position.
					volume2.transform.localPosition = volume1.transform.position + relativePosition.ToRealPosition();
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
		// Combine both volumeData.
		public static bool CombineVolumeObject(Volume originVolume, Volume newVolume, ConnectionInfo originConnection, ConnectionInfo newConnection) {
			Quaternion rotationOfVolume1 = originVolume.transform.rotation;
			Quaternion rotationOfVolume2 = newVolume.transform.rotation;
			// Added vdata need to rotate for matching  origin vdata.
			int rotateAngle = ( ( (int) ( originConnection.rotation.eulerAngles + rotationOfVolume1.eulerAngles ).y % 360 ) - (int) newConnection.rotation.eulerAngles.y );
			if (rotateAngle < 0) {
				rotateAngle += 360;
			}
			// Relative position between connections.
			WorldPos relativePosition = AbsolutePosition(originConnection.position, rotationOfVolume1.eulerAngles.y) - AbsolutePosition(newConnection.position, rotateAngle);
			relativePosition += originConnection.RelativePosition(( rotationOfVolume1.eulerAngles.y ));

			// Rotation
			newVolume.transform.eulerAngles = new Vector3(0, rotateAngle, 0);
			// Absolute position.
			newVolume.transform.localPosition = originVolume.transform.position + relativePosition.ToRealPosition();
			if (!IsCollider(newVolume)) {
				Debug.Log("Combine finish.");
				return true;
			}
			Debug.Log("No door can combine.");
			return false;
		}
		public static void ReplaceConnection() {
			foreach (var volume in resultVolumeManager.GetComponentsInChildren<Volume>()) {
				VolumeExtend volumeExtend = volume.GetComponent<VolumeExtend>();
				foreach (var connection in volumeExtend.ConnectionInfos.FindAll( c => !c.used && c.type == ConnectionInfoType.Connection )) {
					bool success = false;
					foreach (var vdata in SpaceAlphabet.replaceDictionary[connection.connectionName].OrderBy(x=>Random.value)) {
						Volume replaceVol = CreateVolumeObject(vdata);
						ConnectionInfo replaceStartingNode = replaceVol.GetComponent<VolumeExtend>().ConnectionInfos.Find(x=>x.type==ConnectionInfoType.StartingNode);
						if (CombineVolumeObject(volume, replaceVol, connection, replaceStartingNode)) {
							connection.used = true;
							success = true;
							break;
						}
					}
					if (!success) {
						Debug.Log(volume.name + ":" + connection.connectionName + " replace failed.");
					}
				}
			}
		}





		// Get volumedata via path as string.
		public static VolumeData GetVolumeData(string path) {
			VolumeData vdata = (VolumeData) AssetDatabase.LoadAssetAtPath(path, typeof(VolumeData));
			return vdata;
		}
		private static string regex = @"Connection_(\w+)$";
		// Get the list that contains postisions and directions of door(conntection) via volumedata.
		private static List<ConnectionInfo> GetDoorPosition(VolumeData vdata) {
			List<ConnectionInfo> connections = new List<ConnectionInfo>();
			// All chunk.
			foreach (var blockItem in vdata.blockItems) {
				if (blockItem.pieceName == "") { continue; }
				if (blockItem.pieceName == "Starting Node") {
					connections.Add(new ConnectionInfo(blockItem.BlockPos, new Quaternion(blockItem.rotX, blockItem.rotY, blockItem.rotZ, blockItem.rotW), ConnectionInfoType.StartingNode));
				}
				else if (Regex.IsMatch(blockItem.pieceName, regex)) { 
					string connectionName = Regex.Match(blockItem.pieceName, regex).Groups[1].Value;
					connections.Add(new ConnectionInfo(blockItem.BlockPos, new Quaternion(blockItem.rotX, blockItem.rotY, blockItem.rotZ, blockItem.rotW), ConnectionInfoType.Connection, connectionName));
				}
			}
			return connections;
		}

		#region 重疊判定
		static float CHUNK_DISTANCE_MAXIMUM = 37.5233f; // Vector3.Magnitude(new Vector3(24, 16, 24))
		
		// Collision
		private static bool IsCollider(Volume volume) {
			foreach (var chunkdata in volume.vd.chunkDatas) {
				foreach (var compareVolume in resultVolumeManager.GetComponentsInChildren<Volume>()) {
					if (compareVolume.GetHashCode() == volume.GetHashCode()) {
						continue;
					}
					float rotateAngle = volume.transform.eulerAngles.y >= 0 ? volume.transform.eulerAngles.y : volume.transform.eulerAngles.y + 360;
					float compareRotateAngle = compareVolume.transform.eulerAngles.y >= 0 ? compareVolume.transform.eulerAngles.y : compareVolume.transform.eulerAngles.y + 360;
					Vector3 chunkPosition = volume.transform.position + AbsolutePosition(chunkdata.ChunkPos, rotateAngle).ToRealPosition();
					foreach (var compareChunkData in compareVolume.vd.chunkDatas) {
						Vector3 compareChunkPosition = compareVolume.transform.position + AbsolutePosition(compareChunkData.ChunkPos, compareRotateAngle).ToRealPosition();
						// Calculate both distance. If it is out of maximum distance of interact then ignore it. 
						if (Vector3.Distance(chunkPosition, compareChunkPosition) > CHUNK_DISTANCE_MAXIMUM) {
							//Debug.Log(compareVolume.name);
							continue;
						}
						// Chunk interact.
						if (ChunkInteract(chunkdata, compareChunkData, chunkPosition, compareChunkPosition, rotateAngle, compareRotateAngle)) {
							return true;
						}
					}
				}
			}
			return false;
		}
		// Chunk interact.
		private static bool ChunkInteract(ChunkData chunkData, ChunkData compareChunkData, Vector3 chunkPosition, Vector3 compareChunkPosition, float rotateAngle, float compareRotateAngle) {
			// Get all of Blocks.
			foreach (var block in chunkData.blocks) {
				Vector3 blockPosition = chunkPosition + AbsolutePosition(block.BlockPos, rotateAngle).ToRealPosition();
				// Get all of compared blocks.
				foreach (var compareBlock in compareChunkData.blocks) {
					Vector3 compareBlockPosition = compareChunkPosition + AbsolutePosition(compareBlock.BlockPos, compareRotateAngle).ToRealPosition();
					// Both postition interact.
					if (blockPosition == compareBlockPosition) {
						return true;
					}
				}
			}
			// No interact then return false.
			return false;
		}
		#endregion
	}
}