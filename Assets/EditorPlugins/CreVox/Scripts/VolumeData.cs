using UnityEngine;
using System.Collections.Generic;

namespace CreVox
{
    [CreateAssetMenu (menuName = "CreVox/Volume Data"),System.Serializable]
    public class VolumeData : ScriptableObject
    {
        // free chunk
        public bool useFreeChunk;
        public ChunkData freeChunk = new ChunkData { isFreeChunk = true };
        // ==============
        public int chunkSize;
        public int chunkX = 1;
        public int chunkY = 1;
        public int chunkZ = 1;
        public List <ChunkData> chunkDatas = new List<ChunkData> ();
        public List<BlockItem> blockItems = new List<BlockItem> ();
        public string subArtPack = "";

        // [XAOCX add]
        public VolumeData ()
        {
        }

        public VolumeData (VolumeData clone)
        {
            useFreeChunk = clone.useFreeChunk;
            freeChunk = clone.freeChunk;
            chunkSize = clone.chunkSize;
            chunkX = clone.chunkX;
            chunkY = clone.chunkY;
            chunkZ = clone.chunkZ;
            chunkDatas = new List<ChunkData> ();
            foreach (var chunkData in clone.chunkDatas) {
                chunkDatas.Add (new ChunkData (chunkData));
            }
            blockItems = new List<BlockItem> ();
            foreach (var blockItem in clone.blockItems) {
                blockItems.Add (new BlockItem (blockItem));
            }
            subArtPack = clone.subArtPack;
        }
        // ==============

        public void Awake ()
        {
            if (chunkDatas.Count < 1)
                chunkDatas.Add (new ChunkData ());
        }

        public List<ChunkData> GetChunkDatas ()
        {
            if (useFreeChunk) {
                List<ChunkData> freeChunkDatas = new List<ChunkData> ();
                freeChunkDatas.Add (freeChunk);
                return freeChunkDatas;
            }
            return chunkDatas;
        }

        ChunkData GetChunkData (WorldPos _pos)
        {
            if (useFreeChunk) {
                if (freeChunk == null)
                    freeChunk = new ChunkData { isFreeChunk = true, freeChunkSize = new WorldPos (0, 0, 0) };
                return freeChunk;
            } else {
                WorldPos _chunkPos = new WorldPos (
                                         Mathf.FloorToInt (_pos.x / chunkSize) * chunkSize,
                                         Mathf.FloorToInt (_pos.y / chunkSize) * chunkSize,
                                         Mathf.FloorToInt (_pos.z / chunkSize) * chunkSize
                                     );
                return chunkDatas.Find (p => p.ChunkPos.Compare (_chunkPos));
            }
        }

        public void ConvertToFreeChunk ()
        {
            useFreeChunk = true;
            int x = 0, y = 0, z = 0;

            freeChunk = new ChunkData ();
            foreach (ChunkData cd in chunkDatas) {
                foreach (Block b in cd.blocks) {
                    Block newb = b;
                    newb.BlockPos.x += cd.ChunkPos.x;
                    newb.BlockPos.y += cd.ChunkPos.y;
                    newb.BlockPos.z += cd.ChunkPos.z;
                    freeChunk.blocks.Add (newb);
                    x = Mathf.Max (x, newb.BlockPos.x);
                    y = Mathf.Max (y, newb.BlockPos.y);
                    z = Mathf.Max (z, newb.BlockPos.z);
                }
                foreach (BlockAir b in cd.blockAirs) {
                    BlockAir newb = b;
                    newb.BlockPos.x += cd.ChunkPos.x;
                    newb.BlockPos.y += cd.ChunkPos.y;
                    newb.BlockPos.z += cd.ChunkPos.z;
                    freeChunk.blockAirs.Add (newb);
                    x = Mathf.Max (x, newb.BlockPos.x);
                    y = Mathf.Max (y, newb.BlockPos.y);
                    z = Mathf.Max (z, newb.BlockPos.z);
                }
            }
            chunkDatas.Clear ();
            chunkX = 0;
            chunkY = 0;
            chunkZ = 0;
            freeChunk.isFreeChunk = true;
            freeChunk.freeChunkSize.x = x + 1;
            freeChunk.freeChunkSize.y = y + 1;
            freeChunk.freeChunkSize.z = z + 1;
        }

        public void ConvertToVoxelChunk ()
        {
            useFreeChunk = false;
            int size = chunkSize;
            WorldPos fSize = freeChunk.freeChunkSize;
            chunkX = fSize.x / size + (((fSize.x % size) > 0) ? 1 : 0);
            chunkY = fSize.y / size + (((fSize.y % size) > 0) ? 1 : 0);
            chunkZ = fSize.z / size + (((fSize.z % size) > 0) ? 1 : 0);

            chunkDatas.Clear ();
            for (int x = 0; x < chunkX; x++) {
                for (int y = 0; y < chunkY; y++) {
                    for (int z = 0; z < chunkZ; z++) {
                        ChunkData newChunkData = new ChunkData ();
                        newChunkData.isFreeChunk = false;
                        newChunkData.ChunkPos = new WorldPos (x * chunkSize, y * chunkSize, z * chunkSize);
                        chunkDatas.Add (newChunkData);
                    }
                }
            }
            foreach (Block b in freeChunk.blocks) {
                ChunkData c = GetChunkData (b.BlockPos);
                b.BlockPos = new WorldPos (b.BlockPos.x - c.ChunkPos.x, b.BlockPos.y - c.ChunkPos.y, b.BlockPos.z - c.ChunkPos.z);
                c.blocks.Add (b);
            }
            foreach (BlockAir b in freeChunk.blockAirs) {
                ChunkData c = GetChunkData (b.BlockPos);
                b.BlockPos = new WorldPos (b.BlockPos.x - c.ChunkPos.x, b.BlockPos.y - c.ChunkPos.y, b.BlockPos.z - c.ChunkPos.z);
                c.blockAirs.Add (b);
            }
            freeChunk = new ChunkData { isFreeChunk = true, freeChunkSize = new WorldPos (0, 0, 0) };
        }
    }
}