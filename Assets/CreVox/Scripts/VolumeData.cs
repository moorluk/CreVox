using UnityEngine;
using System.Collections.Generic;

namespace CreVox
{
	[CreateAssetMenu (menuName = "CreVox/Volume Data")]
	public class VolumeData : ScriptableObject
	{
	
		[System.Serializable]
		public class ChunkData
		{
			public WorldPos ChunkPos;
			public List<Block> blocks = new List<Block> ();
			public List<BlockAir> blockAirs = new List<BlockAir> ();
		}

		public int chunkX = 1;
		public int chunkY = 1;
		public int chunkZ = 1;
		public List <ChunkData> chunkDatas = new List<ChunkData> ();

		public void AddChunk (Chunk _chunk)
		{
			foreach (ChunkData cd in chunkDatas) {
				if (cd.ChunkPos.Compare (_chunk.pos))
					return;
			}
			ChunkData newCD = new ChunkData ();
			newCD.ChunkPos = _chunk.pos;
			chunkDatas.Add (newCD);
		}

		public ChunkData GetChunk (WorldPos _pos)
		{
			foreach (ChunkData cd in chunkDatas) {
				if (cd.ChunkPos.Compare (_pos))
					return cd;
			}
			return null;
		}

		public Dictionary<WorldPos,Block> GetBlockDictionary (ChunkData _cd)
		{
			var blocksDictionary = new Dictionary<WorldPos,Block> ();
			foreach (Block b in _cd.blocks) {
				blocksDictionary.Add (b.BlockPos, b);
			}
			return blocksDictionary;
		}

		public Block GetBlock (WorldPos _blockPos, WorldPos _chunkPos)
		{
			if (GetChunk (_chunkPos) != null) {
				foreach (Block b in GetChunk(_chunkPos).blocks) {
					if (b.BlockPos.Compare (_blockPos))
						return b;
				}
			}
			return null;
		}

		public static VolumeData GetVData (string workFile)
		{
			VolumeData _vData = Resources.Load (workFile + "_vData", typeof(VolumeData)) as VolumeData;
			#if UNITY_EDITOR
			if (_vData == null) {
				string bytesPath = PathCollect.resourcesPath + workFile + "_vData" + ".asset";
				Debug.Log (bytesPath + " -> " + workFile + "_vData");
				VolumeData vd = ScriptableObject.CreateInstance<VolumeData> ();
				UnityEditor.AssetDatabase.CreateAsset (vd, bytesPath);
				_vData = Resources.Load (workFile + "_vData", typeof(VolumeData)) as VolumeData;
			}
			#endif
			return _vData;
		}
	}
}