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
				blockPos = clone.blockPos;
				pieceID = clone.pieceID;
			}

			public bool Compare (piecePos obj)
            {
                return (blockPos.Compare (obj.blockPos) && pieceID == obj.pieceID);
            }
		}

		[SerializeField]
		private bool isSolid = false;
		public List<piecePos> roots = new List<piecePos>();

		// [XAOCX add]
		public BlockHold () { }
		public BlockHold(BlockHold clone) : base(clone) {
			roots = new List<piecePos>();
			foreach (var item in clone.roots) {
				roots.Add(new piecePos(item));
			}
			isSolid = clone.isSolid;
		}
        public BlockHold (int x,int y, int z) {
            BlockPos = new WorldPos (x, y, z);
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