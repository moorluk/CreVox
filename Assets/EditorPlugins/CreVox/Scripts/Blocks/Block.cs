using UnityEngine;
using System;

namespace CreVox
{
	public enum Direction
	{
		north,
		east,
		south,
		west,
		up,
		down

	}

	public struct Tile
	{
		public int x;
		public int y;
	}

	[Serializable]
	public class Block
	{
		public WorldPos BlockPos;

		public Block ()
		{
        }
        public Block (int x, int y, int z) {
            BlockPos = new WorldPos (x, y, z);
        }
		// [XAOCX add]
		public Block(Block clone) {
			BlockPos = clone.BlockPos;
		}

		public virtual void Destroy ()
		{
		}

		public virtual MeshData MeahAddMe (Chunk chunk, int x, int y, int z, MeshData meshData)
		{

			if (chunk.GetBlock (x, y + 1, z) == null || !chunk.GetBlock (x, y + 1, z).IsSolid (Direction.down)) {
				meshData = FaceDataUp (chunk, x, y, z, meshData);
			}

			if (chunk.GetBlock (x, y - 1, z) == null || !chunk.GetBlock (x, y - 1, z).IsSolid (Direction.up)) {
				meshData = FaceDataDown (chunk, x, y, z, meshData);
			}

			if (chunk.GetBlock (x, y, z + 1) == null || !chunk.GetBlock (x, y, z + 1).IsSolid (Direction.south)) {
				meshData = FaceDataNorth (chunk, x, y, z, meshData);
			}

			if (chunk.GetBlock (x, y, z - 1) == null || !chunk.GetBlock (x, y, z - 1).IsSolid (Direction.north)) {
				meshData = FaceDataSouth (chunk, x, y, z, meshData);
			}

			if (chunk.GetBlock (x + 1, y, z) == null || !chunk.GetBlock (x + 1, y, z).IsSolid (Direction.west)) {
				meshData = FaceDataEast (chunk, x, y, z, meshData);
			}

			if (chunk.GetBlock (x - 1, y, z) == null || !chunk.GetBlock (x - 1, y, z).IsSolid (Direction.east)) {
				meshData = FaceDataWest (chunk, x, y, z, meshData);
			}

			return meshData;

		}

		static bool SolidCheck(Chunk chunk, int x, int y, int z)
		{
			Block b = chunk.GetBlock (x, y, z);
			if (b == null)
				return true;
			else {
				if (b is BlockAir || b is BlockHold)
					return true;
				else
					return false;
			}
		}

		public virtual MeshData ColliderAddMe (Chunk chunk, int x, int y, int z, MeshData meshData)
		{
			meshData.useRenderDataForCol = true;

			if (SolidCheck (chunk, x, y + 1, z)) {
				meshData = FaceDataUp (chunk, x, y, z, meshData);
			}

			if (SolidCheck (chunk, x, y - 1, z)) {
				meshData = FaceDataDown (chunk, x, y, z, meshData);
			}

			if (SolidCheck (chunk, x, y, z + 1)) {
				meshData = FaceDataNorth (chunk, x, y, z, meshData);
			}

			if (SolidCheck (chunk, x, y, z - 1)) {
				meshData = FaceDataSouth (chunk, x, y, z, meshData);
			}

			if (SolidCheck (chunk, x + 1, y, z)) {
				meshData = FaceDataEast (chunk, x, y, z, meshData);
			}

			if (SolidCheck (chunk, x - 1, y, z)) {
				meshData = FaceDataWest (chunk, x, y, z, meshData);
			}

			return meshData;

		}

		public virtual bool IsSolid (Direction direction)
		{
			switch (direction) {
			case Direction.north:
				return true;
			case Direction.east:
				return true;
			case Direction.south:
				return true;
			case Direction.west:
				return true;
			case Direction.up:
				return true;
			case Direction.down:
				return true;
			}
			return false;
		}

