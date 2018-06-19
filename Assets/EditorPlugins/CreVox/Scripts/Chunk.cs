using UnityEngine;
using System.Collections.Generic;
using System;

namespace CreVox
{
    [Serializable]
    public class ChunkData
    {
        // free chunk
        public bool isFreeChunk;
        public WorldPos freeChunkSize = new WorldPos (9, 9, 9);
        // ==============
        public WorldPos ChunkPos;
        public List<Block> blocks = new List<Block> ();
        public List<BlockAir> blockAirs = new List<BlockAir> ();
        public List<BlockHold> blockHolds = new List<BlockHold> ();
        // [XAOCX add]
        public ChunkData ()
        {
        }

        public ChunkData (ChunkData clone)
        {
            // free chunk
            isFreeChunk = clone.isFreeChunk;
            freeChunkSize = clone.freeChunkSize;
            // ==============
            ChunkPos = clone.ChunkPos;
            blocks = new List<Block> ();
            foreach (var block in clone.blocks) {
                blocks.Add (new Block (block));
            }
            blockAirs = new List<BlockAir> ();
            foreach (var blockAir in clone.blockAirs) {
                blockAirs.Add (new BlockAir (blockAir));
            }
            blockHolds = new List<BlockHold> ();
            foreach (var blockHold in clone.blockHolds) {
                blockHolds.Add (new BlockHold (blockHold));
            }
        }
        // ==============
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
                    i--;
                } else { 
                    if (volume) {
                        WorldPos volumePos = new WorldPos (
                                             cData.ChunkPos.x + bAir.BlockPos.x, 
                                             cData.ChunkPos.y + bAir.BlockPos.y, 
                                             cData.ChunkPos.z + bAir.BlockPos.z);
                        GameObject node = volume.GetNode (volumePos);
                        if (node != null) {
                            #if UNITY_EDITOR
                            bool isShow = !(!UnityEditor.EditorApplication.isPlaying && volume.cuter && bAir.BlockPos.y + cData.ChunkPos.y > volume.cutY);
                            node.SetActive (isShow);
                            #else
                            node.SetActive (true);
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
            for (int i = 0; i < cData.blockAirs.Count; i++) {
                if (cData.blockAirs [i].BlockPos.Compare (pos))
                    return cData.blockAirs [i];
            }
            return null;
        }



        public static bool InRange (int index)
        {
            return index >= 0 && index < chunkSize;

        }

        public void UpdateMeshFilter ()
        {
            MeshData meshData = new MeshData ();
            foreach (var b in cData.blocks) {
                #if UNITY_EDITOR
                bool isCut = (volume != null) && (volume.cuter && b.BlockPos.y + cData.ChunkPos.y > volume.cutY);
                if (isCut)
                    continue;
                #endif
                meshData = b.MeahAddMe (this, b.BlockPos.x, b.BlockPos.y, b.BlockPos.z, meshData);
            }
            AssignRenderMesh (meshData);
        }

        public void UpdateMeshCollider ()
        {
            MeshData meshData = new MeshData ();
            foreach (var b in cData.blocks) {
                #if UNITY_EDITOR
                bool isCut = (volume != null) && (volume.cuter && b.BlockPos.y + cData.ChunkPos.y > volume.cutY);
                if (isCut)
                    continue;
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