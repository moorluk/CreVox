//Chunk.cs
using UnityEngine;
using System.Collections;
using UnityEditor;

namespace CreVox
{

	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	[RequireComponent(typeof(MeshCollider))]
	public class Chunk : MonoBehaviour
	{
		public Block[,,] blocks = new Block[chunkSize, chunkSize, chunkSize];
		public static int chunkSize = 16;
		public bool update = true;
		MeshFilter filter;
		MeshCollider coll;

		public World world;
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
			foreach (var block in blocks)
				block.Destroy();
		}

		void Start()
		{
			filter = gameObject.GetComponent<MeshFilter>();
			coll = gameObject.GetComponent<MeshCollider>();
			UpdateChunk();
		}

		public Block GetBlock(int x, int y, int z)
		{
			if (InRange(x) && InRange(y) && InRange(z))
				return blocks[x, y, z];
			return world.GetBlock(pos.x + x, pos.y + y, pos.z + z);
		}

		public void SetBlock(int x, int y, int z, Block block)
		{
			if (InRange(x) && InRange(y) && InRange(z)) {
				if (blocks[x, y, z] != null)
					blocks[x, y, z].Destroy();
				blocks[x, y, z] = block;
			} else {
				world.SetBlock(pos.x + x, pos.y + y, pos.z + z, block);
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
						if (!EditorApplication.isPlaying && world.pointer && y > world.editY) {
						} else {
							meshData = blocks[x, y, z].Blockdata(this, x, y, z, meshData);
						}
					}
				}
			}
			AssignRenderMesh(meshData);
		}

		public void UodateMeshCollider()
		{
			MeshData meshData = new MeshData();
			for (int x = 0; x < chunkSize; x++) {
				for (int y = 0; y < chunkSize; y++) {
					for (int z = 0; z < chunkSize; z++) {
						meshData = blocks[x, y, z].Blockdata(this, x, y, z, meshData);
					}
				}
			}
			AssignCollisionMesh(meshData);
		}

		public void UpdateChunk()
		{
			MeshData meshData = new MeshData();
			for (int x = 0; x < chunkSize; x++) {
				for (int y = 0; y < chunkSize; y++) {
					for (int z = 0; z < chunkSize; z++) {
						BlockAir air = blocks [x, y, z] as BlockAir;

						if (!EditorApplication.isPlaying && world.pointer && y > world.editY) {
							if (air != null)
								air.ShowPiece(false);
						} else {
							meshData = blocks[x, y, z].Blockdata(this, x, y, z, meshData);
							if (air != null)
								air.ShowPiece(true);
						}
					}
				}
			}
			AssignRenderMesh(meshData);
			AssignCollisionMesh(meshData);
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

        //Add the following two lines
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