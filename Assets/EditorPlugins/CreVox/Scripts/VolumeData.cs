﻿using UnityEngine;
using System.Collections.Generic;

namespace CreVox
{
	[CreateAssetMenu (menuName = "CreVox/Volume Data")]
	public class VolumeData : ScriptableObject
	{

		public int chunkX = 1;
		public int chunkY = 1;
		public int chunkZ = 1;
		public List <ChunkData> chunkDatas = new List<ChunkData> ();
		public List<BlockItem> blockItems = new List<BlockItem> ();
		public string subArtPack = "";

		// [XAOCX add]
		public VolumeData() : base() { }
		public VolumeData(VolumeData clone) : base() {
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
		}

		public void Awake()
		{
			if (chunkDatas.Count < 1)
				chunkDatas.Add (new ChunkData ());
		}

		public ChunkData GetChunk (WorldPos _pos)
		{
			foreach (ChunkData cd in chunkDatas) {
				if (cd.ChunkPos.Compare (_pos))
					return cd;
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
	}
}