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
		public List<BlockItem> blockItems = new List<BlockItem> ();
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
		private Dictionary<WorldPos,Block> BlockDict;

		public Volume volume;
		[SerializeField] MeshFilter filter;
		[SerializeField] MeshCollider coll;

		public void Init ()
		{
			BlockDict = new Dictionary<WorldPos, Block> ();
			filter = gameObject.GetComponent<MeshFilter> ();
			coll = gameObject.GetComponent<MeshCollider> ();
//			coll.hideFlags = HideFlags.HideInHierarchy;
			UpdateChunk ();
		}

		public void Destroy ()
		{
			cData = new ChunkData ();
			foreach (Block block in cData.blocks)
				if (block != null)
					block.Destroy ();
			foreach (BlockAir bAir in cData.blockAirs)
				if (bAir != null)
					bAir.Destroy ();
			foreach (BlockItem bItem in cData.blockItems)
				if (bItem != null)
					bItem.Destroy ();
			foreach (BlockHold bHold in cData.blockHolds)
				if (bHold != null)
					bHold.Destroy ();
		}

		void Start ()
		{
			filter = gameObject.GetComponent<MeshFilter> ();
			coll = gameObject.GetComponent<MeshCollider> ();
		}

		public void UpdateChunk ()
		{
			MeshData meshData = new MeshData ();
			foreach (Block block in cData.blocks) {
				#if UNITY_EDITOR
				if (!EditorApplication.isPlaying && volume.cuter && block.BlockPos.y + cData.ChunkPos.y > volume.cutY)
				#endif
					block.MeahAddMe (this, block.BlockPos.x, block.BlockPos.y, block.BlockPos.z, meshData);
				if (!BlockDict.ContainsKey (block.BlockPos))
					BlockDict.Add (block.BlockPos, block);
			}

			foreach (BlockAir bAir in cData.blockAirs) {
				if (!BlockDict.ContainsKey (bAir.BlockPos))
					BlockDict.Add (bAir.BlockPos, bAir);
				WorldPos volumePos = new WorldPos (
					cData.ChunkPos.x + bAir.BlockPos.x, 
					cData.ChunkPos.y + bAir.BlockPos.y, 
					cData.ChunkPos.z + bAir.BlockPos.z);

				if (volume.GetNode (volumePos) != null) {
					#if UNITY_EDITOR
					if (!EditorApplication.isPlaying && volume.cuter && bAir.BlockPos.y + cData.ChunkPos.y > volume.cutY)
						volume.GetNode (volumePos).SetActive (false);
					else 
					#endif
						volume.GetNode (volumePos).SetActive (true);
				}
			}

			UpdateMeshFilter ();
			UpdateMeshCollider ();
		}

		public Block GetBlock (int x, int y, int z)
		{
			if (InRange (x) && InRange (y) && InRange (z)) {
				return GetChunkBlock (x, y, z);
			} else {
				if (!VGlobal.GetSetting().FakeDeco)
					return volume.GetBlock (cData.ChunkPos.x + x, cData.ChunkPos.y + y, cData.ChunkPos.z + z);
			}
			return null;
		}

		private Block GetChunkBlock (int x, int y, int z)
		{
			foreach (BlockAir bAir in cData.blockAirs) {
				if (bAir != null && bAir.BlockPos.Compare (new WorldPos (x, y, z)))
					return bAir;
			}
			foreach (Block block in cData.blocks) {
				if (block != null && block.BlockPos.Compare (new WorldPos (x, y, z)))
					return block;
			}
			foreach (BlockHold bHold in cData.blockHolds) {
				if (bHold != null && bHold.BlockPos.Compare (new WorldPos (x, y, z)))
					return bHold;
			}
			return null;
		}

		public void SetBlock (int x, int y, int z, Block block)
		{
			if (InRange (x) && InRange (y) && InRange (z)) {
				Block _b = GetChunkBlock (x, y, z); 
				if (_b != null) {
					if (_b is BlockHold){
						BlockHold _bh = (BlockHold)_b;
						if (_bh.roots.Count < 1)
							cData.blockHolds.Remove (_bh);
					}else if (_b is BlockAir)
						cData.blockAirs.Remove ((BlockAir)_b);
					else
						cData.blocks.Remove (_b);
				}
				if (block != null) {
					if (block is BlockAir)
						cData.blockAirs.Add ((BlockAir)block);
					else if (block is BlockHold)
						cData.blockHolds.Add ((BlockHold)block);
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
							meshData = GetChunkBlock (x, y, z).ColliderAddMe (this, x, y, z, meshData);
					}
				}
			}
			AssignCollisionMesh (meshData);
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