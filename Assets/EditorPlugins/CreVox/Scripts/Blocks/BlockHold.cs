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

			//[XAOCX add]
			public piecePos() { }
			public piecePos(piecePos clone) {
				this.blockPos = clone.blockPos;
				this.pieceID = clone.pieceID;
			}

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

		// [XAOCX add]
		public BlockHold () : base () { }
		public BlockHold(BlockHold clone) : base(clone) {
			this.roots = new List<piecePos>();
			foreach (var item in clone.roots) {
				this.roots.Add(new piecePos(item));
			}
			this.isSolid = clone.isSolid;
		}


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