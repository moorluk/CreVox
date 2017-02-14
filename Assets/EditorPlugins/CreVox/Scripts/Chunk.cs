using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace CreVox
{

	[System.Serializable]
	public class ChunkData
	{
		public WorldPos ChunkPos;
		public List<Block> blocks = new List<Block> ();
		public List<BlockAir> blockAirs = new List<BlockAir> ();
		public List<BlockHold> blockHolds = new List<BlockHold> ();
	}

	[RequireComponent (typeof(MeshFilter))]
	[RequireComponent (typeof(MeshRenderer))]
	[RequireComponent (typeof(MeshCollider))]
	[Serializable]
	public class Chunk : MonoBehaviour
	{
		public static int chunkSize{ get { return VGlobal.GetSetting ().chunkSize; } }

		public ChunkData cData;

		public Volume volume;
		[SerializeField] MeshFilter filter;
		[SerializeField] MeshCollider coll;

		public void Init ()
		{
			filter = gameObject.GetComponent<MeshFilter> ();
			coll = gameObject.GetComponent<MeshCollider> ();
//			coll.hideFlags = HideFlags.HideInHierarchy;
			UpdateChunk ();
		}

		public void Destroy ()
		{
			cData = new ChunkData ();
		}

		void Start ()
		{
			filter = gameObject.GetComponent<MeshFilter> ();
			coll = gameObject.GetComponent<MeshCollider> ();
		}

		public void UpdateChunk ()
		{

			for (int i = 0; i < cData.blockAirs.Count; i++) {
				BlockAir bAir = cData.blockAirs [i];
				bool isEmpty = true;
				foreach (string p in bAir.pieceNames) {
					if (p != "") {
						isEmpty = false;
						break;
					}
				}
				if (isEmpty) {
					cData.blockAirs.Remove (bAir);
				} else { 
					WorldPos volumePos = new WorldPos (
						                     cData.ChunkPos.x + bAir.BlockPos.x, 
						                     cData.ChunkPos.y + bAir.BlockPos.y, 
						                     cData.ChunkPos.z + bAir.BlockPos.z);
					if (volume != null) {
						if (volume.GetNode (volumePos) != null) {
							#if UNITY_EDITOR
							if (!UnityEditor.EditorApplication.isPlaying && volume.cuter && bAir.BlockPos.y + cData.ChunkPos.y > volume.cutY)
								volume.GetNode (volumePos).SetActive (false);
							else
								volume.GetNode (volumePos).SetActive (true);
							#else
						volume.GetNode (volumePos).SetActive (true);
							#endif
						}
					}
				}
			}
			UpdateMeshFilter ();
			UpdateMeshCollider ();
		}

		public Block GetBlock (int x, int y, int z)
		{
			WorldPos pos = new WorldPos (x, y, z);
			for (int i = 0; i < cData.blocks.Count; i++) {
				if (cData.blocks [i].BlockPos.Compare (pos))
					return cData.blocks [i];
			}
			for (int i = 0; i < cData.blockHolds.Count; i++) {
				if (cData.blockHolds [i].BlockPos.Compare (pos))
					return cData.blockHolds [i];
			}
			for (int i = 0; i < cData.blockAirs.Count; i++) {
				if (cData.blockAirs [i].BlockPos.Compare (pos))
					return cData.blockAirs [i];
			}
				return null;
		}



		public static bool InRange (int index)
		{
			if (index < 0 || index >= chunkSize)
				return false;

			return true;
		}

		public void UpdateMeshFilter ()
		{
			MeshData meshData = new MeshData ();
			for (int i = 0; i < cData.blocks.Count; i++) {
				Block b = cData.blocks [i];
				#if UNITY_EDITOR
				bool isCut = (volume == null) ? false : (volume.cuter && b.BlockPos.y + cData.ChunkPos.y > volume.cutY);
				if (!isCut)
				#endif
					meshData = b.MeahAddMe (this, b.BlockPos.x, b.BlockPos.y, b.BlockPos.z, meshData);
			}
			AssignRenderMesh (meshData);
		}

		public void UpdateMeshCollider ()
		{
			MeshData meshData = new MeshData ();
			for (int i = 0; i < cData.blocks.Count; i++) {
				Block b = cData.blocks [i];
				#if UNITY_EDITOR
				bool isCut = (volume != null) ? (volume.cuter && b.BlockPos.y + cData.ChunkPos.y > volume.cutY) : false;
				if (!isCut)
				#endif
					meshData = b.ColliderAddMe (this, b.BlockPos.x, b.BlockPos.y, b.BlockPos.z, meshData);
			}
			AssignCollisionMesh (meshData);
		}

		void AssignRenderMesh (MeshData meshData)
		{
#if UNITY_EDITOR
			filter.sharedMesh = null;
			Mesh mesh = new Mesh ();
			mesh.vertices = meshData.vertices.ToArray ();
			mesh.triangles = meshData.triangles.ToArray ();
			mesh.uv = meshData.uv.ToArray ();
			mesh.RecalculateNormals ();
			filter.sharedMesh = mesh;
#else
	        filter.mesh.Clear();
	        filter.mesh.vertices = meshData.vertices.ToArray();
	        filter.mesh.triangles = meshData.triangles.ToArray();
	        filter.mesh.uv = meshData.uv.ToArray();
	        filter.mesh.RecalculateNormals();
#endif
		}

		void AssignCollisionMesh (MeshData meshData)
		{
			coll.sharedMesh = null;
			Mesh cmesh = new Mesh ();
			cmesh.vertices = meshData.colVertices.ToArray ();
			cmesh.triangles = meshData.colTriangles.ToArray ();
			cmesh.RecalculateNormals ();
			coll.sharedMesh = cmesh;
		}
	}
}