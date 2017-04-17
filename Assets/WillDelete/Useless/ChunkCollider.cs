using CreVox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ChunkCollider : MonoBehaviour {
	public bool isColi = false;
	public GameObject resultVolumeManager;
	void Start() {
		resultVolumeManager = GameObject.Find("resultVolumeManager");
	}
	void Update() {
		isColi = isCollider(GetComponent<Volume>());
	}
	private bool isCollider(Volume volume) {
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
	private bool interact(Vector3 position, Volume volume) {
		foreach (var compareVolume in resultVolumeManager.GetComponentsInChildren<Volume>()) {
			if(compareVolume == volume) {
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
