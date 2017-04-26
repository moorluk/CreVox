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
		aPoint -= new Vector2(4, 4);
		float rad = degree * Mathf.Deg2Rad;
		float sin = Mathf.Sin(rad);
		float cos = Mathf.Cos(rad);
		return new WorldPos((int) Mathf.Round(aPoint.x * cos + aPoint.y * sin + 4),
			position.y,
			(int) Mathf.Round(aPoint.y * cos - aPoint.x * sin + 4));
	}
	private static WorldPos[] RotationOffset = new WorldPos[] {
			new WorldPos(0, 0, 0),
			new WorldPos(0, 0, 8),
			new WorldPos(8, 0, 8),
			new WorldPos(8, 0, 0)
		};
	// The set about Size of BlockAir object.
	private static Dictionary<string, Vector3> BlockAirScale = new Dictionary<string, Vector3>() {
			{ "Door", new Vector3(1, 3, 1) },
			{ "Wall", new Vector3(1, 3, 1) },
			{ "WindowX1.0", new Vector3(0, 0, 0) },
			{ "WindowX2.0", new Vector3(0, 0, 0)}
		};
	private static Vector3 MINIMUM_SIZE = new Vector3(1.5f, 1.0f, 1.5f);
	private static Vector3 OFFSET_SIZE = new Vector3(0.2f, 0.2f, 0.2f);
	const float CHUNK_DISTANCE_MAXIMUM = 41.5692f; // Vector3.Magnitude(new Vector3(24, 24, 24))

	// Collision
	private bool IsCollider(Volume volume) {
		foreach (var chunkdata in volume.vd.chunkDatas) {
			foreach (var compareVolume in resultVolumeManager.GetComponentsInChildren<Volume>()) {
				if (compareVolume.GetHashCode() == volume.GetHashCode()) {
					continue;
				}
				float rotateAngle = volume.transform.eulerAngles.y >= 0 ? volume.transform.eulerAngles.y : volume.transform.eulerAngles.y + 360;
				float compareRotateAngle = compareVolume.transform.eulerAngles.y >= 0 ? compareVolume.transform.eulerAngles.y : compareVolume.transform.eulerAngles.y + 360;
				Vector3 chunkPosition = volume.transform.position + chunkdata.ChunkPos.ToRealPosition() - RotationOffset[(int)rotateAngle / 90].ToRealPosition();
				foreach (var compareChunkData in compareVolume.vd.chunkDatas) {
					Vector3 compareChunkPosition = compareVolume.transform.position + compareChunkData.ChunkPos.ToRealPosition() - RotationOffset[(int)compareRotateAngle / 90].ToRealPosition();
					// Calculate both distance. If it is out of maximum distance of interact then ignore it. 
					if (Vector3.Distance(chunkPosition, compareChunkPosition) > CHUNK_DISTANCE_MAXIMUM) {
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
		foreach (var block in chunkData.blocks) {
			Vector3 blockPosition = chunkPosition + AbsolutePosition(block.BlockPos, rotateAngle).ToRealPosition();
			// Transform Block into Bounds.
			Bounds bounds = new Bounds(blockPosition + MINIMUM_SIZE, MINIMUM_SIZE * 2 - OFFSET_SIZE);
			// Bounds interact.
			if (BoundsIntersect(bounds, compareChunkData, compareChunkPosition, rotateAngle, compareRotateAngle)) {
				return true;
			}
		}
		// Get all of BlockAirs.
		foreach (var blockAir in chunkData.blockAirs) {
			Vector3 blockPosition = chunkPosition + AbsolutePosition(blockAir.BlockPos, rotateAngle).ToRealPosition();
			// Through pass all of pieceNames to get maximum of item size.
			Vector3 maximumScale = new Vector3(0, 0, 0);
			foreach (var names in blockAir.pieceNames) {
				if (names == "") { continue; }
				if (BlockAirScale.ContainsKey(names)) {
					if (maximumScale.x < BlockAirScale[names].x) { maximumScale.x = BlockAirScale[names].x; }
					if (maximumScale.y < BlockAirScale[names].y) { maximumScale.y = BlockAirScale[names].y; }
					if (maximumScale.z < BlockAirScale[names].z) { maximumScale.z = BlockAirScale[names].z; }
				} else {
					if (maximumScale.x < 1) { maximumScale.x = 1; }
					if (maximumScale.y < 1) { maximumScale.y = 1; }
					if (maximumScale.z < 1) { maximumScale.z = 1; }

				}
			}
			// Transform relative size into absolute size.
			maximumScale = Vector3.Scale(MINIMUM_SIZE, maximumScale);
			// Transform BlockAirs into Bounds.
			Bounds bounds = new Bounds(blockPosition + maximumScale, maximumScale * 2 - OFFSET_SIZE);
			// Bounds interact.
			if (BoundsIntersect(bounds, compareChunkData, compareChunkPosition, rotateAngle, compareRotateAngle)) {
				return true;
			}
		}
		// No interact then return false.
		return false;
	}
	// Bounds Interact.
	private bool BoundsIntersect(Bounds bounds, ChunkData compareChunkData, Vector3 compareChunkPosition, float rotateAngle, float compareRotateAngle) {
		// Get all of Blocks.
		foreach (var compareBlock in compareChunkData.blocks) {
			Vector3 compareBlockPosition = compareChunkPosition + AbsolutePosition(compareBlock.BlockPos, compareRotateAngle).ToRealPosition();
			// Transform into Bounds.
			Bounds compareBounds = new Bounds(compareBlockPosition + MINIMUM_SIZE, MINIMUM_SIZE * 2);
			// Both bounds interact.
			if (bounds.Intersects(compareBounds)) {
				return true;
			}
		}
		// Get all of BlockAirs.
		foreach (var compareBlockAir in compareChunkData.blockAirs) {
			Vector3 compareBlockPosition = compareChunkPosition + AbsolutePosition(compareBlockAir.BlockPos, compareRotateAngle).ToRealPosition();
			// Through pass all of pieceNames to get maximum of item size.
			Vector3 maximumScale = new Vector3(0, 0, 0);
			foreach (var names in compareBlockAir.pieceNames) {
				if (names == "") { continue; }
				if (BlockAirScale.ContainsKey(names)) {
					if (maximumScale.x < BlockAirScale[names].x) { maximumScale.x = BlockAirScale[names].x; }
					if (maximumScale.y < BlockAirScale[names].y) { maximumScale.y = BlockAirScale[names].y; }
					if (maximumScale.z < BlockAirScale[names].z) { maximumScale.z = BlockAirScale[names].z; }
				} else {
					if (maximumScale.x < 1) { maximumScale.x = 1; }
					if (maximumScale.y < 1) { maximumScale.y = 1; }
					if (maximumScale.z < 1) { maximumScale.z = 1; }

				}
			}
			// Transform relative size into absolute size.
			maximumScale = Vector3.Scale(MINIMUM_SIZE, maximumScale);
			// Transform BlockAirs into Bounds.
			Bounds compareBounds = new Bounds(compareBlockPosition + maximumScale, maximumScale * 2);
			if (bounds.Intersects(compareBounds)) {
				Debug.Log(compareBlockAir.BlockPos);
				foreach (var names in compareBlockAir.pieceNames) {
					if (names != "") {
						Debug.Log(names);
					}
				}
				// Both bounds interact.
				return true;
			}
		}
		return false;
	}

}
