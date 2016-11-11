using UnityEngine;
using System.Collections.Generic;

//using System.Collections;

namespace CreVox
{
	public class VolumeData : ScriptableObject
	{
	
		[System.Serializable]
		public class Chunk
		{
			public Vector3 ChunkPos;

			[System.Serializable]
			public class Block
			{
				public Vector3 BlockPos;
				public bool isAir;
				public string[] pieces;
			}

			public Block[] blocks;
		}

		public List<Chunk> chunks;

		public void OnInspectorGUI ()
		{

		}
	}
}