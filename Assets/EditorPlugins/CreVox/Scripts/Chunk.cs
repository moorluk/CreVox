using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CreVox
{

	[System.Serializable]
	public class ChunkData
	{
		public WorldPos ChunkPos;
		public List<Block> blocks = new List<Block> ();
		public List<BlockAir> blockAirs = new List<BlockAir> ();
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
			coll.hideFlags = HideFlags.HideInHierarchy;
			UpdateChunk ();
		}

		public void Destroy ()
		{
			foreach (Block block in cData.blocks)
				if (block != null)
					block.Destroy ();
			foreach (BlockAir bAir in cData.blockAirs)
				if (bAir != null)
					bAir.Destroy ();
		}

		void Start ()
		{
			filter = gameObject.GetComponent<MeshFilter> ();
			coll = gameObject.GetComponent<MeshCollider> ();
		}

		public Block GetBlock (int x, int y, int z)
		{
			if (InRange (x) && InRange (y) && InRange (z)) {
				return GetChunkBlock (x, y, z);
			} else {
				return volume.GetBlock (cData.ChunkPos.x + x, cData.ChunkPos.y + y, cData.ChunkPos.z + z);
			}
		}

		private Block GetChunkBlock (int x, int y, int z)
		{
			foreach (Block block in cData.blocks) {
				if (block != null && block.BlockPos.Compare (new WorldPos (x, y, z)))
					return block;
			}
			foreach (BlockAir bAir in cData.blockAirs) {
				if (bAir != null && bAir.BlockPos.Compare (new WorldPos (x, y, z)))
					return bAir;
			}
			return null;
		}

		public void SetBlock (int x, int y, int z, Block block)
		{
			if (InRange (x) && InRange (y) && InRange (z)) {
				Block _block = GetChunkBlock (x, y, z); 
				if (_block != null) {
					BlockAir bAir = _block as BlockAir;
					if (bAir != null)
						cData.blockAirs.Remove (bAir);
					else
						cData.blocks.Remove (_block);
				}
				if (block != null) {
					BlockAir bAir = block as BlockAir;
					if (bAir != null)
						cData.blockAirs.Add (bAir);
					else
						cData.blocks.Add (block);
				}
					
			} else {
				volume.SetBlock (cData.ChunkPos.x + x, cData.ChunkPos.y + y, cData.ChunkPos.z + z, block);
			}
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
			for (int x = 0; x < chunkSize; x++) {
				for (int y = 0; y < chunkSize; y++) {
					for (int z = 0; z < chunkSize; z++) {
						#if UNITY_EDITOR
						if ((!EditorApplication.isPlaying && volume.cuter && y + cData.ChunkPos.y > volume.cutY) == false)
						#endif
						if (GetChunkBlock (x, y, z) != null)
							meshData = GetChunkBlock (x, y, z).MeahAddMe (this, x, y, z, meshData);
					}
				}
			}
			AssignRenderMesh (meshData);
		}

		public void UpdateMeshCollider ()
		{
			MeshData meshData = new MeshData ();
			for (int x = 0; x < chunkSize; x++) {
				for (int y = 0; y < chunkSize; y++) {
					for (int z = 0; z < chunkSize; z++) {
						#if UNITY_EDITOR
						if ((!EditorApplication.isPlaying && volume.cuter && y + cData.ChunkPos.y > volume.cutY) == false)
						#endif
						if (GetChunkBlock (x, y, z) != null)
							meshData = GetChunkBlock (x, y, z).MeahAddMe (this, x, y, z, meshData);
					}
				}
			}
			AssignCollisionMesh (meshData);
		}

		public void UpdateChunk ()
		{
			MeshData meshData = new MeshData ();
			foreach (Block block in cData.blocks) {
				#if UNITY_EDITOR
				if (!EditorApplication.isPlaying && volume.cuter && block.BlockPos.y + cData.ChunkPos.y > volume.cutY)
				#endif
					block.MeahAddMe (this, block.BlockPos.x, block.BlockPos.y, block.BlockPos.z, meshData);
			}

			foreach (BlockAir bAir in cData.blockAirs) {
				WorldPos vPos = new WorldPos (
					                cData.ChunkPos.x + bAir.BlockPos.x, 
					                cData.ChunkPos.y + bAir.BlockPos.y, 
					                cData.ChunkPos.z + bAir.BlockPos.z);
				
				if (volume.GetNode (vPos) != null) {
					#if UNITY_EDITOR
					if (!EditorApplication.isPlaying && volume.cuter && bAir.BlockPos.y + cData.ChunkPos.y > volume.cutY)
						volume.GetNode (vPos).SetActive (false);
					else 
					#endif
						volume.GetNode (vPos).SetActive (true);
				}
			}

			UpdateMeshFilter ();
			UpdateMeshCollider ();
		}

		void AssignRenderMesh (MeshData meshData)
		{
#if UNITY_EDITOR
			filter.sharedMesh = null;
			Mesh mesh = new Mesh ();
			mesh.vertices = meshData.colVertices.ToArray ();
			mesh.triangles = meshData.colTriangles.ToArray ();
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