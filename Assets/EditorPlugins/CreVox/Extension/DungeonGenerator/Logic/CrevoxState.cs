using System.Collections.Generic;
using UnityEngine;
using CreVox;
using System.Linq;
using System.Text.RegularExpressions;
using System;

namespace CrevoxExtend {

	public class CrevoxState {
		// Static member.
		// Get connectionInfos from vdata.
		public static Dictionary<VolumeData, List<ConnectionInfo>> ConnectionInfoVdataTable = new Dictionary<VolumeData, List<ConnectionInfo>>();
		// Compute the position after rotated.
		public static WorldPos AbsolutePosition(WorldPos position, float degree) {
			Vector2 aPoint = new Vector2(position.x, position.z);
			// Set 4, 4 to be center point.
			float rad = degree * Mathf.Deg2Rad;
			float sin = Mathf.Sin(rad);
			float cos = Mathf.Cos(rad);
			return new WorldPos((int) Mathf.Round(
                aPoint.x * cos + aPoint.y * sin),
				position.y,
				(int) Mathf.Round(aPoint.y * cos - aPoint.x * sin));
		}
		public static Vector3 AbsolutePosition(Vector3 position, float degree) {
			Vector2 aPoint = new Vector2(position.x, position.z);
			// Set 4, 4 to be center point.
			float rad = degree * Mathf.Deg2Rad;
			float sin = Mathf.Sin(rad);
			float cos = Mathf.Cos(rad);
			return new Vector3(
                /*Mathf.Round(*/aPoint.x * cos + aPoint.y * sin/*)*/,
				position.y,
				/*Mathf.Round(*/aPoint.y * cos - aPoint.x * sin/*)*/
            );
		}
		// Get the list that contains postisions and directions of door(conntection) via volumedata.
		private static List<ConnectionInfo> GetConnectionPosition(VolumeData vdata) {
			const string regex = @"Connection_(\w+)$";
			List<ConnectionInfo> connections = new List<ConnectionInfo>();
			// All chunk.
			foreach (var blockItem in vdata.blockItems) {
				if (blockItem.pieceName == "") { continue; }
				if (blockItem.pieceName == "Starting Node") {
					connections.Add(new ConnectionInfo(blockItem.BlockPos, new Quaternion(blockItem.rotX, blockItem.rotY, blockItem.rotZ, blockItem.rotW), ConnectionInfoType.StartingNode, "Starting Node"));
				} else if (Regex.IsMatch(blockItem.pieceName, regex)) {
					string connectionName = Regex.Match(blockItem.pieceName, regex).Groups[1].Value;
					connections.Add(new ConnectionInfo(blockItem.BlockPos, new Quaternion(blockItem.rotX, blockItem.rotY, blockItem.rotZ, blockItem.rotW), ConnectionInfoType.Connection, connectionName));
				}
			}
			return connections;
		}

		// Volume List
		private List<VolumeDataEx> _resultVolumeDatas;
		public Dictionary<Guid, VolumeDataEx> VolumeDatasByID;
		public List<VolumeDataEx> ResultVolumeDatas {
			get { return _resultVolumeDatas; }
			set { _resultVolumeDatas = value; }
		}
		// Constructor.
		public CrevoxState() {
			_resultVolumeDatas = new List<VolumeDataEx>();
			VolumeDatasByID = new Dictionary<Guid, VolumeDataEx>();
		}
		public CrevoxState(VolumeData vdata) {
			_resultVolumeDatas = new List<VolumeDataEx>();
			_resultVolumeDatas.Add(new VolumeDataEx(vdata));
			VolumeDatasByID = new Dictionary<Guid, VolumeDataEx>();
		}
		public CrevoxState(CrevoxState clone) {
			_resultVolumeDatas = new List<VolumeDataEx>(clone._resultVolumeDatas.Select(x => new VolumeDataEx(x)).ToArray());
			VolumeDatasByID = clone.VolumeDatasByID;
		}
		public CrevoxState Clone() {
			return new CrevoxState(this);
		}
		// Combine both volumeDataEx.
		public bool CombineVolumeObject(VolumeDataEx originVolumeEx, VolumeDataEx newVolumeEx, ConnectionInfo originConnection, ConnectionInfo newConnection) {
			Quaternion rotationOfVolume1 = originVolumeEx.rotation;
			// If new connection is not starting node.
			int connectionRotationOffset = newConnection.type == ConnectionInfoType.Connection ? 180 : 0;
			// Added vdata need to rotate for matching  origin vdata.
			int rotateAngle = ( ( (int) (( originConnection.rotation.eulerAngles + rotationOfVolume1.eulerAngles ).y + connectionRotationOffset ) % 360 ) - (int) newConnection.rotation.eulerAngles.y );
			if (rotateAngle < 0) {
				rotateAngle += 360;
			}
			// Relative position between connections.
			WorldPos relativePosition = AbsolutePosition(originConnection.position, rotationOfVolume1.eulerAngles.y) - AbsolutePosition(newConnection.position, rotateAngle);
			relativePosition += originConnection.RelativePosition(( rotationOfVolume1.eulerAngles.y ));

			// Rotation
			newVolumeEx.rotation.eulerAngles = new Vector3(0, rotateAngle, 0);
			// Absolute position.
			newVolumeEx.position = originVolumeEx.position + relativePosition.ToRealPosition();
			if (!IsBoundCollider(newVolumeEx)) {
				//Debug.Log("Combine finish.");
				return true;
			}
			//Debug.Log("No door can combine.");
			return false;
		}