		#region Face Mesh Culculate
		protected virtual MeshData FaceDataUp (Chunk chunk, int x, int y, int z, MeshData meshData)
		{
			VGlobal vg = VGlobal.GetSetting ();
			meshData.AddVertex (new Vector3 (vg.w * (x - 0.5f), vg.h * (y + 0.5f), vg.d * (z + 0.5f)));
			meshData.AddVertex (new Vector3 (vg.w * (x + 0.5f), vg.h * (y + 0.5f), vg.d * (z + 0.5f)));
			meshData.AddVertex (new Vector3 (vg.w * (x + 0.5f), vg.h * (y + 0.5f), vg.d * (z - 0.5f)));
			meshData.AddVertex (new Vector3 (vg.w * (x - 0.5f), vg.h * (y + 0.5f), vg.d * (z - 0.5f)));
			meshData.AddQuadTriangles ();
			//Add the following line to every FaceData function with the direction of the face
			meshData.uv.AddRange (FaceUVs (Direction.up));
			return meshData;
		}

		protected virtual MeshData FaceDataDown (Chunk chunk, int x, int y, int z, MeshData meshData)
		{
			VGlobal vg = VGlobal.GetSetting ();
			meshData.AddVertex (new Vector3 (vg.w * (x - 0.5f), vg.h * (y - 0.5f), vg.d * (z - 0.5f)));
			meshData.AddVertex (new Vector3 (vg.w * (x + 0.5f), vg.h * (y - 0.5f), vg.d * (z - 0.5f)));
			meshData.AddVertex (new Vector3 (vg.w * (x + 0.5f), vg.h * (y - 0.5f), vg.d * (z + 0.5f)));
			meshData.AddVertex (new Vector3 (vg.w * (x - 0.5f), vg.h * (y - 0.5f), vg.d * (z + 0.5f)));

			meshData.AddQuadTriangles ();
			//Add the following line to every FaceData function with the direction of the face
			meshData.uv.AddRange (FaceUVs (Direction.down));
			return meshData;
		}

		protected virtual MeshData FaceDataNorth (Chunk chunk, int x, int y, int z, MeshData meshData)
		{
			VGlobal vg = VGlobal.GetSetting ();
			meshData.AddVertex (new Vector3 (vg.w * (x + 0.5f), vg.h * (y - 0.5f), vg.d * (z + 0.5f)));
			meshData.AddVertex (new Vector3 (vg.w * (x + 0.5f), vg.h * (y + 0.5f), vg.d * (z + 0.5f)));
			meshData.AddVertex (new Vector3 (vg.w * (x - 0.5f), vg.h * (y + 0.5f), vg.d * (z + 0.5f)));
			meshData.AddVertex (new Vector3 (vg.w * (x - 0.5f), vg.h * (y - 0.5f), vg.d * (z + 0.5f)));

			meshData.AddQuadTriangles ();
			//Add the following line to every FaceData function with the direction of the face
			meshData.uv.AddRange (FaceUVs (Direction.north));
			return meshData;
		}

		protected virtual MeshData FaceDataSouth (Chunk chunk, int x, int y, int z, MeshData meshData)
		{
			VGlobal vg = VGlobal.GetSetting ();
			meshData.AddVertex (new Vector3 (vg.w * (x - 0.5f), vg.h * (y - 0.5f), vg.d * (z - 0.5f)));
			meshData.AddVertex (new Vector3 (vg.w * (x - 0.5f), vg.h * (y + 0.5f), vg.d * (z - 0.5f)));
			meshData.AddVertex (new Vector3 (vg.w * (x + 0.5f), vg.h * (y + 0.5f), vg.d * (z - 0.5f)));
			meshData.AddVertex (new Vector3 (vg.w * (x + 0.5f), vg.h * (y - 0.5f), vg.d * (z - 0.5f)));

			meshData.AddQuadTriangles ();
			//Add the following line to every FaceData function with the direction of the face
			meshData.uv.AddRange (FaceUVs (Direction.south));
			return meshData;
		}

		protected virtual MeshData FaceDataEast (Chunk chunk, int x, int y, int z, MeshData meshData)
		{
			VGlobal vg = VGlobal.GetSetting ();
			meshData.AddVertex (new Vector3 (vg.w * (x + 0.5f), vg.h * (y - 0.5f), vg.d * (z - 0.5f)));
			meshData.AddVertex (new Vector3 (vg.w * (x + 0.5f), vg.h * (y + 0.5f), vg.d * (z - 0.5f)));
			meshData.AddVertex (new Vector3 (vg.w * (x + 0.5f), vg.h * (y + 0.5f), vg.d * (z + 0.5f)));
			meshData.AddVertex (new Vector3 (vg.w * (x + 0.5f), vg.h * (y - 0.5f), vg.d * (z + 0.5f)));

			meshData.AddQuadTriangles ();
			//Add the following line to every FaceData function with the direction of the face
			meshData.uv.AddRange (FaceUVs (Direction.east));
			return meshData;
		}

