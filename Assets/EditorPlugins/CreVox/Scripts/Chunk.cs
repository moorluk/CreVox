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
		public List<BlockHold> blockHolds = new List<BlockHold> ();
		public List<BlockItem> blockItems = new List<BlockItem> ();
	}

	[RequireComponent (typeof(MeshFilter))]
	[RequireComponent (typeof(MeshRenderer))]
	[RequireComponent (typeof(MeshCollider))]
	[Serializable]
	public class Chunk : MonoBehaviour
	{
		public static int chunkSize{ get { return VGlobal.GetSetting ().chunkSize; } }

		public ChunkData cData;
		private Dictionary<WorldPos,Block> BlockDict = new Dictionary<WorldPos, Block> ();

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
				if (!BlockDict.ContainsKey (block.BlockPos))
					setBlockDict (block.BlockPos, block);
				#if UNITY_EDITOR
				if (volume.cuter && block.BlockPos.y + cData.ChunkPos.y > volume.cutY)
				#endif
					block.MeahAddMe (this, block.BlockPos.x, block.BlockPos.y, block.BlockPos.z, meshData);
			}

			foreach (BlockHold bHold in cData.blockHolds) {
				if (!BlockDict.ContainsKey (bHold.BlockPos))
					setBlockDict (bHold.BlockPos, bHold);
			}

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
					if (BlockDict.ContainsKey (bAir.BlockPos))
						BlockDict.Remove (bAir.BlockPos);
				} else { 
					if (!BlockDict.ContainsKey (bAir.BlockPos))
						setBlockDict (bAir.BlockPos, bAir);
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
			}

			UpdateMeshFilter ();
			UpdateMeshCollider ();
		}

		public Block GetBlock (int x, int y, int z)
		{
			if (InRange (x) && InRange (y) && InRange (z)) {
				return GetChunkBlock (x, y, z);
			} else {
				if (!VGlobal.GetSetting ().FakeDeco)
					return volume.GetBlock (cData.ChunkPos.x + x, cData.ChunkPos.y + y, cData.ChunkPos.z + z);
			}
			return null;
		}

		public void setBlockDict (WorldPos blockPos, Block block)
		{
			if(BlockDict.ContainsKey(blockPos))
				BlockDict.Remove(blockPos);
			if (block != null)
				BlockDict.Add (blockPos, block);
		}

		private Block GetChunkBlock (int x, int y, int z)
		{
			WorldPos blockPos = new WorldPos (x, y, z);
			if (BlockDict.ContainsKey (blockPos))
				return BlockDict [blockPos];
			else
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
				meshData = b.MeahAddMe (this, b.BlockPos.x, b.BlockPos.y, b.BlockPos.z, meshData);
			}
			AssignRenderMesh (meshData);
		}

		public void UpdateMeshCollider ()
		{
			MeshData meshData = new MeshData ();
			for (int i = 0; i < cData.blocks.Count; i++) {
				Block b = cData.blocks [i];
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