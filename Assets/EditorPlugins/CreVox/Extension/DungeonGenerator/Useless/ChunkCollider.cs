using CreVox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ChunkCollider : MonoBehaviour {
	public bool isColi = false;
	public VolumeManager resultVolumeManager;
	private void Update() {
		isColi = IsCollider(GetComponent<Volume>());
	}
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
	static float CHUNK_DISTANCE_MAXIMUM = 37.5233f; // Vector3.Magnitude(new Vector3(24, 16, 24))

	// Collision
	private bool IsCollider(Volume volume) {
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
	private bool ChunkInteract(ChunkData chunkData, ChunkData compareChunkData, Vector3 chunkPosition, Vector3 compareChunkPosition, float rotateAngle, float compareRotateAngle) {
		// Get all of Blocks.
		foreach (var block in chunkData.blockHolds) {
			Vector3 blockPosition = chunkPosition + AbsolutePosition(block.BlockPos, rotateAngle).ToRealPosition();
			// Get all of compared blocks.
			foreach (var compareBlock in compareChunkData.blockHolds) {
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

}