		protected virtual MeshData FaceDataWest (Chunk chunk, int x, int y, int z, MeshData meshData)
		{
			VGlobal vg = VGlobal.GetSetting ();
			meshData.AddVertex (new Vector3 (vg.w * (x - 0.5f), vg.h * (y - 0.5f), vg.d * (z + 0.5f)));
			meshData.AddVertex (new Vector3 (vg.w * (x - 0.5f), vg.h * (y + 0.5f), vg.d * (z + 0.5f)));
			meshData.AddVertex (new Vector3 (vg.w * (x - 0.5f), vg.h * (y + 0.5f), vg.d * (z - 0.5f)));
			meshData.AddVertex (new Vector3 (vg.w * (x - 0.5f), vg.h * (y - 0.5f), vg.d * (z - 0.5f)));

			meshData.AddQuadTriangles ();
			//Add the following line to every FaceData function with the direction of the face
			meshData.uv.AddRange (FaceUVs (Direction.west));
			return meshData;
		}
		#endregion

		public virtual Vector2[] FaceUVs (Direction direction)
		{
			VGlobal vg = VGlobal.GetSetting ();
			Vector2[] UVs = new Vector2[4];
			Tile tilePos = TexturePosition (direction);
			float max = Mathf.Max (new []{ vg.w, vg.h, vg.d });

			switch (direction) {
			case Direction.up:
			case Direction.down:
				UVs [0] = new Vector2 (vg.tileSize * tilePos.x, vg.tileSize * (tilePos.y + vg.d / max));
				UVs [1] = new Vector2 (vg.tileSize * (tilePos.x + vg.w / max), vg.tileSize * (tilePos.y + vg.d / max));
				UVs [2] = new Vector2 (vg.tileSize * (tilePos.x + vg.w / max), vg.tileSize * tilePos.y);
				UVs [3] = new Vector2 (vg.tileSize * tilePos.x, vg.tileSize * tilePos.y);
				return UVs;
			case Direction.north:
			case Direction.south:
				UVs [0] = new Vector2 (vg.tileSize * tilePos.x, vg.tileSize * tilePos.y);
				UVs [1] = new Vector2 (vg.tileSize * tilePos.x, vg.tileSize * (tilePos.y + vg.h / max));
				UVs [2] = new Vector2 (vg.tileSize * (tilePos.x + vg.w / max), vg.tileSize * (tilePos.y + vg.h / max));
				UVs [3] = new Vector2 (vg.tileSize * (tilePos.x + vg.w / max), vg.tileSize * tilePos.y);
				return UVs;
			case Direction.east:
			case Direction.west:
				UVs [0] = new Vector2 (vg.tileSize * tilePos.x, vg.tileSize * tilePos.y);
				UVs [1] = new Vector2 (vg.tileSize * tilePos.x, vg.tileSize * (tilePos.y + vg.h / max));
				UVs [2] = new Vector2 (vg.tileSize * (tilePos.x + vg.d / max), vg.tileSize * (tilePos.y + vg.h / max));
				UVs [3] = new Vector2 (vg.tileSize * (tilePos.x + vg.d / max), vg.tileSize * tilePos.y);
                return UVs;
            default:
                return null;
			}
		}

		public virtual Tile TexturePosition (Direction direction)
		{
			Tile tile = new Tile ();
			switch (direction) {
			case Direction.up:
				tile.x = 0;
				tile.y = 1;
				break;
			case Direction.down:
				tile.x = 1;
				tile.y = 1;
				break;
			case Direction.north:
			case Direction.south:
				tile.x = 0;
				tile.y = 0;
				break;
			case Direction.east:
			case Direction.west:
				tile.x = 1;
				tile.y = 0;
				break;
			}
			return tile;
		}
	}
}