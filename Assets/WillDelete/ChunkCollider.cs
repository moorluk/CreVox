using CreVox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ChunkCollider : MonoBehaviour {
	public bool isCollider = false;
	public GameObject resultVolumeManager;
	void Start() {
		resultVolumeManager = GameObject.Find("resultVolumeManager");
	}
	void Update() {
		Chunk[] chunks = GetComponentsInChildren<Chunk>();
		foreach (var chunk in chunks) {
			foreach (var otherChunk in resultVolumeManager.GetComponentsInChildren<Chunk>()) {
				if (otherChunk == chunk) {
					Debug.Log("pass");
					continue;
				}
				if (chunk.GetComponent<MeshCollider>().bounds.Intersects(otherChunk.GetComponent<MeshCollider>().bounds)) {
					isCollider = true;
					Debug.Log(otherChunk.gameObject.name);
					return;
				}
			}
		}
		isCollider = false;
	}
}
