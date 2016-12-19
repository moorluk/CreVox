using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace CreVox
{
	[Serializable]
	public class BlockHold : Block
	{
		[Serializable]
		public class piecePos{
			public WorldPos blockPos;
			public int pieceID;

			public bool Compare (piecePos obj)
			{
				if (this.blockPos.Compare (obj.blockPos) && this.pieceID == obj.pieceID)
					return true;
				else
					return false;
			}
		}

		[SerializeField]
		private bool isSolid = false;
		public List<piecePos> roots = new List<piecePos>();

//		public BlockHold () : base ()
//		{
//		}

		public override bool IsSolid (Direction direction)
		{
			return isSolid;
		}

		public override MeshData MeahAddMe (Chunk chunk, int x, int y, int z, MeshData meshData)
		{
			return meshData;
		}

		public override MeshData ColliderAddMe (Chunk chunk, int x, int y, int z, MeshData meshData)
		{
			return meshData;
		}

		public void SetSolid(bool solid)
		{
			isSolid = solid;
		}
	}
}