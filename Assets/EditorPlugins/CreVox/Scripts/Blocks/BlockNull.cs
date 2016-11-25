using UnityEngine;
using System;

namespace CreVox
{
	[Serializable]
	public class BlockNull : Block
	{
		public BlockNull () : base ()
		{
		}

		public override MeshData MeahAddMe (Chunk chunk, int x, int y, int z, MeshData meshData)
		{
			return meshData;
		}

		public override bool IsSolid (Direction direction)
		{
			return false;
		}

	}
}