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

	private static WorldPos[] RotationOffset = new WorldPos[] {
			new WorldPos(0, 0, 0),
			new WorldPos(0, 0, 8),
			new WorldPos(8, 0, 8),
			new WorldPos(8, 0, 0)
		};
	// Collision
	const float CHUNK_DISTANCE_MAXIMUM = 41.5692f; // Vector3.Magnitude(new Vector3(24, 24, 24))
	private bool IsCollider(Volume volume) {
		foreach (var chunkdata in volume.vd.chunkDatas) {
			foreach (var compareVolume in resultVolumeManager.GetComponentsInChildren<Volume>()) {
				if (compareVolume == volume) {
					continue;
				}
				Vector3 chunkPosition = volume.transform.position + chunkdata.ChunkPos.ToRealPosition() - RotationOffset[(int) volume.transform.eulerAngles.y / 90].ToRealPosition();
				foreach (var compareChunkData in compareVolume.vd.chunkDatas) {
					Vector3 compareChunkPosition = compareVolume.transform.position + compareChunkData.ChunkPos.ToRealPosition() - RotationOffset[(int) compareVolume.transform.eulerAngles.y / 90].ToRealPosition();
					if (Vector3.Distance(chunkPosition, compareChunkPosition) > CHUNK_DISTANCE_MAXIMUM) {
						continue;
					}
					if (ChunkInteract(chunkdata, compareChunkData, chunkPosition, compareChunkPosition)) {
						return true;
					}
				}
			}
		}
		return false;
	}
	private Dictionary<string, Vector3> BlockAirScale = new Dictionary<string, Vector3>() {
			{ "Door", new Vector3(1, 3, 1) },
			{ "Wall", new Vector3(1, 3, 1) },
			{ "Gnd.in.bottom", new Vector3(1, 1, 1) },
			{ "Stair", new Vector3(1, 1, 1) }
		};
	private static Vector3 MINIMUM_SIZE = new Vector3(1.5f, 1.0f, 1.5f);
	private bool ChunkInteract(ChunkData chunkData, ChunkData compareChunkData, Vector3 chunkPosition, Vector3 compareChunkPosition) {
		foreach (var block in chunkData.blocks) {
			Vector3 blockPosition = chunkPosition + block.BlockPos.ToRealPosition();
			Bounds bounds = new Bounds(blockPosition + MINIMUM_SIZE, MINIMUM_SIZE * 2 - new Vector3(0.2f, 0.2f, 0.2f));
			if (BoundsInteract(bounds, compareChunkData, compareChunkPosition)) {
				return true;
			}
		}
		foreach (var blockAir in chunkData.blockAirs) {
			Vector3 blockPosition = chunkPosition + blockAir.BlockPos.ToRealPosition();
			Vector3 maximumScale = new Vector3(1, 1, 1);
			foreach (var names in blockAir.pieceNames) {
				if (names == "") { continue; }
				if (maximumScale.x < BlockAirScale[names].x) { maximumScale.x = BlockAirScale[names].x; }
				if (maximumScale.y < BlockAirScale[names].y) { maximumScale.y = BlockAirScale[names].y; }
				if (maximumScale.z < BlockAirScale[names].z) { maximumScale.z = BlockAirScale[names].z; }
			}
			maximumScale = Vector3.Scale(MINIMUM_SIZE, maximumScale);
			Bounds bounds = new Bounds(blockPosition + maximumScale, maximumScale * 2 - new Vector3(1f, 1f, 1f));
			if (BoundsInteract(bounds, compareChunkData, compareChunkPosition)) {
				return true;
			}
		}
		return false;
	}
	private bool BoundsInteract(Bounds bounds, ChunkData compareChunkData, Vector3 compareChunkPosition) {

		foreach (var compareBlock in compareChunkData.blocks) {
			Vector3 compareBlockPosition = compareChunkPosition + compareBlock.BlockPos.ToRealPosition();
			Bounds compareBounds = new Bounds(compareBlockPosition + MINIMUM_SIZE, MINIMUM_SIZE * 2);
			if (bounds.Intersects(compareBounds)) {
				
				return true;
			}
		}
		foreach (var compareBlockAir in compareChunkData.blockAirs) {
			Vector3 compareBlockPosition = compareChunkPosition + compareBlockAir.BlockPos.ToRealPosition();
			Vector3 maximumScale = new Vector3(1, 1, 1);
			foreach (var names in compareBlockAir.pieceNames) {
				if (names == "") { continue; }
				if (maximumScale.x < BlockAirScale[names].x) { maximumScale.x = BlockAirScale[names].x; }
				if (maximumScale.y < BlockAirScale[names].y) { maximumScale.y = BlockAirScale[names].y; }
				if (maximumScale.z < BlockAirScale[names].z) { maximumScale.z = BlockAirScale[names].z; }
			}
			maximumScale = Vector3.Scale(MINIMUM_SIZE, maximumScale);
			Bounds compareBounds = new Bounds(compareBlockPosition + maximumScale, maximumScale * 2);
			if (bounds.Intersects(compareBounds)) {
				Debug.Log(MINIMUM_SIZE);
				Debug.Log(bounds);
				Debug.Log(compareBounds);
				return true;
			}
		}
		return false;
	}

}
