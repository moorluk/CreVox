using UnityEngine;
using System;

namespace CreVox
{
	
	[Serializable]
	public class BlockItem : Block
	{
		public string pieceName = "";
		public float posX = 0;
		public float posY = 0;
		public float posZ = 0;

		public float rotX = 0;
		public float rotY = 0;
		public float rotZ = 0;
		public float rotW = 1;

        public string[] attributes;

        public BlockItem () : base ()
		{
            attributes = new string[5];
            for (int i = 0; i < attributes.Length; i++)
                attributes[i] = "";
        }

		// [XAOCX add]
		public BlockItem(BlockItem clone) : base(clone) {
			this.pieceName = clone.pieceName;
			this.posX = clone.posX;
			this.posY = clone.posY;
			this.posZ = clone.posZ;
			this.rotX = clone.rotX;
			this.rotY = clone.rotY;
			this.rotZ = clone.rotZ;
			this.rotW = clone.rotW;
			this.attributes = (string[])clone.attributes.Clone();
		}

		public override void Destroy ()
		{
			base.Destroy ();
		}

		public override MeshData MeahAddMe (Chunk chunk, int x, int y, int z, MeshData meshData)
		{
			return meshData;
		}

		public override MeshData ColliderAddMe (Chunk chunk, int x, int y, int z, MeshData meshData)
		{
			return meshData;
		}

		public override bool IsSolid (Direction direction)
		{
			return false;
		}
	}
}