        #region 碰撞判定
        private bool IsBoundCollider(VolumeDataEx volumeEx) {
            foreach (var bb in volumeEx.volumeData.blockBounds) {
                float r = volumeEx.rotation.eulerAngles.y;
                r = r >= 0 ? r : r + 360;
                Vector3 min = volumeEx.position + AbsolutePosition (bb.GetMin (), r);
                Vector3 max = volumeEx.position + AbsolutePosition (bb.GetMax (), r);
                float minX = Mathf.Min (min.x, max.x);
                float maxX = Mathf.Max (min.x, max.x);
                float minY = Mathf.Min (min.y, max.y);
                float maxY = Mathf.Max (min.y, max.y);
                float minZ = Mathf.Min (min.z, max.z);
                float maxZ = Mathf.Max (min.z, max.z);

                foreach (var compareVolumeEx in _resultVolumeDatas) {
                    foreach (var cbb in compareVolumeEx.volumeData.blockBounds) {
                        if (ReferenceEquals (volumeEx, compareVolumeEx))
                            continue;

                        float cr = compareVolumeEx.rotation.eulerAngles.y;
                        cr = cr >= 0 ? cr : cr + 360;
                        Vector3 cMin = compareVolumeEx.position + AbsolutePosition (cbb.GetMin (), cr);
                        Vector3 cMax = compareVolumeEx.position + AbsolutePosition (cbb.GetMax (), cr);
                        float cMinX = Mathf.Min (cMin.x, cMax.x);
                        float cMaxX = Mathf.Max (cMin.x, cMax.x);
                        if (cMinX < maxX || cMaxX > minX)
                            return true;
                        float cMinZ = Mathf.Min (cMin.z, cMax.z);
                        float cMaxZ = Mathf.Max (cMin.z, cMax.z);
                        if (cMinZ < maxZ || cMaxZ > minZ)
                            return true;
                        float cMinY = Mathf.Min (cMin.y, cMax.y);
                        float cMaxY = Mathf.Max (cMin.y, cMax.y);
                        if (cMinY < maxY || cMaxY > minY)
                            return true;
                    }
                }
            }
            return false;
        }

        static float CHUNK_DISTANCE_MAXIMUM = 100f; //37.5233f; // Vector3.Magnitude(new Vector3(24, 16, 24))

        // Collision
		private bool IsCollider(VolumeDataEx volumeEx) {
            foreach (var chunkdata in volumeEx.volumeData.GetChunkDatas()) {
				foreach (var compareVolumeEx in _resultVolumeDatas) {
					if (ReferenceEquals(volumeEx, compareVolumeEx)) {
						continue;
					}
					float rotateAngle = volumeEx.rotation.eulerAngles.y >= 0 ? volumeEx.rotation.eulerAngles.y : volumeEx.rotation.eulerAngles.y + 360;
					float compareRotateAngle = compareVolumeEx.rotation.eulerAngles.y >= 0 ? compareVolumeEx.rotation.eulerAngles.y : compareVolumeEx.rotation.eulerAngles.y + 360;
					Vector3 chunkPosition = volumeEx.position + AbsolutePosition(chunkdata.ChunkPos, rotateAngle).ToRealPosition();
                    foreach (var compareChunkData in compareVolumeEx.volumeData.GetChunkDatas()) {
						Vector3 compareChunkPosition = compareVolumeEx.position + AbsolutePosition(compareChunkData.ChunkPos, compareRotateAngle).ToRealPosition();
                        // Calculate both distance. If it is out of maximum distance of interact then ignore it. 
                        if (Vector3.Distance(chunkPosition, compareChunkPosition) > CHUNK_DISTANCE_MAXIMUM) { continue; }
                        // Chunk interact.
                        if (ChunkInteract(chunkdata, compareChunkData, chunkPosition, compareChunkPosition, rotateAngle, compareRotateAngle)) { return true; }
					}
				}
			}
			return false;
        }
		// Chunk interact.
		private bool ChunkInteract(ChunkData chunkData, ChunkData compareChunkData, Vector3 chunkPosition, Vector3 compareChunkPosition, float rotateAngle, float compareRotateAngle) {
			// Get all of BlockHolds.
			foreach (var blockHold in chunkData.blockHolds) {
				Vector3 blockPosition = chunkPosition + AbsolutePosition(blockHold.BlockPos, rotateAngle).ToRealPosition();
				// Get all of compared BlockHolds.
				foreach (var compareBlockHold in compareChunkData.blockHolds) {
					Vector3 compareBlockPosition = compareChunkPosition + AbsolutePosition(compareBlockHold.BlockPos, compareRotateAngle).ToRealPosition();
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
		
		public class VolumeDataEx {
			public Vector3 position;
			public Quaternion rotation;
			public VolumeData volumeData;
			private List<ConnectionInfo> _connectionInfos;
			public List<ConnectionInfo> ConnectionInfos {
				get { return _connectionInfos; }
				set { _connectionInfos = value; }
			}
			public VolumeDataEx() {
				this.position = Vector3.zero;
				this.rotation = Quaternion.identity;
				this.volumeData = null;
				this._connectionInfos = null;
			}
			public VolumeDataEx(VolumeData vdata) {
				this.position = Vector3.zero;
				this.rotation = Quaternion.identity;
				this.volumeData = vdata;
				if (!ConnectionInfoVdataTable.ContainsKey(vdata)) {
					ConnectionInfoVdataTable[vdata] = GetConnectionPosition(vdata);
				}
				this._connectionInfos = new List<ConnectionInfo>(ConnectionInfoVdataTable[vdata].Select(x => x.Clone()).ToArray());
			}
			public VolumeDataEx(VolumeDataEx clone) {
				this.position = clone.position;
				this.rotation = clone.rotation;
				this.volumeData = clone.volumeData;
				this._connectionInfos = new List<ConnectionInfo>(clone._connectionInfos.Select(x => x.Clone()).ToArray());
			}
		}
	}
}