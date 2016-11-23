using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CreVox
{

	[RequireComponent (typeof(MeshFilter))]
	[RequireComponent (typeof(MeshRenderer))]
	[RequireComponent (typeof(MeshCollider))]
	[Serializable]
	public class Chunk : MonoBehaviour
	{
		public static int chunkSize{get{return VGlobal.GetSetting().chunkSize; }} 
		public List<Block> blocks = new List<Block>();
		public List<BlockAir> blockAirs = new List<BlockAir>();
		[SerializeField]
		MeshFilter filter;
		[SerializeField]
		MeshCollider coll;

		public Volume volume;
		public WorldPos pos;

		public void Init()
		{
			filter = gameObject.GetComponent<MeshFilter>();
			coll = gameObject.GetComponent<MeshCollider>();
			coll.hideFlags = HideFlags.HideInHierarchy;
			UpdateChunk();
		}

		public void Destroy()
		{
			foreach (Block block in blocks)
				if (block != null)
					block.Destroy ();
			foreach (BlockAir bAir in blockAirs)
				if (bAir != null)
					bAir.Destroy ();
		}

		void Start()
		{
			filter = gameObject.GetComponent<MeshFilter>();
			coll = gameObject.GetComponent<MeshCollider>();
			UpdateChunk();
		}

		public Block GetBlock(int x, int y, int z)
		{
			if (InRange (x) && InRange (y) && InRange (z)) {
				return GetChunkBlock (x, y, z);
			} else {
				return volume.GetBlock (pos.x + x, pos.y + y, pos.z + z);
			}
		}

		private Block GetChunkBlock (int x, int y, int z)
		{
			foreach (Block block in blocks) {
				if (block != null && block.BlockPos.Compare (new WorldPos (x, y, z)))
					return block;
			}
			foreach (BlockAir bAir in blockAirs) {
				if (bAir != null && bAir.BlockPos.Compare (new WorldPos (x, y, z)))
					return bAir;
			}
			return null;
		}

		public void SetBlock(int x, int y, int z, Block block)
		{
			if (InRange(x) && InRange(y) && InRange(z)) {
				Block _block = GetChunkBlock (x, y, z); 
				if (_block != null) {
					BlockAir bAir = _block as BlockAir;
					if (bAir != null)
						blockAirs.Remove (bAir);
					else
						blocks.Remove (_block);
				}
				if (block != null) {
					BlockAir bAir = block as BlockAir;
					if (bAir != null)
						blockAirs.Add (bAir);
					else
						blocks.Add (block);
				}
					
			} else {
				volume.SetBlock(pos.x + x, pos.y + y, pos.z + z, block);
			}
		}

		public static bool InRange(int index)
		{
			if (index < 0 || index >= chunkSize)
				return false;

			return true;
		}

		public void UpdateMeshFilter()
		{
			MeshData meshData = new MeshData();
			for (int x = 0; x < chunkSize; x++) {
				for (int y = 0; y < chunkSize; y++) {
					for (int z = 0; z < chunkSize; z++) {
						#if UNITY_EDITOR
						if ((!EditorApplication.isPlaying && volume.cuter && y + pos.y > volume.cutY) == false)
						#endif
						if(GetChunkBlock (x, y, z) != null)
							meshData = GetChunkBlock (x, y, z).MeahAddMe (this, x, y, z, meshData);
					}
				}
			}
			AssignRenderMesh(meshData);
		}

		public void UpdateMeshCollider()
		{
			MeshData meshData = new MeshData();
			for (int x = 0; x < chunkSize; x++) {
				for (int y = 0; y < chunkSize; y++) {
					for (int z = 0; z < chunkSize; z++) {
						#if UNITY_EDITOR
						if ((!EditorApplication.isPlaying && volume.cuter && y + pos.y > volume.cutY) == false)
						#endif
						if(GetChunkBlock (x, y, z) != null)
							meshData = GetChunkBlock (x, y, z).MeahAddMe (this, x, y, z, meshData);
					}
				}
			}
			AssignCollisionMesh(meshData);
		}

		public void UpdateChunk()
		{
			MeshData meshData = new MeshData ();
			for (int x = 0; x < chunkSize; x++) {
				for (int y = 0; y < chunkSize; y++) {
					for (int z = 0; z < chunkSize; z++) {
						BlockAir air = GetChunkBlock (x, y, z) as BlockAir;

						#if UNITY_EDITOR
						if (!EditorApplication.isPlaying && volume.cuter && y + pos.y > volume.cutY) {
							if (air != null)
								air.ShowPiece (false);
						} else {
							if(GetChunkBlock (x, y, z) != null)
								meshData = GetChunkBlock (x, y, z).MeahAddMe (this, x, y, z, meshData);
							if (air != null)
								air.ShowPiece (true);
						}
						#else
						if(GetChunkBlock (x, y, z) != null)
							meshData = GetChunkBlock (x, y, z).BlockMesh (this, x, y, z, meshData);
						if (air != null)
							air.ShowPiece (true);
						#endif
					}
				}
			}
			UpdateMeshFilter ();
			UpdateMeshCollider ();
		}

		void AssignRenderMesh(MeshData meshData)
		{
#if UNITY_EDITOR
			filter.sharedMesh = null;
			Mesh mesh = new Mesh();
			mesh.vertices = meshData.colVertices.ToArray();
			mesh.triangles = meshData.colTriangles.ToArray();
			mesh.uv = meshData.uv.ToArray();
			mesh.RecalculateNormals();
			filter.sharedMesh = mesh;
#else
        filter.mesh.Clear();
        filter.mesh.vertices = meshData.vertices.ToArray();
        filter.mesh.triangles = meshData.triangles.ToArray();
        filter.mesh.uv = meshData.uv.ToArray();
        filter.mesh.RecalculateNormals();
#endif
		}

		void AssignCollisionMesh(MeshData meshData)
		{
			coll.sharedMesh = null;
			Mesh cmesh = new Mesh();
			cmesh.vertices = meshData.colVertices.ToArray();
			cmesh.triangles = meshData.colTriangles.ToArray();
			cmesh.RecalculateNormals();
			coll.sharedMesh = cmesh;
		}
	}
}