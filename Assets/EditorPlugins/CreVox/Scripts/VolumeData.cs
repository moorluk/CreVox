using UnityEngine;
using System.Collections.Generic;

namespace CreVox
{
	[CreateAssetMenu (menuName = "CreVox/Volume Data")]
	public class VolumeData : ScriptableObject
	{
		// free chunk
		public bool useFreeChunk = false;
		public ChunkData freeChunk = new ChunkData (){isFreeChunk = true};
		// ==============
        public int chunkSize = 0;
		public int chunkX = 1;
		public int chunkY = 1;
		public int chunkZ = 1;
		public List <ChunkData> chunkDatas = new List<ChunkData> ();
		public List<BlockItem> blockItems = new List<BlockItem> ();
		public string subArtPack = "";

		// [XAOCX add]
		public VolumeData() : base() { }
        public VolumeData(VolumeData clone) : base() {
            this.useFreeChunk = clone.useFreeChunk;
            this.freeChunk = clone.freeChunk;
            this.chunkSize = clone.chunkSize;
			this.chunkX = clone.chunkX;
			this.chunkY = clone.chunkY;
			this.chunkZ = clone.chunkZ;
			this.chunkDatas = new List<ChunkData>();
			foreach (var chunkData in clone.chunkDatas) {
				this.chunkDatas.Add(new ChunkData(chunkData));
			}
			this.blockItems = new List<BlockItem>();
			foreach (var blockItem in clone.blockItems) {
				this.blockItems.Add(new BlockItem(blockItem));
			}
            this.subArtPack = clone.subArtPack;
		}
		// ==============

		public void Awake()
		{
			if (chunkDatas.Count < 1)
				chunkDatas.Add (new ChunkData ());
		}

		public ChunkData GetChunkData (WorldPos _pos)
        {
            if (useFreeChunk) {
                if (freeChunk == null)
                    freeChunk = new ChunkData ();
                return freeChunk;
            } else {
                WorldPos _chunkPos = new WorldPos (
                    Mathf.FloorToInt (_pos.x / chunkSize) * chunkSize,
                    Mathf.FloorToInt (_pos.y / chunkSize) * chunkSize,
                    Mathf.FloorToInt (_pos.z / chunkSize) * chunkSize
                                    );
                foreach (ChunkData cd in chunkDatas) {
                    if (cd.ChunkPos.Compare (_chunkPos))
                        return cd;
                }
            }
            return null;
        }

		public static VolumeData GetVData (string workFile)
		{
			VolumeData _vData = ScriptableObject.CreateInstance<VolumeData> ();
			#if UNITY_EDITOR
			if (_vData == null) {
				string bytesPath = PathCollect.resourcesPath + workFile;
				VolumeData vd = ScriptableObject.CreateInstance<VolumeData> ();
				UnityEditor.AssetDatabase.CreateAsset (vd, bytesPath);
				UnityEditor.AssetDatabase.Refresh();
			}
			#endif
			_vData = Resources.Load (workFile.Replace(".asset",""), typeof(VolumeData)) as VolumeData;
			return _vData;
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
                foreach (BlockHold b in cd.blockHolds) {
                    BlockHold newb = b;
                    newb.BlockPos.x += cd.ChunkPos.x;
                    newb.BlockPos.y += cd.ChunkPos.y;
                    newb.BlockPos.z += cd.ChunkPos.z;
                    x = Mathf.Max (x, newb.BlockPos.x);
                    y = Mathf.Max (y, newb.BlockPos.y);
                    z = Mathf.Max (z, newb.BlockPos.z);
                }
            }
            chunkDatas.Clear ();
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
                b.BlockPos = new WorldPos(b.BlockPos.x - c.ChunkPos.x, b.BlockPos.y - c.ChunkPos.y, b.BlockPos.z - c.ChunkPos.z);
                c.blocks.Add (b);
            }
            foreach (BlockAir b in freeChunk.blockAirs) {
                ChunkData c = GetChunkData (b.BlockPos);
                b.BlockPos = new WorldPos(b.BlockPos.x - c.ChunkPos.x, b.BlockPos.y - c.ChunkPos.y, b.BlockPos.z - c.ChunkPos.z);
                c.blockAirs.Add (b);
            }
            freeChunk = new ChunkData () {
                isFreeChunk = true,
                freeChunkSize = new WorldPos (0, 0, 0)
            };
        }
    }
}